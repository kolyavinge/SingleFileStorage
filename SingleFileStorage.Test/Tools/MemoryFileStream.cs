using System;
using System.IO;
using SingleFileStorage.Core;

namespace SingleFileStorage.Test.Tools
{
    public class MemoryFileStream : IStorageFileStream
    {
        private readonly MemoryStream _memoryStream;
        private readonly BinaryReader _reader;
        private readonly BinaryWriter _writer;
        private bool _beginRead;
        private bool _beginReadWrite;

        public long Position { get { return _memoryStream.Position; } }

        public long Length { get { return _memoryStream.Length; } }

        public MemoryFileStream()
        {
            _memoryStream = new MemoryStream();
            _reader = new BinaryReader(_memoryStream);
            _writer = new BinaryWriter(_memoryStream);
        }

        public void BeginRead()
        {
            _beginRead = true;
            _beginReadWrite = false;
        }

        public void BeginReadWrite()
        {
            _beginRead = true;
            _beginReadWrite = true;
        }

        public void EndReadWrite()
        {
            _beginRead = false;
            _beginReadWrite = true;
        }

        public byte ReadByte()
        {
            CheckBeginRead();
            return _reader.ReadByte();
        }

        public uint ReadUInt32()
        {
            CheckBeginRead();
            return _reader.ReadUInt32();
        }

        public int ReadByteArray(byte[] buffer, int offset, int count)
        {
            CheckBeginRead();
            return _memoryStream.Read(buffer, offset, count);
        }

        public void WriteByte(byte value)
        {
            CheckBeginReadWrite();
            _writer.Write(value);
        }

        public void WriteUInt32(uint value)
        {
            CheckBeginReadWrite();
            _writer.Write(value);
        }

        public void WriteByteArray(byte[] buffer, int offset, int count)
        {
            CheckBeginReadWrite();
            _memoryStream.Write(buffer, offset, count);
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            CheckBeginRead();
            return _memoryStream.Seek(offset, origin);
        }

        public void Flush()
        {
        }

        private void CheckBeginRead()
        {
            if (_beginRead == false) throw new Exception();
        }

        private void CheckBeginReadWrite()
        {
            if (_beginReadWrite == false) throw new Exception();
        }
    }
}
