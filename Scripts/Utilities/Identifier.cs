using System;

namespace Scripts.Utilities
{
    /// <summary>
    ///     A sequential unique non-random identifier.
    /// </summary>
    /// <remarks>
    ///     The identifiers are only unique if they are sequenced.
    /// </remarks>
    [Serializable]
    public struct SeqId
    {
        public ulong M1;
        public uint  U1, U2;

        public SeqId(ulong m1, uint u1, uint u2)
        {
            M1 = m1;
            U1 = u1;
            U2 = u2;
        }

        public Guid ToGuid()
        {
            BitConverter.GetBytes(M1);

            return new Guid(U1,
                (ushort) U2, (ushort) (U2 >> 16),
                (byte) M1,
                (byte) (M1 >> 8),
                (byte) (M1 >> 16),
                (byte) (M1 >> 24),
                (byte) (M1 >> 32),
                (byte) (M1 >> 40),
                (byte) (M1 >> 48),
                (byte) (M1 >> 56));
        }

        public static SeqId Create(SeqId previous)
        {
            return Create(previous.M1, previous.U1, previous.U2);
        }

        public static SeqId Create(ulong m1, uint u1, uint u2)
        {
            #region Italian food

            var   __m1 = m1;
            ulong __u1 = u1;
            ulong __u2 = u2;

            __u2++;
            if (__u2 >= uint.MaxValue)
            {
                __u2 = 0;
                __u1++;
            }

            if (__u1 >= uint.MaxValue)
            {
                __u1 = 0;
                __m1++;
            }

            m1 = __m1;
            u1 = (uint) __u1;
            u2 = (uint) __u2;

            #endregion

            return new SeqId(m1, u1, u2);
        }

        public override string ToString()
        {
            return $"#<{M1}:{U1}:{U2}>";
        }
    }
}