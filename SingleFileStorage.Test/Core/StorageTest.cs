using System.IO;
using NUnit.Framework;
using SingleFileStorage.Core;
using SingleFileStorage.Test.Tools;

namespace SingleFileStorage.Test.Core
{
    class StorageTest : BaseTest
    {
        [SetUp]
        public void Setup()
        {
            InitStorage();
            OpenStorage();
        }

        [TearDown]
        public void TearDown()
        {
            DisposeStorage();
        }

        [Test]
        public void CreateRecord_AlreadyExists()
        {
            _storage.CreateRecord("xxx");
            try
            {
                _storage.CreateRecord("xxx");
                Assert.Fail();
            }
            catch (IOException exp)
            {
                Assert.AreEqual("Record 'xxx' already exists.", exp.Message);
            }
        }

        [Test]
        public void CreateRecord_Full()
        {
            for (int i = 0; i < SizeConstants.MaxRecordsCount; i++)
            {
                _storage.CreateRecord(i.ToString());
            }
            try
            {
                _storage.CreateRecord("xxx");
                Assert.Fail();
            }
            catch (IOException exp)
            {
                Assert.AreEqual("Cannot find any free record description.", exp.Message);
            }
        }

        [Test]
        public void OpenRecord_NotExists()
        {
            try
            {
                _storage.OpenRecord("xxx");
                Assert.Fail();
            }
            catch (IOException exp)
            {
                Assert.AreEqual("Record 'xxx' does not exist.", exp.Message);
            }
        }
    }
}
