using System;
using System.IO;

namespace SingleFileStorage.Infrastructure
{
    internal abstract class StorageFileStream : IReadableStream, IWriteableStream, IDisposable
    {
        private Stream _stream;
        private BinaryReader _reader;
        private BinaryWriter _writer;

        public Access AccessMode;
        public long Position; // сделано полем для оптимизации 
        public long Length => _stream.Length;

        public void Open(Access access)
        {
            AccessMode = access;
            _stream = OpenStream(access);
            Position = _stream.Position;
            _reader = new BinaryReader(_stream);
            if (access == Access.Modify) _writer = new BinaryWriter(_stream);
            else _writer = null;
        }

        protected abstract Stream OpenStream(Access access);

        public virtual void Dispose()
        {
            _stream.Dispose();
            _reader.Dispose();
            if (_writer != null) _writer.Dispose();
        }

        public byte ReadByte()
        {
            Position++;
            return _reader.ReadByte();
        }

        public uint ReadUInt32()
        {
            Position += sizeof(uint);
            return _reader.ReadUInt32();
        }

        public int ReadByteArray(byte[] buffer, int offset, int count)
        {
            var result = _stream.Read(buffer, offset, count);
            Position += result;

            return result;
        }

        public void WriteByte(byte value)
        {
            Position++;
            _writer.Write(value);
        }

        public void WriteUInt32(uint value)
        {
            Position += sizeof(uint);
            _writer.Write(value);
        }

        public void WriteByteArray(byte[] buffer, int offset, int count)
        {
            Position += count;
            _writer.Write(buffer, offset, count);
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin) Position = offset;
            else if (origin == SeekOrigin.Current) Position += offset;
            else Position = Length + offset;

            return _stream.Seek(offset, origin);
        }

        public void Flush()
        {
            _stream.Flush();
        }
    }
}
