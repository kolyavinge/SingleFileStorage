using System.IO;

namespace SingleFileStorage.Infrastructure
{
    internal class Memory : IReadableStream, IWriteableStream
    {
        private readonly MemoryStream _stream;
        private readonly BinaryReader _reader;
        private readonly BinaryWriter _writer;

        public Memory(byte[] array)
        {
            _stream = new MemoryStream(array);
            _stream.Seek(0, SeekOrigin.Begin);
            _reader = new BinaryReader(_stream);
            _writer = new BinaryWriter(_stream);
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
    }
}
