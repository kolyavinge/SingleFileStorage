using System;
using System.IO;
using NUnit.Framework;
using SingleFileStorage.Core;
using SingleFileStorage.Maintenance;
using SingleFileStorage.Test.Tools;

namespace SingleFileStorage.Test.Maintenance
{
    class DefragmentatorTest
    {
        private TestFileSystem _fileSystem;
        private Defragmentator _defragmentator;

        [SetUp]
        public void Setup()
        {
            _fileSystem = new TestFileSystem();
            _defragmentator = new Defragmentator(_fileSystem);
        }

        [Test]
        public void OneRecord_NoFragmentation()
        {
            _fileSystem.CurrentStorageFileStream.Open(Access.Modify);
            _fileSystem.CurrentStorage.CreateRecord("record");
            _fileSystem.CurrentStorageFileStream.Dispose();
            _defragmentator.Defragment("current");

            Assert.AreEqual(1, _fileSystem.DefragmentStorage.GetAllRecordNames().Count);
        }

        [Test]
        public void OneRecord_Fragmentation()
        {
            _fileSystem.CurrentStorageFileStream.Open(Access.Modify);
            _fileSystem.CurrentStorage.CreateRecord("record");
            _fileSystem.CurrentStorageFileStream.Dispose();
            _fileSystem.CurrentStorageFileStream.Open(Access.Modify);
            _fileSystem.CurrentStorage.DeleteRecord("record");
            _fileSystem.CurrentStorageFileStream.Dispose();
            _defragmentator.Defragment("current");

            Assert.AreEqual(0, _fileSystem.DefragmentStorage.GetAllRecordNames().Count);
        }

        [Test]
        public void OneRecord_FragmentationContent()
        {
            _fileSystem.CurrentStorageFileStream.Open(Access.Modify);
            _fileSystem.CurrentStorage.CreateRecord("record");
            using (var record = _fileSystem.CurrentStorage.OpenRecord("record"))
            {
                var buffer = new byte[2 * SizeConstants.Segment];
                record.Write(buffer, 0, buffer.Length);
            }
            _fileSystem.CurrentStorageFileStream.Dispose();
            _fileSystem.CurrentStorageFileStream.Open(Access.Modify);
            _fileSystem.CurrentStorage.DeleteRecord("record");
            _fileSystem.CurrentStorageFileStream.Dispose();
            _defragmentator.Defragment("current");

            Assert.AreEqual(0, _fileSystem.DefragmentStorage.GetAllRecordNames().Count);
        }
    }

    class TestFileSystem : IFileSystem
    {
        public MemoryStorageFileStream CurrentStorageFileStream;
        public MemoryStorageFileStream DefragmentStorageFileStream;
        public Storage CurrentStorage;
        public Storage DefragmentStorage;

        public TestFileSystem()
        {
            CurrentStorageFileStream = new MemoryStorageFileStream();
            CurrentStorageFileStream.Open(Access.Modify);
            CurrentStorage = new Storage(CurrentStorageFileStream);
            Storage.InitDescription(CurrentStorageFileStream);
            CurrentStorageFileStream.Dispose();
        }

        public void CreateStorageFile(string fullPath)
        {
            if (fullPath == "current.defrag")
            {
                DefragmentStorageFileStream = new MemoryStorageFileStream();
                DefragmentStorageFileStream.Open(Access.Modify);
                DefragmentStorage = new Storage(DefragmentStorageFileStream);
                Storage.InitDescription(DefragmentStorageFileStream);
                DefragmentStorageFileStream.Dispose();
            }
            else throw new ArgumentException();
        }

        public IStorage OpenStorageFile(string fullPath, Access access)
        {
            if (fullPath == "current")
            {
                CurrentStorageFileStream.Open(access);
                CurrentStorageFileStream.Seek(0, SeekOrigin.Begin);
                return CurrentStorage;
            }

            if (fullPath == "current.defrag")
            {
                DefragmentStorageFileStream.Open(access);
                DefragmentStorageFileStream.Seek(0, SeekOrigin.Begin);
                return DefragmentStorage;
            }

            throw new ArgumentException();
        }

        public void RenameFile(string fullPath, string renamedFilePath)
        {
        }

        public void DeleteFile(string fullPath)
        {
        }
    }
}
