using System;
using System.Collections.Generic;
using System.Text;
using SingleFileStorage.Utils;

namespace SingleFileStorage.Core
{
    internal static class RecordState
    {
        public static bool IsFree(byte state)
        {
            return BitMask.GetValue(state, 0) == 0;
        }

        public static void SetFree(ref byte state)
        {
            BitMask.SetValue(ref state, 0, 0);
        }

        public static void SetUsed(ref byte state)
        {
            BitMask.SetValue(ref state, 0, 1);
        }

        public static byte Free
        {
            get
            {
                byte state = 0;
                SetFree(ref state);

                return state;
            }
        }
    }
}
