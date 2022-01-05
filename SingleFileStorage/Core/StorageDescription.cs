using System.IO;
using SingleFileStorage.Infrastructure;

namespace SingleFileStorage.Core
{
    static class StorageDescription
    {
        public static Memory GetStorageDescription(StorageFileStream fileStream)
        {
            fileStream.Seek(0, SeekOrigin.Begin);
            var storageDescription = new byte[SizeConstants.StorageDescription];
            fileStream.ReadByteArray(storageDescription, 0, storageDescription.Length);
            var memory = new Memory(storageDescription);

            return memory;
        }
    }
}
