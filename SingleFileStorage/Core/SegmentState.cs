using System;
using System.Collections.Generic;
using System.Text;
using SingleFileStorage.Utils;

namespace SingleFileStorage.Core
{
    internal static class SegmentState
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

        public static bool IsLast(byte state)
        {
            return BitMask.GetValue(state, 1) == 0;
        }

        public static void SetLast(ref byte state)
        {
            BitMask.SetValue(ref state, 1, 0);
        }

        public static void SetChained(ref byte state)
        {
            BitMask.SetValue(ref state, 1, 1);
        }

        public static byte UsedAndLast
        {
            get
            {
                byte state = 0;
                SetUsed(ref state);
                SetLast(ref state);

                return state;
            }
        }

        public static byte UsedAndChained
        {
            get
            {
                byte state = 0;
                SetUsed(ref state);
                SetChained(ref state);

                return state;
            }
        }
    }
}
