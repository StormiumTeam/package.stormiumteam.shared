using System;
using package.stormiumteam.shared;
using package.stormiumteam.shared.ecs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Profiling;

namespace Scripts.Utilities
{
#if USE_WEIRD_THINGS
    public static unsafe class EntitySerializer
    {
        public static void CreateDataHeader(NativeList<byte> data)
        {
            PutInt(data, 0);
        }

        public static void SetHeaderComponentCount(NativeList<byte> data, int length)
        {
            ReplaceInt(data, length, 0);
        }

        public static int RetrieveComponentCount(NativeList<byte> data)
        {
            return ReadInt(data, 0);
        }

        public static void SerializeComponent(NativeList<byte> data, int componentTypeIndex, Entity entity, EntityManager entityManager, bool enableCheck)
        {            
            if (enableCheck)
            {
                Debug.Assert(entityManager.Exists(entity), "entityManager.Exists(entity)");
                Debug.Assert(entityManager.HasComponent(entity, TypeManager.GetType(componentTypeIndex)),
                    "entityManager.HasComponent(entity, TypeManager.GetType(componentTypeIndex))");
            }

            var typeInfo = TypeManager.GetTypeInfo(componentTypeIndex);
            
            PutInt(data, componentTypeIndex);
            if (typeInfo.Category == TypeManager.TypeCategory.ComponentData)
            {
                SerializeComponentData(data, componentTypeIndex, entity, entityManager);
            }
            else if (typeInfo.Category == TypeManager.TypeCategory.BufferData)
            {
                SerializeBufferData(data, componentTypeIndex, entity, entityManager);
            }
            else
            {
                throw new NotImplementedException(typeInfo.Category.ToString());
            }
        }

        public static void DeserializeComponentFromHeader(NativeList<byte> data, int componentIndex, Entity entity, EntityManager entityManager,
                                                          bool createComponent, bool enableCheck)
        {
            if (enableCheck)
            {
                Debug.Assert(entityManager.Exists(entity), "entityManager.Exists(entity)");
            }
            
            var componentCount = RetrieveComponentCount(data);
            if (componentIndex == 0) // easy
            {
                DeserializeComponent(data, 4, entity, entityManager, createComponent);
            }
            else // it's harder as we don't know any components information before
            {
                var skippedBytes = 4;
                for (int i = 0; i != componentCount; i++)
                {
                    if (componentIndex == i)
                    {
                        DeserializeComponent(data, skippedBytes, entity, entityManager, createComponent);
                        break;
                    }
                    
                    skippedBytes += 4;
                    var length = ReadInt(data, skippedBytes);
                    skippedBytes += 4;
                    skippedBytes += length;
                }
            }
        }
        
        public static int DeserializeComponent(NativeList<byte> data, int readIndex, int componentTypeIndex, Entity entity, EntityManager entityManager,
                                               bool             createComponent)
        {
            var typeInfo = TypeManager.GetTypeInfo(componentTypeIndex);

            if (!entityManager.HasComponent(entity, TypeManager.GetType(componentTypeIndex)))
            {
                if (createComponent) entityManager.AddComponent(entity, ComponentType.FromTypeIndex(componentTypeIndex));
                else throw new ArgumentException("The component has not been added to the entity.");
            }

            if (typeInfo.Category == TypeManager.TypeCategory.ComponentData)
            {
                return DeserializeComponentData(data, readIndex, componentTypeIndex, entity, entityManager, createComponent);
            }
            else if (typeInfo.Category == TypeManager.TypeCategory.BufferData)
            {
                return DeserializeBufferData(data, readIndex, componentTypeIndex, entity, entityManager, createComponent);
            }
            else
            {
                throw new NotImplementedException(typeInfo.Category.ToString());
            }
        }

        public static int DeserializeComponent(NativeList<byte> data, int readIndex, Entity entity, EntityManager entityManager,
                                                             bool createComponent)
        {
            var componentTypeIndex = ReadInt(data, readIndex);
            readIndex += 4;
            
            var typeInfo = TypeManager.GetTypeInfo(componentTypeIndex);

            if (!entityManager.HasComponent(entity, TypeManager.GetType(componentTypeIndex)))
            {
                if (createComponent) entityManager.AddComponent(entity, ComponentType.FromTypeIndex(componentTypeIndex));
                else throw new ArgumentException("The component has not been added to the entity.");
            }

            if (typeInfo.Category == TypeManager.TypeCategory.ComponentData)
            {
                return DeserializeComponentData(data, readIndex, componentTypeIndex, entity, entityManager, createComponent);
            }
            else if (typeInfo.Category == TypeManager.TypeCategory.BufferData)
            {
                return DeserializeBufferData(data, readIndex, componentTypeIndex, entity, entityManager, createComponent);
            }
            else
            {
                throw new NotImplementedException(typeInfo.Category.ToString());
            }
        }
        
