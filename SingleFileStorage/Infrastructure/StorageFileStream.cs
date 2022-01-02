using System;
using System.IO;
using SingleFileStorage.Core;

namespace SingleFileStorage.Infrastructure
{
    internal abstract class StorageFileStream : IStorageFileStream
    {
        private Stream _stream;
        private BinaryReader _reader;
        private BinaryWriter _writer;

        public Access? AccessMode { get; private set; }

        public bool CanWrite => _stream.CanWrite;

        public long Position => _stream.Position;

        public long Length => _stream.Length;

        public void Open(Access access)
        {
            AccessMode = access;
            _stream = OpenStream(access);
            _reader = new BinaryReader(_stream);
            if (access == Access.Modify) _writer = new BinaryWriter(_stream);
            else _writer = null;
        }

        protected abstract Stream OpenStream(Access access);

        public virtual void Dispose()
        {
            _stream.Close();
        }

        public byte ReadByte()
        {
            return _reader.ReadByte();
        }

        public uint ReadUInt32()
        {
            return _reader.ReadUInt32();
        }

        public int ReadByteArray(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public void WriteByte(byte value)
        {
            _writer.Write(value);
        }

        public void WriteUInt32(uint value)
        {
            _writer.Write(value);
        }

        public void WriteByteArray(byte[] buffer, int offset, int count)
        {
            _writer.Write(buffer, offset, count);
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public void Flush()
        {
            _stream.Flush();
        }
    }
}
