using System;
using System.Collections.Generic;
using System.Text;

namespace SingleFileStorage.Utils
{
    internal static class BitMask
    {
        public static byte GetValue(byte flags, byte bitNumber)
        {
            ThrowErrorIfInvalid(bitNumber);
            return (flags & GetPowerOf2(bitNumber)) == 0 ? (byte)0 : (byte)1;
        }

        public static void SetValue(ref byte flags, byte bitNumber, byte bitValue)
        {
            ThrowErrorIfInvalid(bitNumber);
            if (bitValue == 0)
            {
                flags &= (byte)(255 - GetPowerOf2(bitNumber));
            }
            else
            {
                flags |= GetPowerOf2(bitNumber);
            }
        }

        private static byte GetPowerOf2(byte bitNumber)
        {
            if (bitNumber == 0) return 1;
            if (bitNumber == 1) return 2;
            if (bitNumber == 2) return 4;
            if (bitNumber == 3) return 8;
            if (bitNumber == 4) return 16;
            if (bitNumber == 5) return 32;
            if (bitNumber == 6) return 64;
            if (bitNumber == 7) return 128;
            throw new ArgumentException(nameof(bitNumber));
        }

        private static void ThrowErrorIfInvalid(byte bitNumber)
        {
            if (bitNumber > 7) throw new ArgumentException(nameof(bitNumber));
        }
    }
}
