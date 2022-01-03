using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SingleFileStorage.Core;
using SingleFileStorage.Infrastructure;

namespace SingleFileStorage
{
    public class StorageFile
    {
        public static void Create(string fullPath)
        {
            File.Create(fullPath).Close();
            using (var diskStorageFileStream = new DiskStorageFileStream(fullPath))
            {
                diskStorageFileStream.Open(Access.Modify);
                Storage.InitDescription(diskStorageFileStream);
            }
        }

        public static IStorage Open(string fullPath, Access access)
        {
            var diskStorageFileStream = new DiskStorageFileStream(fullPath);
            diskStorageFileStream.Open(access);
            var storage = new Storage(diskStorageFileStream);

            return storage;
        }
    }
}
