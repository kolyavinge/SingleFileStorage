using System.IO;

namespace SingleFileStorage.Infrastructure;

internal interface IWriteableStream
{
    void WriteByte(byte value);

    void WriteUInt32(uint value);

    void WriteByteArray(byte[] buffer, int offset, int count);

    long Seek(long offset, SeekOrigin origin);
}
