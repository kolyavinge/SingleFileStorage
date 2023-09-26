using System.IO;

namespace SingleFileStorage.Infrastructure;

internal class MemoryStorageFileStream : StorageFileStream
{
    private byte[] _memoryBuffer = new byte[0];
    private MemoryStream? _memoryStream;

    protected override Stream OpenStream(Access access)
    {
        if (access == Access.Read)
        {
            _memoryStream = new MemoryStream(_memoryBuffer, false);
        }
        else
        {
            _memoryStream = new MemoryStream();
            if (_memoryBuffer.Length > 0)
            {
                _memoryStream.Write(_memoryBuffer, 0, _memoryBuffer.Length);
                _memoryStream.Seek(0, SeekOrigin.Begin);
            }
        }

        return _memoryStream;
    }

    public override void Dispose()
    {
        if (_memoryStream is not null)
        {
            _memoryBuffer = _memoryStream.ToArray();
        }
        _memoryStream = null;
        base.Dispose();
    }
}