        public static void SerializeComponentData(NativeList<byte> data, int componentTypeIndex, Entity entity, EntityManager entityManager)
        {
            var typeInfo = TypeManager.GetTypeInfo(componentTypeIndex);
            if (typeInfo.SizeInChunk == 0)
            {
                PutInt(data, 0);
                
                return;
            }
            
            var dataPointer = entityManager.P_GetComponentDataRaw(entity, componentTypeIndex);
            PutInt(data, typeInfo.ElementSize);
            PutBuffer(data, dataPointer, typeInfo.ElementSize);
        }
        
        public static int DeserializeComponentData(NativeList<byte> data, int readIndex, int typeIndex, Entity entity, EntityManager entityManager,
                                                              bool createComponent)
        {
            var length = ReadInt(data, readIndex);

            readIndex += 4;
            if (length == 0) // zero sized struct, do nothing
            {
                return readIndex;
            }
            
            entityManager.P_SetComponentDataRaw(entity, typeIndex, (void*)IntPtr.Add((IntPtr)data.GetUnsafePtr(), readIndex), length);

            return readIndex + length;
        }
        
        public static void SerializeBufferData(NativeList<byte> data, int componentTypeIndex, Entity entity, EntityManager entityManager)
        {
            if (!entityManager.HasComponent(entity, ComponentType.FromTypeIndex(componentTypeIndex)))
                throw new ArgumentException("The component has not been added to the entity.");
            
            var typeInfo = TypeManager.GetTypeInfo(componentTypeIndex);
            var header = (BufferHeader*) entityManager.P_GetComponentDataRaw(entity, componentTypeIndex);
            var dataPointer = GetElementPointer(header);
            var length = header->Length * typeInfo.ElementSize;
            
            PutInt(data, length);
            PutBuffer(data, dataPointer, length);
        }
        
        public static int DeserializeBufferData(NativeList<byte> data, int readIndex, int typeIndex, Entity entity, EntityManager entityManager,
                                                   bool             createComponent)
        {
            var length = ReadInt(data, readIndex);

            readIndex += 4;
            if (length == 0) // zero sized struct, do nothing
            {
                return readIndex;
            }

            var align = UnsafeUtility.SizeOf(TypeManager.GetType(typeIndex));
            // The entities package should automatically free our allocation.
            var bufferPointer = UnsafeUtility.Malloc(length, align, Allocator.Persistent);
            UnsafeUtility.MemCpy(bufferPointer, (void*)IntPtr.Add((IntPtr)data.GetUnsafePtr(), readIndex), length); 
            
            entityManager.P_SetBufferDataRaw(entity, typeIndex, (byte*)bufferPointer, length);
            
            return readIndex + length;
        }

        public static void PutInt(NativeList<byte> data, int value)
        {
            data.Add((byte) (value >> 24));
            data.Add((byte) (value >> 16));
            data.Add((byte) (value >> 8));
            data.Add((byte) value);
        }

        public static int ReadInt(NativeList<byte> data, int index)
        {
            return data[index] << 24 | data[index + 1] << 16 | data[index + 2] << 8 | data[index + 3];
        }

        public static void ReplaceInt(NativeList<byte> data, int value, int index)
        {
            data[0] = (byte) (value >> 24);
            data[1] = (byte) (value >> 16);
            data[2] = (byte) (value >> 8);
            data[3] = (byte) value;
        }

        public static void PutBuffer(NativeList<byte> data, void* buffer, int size)
        {
            var start = data.Length;
            var length = data.Length + size;
            
            data.ResizeUninitialized(length);

            var ptr = data.GetUnsafePtr();
            UnsafeUtility.MemCpy((void*)((IntPtr)ptr + (start)), buffer, size);
        }
        
        public static byte* GetElementPointer(BufferHeader* header)
        {
            if (header->Pointer != null)
                return header->Pointer;
            
            return (byte*) (header + 1);
        }

        public struct BufferHeader
        {
            public byte* Pointer;
            public int   Length;
            public int   Capacity;
        }
    }
#endif
}