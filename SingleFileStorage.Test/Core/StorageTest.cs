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
            _memoryStream = new MemoryFileStream();
            _storage = new Storage(_memoryStream);
            _memoryStream.BeginReadWrite();
            _storage.InitDescription();
            _memoryStream.EndReadWrite();
        }

        [Test]
        public void Record_Position_Read()
        {
            _storage.CreateRecord("record");
            var record = _storage.OpenRecord("record", RecordAccess.ReadWrite);
            var recordContent = GetRandomByteArray(2 * SizeConstants.SegmentData + SizeConstants.SegmentData / 2);
            record.Write(recordContent, 0, recordContent.Length);
            record.Seek(0, SeekOrigin.Begin);

            Assert.AreEqual(0, record.Position);

            record.Read(recordContent, 0, 100);
            Assert.AreEqual(100, record.Position);

            record.Read(recordContent, 100, SizeConstants.SegmentData - 100);
            Assert.AreEqual(SizeConstants.SegmentData, record.Position);

            record.Read(recordContent, SizeConstants.SegmentData, SizeConstants.SegmentData);
            Assert.AreEqual(2 * SizeConstants.SegmentData, record.Position);

            record.Read(recordContent, 2 * SizeConstants.SegmentData, SizeConstants.SegmentData / 2);
            Assert.AreEqual(2 * SizeConstants.SegmentData + SizeConstants.SegmentData / 2, record.Position);
        }

        [Test]
        public void Record_Position_Write()
        {
            _storage.CreateRecord("record");
            var record = _storage.OpenRecord("record", RecordAccess.ReadWrite);
            Assert.AreEqual(0, record.Position);

            var recordContent = GetRandomByteArray(2 * SizeConstants.SegmentData + SizeConstants.SegmentData / 2);
            record.Write(recordContent, 0, 100);
            Assert.AreEqual(100, record.Position);

            record.Write(recordContent, 100, SizeConstants.SegmentData - 100);
            Assert.AreEqual(SizeConstants.SegmentData, record.Position);

            record.Write(recordContent, SizeConstants.SegmentData, SizeConstants.SegmentData);
            Assert.AreEqual(2 * SizeConstants.SegmentData, record.Position);

            record.Write(recordContent, 2 * SizeConstants.SegmentData, SizeConstants.SegmentData / 2);
            Assert.AreEqual(2 * SizeConstants.SegmentData + SizeConstants.SegmentData / 2, record.Position);
        }

        [Test]
        public void Record_ManyRead()
        {
            _storage.CreateRecord("record");
            var record = _storage.OpenRecord("record", RecordAccess.ReadWrite);
            var recordContent = GetRandomByteArray(2 * SizeConstants.SegmentData + SizeConstants.SegmentData / 2);
            record.Write(recordContent, 0, recordContent.Length);
            record.Seek(0, SeekOrigin.Begin);
            var recordContentReadResult = new byte[recordContent.Length];
            record.Read(recordContentReadResult, 0, 100);
            record.Read(recordContentReadResult, 100, SizeConstants.SegmentData - 100);
            record.Read(recordContentReadResult, SizeConstants.SegmentData, SizeConstants.SegmentData);
            record.Read(recordContentReadResult, 2 * SizeConstants.SegmentData, SizeConstants.SegmentData / 2);
            Assert.IsTrue(ByteArray.IsEqual(recordContent, recordContentReadResult));
        }

        [Test]
        public void Record_ManyWrite()
        {
            _storage.CreateRecord("record");
            var record = _storage.OpenRecord("record", RecordAccess.ReadWrite);
            var recordContent = GetRandomByteArray(2 * SizeConstants.SegmentData + SizeConstants.SegmentData / 2);
            record.Write(recordContent, 0, 100);
            record.Write(recordContent, 100, SizeConstants.SegmentData - 100);
            record.Write(recordContent, SizeConstants.SegmentData, SizeConstants.SegmentData);
            record.Write(recordContent, 2 * SizeConstants.SegmentData, SizeConstants.SegmentData / 2);
            var recordContentReadResult = new byte[recordContent.Length];
            record.Seek(0, SeekOrigin.Begin);
            record.Read(recordContentReadResult, 0, recordContentReadResult.Length);
            Assert.IsTrue(ByteArray.IsEqual(recordContent, recordContentReadResult));
        }

        [Test]
        public void Record_RecordDescription()
        {
            _storage.CreateRecord("record");
            var record = _storage.OpenRecord("record", RecordAccess.ReadWrite);
            var recordContent = GetRandomByteArray(2 * SizeConstants.SegmentData + SizeConstants.SegmentData / 2);

            var recordDescription = GetRecordDescription("record");
            Assert.AreEqual(0, recordDescription.FirstSegmentIndex);
            Assert.AreEqual(0, recordDescription.LastSegmentIndex);
            Assert.AreEqual(0, recordDescription.RecordLength);

            record.Write(recordContent, 0, 100);
            recordDescription = GetRecordDescription("record");
            Assert.AreEqual(0, recordDescription.FirstSegmentIndex);
            Assert.AreEqual(0, recordDescription.LastSegmentIndex);
            Assert.AreEqual(100, recordDescription.RecordLength);

            record.Write(recordContent, 100, SizeConstants.SegmentData - 100);
            recordDescription = GetRecordDescription("record");
            Assert.AreEqual(0, recordDescription.FirstSegmentIndex);
            Assert.AreEqual(0, recordDescription.LastSegmentIndex);
            Assert.AreEqual(SizeConstants.SegmentData, recordDescription.RecordLength);

            record.Write(recordContent, SizeConstants.SegmentData, SizeConstants.SegmentData);
            recordDescription = GetRecordDescription("record");
            Assert.AreEqual(0, recordDescription.FirstSegmentIndex);
            Assert.AreEqual(1, recordDescription.LastSegmentIndex);
            Assert.AreEqual(2 * SizeConstants.SegmentData, recordDescription.RecordLength);

            record.Write(recordContent, 2 * SizeConstants.SegmentData, SizeConstants.SegmentData / 2);
            recordDescription = GetRecordDescription("record");
            Assert.AreEqual(0, recordDescription.FirstSegmentIndex);
            Assert.AreEqual(2, recordDescription.LastSegmentIndex);
            Assert.AreEqual(2 * SizeConstants.SegmentData + SizeConstants.SegmentData / 2, recordDescription.RecordLength);

            record.Write(recordContent, 0, 100);
            recordDescription = GetRecordDescription("record");
            Assert.AreEqual(0, recordDescription.FirstSegmentIndex);
            Assert.AreEqual(2, recordDescription.LastSegmentIndex);
            Assert.AreEqual(2 * SizeConstants.SegmentData + SizeConstants.SegmentData / 2 + 100, recordDescription.RecordLength);
        }

        [Test]
        public void Record_Length()
        {
            _storage.CreateRecord("record");
            var record = _storage.OpenRecord("record", RecordAccess.ReadWrite);
            var recordContent = GetRandomByteArray(2 * SizeConstants.SegmentData + SizeConstants.SegmentData / 2);

            Assert.AreEqual(0, record.Length);

            record.Write(recordContent, 0, 100);
            Assert.AreEqual(100, record.Length);

            record.Write(recordContent, 100, SizeConstants.SegmentData - 100);
            Assert.AreEqual(SizeConstants.SegmentData, record.Length);

            record.Write(recordContent, SizeConstants.SegmentData, SizeConstants.SegmentData);
            Assert.AreEqual(2 * SizeConstants.SegmentData, record.Length);

            record.Write(recordContent, 2 * SizeConstants.SegmentData, SizeConstants.SegmentData / 2);
            Assert.AreEqual(2 * SizeConstants.SegmentData + SizeConstants.SegmentData / 2, record.Length);
        }

        [Test]
        public void Record_WithinSegment()
        {
            var recordContent = GetRandomByteArray(100);
            var record = CreateRecordWithContent("record", recordContent);
            var recordContentReadResult = new byte[recordContent.Length];
            record.Seek(0, SeekOrigin.Begin);
            var readedBytes = record.Read(recordContentReadResult, 0, recordContentReadResult.Length);
            Assert.IsTrue(ByteArray.IsEqual(recordContent, recordContentReadResult));
            Assert.AreEqual(recordContent.Length, readedBytes);
        }

        [Test]
        public void Record_WithinSegment_TwoSizeReadArray()
        {
            var recordContent = GetRandomByteArray(100);
            var record = CreateRecordWithContent("record", recordContent);
            var recordContentReadResult = new byte[recordContent.Length];
            record.Seek(0, SeekOrigin.Begin);
            var readedBytes = record.Read(recordContentReadResult, 0, 2 * recordContent.Length);
            Assert.IsTrue(ByteArray.IsEqual(recordContent, recordContentReadResult));
            Assert.AreEqual(recordContent.Length, readedBytes);
        }

        [Test]
        public void Record_TwoSizeSegments()
        {
            var recordContent = GetRandomByteArray(2 * SizeConstants.SegmentData);
            var record = CreateRecordWithContent("record", recordContent);
            var recordContentReadResult = new byte[recordContent.Length];
            record.Seek(0, SeekOrigin.Begin);
            var readedBytes = record.Read(recordContentReadResult, 0, recordContentReadResult.Length);
            Assert.IsTrue(ByteArray.IsEqual(recordContent, recordContentReadResult));
            Assert.AreEqual(recordContent.Length, readedBytes);
        }

        [Test]
        public void Record_TwoSizeAndHalfSegments()
        {
            var recordContent = GetRandomByteArray(2 * SizeConstants.SegmentData + SizeConstants.SegmentData / 2);
            var record = CreateRecordWithContent("record", recordContent);
            var recordContentReadResult = new byte[recordContent.Length];
            record.Seek(0, SeekOrigin.Begin);
            var readedBytes = record.Read(recordContentReadResult, 0, recordContentReadResult.Length);
            Assert.IsTrue(ByteArray.IsEqual(recordContent, recordContentReadResult));
            Assert.AreEqual(recordContent.Length, readedBytes);
        }

        [Test]
        public void TwoRecord_WithinSegments()
        {
            var record1Content = GetRandomByteArray(100);
            var record2Content = GetRandomByteArray(100);
            var record1 = CreateRecordWithContent("record 1", record1Content);
            var record2 = CreateRecordWithContent("record 2", record2Content);
            var record1ContentReadResult = new byte[record1Content.Length];
            var record2ContentReadResult = new byte[record2Content.Length];
            record1.Seek(0, SeekOrigin.Begin);
            record1.Read(record1ContentReadResult, 0, record1ContentReadResult.Length);
            record2.Seek(0, SeekOrigin.Begin);
            record2.Read(record2ContentReadResult, 0, record2ContentReadResult.Length);
            Assert.IsTrue(ByteArray.IsEqual(record1Content, record1ContentReadResult));
            Assert.IsTrue(ByteArray.IsEqual(record2Content, record2ContentReadResult));
        }

        [Test]
        public void TwoRecord_OneSegments()
        {
            var record1Content = GetRandomByteArray(SizeConstants.SegmentData);
            var record2Content = GetRandomByteArray(SizeConstants.SegmentData);
            var record1 = CreateRecordWithContent("record 1", record1Content);
            var record2 = CreateRecordWithContent("record 2", record2Content);
            var record1ContentReadResult = new byte[record1Content.Length];
            var record2ContentReadResult = new byte[record2Content.Length];
            record1.Seek(0, SeekOrigin.Begin);
            record1.Read(record1ContentReadResult, 0, record1ContentReadResult.Length);
            record2.Seek(0, SeekOrigin.Begin);
            record2.Read(record2ContentReadResult, 0, record2ContentReadResult.Length);
            Assert.IsTrue(ByteArray.IsEqual(record1Content, record1ContentReadResult));
            Assert.IsTrue(ByteArray.IsEqual(record2Content, record2ContentReadResult));
        }

        [Test]
        public void TwoRecord_OneAndHalfSegments()
        {
            var record1Content = GetRandomByteArray(SizeConstants.SegmentData + SizeConstants.SegmentData / 2);
            var record2Content = GetRandomByteArray(SizeConstants.SegmentData + SizeConstants.SegmentData / 2);
            var record1 = CreateRecordWithContent("record 1", record1Content);
            var record2 = CreateRecordWithContent("record 2", record2Content);
            var record1ContentReadResult = new byte[record1Content.Length];
            var record2ContentReadResult = new byte[record2Content.Length];
            record1.Seek(0, SeekOrigin.Begin);
            record1.Read(record1ContentReadResult, 0, record1ContentReadResult.Length);
            record2.Seek(0, SeekOrigin.Begin);
            record2.Read(record2ContentReadResult, 0, record2ContentReadResult.Length);
            Assert.IsTrue(ByteArray.IsEqual(record1Content, record1ContentReadResult));
            Assert.IsTrue(ByteArray.IsEqual(record2Content, record2ContentReadResult));
        }

        [Test]
        public void TwoRecord_TwoWriteOperations()
        {
            var record1 = CreateEmptyRecord("record 1");
            var record2 = CreateEmptyRecord("record 2");
            var record1Content = GetRandomByteArray(2 * SizeConstants.SegmentData + SizeConstants.SegmentData / 2);
            var record2Content = GetRandomByteArray(2 * SizeConstants.SegmentData + SizeConstants.SegmentData / 2);
            record1.Write(record1Content, 0, SizeConstants.SegmentData);
            record2.Write(record2Content, 0, SizeConstants.SegmentData);
            record1.Write(record1Content, SizeConstants.SegmentData, SizeConstants.SegmentData);
            record2.Write(record2Content, SizeConstants.SegmentData, SizeConstants.SegmentData);
            record1.Write(record1Content, 2 * SizeConstants.SegmentData, SizeConstants.SegmentData / 2);
            record2.Write(record2Content, 2 * SizeConstants.SegmentData, SizeConstants.SegmentData / 2);
            var record1ContentReadResult = new byte[record1Content.Length];
            var record2ContentReadResult = new byte[record2Content.Length];
            record1.Seek(0, SeekOrigin.Begin);
            record1.Read(record1ContentReadResult, 0, record1ContentReadResult.Length);
            record2.Seek(0, SeekOrigin.Begin);
            record2.Read(record2ContentReadResult, 0, record2ContentReadResult.Length);
            Assert.IsTrue(ByteArray.IsEqual(record1Content, record1ContentReadResult));
            Assert.IsTrue(ByteArray.IsEqual(record2Content, record2ContentReadResult));
        }

        [Test]
        public void SeekPosition()
        {
            var empty = GetEmptyArray(2 * SizeConstants.SegmentData);
            var record = CreateRecordWithContent("record", empty);
            var recordContent = GetRandomByteArray(2 * SizeConstants.SegmentData);

            record.Position = SizeConstants.SegmentData / 2;
            record.Write(recordContent, SizeConstants.SegmentData / 2, SizeConstants.SegmentData / 2);

            record.Position = 0;
            record.Write(recordContent, 0, SizeConstants.SegmentData / 2);

            record.Position = SizeConstants.SegmentData + SizeConstants.SegmentData / 2;
            record.Write(recordContent, SizeConstants.SegmentData + SizeConstants.SegmentData / 2, SizeConstants.SegmentData / 2);

            record.Position = SizeConstants.SegmentData;
            record.Write(recordContent, SizeConstants.SegmentData, SizeConstants.SegmentData / 2);

            var recordContentReadResult = new byte[recordContent.Length];
            record.Seek(0, SeekOrigin.Begin);
            record.Read(recordContentReadResult, 0, recordContentReadResult.Length);
            Assert.IsTrue(ByteArray.IsEqual(recordContent, recordContentReadResult));
        }

        [Test]
        public void SeekBegin()
        {
            var empty = GetEmptyArray(2 * SizeConstants.SegmentData);
            var record = CreateRecordWithContent("record", empty);
            var recordContent = GetRandomByteArray(2 * SizeConstants.SegmentData);

            record.Seek(SizeConstants.SegmentData / 2, SeekOrigin.Begin);
            record.Write(recordContent, SizeConstants.SegmentData / 2, SizeConstants.SegmentData / 2);

            record.Seek(0, SeekOrigin.Begin);
            record.Write(recordContent, 0, SizeConstants.SegmentData / 2);

            record.Seek(SizeConstants.SegmentData + SizeConstants.SegmentData / 2, SeekOrigin.Begin);
            record.Write(recordContent, SizeConstants.SegmentData + SizeConstants.SegmentData / 2, SizeConstants.SegmentData / 2);

            record.Seek(SizeConstants.SegmentData, SeekOrigin.Begin);
            record.Write(recordContent, SizeConstants.SegmentData, SizeConstants.SegmentData / 2);

            var recordContentReadResult = new byte[recordContent.Length];
            record.Seek(0, SeekOrigin.Begin);
            record.Read(recordContentReadResult, 0, recordContentReadResult.Length);
            Assert.IsTrue(ByteArray.IsEqual(recordContent, recordContentReadResult));
        }

        [Test]
        public void SeekCurrent()
        {
            var empty = GetEmptyArray(2 * SizeConstants.SegmentData);
            var record = CreateRecordWithContent("record", empty);
            record.Seek(0, SeekOrigin.Begin);

            var recordContent = GetRandomByteArray(2 * SizeConstants.SegmentData);

            record.Seek(SizeConstants.SegmentData / 2, SeekOrigin.Current);
            record.Write(recordContent, SizeConstants.SegmentData / 2, SizeConstants.SegmentData / 2);

            record.Seek(-SizeConstants.SegmentData, SeekOrigin.Current);
            record.Write(recordContent, 0, SizeConstants.SegmentData / 2);

            record.Seek(SizeConstants.SegmentData, SeekOrigin.Current);
            record.Write(recordContent, SizeConstants.SegmentData + SizeConstants.SegmentData / 2, SizeConstants.SegmentData / 2);

            record.Seek(-SizeConstants.SegmentData, SeekOrigin.Current);
            record.Write(recordContent, SizeConstants.SegmentData, SizeConstants.SegmentData / 2);

            var recordContentReadResult = new byte[recordContent.Length];
            record.Seek(0, SeekOrigin.Begin);
            record.Read(recordContentReadResult, 0, recordContentReadResult.Length);
            Assert.IsTrue(ByteArray.IsEqual(recordContent, recordContentReadResult));
        }

        [Test]
        public void SeekEnd()
        {
            var empty = GetEmptyArray(2 * SizeConstants.SegmentData);
            var record = CreateRecordWithContent("record", empty);
            record.Seek(0, SeekOrigin.Begin);

            var recordContent = GetRandomByteArray(2 * SizeConstants.SegmentData);

            record.Seek(-2 * SizeConstants.SegmentData + SizeConstants.SegmentData / 2, SeekOrigin.End);
            record.Write(recordContent, SizeConstants.SegmentData / 2, SizeConstants.SegmentData / 2);

            record.Seek(-2 * SizeConstants.SegmentData, SeekOrigin.End);
            record.Write(recordContent, 0, SizeConstants.SegmentData / 2);

            record.Seek(-SizeConstants.SegmentData / 2, SeekOrigin.End);
            record.Write(recordContent, SizeConstants.SegmentData + SizeConstants.SegmentData / 2, SizeConstants.SegmentData / 2);

            record.Seek(-SizeConstants.SegmentData, SeekOrigin.End);
            record.Write(recordContent, SizeConstants.SegmentData, SizeConstants.SegmentData / 2);

            var recordContentReadResult = new byte[recordContent.Length];
            record.Seek(0, SeekOrigin.Begin);
            record.Read(recordContentReadResult, 0, recordContentReadResult.Length);
            Assert.IsTrue(ByteArray.IsEqual(recordContent, recordContentReadResult));
        }
    }
}
