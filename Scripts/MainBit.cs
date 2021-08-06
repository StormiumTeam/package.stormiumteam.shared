using System;

namespace package.stormiumteam.shared
{
    public static class Bits
    {
        // ------------
        // SET GET
        // ------------

        //
        // BYTE
        //
        public static void SetAt(ref byte curr, int pos, bool val)
        {
            SetAt(ref curr, pos, (byte) (val ? 1 : 0));
        }

        public static void SetAt(ref byte curr, int pos, byte val)
        {
            var cd = (curr & (1 << pos)) == 0;
            if (val != 0)
            {
                // Set
                if (cd)
                    curr = (byte) (curr | (1 << pos));
            }

            else
            {
                if (!cd)
                    curr = (byte) (curr & ~(1 << pos));
            }
        }

        public static bool ToBoolean(byte curr, int pos)
        {
            return ((curr & (1 << pos))) != 0;
        }

        //
        // INT
        //
        public static void SetAt(ref int curr, int pos, bool val)
        {
            SetAt(ref curr, pos, (byte) (val ? 1 : 0));
        }

        public static void SetAt(ref int curr, int pos, byte val)
        {
            var cd = (curr & (1 << pos)) == 0;
            if (val != 0)
            {
                // Set
                if (cd)
                    curr = curr | (1 << pos);
            }

            else
            {
                if (!cd)
                    curr = curr & ~(1 << pos);
            }
        }

        public static bool ToBoolean(int curr, int pos)
        {
            return ((curr & (1 << pos)) != 0);
        }

        //
        // UNSIGNED INT
        //
        public static void SetAt(ref uint curr, int pos, bool val)
        {
            SetAt(ref curr, pos, (byte) (val ? 1 : 0));
        }

        public static void SetAt(ref uint curr, int pos, byte val)
        {
            var cd = (curr & (1 << pos)) == 0;
            if (val != 0)
            {
                // Set
                if (cd)
                    curr = (uint) (curr | (1 << pos));
            }

            else
            {
                if (!cd)
                    curr = (uint) (curr & ~(1 << pos));
            }
        }

        public static bool ToBoolean(uint curr, int pos)
        {
            return (curr & (1 << pos)) != 0;
        }

        public static byte GetBitAt(byte curr, int pos) => (byte) (ToBoolean(curr, pos) ? 1 : 0);
        public static byte GetBitAt(int  curr, int pos) => (byte) (ToBoolean(curr, pos) ? 1 : 0);
        public static byte GetBitAt(uint curr, int pos) => (byte) (ToBoolean(curr, pos) ? 1 : 0);

        // -------------
        // RANGE
        // -------------

        //
        // BYTE
        //
        public static void SetRange(ref byte curr, byte pos, byte val, byte size)
        {
            if (size != 2)
                throw new NotImplementedException("Only support size of 2");

            SetAt(ref curr, pos, ToBoolean(val, 0));
            SetAt(ref curr, (byte) (pos + 1), ToBoolean(val, 1));
        }

        public static byte GetRange(byte curr, byte pos, byte size)
        {
            if (size != 2)
                throw new NotImplementedException("Only support size of 2");

            byte newByte = default;

            newByte |= (byte) (GetBitAt(curr, pos) << pos);
            newByte |= (byte) (GetBitAt(curr, (byte) (pos + 1)) << (pos + 1));

            return (byte) (newByte >> pos);
        }
    }
}