// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  Dictionary
** 
** <OWNER>Microsoft</OWNER>
===========================================================*/
// The dictionary class was a bit modified to be a bit faster (FastTryGet(), Insert() and getting a value is faster)
// Modification by Guerro323

using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;

namespace System.Collections.Generic
{
	[DebuggerDisplay("Count = {Count}")]
	[Serializable]
	[ComVisible(false)]
	public class FastDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, IReadOnlyDictionary<TKey, TValue>, ISerializable
	{
		// constants for serialization
		private const string VersionName       = "Version";
		private const string HashSizeName      = "HashSize"; // Must save Buckets.Length
		private const string KeyValuePairsName = "KeyValuePairs";
		private const string ComparerName      = "Comparer";
		private       object _syncRoot;

		public  int[]   Buckets;
		private int     count;
		private Entry[] entries;
		private int     freeCount;
		private int     freeList;

		private bool          keyIsInteger = typeof(TKey) == typeof(int);
		private KeyCollection keys;

		private TValue          m_Default;
		private ValueCollection values;
		private int             version;

		public FastDictionary() : this(0, null)
		{
		}

		public FastDictionary(int capacity) : this(capacity, null)
		{
		}

		public FastDictionary(IEqualityComparer<TKey> comparer) : this(0, comparer)
		{
		}

		public FastDictionary(int capacity, IEqualityComparer<TKey> comparer)
		{
			Comparer = comparer ?? EqualityComparer<TKey>.Default;

#if FEATURE_CORECLR
            if (PublicHashHelpers.s_UseRandomizedStringHashing && comparer == EqualityComparer<string>.Default)
            {
                this.comparer = (IEqualityComparer<TKey>) NonRandomizedStringEqualityComparer.Default;
            }
#endif // FEATURE_CORECLR
		}

		public FastDictionary(IDictionary<TKey, TValue> dictionary) : this(dictionary, null)
		{
		}

		public FastDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) :
			this(dictionary != null ? dictionary.Count : 0, comparer)
		{
			foreach (var pair in dictionary) Add(pair.Key, pair.Value);
		}

		public IEqualityComparer<TKey> Comparer { get; }

		public KeyCollection Keys
		{
			get
			{
				Contract.Ensures(Contract.Result<KeyCollection>() != null);
				if (keys == null) keys = new KeyCollection(this);
				return keys;
			}
		}

		public ValueCollection Values
		{
			get
			{
				Contract.Ensures(Contract.Result<ValueCollection>() != null);
				if (values == null) values = new ValueCollection(this);
				return values;
			}
		}

