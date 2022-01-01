using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SingleFileStorage.Core;
using SingleFileStorage.Infrastructure;

namespace SingleFileStorage
{
    public class StorageBuilder
    {
        public static IStorage Create(string fullPath)
        {
            using (var fs = File.Create(fullPath)) { }
            var storage = new Storage(new StorageFileStream(fullPath));
            storage.InitDescription();

            return storage;
        }

        public static IStorage Open(string fullPath)
        {
            var storage = new Storage(new StorageFileStream(fullPath));

            return storage;
        }
    }
}
