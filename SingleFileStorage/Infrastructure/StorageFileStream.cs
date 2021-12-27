using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SingleFileStorage.Core;

namespace SingleFileStorage.Infrastructure
{
    internal class StorageFileStream : IStorageFileStream
    {
        public long Position { get; }

        public long Length { get; }

        public StorageFileStream(string fullPath)
        {

        }

        public void BeginRead()
        {
        }

        public void BeginReadWrite()
        {
        }

        public void EndReadWrite()
        {
        }

        public byte ReadByte()
        {
            throw new NotImplementedException();
        }

        public uint ReadUInt32()
        {
            throw new NotImplementedException();
        }

        public int ReadByteArray(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public void WriteByte(byte value)
        {
        }

        public void WriteUInt32(uint value)
        {
        }

        public void WriteByteArray(byte[] buffer, int offset, int count)
        {
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public void Flush()
        {
        }
    }
}
