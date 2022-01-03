using System;
using System.IO;
using SingleFileStorage.Core;

namespace SingleFileStorage.Infrastructure
{
    internal class DiskStorageFileStream : StorageFileStream
    {
        private readonly string _fullPath;

        public DiskStorageFileStream(string fullPath)
        {
            _fullPath = fullPath;
        }

        protected override Stream OpenStream(Access access)
        {
            if (access == Access.Read) return File.Open(_fullPath, FileMode.Open, FileAccess.Read);
            else if (access == Access.Modify) return File.Open(_fullPath, FileMode.Open, FileAccess.ReadWrite);
            else throw new ArgumentException(nameof(access));
        }
    }
}
