using System;
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
        public void CreateRecord_ReadMode()
        {
            _memoryStream.Dispose();
            _memoryStream.Open(Access.Read);
            try
            {
                _storage.CreateRecord("xxx");
                Assert.Fail();
            }
            catch (Exception exp)
            {
                Assert.AreEqual("Storage cannot be modified.", exp.Message);
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

        [Test]
        public void RenameRecord_ReadMode()
        {
            _storage.CreateRecord("record");
            _memoryStream.Dispose();
            _memoryStream.Open(Access.Read);
            try
            {
                _storage.RenameRecord("record", "new record");
                Assert.Fail();
            }
            catch (Exception exp)
            {
                Assert.AreEqual("Storage cannot be modified.", exp.Message);
            }
        }

        [Test]
        public void DeleteRecord_NotExists()
        {
            try
            {
                _storage.DeleteRecord("record");
                Assert.Fail();
            }
            catch (IOException exp)
            {
                Assert.AreEqual("Record 'record' does not exist.", exp.Message);
            }
        }

        [Test]
        public void DeleteRecord()
        {
            _storage.CreateRecord("record");
            _storage.DeleteRecord("record");
            var exist = _storage.IsRecordExist("record");
            Assert.IsFalse(exist);
        }

        [Test]
        public void DeleteRecord_CreateWithSameName()
        {
            _storage.CreateRecord("record");
            _storage.DeleteRecord("record");
            _storage.CreateRecord("record");
            var exist = _storage.IsRecordExist("record");
            Assert.IsTrue(exist);
        }

        [Test]
        public void DeleteRecord_ReadMode()
        {
            _storage.CreateRecord("record");
            _memoryStream.Dispose();
            _memoryStream.Open(Access.Read);
            try
            {
                _storage.DeleteRecord("record");
                Assert.Fail();
            }
            catch (Exception exp)
            {
                Assert.AreEqual("Storage cannot be modified.", exp.Message);
            }
        }

        [Test]
        public void GetAllRecordNames_Empty()
        {
            var result = _storage.GetAllRecordNames();
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetAllRecordNames_1()
        {
            _storage.CreateRecord("record 1");
            _storage.CreateRecord("record 2");
            var result = _storage.GetAllRecordNames();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("record 1", result[0]);
            Assert.AreEqual("record 2", result[1]);
        }

        [Test]
        public void GetAllRecordNames_2()
        {
            _storage.CreateRecord("record 1");
            _storage.CreateRecord("record 2");
            _storage.DeleteRecord("record 2");
            _storage.CreateRecord("record 3");
            var result = _storage.GetAllRecordNames();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("record 1", result[0]);
            Assert.AreEqual("record 3", result[1]);
        }

        [Test]
        public void GetAllRecordNames_NameMaxSize()
        {
            var maxNameSize = new string('a', SizeConstants.RecordName);
            _storage.CreateRecord(maxNameSize);
            var result = _storage.GetAllRecordNames();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(maxNameSize, result[0]);
        }

        [Test]
        public void WriteToFirstFreeSegment()
        {
            var record1Content = GetRandomByteArray(SizeConstants.SegmentData);
            var record2Content = GetRandomByteArray(SizeConstants.SegmentData);
            CreateRecordWithContent("record 1", record1Content);
            var record2 = CreateRecordWithContent("record 2", record2Content);
            _storage.DeleteRecord("record 1");
            record2Content = GetRandomByteArray(3 * SizeConstants.SegmentData);
            record2.Seek(0, SeekOrigin.Begin);
            record2.Write(record2Content, 0, record2Content.Length);
            var record2Segments = GetAllRecordSegments("record 2");
            Assert.AreEqual(3, record2Segments.Count);
            Assert.AreEqual(1, record2Segments[0].Index);
            Assert.AreEqual(0, record2Segments[1].Index);
            Assert.AreEqual(3, record2Segments[2].Index);
        }
    }
}
