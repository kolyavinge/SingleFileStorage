using System.IO;
using SingleFileStorage.Core;

namespace SingleFileStorage.Infrastructure
{
    internal class StorageFileStream : IStorageFileStream
    {
        private readonly string _fullPath;
        private FileStream _fileStream;
        private BinaryReader _reader;
        private BinaryWriter _writer;

        public long Position => _fileStream.Position;

        public long Length => _fileStream.Length;

        public StorageFileStream(string fullPath)
        {
            _fullPath = fullPath;
        }

        public void BeginRead()
        {
            _fileStream = File.Open(_fullPath, FileMode.Open, FileAccess.Read);
            _reader = new BinaryReader(_fileStream);
        }

        public void BeginReadWrite()
        {
            _fileStream = File.Open(_fullPath, FileMode.Open, FileAccess.ReadWrite);
            _reader = new BinaryReader(_fileStream);
            _writer = new BinaryWriter(_fileStream);
        }

        public void EndReadWrite()
        {
            _fileStream.Close();
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
            return _fileStream.Read(buffer, offset, count);
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
            return _fileStream.Seek(offset, origin);
        }

        public void Flush()
        {
            _fileStream.Flush();
        }
    }
}
