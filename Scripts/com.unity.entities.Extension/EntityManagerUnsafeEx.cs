using System;
using System.Runtime.InteropServices;
using Scripts.Utilities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace package.stormiumteam.shared.ecs
{
#if USE_WEIRD_THINGS
    public static unsafe class EntityManagerUnsafeEx
    {
        public static void* P_GetComponentDataRaw(this EntityManager em, Entity entity, int typeIndex)
        {
            return em.GetComponentDataRawRW(entity, typeIndex);
        }

        public static void P_GetComponentDataRaw(this EntityManager em, Entity entity, int typeIndex, byte[] data)
        {
            var componentDataPtr = em.GetComponentDataRawRW(entity, typeIndex);
            fixed (byte* array = data)
            {
                UnsafeUtility.MemCpy(array, componentDataPtr, data.Length);
            }
        }

        public static void P_SetComponentDataRaw(this EntityManager em, Entity entity, int typeIndex, void* data, int size)
        {
            em.SetComponentDataRaw(entity, typeIndex, data, size);
        }

        public static void P_SetComponentDataRaw(this EntityManager em, Entity entity, Type type, byte[] data)
        {
            var typeIndex = TypeManager.GetTypeIndex(type);
            fixed (byte* array = data)
            {
                em.P_SetComponentDataRaw(entity, typeIndex, array, data.Length);
            }
        }

        public static void P_SetComponentDataRaw(this EntityManager em, Entity entity, int typeIndex, byte[] data)
        {
            fixed (byte* array = data)
            {
                em.P_SetComponentDataRaw(entity, typeIndex, array, data.Length);
            }
        }

        public static void P_SetBufferDataRaw(this EntityManager em, Entity entity, int componentTypeIndex, byte* pointer, int sizeInChunk)
        {
            var elementSize = TypeManager.GetTypeInfo(componentTypeIndex).ElementSize;
            var num = sizeInChunk / elementSize;
            var ptr = (BufferHeader*) UnsafeUtility.Malloc
            (
                UnsafeUtility.SizeOf<BufferHeader>(), UnsafeUtility.AlignOf<BufferHeader>(), Allocator.Persistent
            );

            BufferHeader.Initialize(ptr, num);
            ptr->Pointer = pointer;
            ptr->Length = num;
            ptr->Capacity = Mathf.Max(TypeManager.GetTypeInfo(componentTypeIndex).BufferCapacity, num);
            em.SetBufferRaw(entity, componentTypeIndex, ptr, sizeInChunk);
        }
    }
#endif
}