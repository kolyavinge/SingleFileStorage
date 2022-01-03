using System;
using System.Linq;
using System.Text;
using SingleFileStorage.Utils;

namespace SingleFileStorage.Core
{
    internal static class RecordName
    {
        private static readonly string _validNameSymbols = "qwertyuiopasdfghjklzxcvbnm1234567890QWERTYUIOPASDFGHJKLZXCVBNM1234567890_. ";

        public static readonly int MaxLength = 256;

        public static void ThrowErrorIfInvalid(string name)
        {
            if (name.Any(s => !_validNameSymbols.Contains(s)))
            {
                throw new ApplicationException("Record name is invalid");
            }

            if (name.Length > MaxLength)
            {
                throw new ApplicationException("Record name is too long");
            }
        }

        public static byte[] GetBytes(string name)
        {
            var nameBytes = new byte[SizeConstants.RecordName];
            Encoding.UTF8.GetBytes(name, 0, name.Length, nameBytes, 0);

            return nameBytes;
        }

        public static string GetString(byte[] nameBytes)
        {
            var zeroIndex = Array.IndexOf<byte>(nameBytes, 0);
            if (zeroIndex == -1) zeroIndex = SizeConstants.RecordName;
            var name = Encoding.UTF8.GetString(nameBytes, 0, zeroIndex);

            return name;
        }

        public static bool IsEqual(byte[] recordNameBytes1, byte[] recordNameBytes2)
        {
            for (int i = 0; i < recordNameBytes1.Length; i++)
            {
                if (recordNameBytes1[i] == 0 && recordNameBytes2[i] == 0)
                {
                    return true;
                }
                else if (recordNameBytes1[i] != recordNameBytes2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
