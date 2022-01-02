using System.IO;
using NUnit.Framework;
using SingleFileStorage.Core;
using SingleFileStorage.Test.Tools;
using SingleFileStorage.Test.Utils;

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
            _storage.CreateRecord("record");
            try
            {
                _storage.CreateRecord("record");
                Assert.Fail();
            }
            catch (IOException exp)
            {
                Assert.AreEqual("Record 'record' already exists.", exp.Message);
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
                _storage.OpenRecord("record");
                Assert.Fail();
            }
            catch (IOException exp)
            {
                Assert.AreEqual("Record 'record' does not exist.", exp.Message);
            }
        }

        [Test]
        public void IsRecordExist_No()
        {
            var result = _storage.IsRecordExist("record");
            Assert.IsFalse(result);
        }

        [Test]
        public void IsRecordExist_Yes()
        {
            _storage.CreateRecord("record");
            var result = _storage.IsRecordExist("record");
            Assert.IsTrue(result);
        }

        [Test]
        public void RenameRecord_NotExists()
        {
            try
            {
                _storage.RenameRecord("record", "new record");
                Assert.Fail();
            }
            catch (IOException exp)
            {
                Assert.AreEqual("Record 'record' does not exist.", exp.Message);
            }
        }

        [Test]
        public void RenameRecord_NewAlreadyExists()
        {
            _storage.CreateRecord("record");
            _storage.CreateRecord("new record");
            try
            {
                _storage.RenameRecord("record", "new record");
                Assert.Fail();
            }
            catch (IOException exp)
            {
                Assert.AreEqual("Record 'new record' already exists.", exp.Message);
            }
        }

        [Test]
        public void RenameRecord()
        {
            var recordContent = GetRandomByteArray(SizeConstants.SegmentData);
            CreateRecordWithContent("record", recordContent);
            _storage.RenameRecord("record", "new record");
            var record = _storage.OpenRecord("new record");
            var recordContentReadResult = new byte[recordContent.Length];
            record.Read(recordContentReadResult, 0, recordContentReadResult.Length);
            Assert.IsTrue(ByteArray.IsEqual(recordContent, recordContentReadResult));
        }
    }
}