		void ICollection.CopyTo(Array array, int index)
		{
			var pairs = array as KeyValuePair<TKey, TValue>[];
			if (pairs != null)
			{
				CopyTo(pairs, index);
			}
			else if (array is DictionaryEntry[])
			{
				var dictEntryArray = array as DictionaryEntry[];
				var entries        = this.entries;
				for (var i = 0; i < count; i++)
					if (entries[i].hashCode >= 0)
						dictEntryArray[index++] = new DictionaryEntry(entries[i].key, entries[i].value);
			}
			else
			{
				var objects = array as object[];

				try
				{
					var count   = this.count;
					var entries = this.entries;
					for (var i = 0; i < count; i++)
						if (entries[i].hashCode >= 0)
							objects[index++] = new KeyValuePair<TKey, TValue>(entries[i].key, entries[i].value);
				}
				catch (ArrayTypeMismatchException)
				{
				}
			}
		}

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null) Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
				return _syncRoot;
			}
		}

		bool IDictionary.IsFixedSize => false;

		bool IDictionary.IsReadOnly => false;

		ICollection IDictionary.Keys => Keys;

		ICollection IDictionary.Values => Values;

		object IDictionary.this[object key]
		{
			get
			{
				if (IsCompatibleKey(key))
				{
					var i = FindEntry((TKey) key);
					if (i >= 0) return entries[i].value;
				}

				return null;
			}
			set
			{
				try
				{
					var tempKey = (TKey) key;
					try
					{
						this[tempKey] = (TValue) value;
					}
					catch (InvalidCastException)
					{
					}
				}
				catch (InvalidCastException)
				{
				}
			}
		}

		void IDictionary.Add(object key, object value)
		{
			try
			{
				var tempKey = (TKey) key;

				try
				{
					Add(tempKey, (TValue) value);
				}
				catch (InvalidCastException)
				{
				}
			}
			catch (InvalidCastException)
			{
			}
		}

		bool IDictionary.Contains(object key)
		{
			if (IsCompatibleKey(key)) return ContainsKey((TKey) key);

			return false;
		}

		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return new Enumerator(this, Enumerator.DictEntry);
		}

		void IDictionary.Remove(object key)
		{
			if (IsCompatibleKey(key)) Remove((TKey) key);
		}

		public int Count => count - freeCount;

		ICollection<TKey> IDictionary<TKey, TValue>.Keys
		{
			get
			{
				if (keys == null) keys = new KeyCollection(this);
				return keys;
			}
		}

		ICollection<TValue> IDictionary<TKey, TValue>.Values
		{
			get
			{
				if (values == null) values = new ValueCollection(this);
				return values;
			}
		}

		public TValue this[TKey key]
		{
			get
			{
				TValue value;
				FastTryGet(key, out value);
				return value;
			}
			set => Insert(key, value, false);
		}

		public void Add(TKey key, TValue value)
		{
			Insert(key, value, true);
		}

		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
		{
			Add(keyValuePair.Key, keyValuePair.Value);
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
		{
			var i = FindEntry(keyValuePair.Key);
			if (i >= 0 && EqualityComparer<TValue>.Default.Equals(entries[i].value, keyValuePair.Value)) return true;
			return false;
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
		{
			var i = FindEntry(keyValuePair.Key);
			if (i >= 0 && EqualityComparer<TValue>.Default.Equals(entries[i].value, keyValuePair.Value))
			{
				Remove(keyValuePair.Key);
				return true;
			}

			return false;
		}

		public void Clear()
		{
			if (count > 0)
			{
				for (var i = 0; i < Buckets.Length; i++) Buckets[i] = -1;
				Array.Clear(entries, 0, count);
				freeList  = -1;
				count     = 0;
				freeCount = 0;
				version++;
			}
		}

		public bool ContainsKey(TKey key)
		{
			return FindEntry(key) >= 0;
		}

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return new Enumerator(this, Enumerator.KeyValuePair);
		}

		public bool Remove(TKey key)
		{
			if (Buckets != null)
			{
				var hashCode = Comparer.GetHashCode(key) & 0x7FFFFFFF;
				var bucket   = hashCode % Buckets.Length;
				var last     = -1;
				for (var i = Buckets[bucket]; i >= 0; last = i, i = entries[i].next)
					if (entries[i].hashCode == hashCode && Comparer.Equals(entries[i].key, key))
					{
						if (last < 0)
							Buckets[bucket] = entries[i].next;
						else
							entries[last].next = entries[i].next;
						entries[i].hashCode = -1;
						entries[i].next     = freeList;
						entries[i].key      = default;
						entries[i].value    = default;
						freeList            = i;
						freeCount++;
						version++;
						return true;
					}
			}

			return false;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			var i = FindEntry(key);
			if (i >= 0)
			{
				value = entries[i].value;
				return true;
			}

			value = default;
			return false;
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
		{
			CopyTo(array, index);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this, Enumerator.KeyValuePair);
		}

		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
		{
			get
			{
				if (keys == null) keys = new KeyCollection(this);
				return keys;
			}
		}

		IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
		{
			get
			{
				if (values == null) values = new ValueCollection(this);
				return values;
			}
		}

		[SecurityCritical] // auto-generated_required
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(VersionName, version);

#if FEATURE_RANDOMIZED_STRING_HASHING
            info.AddValue(ComparerName, PublicHashHelpers.GetEqualityComparerForSerialization(comparer), typeof(IEqualityComparer<TKey>));
#else
			info.AddValue(ComparerName, Comparer, typeof(IEqualityComparer<TKey>));
#endif

			info.AddValue(HashSizeName, Buckets == null ? 0 : Buckets.Length); //This is the length of the bucket array.
			if (Buckets != null)
			{
				var array = new KeyValuePair<TKey, TValue>[Count];
				CopyTo(array, 0);
				info.AddValue(KeyValuePairsName, array, typeof(KeyValuePair<TKey, TValue>[]));
			}
		}

		public ref TValue FastGet(TKey key)
		{
			if (Buckets != null)
			{
				var hashCode = Comparer.GetHashCode(key) & 0x7FFFFFFF;
				for (var i = Buckets[hashCode % Buckets.Length]; i >= 0;)
				{
					ref var entry = ref entries[i];
					i = entry.next;
					if (entry.hashCode == hashCode && Comparer.Equals(entry.key, key)) return ref entry.value;
				}
			}

			throw new KeyNotFoundException();
		}

		public ref TValue FastGetOrDefault(TKey key)
		{
			if (Buckets != null)
			{
				var hashCode = Comparer.GetHashCode(key) & 0x7FFFFFFF;
				for (var i = Buckets[hashCode % Buckets.Length]; i >= 0;)
				{
					ref var entry = ref entries[i];
					i = entry.next;
					if (entry.hashCode == hashCode && Comparer.Equals(entry.key, key)) return ref entry.value;
				}
			}

			m_Default = default;

			return ref m_Default;
		}

		public bool ContainsValue(TValue value)
		{
			if (value == null)
			{
				for (var i = 0; i < count; i++)
					if (entries[i].hashCode >= 0 && entries[i].value == null)
						return true;
			}
			else
			{
				var c = EqualityComparer<TValue>.Default;
				for (var i = 0; i < count; i++)
					if (entries[i].hashCode >= 0 && c.Equals(entries[i].value, value))
						return true;
			}

			return false;
		}

		private void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
		{
			var count   = this.count;
			var entries = this.entries;
			for (var i = 0; i < count; i++)
				if (entries[i].hashCode >= 0)
					array[index++] = new KeyValuePair<TKey, TValue>(entries[i].key, entries[i].value);
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this, Enumerator.KeyValuePair);
		}

		private int FindEntry(TKey key)
		{
			if (Buckets != null)
			{
				var hashCode = Comparer.GetHashCode(key) & 0x7FFFFFFF;
				for (var i = Buckets[hashCode % Buckets.Length]; i >= 0; i = entries[i].next)
					if (entries[i].hashCode == hashCode && Comparer.Equals(entries[i].key, key))
						return i;
			}

			return -1;
		}

		public ref TValue RefGet(TKey key)
		{
			var hasFoundOne = false;
			if (Buckets != null)
			{
				int hashCode;
				if (keyIsInteger)
				{
					hashCode = key.GetHashCode() & 0x7FFFFFFF;

					for (var i = Buckets[hashCode % Buckets.Length]; i >= 0;)
					{
						ref var entry = ref entries[i];
						if (entry.hashCode == hashCode) return ref entry.value;
						i = entry.next;
					}
				}
				else
				{
					hashCode = Comparer.GetHashCode(key) & 0x7FFFFFFF;

					for (var i = Buckets[hashCode % Buckets.Length]; i >= 0;)
					{
						ref var entry = ref entries[i];
						if (entry.hashCode == hashCode && Comparer.Equals(entry.key, key)) return ref entry.value;
						i = entry.next;
					}
				}
			}

			throw new KeyNotFoundException();
		}

		public bool FastTryGet(TKey key, out TValue value)
		{
			if (Buckets != null)
			{
				int hashCode;
				if (keyIsInteger)
				{
					hashCode = key.GetHashCode() & 0x7FFFFFFF;

					for (var i = Buckets[hashCode % Buckets.Length]; i >= 0;)
					{
						ref var entry = ref entries[i];
						if (entry.hashCode == hashCode)
						{
							value = entry.value;
							return true;
						}

						i = entry.next;
					}
				}
				else
				{
					hashCode = Comparer.GetHashCode(key) & 0x7FFFFFFF;

					for (var i = Buckets[hashCode % Buckets.Length]; i >= 0;)
					{
						ref var entry = ref entries[i];
						if (entry.hashCode == hashCode && Comparer.Equals(entry.key, key))
						{
							value = entry.value;
							return true;
						}

						i = entry.next;
					}
				}
			}

			value = default;
			return false;
		}

		public bool RefFastTryGet(int key, ref TValue value)
		{
			if (!keyIsInteger)
				throw new Exception();

			if (Buckets != null)
			{
				int hashCode, bucketLength;
				hashCode     = key & 0x7FFFFFFF;
				bucketLength = Buckets.Length;

				for (var i = Buckets[hashCode % bucketLength]; i >= 0;)
				{
					ref var entry = ref entries[i];
					if (entry.hashCode == hashCode)
					{
						value = entry.value;
						return true;
					}

					i = entry.next;
				}
			}

			value = default;
			return false;
		}

		public bool RefFastTryGet(TKey key, ref TValue value)
		{
			if (Buckets != null)
			{
				int hashCode;
				if (keyIsInteger)
				{
					hashCode = key.GetHashCode() & 0x7FFFFFFF;

					for (var i = Buckets[hashCode % Buckets.Length]; i >= 0;)
					{
						ref var entry = ref entries[i];
						if (entry.hashCode == hashCode)
						{
							value = entry.value;
							return true;
						}

						i = entry.next;
					}
				}
				else
				{
					hashCode = Comparer.GetHashCode(key) & 0x7FFFFFFF;

					for (var i = Buckets[hashCode % Buckets.Length]; i >= 0;)
					{
						ref var entry = ref entries[i];
						if (entry.hashCode == hashCode && Comparer.Equals(entry.key, key))
						{
							value = entry.value;
							return true;
						}

						i = entry.next;
					}
				}
			}

			value = default;
			return false;
		}

		private void Initialize(int capacity)
		{
			var size = PublicHashHelpers.GetPrime(capacity);
			Buckets = new int[size];
			for (var i = 0; i < Buckets.Length; i++) Buckets[i] = -1;
			entries  = new Entry[size];
			freeList = -1;
		}

		private void Insert(TKey key, TValue value, bool add)
		{
			if (Buckets == null) Initialize(0);
			int hashCode;

			if (keyIsInteger)
				hashCode = key.GetHashCode() & 0x7FFFFFFF;
			else
				hashCode = Comparer.GetHashCode(key) & 0x7FFFFFFF;


			var targetBucket = hashCode % Buckets.Length;

#if FEATURE_RANDOMIZED_STRING_HASHING
            int collisionCount = 0;
#endif

			ref var bucket = ref Buckets[targetBucket];
			for (var i = bucket; i >= 0; i = entries[i].next)
			{
				ref var forLoopEntry = ref entries[i];
				if (forLoopEntry.hashCode == hashCode && Comparer.Equals(forLoopEntry.key, key))
				{
					forLoopEntry.value = value;

					version++;
					return;
				}

#if FEATURE_RANDOMIZED_STRING_HASHING
                collisionCount++;
#endif
			}

			int index;
			if (freeCount > 0)
			{
				index    = freeList;
				freeList = entries[index].next;
				freeCount--;
			}
			else
			{
				if (count == entries.Length)
				{
					Resize();
					targetBucket = hashCode % Buckets.Length;
				}

				index = count;
				count++;
			}

			ref var entry   = ref entries[index];
			ref var bucket2 = ref Buckets[targetBucket];

			entry.hashCode = hashCode;
			entry.next     = bucket2;
			entry.key      = key;
			entry.value    = value;

			bucket2 = index;
			version++;

#if FEATURE_RANDOMIZED_STRING_HASHING
#if FEATURE_CORECLR
            // In case we hit the collision threshold we'll need to switch to the comparer which is using randomized string hashing
            // in this case will be EqualityComparer<string>.Default.
            // Note, randomized string hashing is turned on by default on coreclr so EqualityComparer<string>.Default will 
            // be using randomized string hashing

            if (collisionCount > PublicHashHelpers.HashCollisionThreshold && comparer == NonRandomizedStringEqualityComparer.Default) 
            {
                comparer = (IEqualityComparer<TKey>) EqualityComparer<string>.Default;
                Resize(entries.Length, true);
            }
#else
            if(collisionCount > PublicHashHelpers.HashCollisionThreshold && PublicHashHelpers.IsWellKnownEqualityComparer(comparer)) 
            {
                comparer = (IEqualityComparer<TKey>) PublicHashHelpers.GetRandomizedEqualityComparer(comparer);
                Resize(entries.Length, true);
            }
#endif // FEATURE_CORECLR

#endif
		}

		private void Resize()
		{
			Resize(PublicHashHelpers.ExpandPrime(count), false);
		}

		private void Resize(int newSize, bool forceNewHashCodes)
		{
			Contract.Assert(newSize >= entries.Length);
			var newBuckets                                            = new int[newSize];
			for (var i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;
			var newEntries                                            = new Entry[newSize];
			Array.Copy(entries, 0, newEntries, 0, count);
			if (forceNewHashCodes)
				for (var i = 0; i < count; i++)
					if (newEntries[i].hashCode != -1)
						newEntries[i].hashCode = Comparer.GetHashCode(newEntries[i].key) & 0x7FFFFFFF;
			for (var i = 0; i < count; i++)
				if (newEntries[i].hashCode >= 0)
				{
					var bucket = newEntries[i].hashCode % newSize;
					newEntries[i].next = newBuckets[bucket];
					newBuckets[bucket] = i;
				}

			Buckets = newBuckets;
			entries = newEntries;
		}

		// This is a convenience method for the internal callers that were converted from using Hashtable.
		// Many were combining key doesn't exist and key exists but null value (for non-value types) checks.
		// This allows them to continue getting that behavior with minimal code delta. This is basically
		// TryGetValue without the out param
		internal TValue GetValueOrDefault(TKey key)
		{
			var i = FindEntry(key);
			if (i >= 0) return entries[i].value;
			return default;
		}

		private static bool IsCompatibleKey(object key)
		{
			return key is TKey;
		}

		private struct Entry
		{
			public int    hashCode; // Lower 31 bits of hash code, -1 if unused
			public int    next;     // Index of next entry, -1 if last
			public TKey   key;      // Key of entry
			public TValue value;    // Value of entry
		}

		[Serializable]
		public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>,
		                           IDictionaryEnumerator
		{
			private readonly FastDictionary<TKey, TValue> dictionary;
			private          int                          version;
			private          int                          index;
			private          KeyValuePair<TKey, TValue>   current;
			private readonly int                          getEnumeratorRetType; // What should Enumerator.Current return?

			internal const int DictEntry    = 1;
			internal const int KeyValuePair = 2;

			internal Enumerator(FastDictionary<TKey, TValue> dictionary, int getEnumeratorRetType)
			{
				this.dictionary           = dictionary;
				version                   = dictionary.version;
				index                     = 0;
				this.getEnumeratorRetType = getEnumeratorRetType;
				current                   = new KeyValuePair<TKey, TValue>();
			}

			public bool MoveNext()
			{
				// Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
				// dictionary.count+1 could be negative if dictionary.count is Int32.MaxValue
				while ((uint) index < (uint) dictionary.count)
				{
					if (dictionary.entries[index].hashCode >= 0)
					{
						current = new KeyValuePair<TKey, TValue>(dictionary.entries[index].key, dictionary.entries[index].value);
						index++;
						return true;
					}

					index++;
				}

				index   = dictionary.count + 1;
				current = new KeyValuePair<TKey, TValue>();
				return false;
			}

			public KeyValuePair<TKey, TValue> Current => current;

			public void Dispose()
			{
			}

			object IEnumerator.Current
			{
				get
				{
					if (getEnumeratorRetType == DictEntry)
						return new DictionaryEntry(current.Key, current.Value);
					return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
				}
			}

			void IEnumerator.Reset()
			{
				index   = 0;
				current = new KeyValuePair<TKey, TValue>();
			}

			DictionaryEntry IDictionaryEnumerator.Entry => new DictionaryEntry(current.Key, current.Value);

			object IDictionaryEnumerator.Key => current.Key;

			object IDictionaryEnumerator.Value => current.Value;
		}

		[DebuggerDisplay("Count = {Count}")]
		[Serializable]
		public sealed class KeyCollection : ICollection<TKey>, ICollection, IReadOnlyCollection<TKey>
		{
			private readonly FastDictionary<TKey, TValue> dictionary;

			public KeyCollection(FastDictionary<TKey, TValue> dictionary)
			{
				this.dictionary = dictionary;
			}

			void ICollection.CopyTo(Array array, int index)
			{
				var keys = array as TKey[];
				if (keys != null)
				{
					CopyTo(keys, index);
				}
				else
				{
					var objects = array as object[];

					var count   = dictionary.count;
					var entries = dictionary.entries;
					try
					{
						for (var i = 0; i < count; i++)
							if (entries[i].hashCode >= 0)
								objects[index++] = entries[i].key;
					}
					catch (ArrayTypeMismatchException)
					{
					}
				}
			}

			bool ICollection.IsSynchronized => false;

			object ICollection.SyncRoot => ((ICollection) dictionary).SyncRoot;

			public void CopyTo(TKey[] array, int index)
			{
				var count   = dictionary.count;
				var entries = dictionary.entries;
				for (var i = 0; i < count; i++)
					if (entries[i].hashCode >= 0)
						array[index++] = entries[i].key;
			}

			public int Count => dictionary.Count;

			bool ICollection<TKey>.IsReadOnly => true;

			void ICollection<TKey>.Add(TKey item)
			{
			}

			void ICollection<TKey>.Clear()
			{
			}

			bool ICollection<TKey>.Contains(TKey item)
			{
				return dictionary.ContainsKey(item);
			}

			bool ICollection<TKey>.Remove(TKey item)
			{
				return false;
			}

			IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
			{
				return new Enumerator(dictionary);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return new Enumerator(dictionary);
			}

			public Enumerator GetEnumerator()
			{
				return new Enumerator(dictionary);
			}

			[Serializable]
			public struct Enumerator : IEnumerator<TKey>, IEnumerator
			{
				private readonly FastDictionary<TKey, TValue> dictionary;
				private          int                          index;
				private          int                          version;

				internal Enumerator(FastDictionary<TKey, TValue> dictionary)
				{
					this.dictionary = dictionary;
					version         = dictionary.version;
					index           = 0;
					Current         = default;
				}

				public void Dispose()
				{
				}

				public bool MoveNext()
				{
					while ((uint) index < (uint) dictionary.count)
					{
						if (dictionary.entries[index].hashCode >= 0)
						{
							Current = dictionary.entries[index].key;
							index++;
							return true;
						}

						index++;
					}

					index   = dictionary.count + 1;
					Current = default;
					return false;
				}

				public TKey Current { get; private set; }

				object IEnumerator.Current => Current;

				void IEnumerator.Reset()
				{
					index   = 0;
					Current = default;
				}
			}
		}

		[DebuggerDisplay("Count = {Count}")]
		[Serializable]
		public sealed class ValueCollection : ICollection<TValue>, ICollection, IReadOnlyCollection<TValue>
		{
			private readonly FastDictionary<TKey, TValue> dictionary;

			public ValueCollection(FastDictionary<TKey, TValue> dictionary)
			{
				this.dictionary = dictionary;
			}

			void ICollection.CopyTo(Array array, int index)
			{
				var values = array as TValue[];
				if (values != null)
				{
					CopyTo(values, index);
				}
				else
				{
					var objects = array as object[];
					var count   = dictionary.count;
					var entries = dictionary.entries;
					try
					{
						for (var i = 0; i < count; i++)
							if (entries[i].hashCode >= 0)
								objects[index++] = entries[i].value;
					}
					catch (ArrayTypeMismatchException)
					{
					}
				}
			}

			bool ICollection.IsSynchronized => false;

			object ICollection.SyncRoot => ((ICollection) dictionary).SyncRoot;

			public void CopyTo(TValue[] array, int index)
			{
				var count   = dictionary.count;
				var entries = dictionary.entries;
				for (var i = 0; i < count; i++)
					if (entries[i].hashCode >= 0)
						array[index++] = entries[i].value;
			}

			public int Count => dictionary.Count;

			bool ICollection<TValue>.IsReadOnly => true;

			void ICollection<TValue>.Add(TValue item)
			{
			}

			bool ICollection<TValue>.Remove(TValue item)
			{
				return false;
			}

			void ICollection<TValue>.Clear()
			{
			}

			bool ICollection<TValue>.Contains(TValue item)
			{
				return dictionary.ContainsValue(item);
			}

			IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
			{
				return new Enumerator(dictionary);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return new Enumerator(dictionary);
			}

			public Enumerator GetEnumerator()
			{
				return new Enumerator(dictionary);
			}

			[Serializable]
			public struct Enumerator : IEnumerator<TValue>, IEnumerator
			{
				private readonly FastDictionary<TKey, TValue> dictionary;
				private          int                          index;
				private          int                          version;

				internal Enumerator(FastDictionary<TKey, TValue> dictionary)
				{
					this.dictionary = dictionary;
					version         = dictionary.version;
					index           = 0;
					Current         = default;
				}

				public void Dispose()
				{
				}

				public bool MoveNext()
				{
					while ((uint) index < (uint) dictionary.count)
					{
						if (dictionary.entries[index].hashCode >= 0)
						{
							Current = dictionary.entries[index].value;
							index++;
							return true;
						}

						index++;
					}

					index   = dictionary.count + 1;
					Current = default;
					return false;
				}

				public TValue Current { get; private set; }

				object IEnumerator.Current => Current;

				void IEnumerator.Reset()
				{
					index   = 0;
					Current = default;
				}
			}
		}
	}
}