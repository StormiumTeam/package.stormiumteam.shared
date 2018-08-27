using Unity.Collections.LowLevel.Unsafe;

namespace package.stormiumteam.shared
{
    public static unsafe class UnsafeSerializer
    {
        // ---------------------------------------------------------- //
        // Primary raw methods
        // ---------------------------------------------------------- //
        /// <summary>
        /// Serialize the data into a buffer
        /// </summary>
        /// <param name="buffer">The buffer</param>
        /// <param name="data">The data</param>
        /// <typeparam name="T">The type of the data (must be a non-nullable type)</typeparam>
        public static void Serialize<T>(void* buffer, T data)
            where T : struct
        {
            var size = UnsafeUtility.SizeOf<T>();
            var addr = UnsafeUtility.AddressOf(ref data);

            UnsafeUtility.MemCpy(buffer, addr, size);
        }

        /// <summary>
        /// Deserialize the buffer into a new data
        /// </summary>
        /// <param name="buffer">The buffer</param>
        /// <param name="data">The data</param>
        /// <typeparam name="T">The type of the data (must be a non-nullable type)</typeparam>
        public static void Deserialize<T>(void* buffer, out T data)
            where T : struct
        {
            UnsafeUtility.CopyPtrToStructure(buffer, out data);
        }

        // ---------------------------------------------------------- //
        // Secondary methods
        // ---------------------------------------------------------- //
        /// <summary>
        /// Serialize the data into the array
        /// </summary>
        /// <param name="buffer">The array buffer</param>
        /// <param name="data">The data</param>
        /// <typeparam name="T">The type of the data</typeparam>
        public static void Serialize<T>(byte[] buffer, T data)
            where T : struct
        {
            fixed (byte* fixedBuffer = buffer)
            {
                Serialize(fixedBuffer, data);
            }
        }
        
        /// <summary>
        /// Serialize the data into the array (Allocate GC)
        /// </summary>
        /// <param name="data">The data</param>
        /// <typeparam name="T">The type of the data</typeparam>
        /// <returns>The new array with the data</returns>
        public static byte[] Serialize<T>(T data)
            where T : struct
        {
            var buffer = new byte[UnsafeUtility.SizeOf<T>()];
            fixed (byte* fixedBuffer = buffer)
            {
                Serialize(fixedBuffer, data);
            }
            return buffer;
        }
        
        public static void Deserialize<T>(byte[] buffer, out T data)
            where T : struct
        {
            fixed (byte* fixedBuffer = buffer)
            {
                Deserialize(fixedBuffer, out data);
            }
        }
        
        public static T Deserialize<T>(byte[] buffer)
            where T : struct
        {
            T data;
            fixed (byte* fixedBuffer = buffer)
            {
                Deserialize(fixedBuffer, out data);
            }
            return data;
        }
    }
}