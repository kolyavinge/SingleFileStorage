using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SingleFileStorage.Core
{
    internal interface IStorageFileStream
    {
        long Position { get; }

        long Length { get; }

        void BeginRead();

        void BeginReadWrite();

        void EndReadWrite();

        byte ReadByte();

        uint ReadUInt32();

        int ReadByteArray(byte[] buffer, int offset, int count);

        void WriteByte(byte value);

        void WriteUInt32(uint value);

        void WriteByteArray(byte[] buffer, int offset, int count);

        long Seek(long offset, SeekOrigin origin);

        void Flush();
    }
}
