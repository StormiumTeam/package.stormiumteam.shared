using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using package.stormiumteam.shared;
using UnityEngine;
using PATTERN_ID_TYPE = System.Int32;
using PATTERN_STRING_LINK = System.Collections.Generic.Dictionary<string, StormiumTeam.Shared.PatternIdent>;
using PATTERN_RESULT_LINK = System.Collections.Generic.Dictionary<string, StormiumTeam.Shared.PatternResult>;
using PATTERN_ID_LINK = System.Collections.Generic.Dictionary<int, string>;

// ReSharper disable BuiltInTypeReferenceStyle

namespace StormiumTeam.Shared
{
	public class PatternBankExchange : IDisposable
	{
		public readonly int                  Destination;
		public readonly long                 Id;
		public readonly int                  Origin;
		public          Dictionary<int, int> DestinationToOrigin;

		public Dictionary<int, int> OriginToDestination;

		public PatternBankExchange(int origin, int destination)
		{
			Origin      = origin;
			Destination = destination;
			Id          = new LongIntUnion {Int0Value = origin, Int1Value = destination}.LongValue;


			OriginToDestination = new Dictionary<int, int>();
			DestinationToOrigin = new Dictionary<int, int>();
		}

		public PatternBankExchange(long id)
		{
			var union = new LongIntUnion {LongValue = id};
			Origin      = union.Int0Value;
			Destination = union.Int1Value;

			Id = id;

			OriginToDestination = new Dictionary<int, int>();
			DestinationToOrigin = new Dictionary<int, int>();
		}

		public void Dispose()
		{
			OriginToDestination.Clear();
			DestinationToOrigin.Clear();

			OriginToDestination = null;
			DestinationToOrigin = null;
		}

		public void Set(int originId, int destinationId)
		{
			OriginToDestination[originId]      = destinationId;
			DestinationToOrigin[destinationId] = originId;
		}

		public int GetOriginId(int destinationId)
		{
			return DestinationToOrigin[destinationId];
		}

		public bool HasPatternOrigin(int destinationId)
		{
			return DestinationToOrigin.ContainsKey(destinationId);
		}

		public int GetDestinationId(int originId)
		{
			return OriginToDestination[originId];
		}
	}

	public class PatternBank : IDisposable
	{
		public readonly  int                 InstanceId;
		private          PATTERN_ID_TYPE     m_IdCounter;
		private readonly PATTERN_ID_LINK     m_IdLink;
		private readonly PATTERN_RESULT_LINK m_ResultLink;
		private readonly PATTERN_STRING_LINK m_StringLink;

		public PatternBank(int instanceId)
		{
			InstanceId   = instanceId;
			m_StringLink = new PATTERN_STRING_LINK();
			m_IdLink     = new PATTERN_ID_LINK();
			m_ResultLink = new PATTERN_RESULT_LINK();

			m_IdCounter = 1;
		}

		public int Count => m_IdLink.Count;

		public void Dispose()
		{
			m_StringLink.Clear();
			m_IdLink.Clear();
			m_ResultLink.Clear();
		}

		public event Action<PatternResult> PatternRegister;

		public PatternResult Register(PatternIdent patternIdent)
		{
			if (InstanceId != 0) throw new InvalidOperationException();

			if (!m_IdLink.ContainsValue(patternIdent.Name))
			{
				var id = m_IdCounter++;
				m_IdLink[id] = patternIdent.Name;

				var patternResult = new PatternResult
				{
					Id       = id,
					Internal = patternIdent
				};

				m_ResultLink[patternIdent.Name] = patternResult;
				PatternRegister?.Invoke(patternResult);
			}
			else
			{
				Debug.LogWarning($"Some systems are trying to register an already existing pattern! (p={patternIdent.Name})");
			}

			m_StringLink[patternIdent.Name] = patternIdent;

			return GetPatternResult(patternIdent);
		}

		public void RegisterObject(object patternHolder)
		{
			PatternObjectRegister.ObjRegister(patternHolder, this);
		}

		public bool HasPattern(PatternIdent patternIdent)
		{
			return m_StringLink.ContainsKey(patternIdent.Name);
		}

		public PatternResult GetPatternResult(PatternIdent pattern)
		{
			return m_ResultLink[pattern.Name];
		}

		public PatternResult GetPatternResult(int id)
		{
			// This is slow, and it's only used to compare Local Bank and Other Banks.
			// There should be another way.
			return GetPatternResult(new PatternIdent(m_IdLink[id]));
		}

		public string GetPatternName(int id)
		{
			return m_IdLink[id];
		}

		public ReadOnlyDictionary<string, PatternResult> GetResults()
		{
			return new ReadOnlyDictionary<string, PatternResult>(m_ResultLink);
		}

		public void ForceLink(PatternResult patternResult)
		{
			m_IdLink[patternResult.Id]                = patternResult.Internal.Name;
			m_ResultLink[patternResult.Internal.Name] = patternResult;
			m_StringLink[patternResult.Internal.Name] = patternResult.Internal;
		}
	}

	internal static class PatternObjectRegister
	{
		internal static void ObjRegister(object holder, PatternBank bank)
		{
			var type = holder.GetType();
			var fields = type.GetFields
			(
				BindingFlags.Public
				| BindingFlags.NonPublic
				| BindingFlags.Static
				| BindingFlags.Instance
			);

			foreach (var field in fields)
			{
				if (field.FieldType != typeof(PatternIdent) && field.FieldType != typeof(PatternResult)) continue;

				var nv               = field.GetCustomAttribute<PatternAttribute>();
				var nameAttribute    = field.GetCustomAttribute<PatternNameAttribute>();
				var versionAttribute = field.GetCustomAttribute<PatternVersionAttribute>();

				var pattern = new PatternIdent {Name = nv?.Name ?? nameAttribute?.Value};

				if (string.IsNullOrEmpty(pattern.Name)) pattern.Name = $"{type.Namespace}:{type.Name}.{field.Name}";

				if (nv != null) pattern.Version                    = nv.Version;
				else if (versionAttribute != null) pattern.Version = versionAttribute.Value;

				var result = bank.Register(pattern);

				if (field.FieldType == typeof(PatternIdent))
					field.SetValue(holder, pattern);
				else
					field.SetValue(holder, result);
			}
		}
	}
}