using System.IO;

namespace SingleFileStorage.Infrastructure;

internal interface IReadableStream
{
    byte ReadByte();

    uint ReadUInt32();

    int ReadByteArray(byte[] buffer, int offset, int count);

    long Seek(long offset, SeekOrigin origin);
}
