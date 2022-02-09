using System;
using System.IO;
using NUnit.Framework;
using SingleFileStorage.Core;
using SingleFileStorage.Test.Tools;
using SingleFileStorage.Test.Utils;

namespace SingleFileStorage.Test.Core
{
    class RecordStreamTest : BaseTest
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
        public void OpenReadAndTryWrite_Error()
        {
            var recordContent = GetRandomByteArray(SizeConstants.SegmentData);
            CreateRecordWithContent("record", recordContent);
            _memoryStream.Dispose();
            _memoryStream.Open(Access.Read);
            var storageForRead = new Storage(_memoryStream);
            var record = storageForRead.OpenRecord("record");
            try
            {
                record.Write(recordContent, 0, recordContent.Length);
                Assert.Fail();
            }
            catch (Exception exp)
            {
                Assert.AreEqual("Stream is opened with Read access and cannot be modified.", exp.Message);
            }
        }

        [Test]
        public void Position_Read()
        {
            _storage.CreateRecord("record");
            var record = _storage.OpenRecord("record");
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
        public void Position_Write()
        {
            _storage.CreateRecord("record");
            var record = _storage.OpenRecord("record");
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
        public void ManyRead()
        {
            _storage.CreateRecord("record");
            var record = _storage.OpenRecord("record");
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
        public void ManyWrite()
        {
            _storage.CreateRecord("record");
            var record = _storage.OpenRecord("record");
            var recordContent = GetRandomByteArray(2 * SizeConstants.SegmentData + SizeConstants.SegmentData / 2);
            record.Write(recordContent, 0, 100);
            record.Write(recordContent, 100, SizeConstants.SegmentData - 100);
            record.Write(recordContent, SizeConstants.SegmentData, SizeConstants.SegmentData);
            record.Write(recordContent, 2 * SizeConstants.SegmentData, SizeConstants.SegmentData / 2);
            record.Seek(0, SeekOrigin.Begin);
            var recordContentReadResult = new byte[recordContent.Length];
            record.Read(recordContentReadResult, 0, recordContentReadResult.Length);
            Assert.IsTrue(ByteArray.IsEqual(recordContent, recordContentReadResult));
        }

        [Test]
        public void RecordDescription()
        {
            var recordContent = GetEmptyArray(2 * SizeConstants.SegmentData);
            _storage.CreateRecord("record");
            var record = _storage.OpenRecord("record");
            record.Dispose();

            var recordDescription = GetRecordDescription("record");
            Assert.AreEqual(0, recordDescription.FirstSegmentIndex);
            Assert.AreEqual(0, recordDescription.LastSegmentIndex);
            Assert.AreEqual(0, recordDescription.RecordLength);

            record = _storage.OpenRecord("record");
            record.Write(recordContent, 0, 100);
            record.Dispose();
            recordDescription = GetRecordDescription("record");
            Assert.AreEqual(0, recordDescription.FirstSegmentIndex);
            Assert.AreEqual(0, recordDescription.LastSegmentIndex);
            Assert.AreEqual(100, recordDescription.RecordLength);

            record = _storage.OpenRecord("record");
            record.Write(recordContent, 0, SizeConstants.SegmentData);
            record.Dispose();
            recordDescription = GetRecordDescription("record");
            Assert.AreEqual(0, recordDescription.FirstSegmentIndex);
            Assert.AreEqual(0, recordDescription.LastSegmentIndex);
            Assert.AreEqual(SizeConstants.SegmentData, recordDescription.RecordLength);

            record = _storage.OpenRecord("record");
            record.Write(recordContent, 0, 2 * SizeConstants.SegmentData);
            record.Dispose();
            recordDescription = GetRecordDescription("record");
            Assert.AreEqual(0, recordDescription.FirstSegmentIndex);
            Assert.AreEqual(1, recordDescription.LastSegmentIndex);
            Assert.AreEqual(2 * SizeConstants.SegmentData, recordDescription.RecordLength);

            record = _storage.OpenRecord("record");
            record.Write(recordContent, 0, 100);
            record.Dispose();
            recordDescription = GetRecordDescription("record");
            Assert.AreEqual(0, recordDescription.FirstSegmentIndex);
            Assert.AreEqual(1, recordDescription.LastSegmentIndex);
            Assert.AreEqual(2 * SizeConstants.SegmentData, recordDescription.RecordLength);
        }

        [Test]
        public void Length()
        {
            _storage.CreateRecord("record");
            var record = _storage.OpenRecord("record");
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
        public void WithinSegment()
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
        public void TwoSizeSegments()
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
        public void TwoSizeAndHalfSegments()
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
        public void ReadAfterEndRecord()
        {
            var record = CreateEmptyRecord("record");
            var recordContent = GetEmptyArray(2 * SizeConstants.SegmentData);
            record.Write(recordContent, 0, recordContent.Length);
            record.Seek(0, SeekOrigin.Begin);
            var recordContentReadResult = new byte[recordContent.Length];
            var count = record.Read(recordContentReadResult, 0, recordContentReadResult.Length);
            Assert.AreEqual(2 * SizeConstants.SegmentData, count);
            Assert.AreEqual(0, record.Read(recordContentReadResult, 0, recordContentReadResult.Length));
        }

        [Test]
        public void CreateRecordFreeSegments()
        {
            var recordContent = GetRandomByteArray(2 * SizeConstants.SegmentData + SizeConstants.SegmentData / 2);
            var record = CreateRecordWithContent("record1", recordContent);
            record.Dispose();
            _storage.DeleteRecord("record1");
            var allSegments = GetAllSegments();
            Assert.AreEqual(3, allSegments.Count);
            Assert.AreEqual(SegmentState.Free, allSegments[0].State);
            Assert.AreEqual(SegmentState.Free, allSegments[1].State);
            Assert.AreEqual(SegmentState.Free, allSegments[2].State);
            record = CreateRecordWithContent("record2", recordContent);
            record.Dispose();
            allSegments = GetAllSegments();
            Assert.AreEqual(3, allSegments.Count);
            Assert.AreEqual(SegmentState.Chained, allSegments[0].State);
            Assert.AreEqual(SizeConstants.SegmentData, allSegments[0].DataLength);
            Assert.AreEqual(1, allSegments[0].NextSegmentIndex);
            Assert.AreEqual(SegmentState.Chained, allSegments[1].State);
            Assert.AreEqual(SizeConstants.SegmentData, allSegments[1].DataLength);
            Assert.AreEqual(2, allSegments[1].NextSegmentIndex);
            Assert.AreEqual(SegmentState.Last, allSegments[2].State);
            Assert.AreEqual(SizeConstants.SegmentData / 2, allSegments[2].DataLength);
            Assert.AreEqual(Segment.NullValue, allSegments[2].NextSegmentIndex);
            var recordDescription = GetRecordDescription("record2");
            Assert.AreEqual(2, recordDescription.LastSegmentIndex);
            Assert.AreEqual(2 * SizeConstants.SegmentData + SizeConstants.SegmentData / 2, recordDescription.RecordLength);
        }

        [Test]
        public void CreateRecordFreeSegmentsAndNewOne()
        {
            var recordContent = GetRandomByteArray(2 * SizeConstants.SegmentData);
            var record = CreateRecordWithContent("record1", recordContent);
            record.Dispose();
            _storage.DeleteRecord("record1");
            recordContent = GetRandomByteArray(3 * SizeConstants.SegmentData);
            record = CreateRecordWithContent("record2", recordContent);
            record.Dispose();
            var allSegments = GetAllSegments();
            Assert.AreEqual(3, allSegments.Count);
            Assert.AreEqual(SegmentState.Chained, allSegments[0].State);
            Assert.AreEqual(SizeConstants.SegmentData, allSegments[0].DataLength);
            Assert.AreEqual(1, allSegments[0].NextSegmentIndex);
            Assert.AreEqual(SegmentState.Chained, allSegments[1].State);
            Assert.AreEqual(SizeConstants.SegmentData, allSegments[1].DataLength);
            Assert.AreEqual(2, allSegments[1].NextSegmentIndex);
            Assert.AreEqual(SegmentState.Last, allSegments[2].State);
            Assert.AreEqual(SizeConstants.SegmentData, allSegments[2].DataLength);
            Assert.AreEqual(Segment.NullValue, allSegments[2].NextSegmentIndex);
            var recordDescription = GetRecordDescription("record2");
            Assert.AreEqual(2, recordDescription.LastSegmentIndex);
            Assert.AreEqual(3 * SizeConstants.SegmentData, recordDescription.RecordLength);
        }

        [Test]
        public void CreateRecordFreeSegmentsAndWrite()
        {
            var recordContent = GetRandomByteArray(2 * SizeConstants.SegmentData + SizeConstants.SegmentData / 2);
            var record = CreateRecordWithContent("record1", recordContent);
            record.Dispose();
            _storage.DeleteRecord("record1");
            recordContent = GetRandomByteArray(3 * SizeConstants.SegmentData);
            record = CreateRecordWithContent("record2", recordContent);
            record.Write(recordContent, 0, 3 * SizeConstants.SegmentData);
            record.Dispose();
            var allSegments = GetAllSegments();
            Assert.AreEqual(6, allSegments.Count);
            var recordDescription = GetRecordDescription("record2");
            Assert.AreEqual(5, recordDescription.LastSegmentIndex);
            Assert.AreEqual(6 * SizeConstants.SegmentData, recordDescription.RecordLength);
        }

        [Test]
        public void Segments_1()
        {
            var recordContent = GetRandomByteArray(100);
            var record = CreateRecordWithContent("record", recordContent);
            record.Dispose();
            var segments = GetAllRecordSegments("record");
            Assert.AreEqual(1, segments.Count);
            Assert.AreEqual(SegmentState.Last, segments[0].State);
            Assert.AreEqual(100, segments[0].DataLength);
        }

        [Test]
        public void Segments_2()
        {
            var recordContent = GetRandomByteArray(SizeConstants.SegmentData + SizeConstants.SegmentData / 2);
            var record = CreateRecordWithContent("record", recordContent);
            record.Dispose();
            var segments = GetAllRecordSegments("record");
            Assert.AreEqual(2, segments.Count);
            Assert.AreEqual(SegmentState.Chained, segments[0].State);
            Assert.AreEqual(SizeConstants.SegmentData, segments[0].DataLength);
            Assert.AreEqual(1, segments[0].NextSegmentIndex);
            Assert.AreEqual(SegmentState.Last, segments[1].State);
            Assert.AreEqual(SizeConstants.SegmentData / 2, segments[1].DataLength);
            Assert.AreEqual(Segment.NullValue, segments[1].NextSegmentIndex);
        }

        [Test]
        public void Seek_Position()
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
        public void Seek_Begin()
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
        public void Seek_Current()
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
        public void Seek_End()
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

        [Test]
        public void Seek_End0()
        {
            var recordContent = GetRandomByteArray(2 * SizeConstants.SegmentData);
            var record = CreateEmptyRecord("record");
            record.Seek(0, SeekOrigin.End);
            record.Write(recordContent, 0, 2 * SizeConstants.SegmentData);
            var recordContentReadResult = new byte[recordContent.Length];
            record.Seek(0, SeekOrigin.Begin);
            record.Read(recordContentReadResult, 0, recordContentReadResult.Length);
            Assert.IsTrue(ByteArray.IsEqual(recordContent, recordContentReadResult));
        }

        [Test]
        public void SetLength_Decrease_0()
        {
            var recordContent = GetRandomByteArray(SizeConstants.SegmentData + SizeConstants.SegmentData / 2);
            var record = CreateRecordWithContent("record", recordContent);
            record.SetLength(0);
            var recordDescription = GetRecordDescription("record");
            Assert.AreEqual(0, record.Length);
            Assert.AreEqual(0, recordDescription.LastSegmentIndex);
            Assert.AreEqual(0, recordDescription.RecordLength);
        }

        [Test]
        public void SetLength_Decrease_1()
        {
            var recordContent = GetRandomByteArray(SizeConstants.SegmentData + SizeConstants.SegmentData / 2);
            var record = CreateRecordWithContent("record", recordContent);
            record.SetLength(SizeConstants.SegmentData + SizeConstants.SegmentData / 2 - 100);
            var recordDescription = GetRecordDescription("record");
            Array.Resize(ref recordContent, SizeConstants.SegmentData + SizeConstants.SegmentData / 2 - 100);
            var recordContentReadResult = new byte[recordContent.Length];
            record.Read(recordContentReadResult, 0, recordContentReadResult.Length);
            Assert.IsTrue(ByteArray.IsEqual(recordContent, recordContentReadResult));
            Assert.AreEqual(SizeConstants.SegmentData + SizeConstants.SegmentData / 2 - 100, record.Length);
            Assert.AreEqual(1, recordDescription.LastSegmentIndex);
            Assert.AreEqual(SizeConstants.SegmentData + SizeConstants.SegmentData / 2 - 100, recordDescription.RecordLength);
        }

        [Test]
        public void SetLength_Decrease_2()
        {
            var recordContent = GetRandomByteArray(SizeConstants.SegmentData + SizeConstants.SegmentData / 2);
            var record = CreateRecordWithContent("record", recordContent);
            record.SetLength(SizeConstants.SegmentData);
            var recordDescription = GetRecordDescription("record");
            Array.Resize(ref recordContent, SizeConstants.SegmentData);
            var recordContentReadResult = new byte[recordContent.Length];
            record.Read(recordContentReadResult, 0, recordContentReadResult.Length);
            Assert.IsTrue(ByteArray.IsEqual(recordContent, recordContentReadResult));
            Assert.AreEqual(SizeConstants.SegmentData, record.Length);
            Assert.AreEqual(0, recordDescription.LastSegmentIndex);
            Assert.AreEqual(SizeConstants.SegmentData, recordDescription.RecordLength);
        }

        [Test]
        public void SetLength_Decrease_3()
        {
            var recordContent = GetRandomByteArray(SizeConstants.SegmentData + SizeConstants.SegmentData / 2);
            var record = CreateRecordWithContent("record", recordContent);
            record.SetLength(SizeConstants.SegmentData / 2);
            var recordDescription = GetRecordDescription("record");
            Array.Resize(ref recordContent, SizeConstants.SegmentData / 2);
            var recordContentReadResult = new byte[recordContent.Length];
            record.Read(recordContentReadResult, 0, recordContentReadResult.Length);
            Assert.IsTrue(ByteArray.IsEqual(recordContent, recordContentReadResult));
            Assert.AreEqual(SizeConstants.SegmentData / 2, record.Length);
            Assert.AreEqual(0, recordDescription.LastSegmentIndex);
            Assert.AreEqual(SizeConstants.SegmentData / 2, recordDescription.RecordLength);
        }

        [Test]
        public void SetLength_Decrease_Segments()
        {
            var recordContent = GetRandomByteArray(SizeConstants.SegmentData + SizeConstants.SegmentData / 2);
            var record = CreateRecordWithContent("record", recordContent);
            record.SetLength(SizeConstants.SegmentData + SizeConstants.SegmentData / 2 - 100);
            record.Dispose();
            var segments = GetAllRecordSegments("record");
            Assert.AreEqual(2, segments.Count);
            Assert.AreEqual(SegmentState.Chained, segments[0].State);
            Assert.AreEqual(SizeConstants.SegmentData, segments[0].DataLength);
            Assert.AreEqual(1, segments[0].NextSegmentIndex);
            Assert.AreEqual(SegmentState.Last, segments[1].State);
            Assert.AreEqual(SizeConstants.SegmentData / 2 - 100, segments[1].DataLength);
            Assert.AreEqual(Segment.NullValue, segments[1].NextSegmentIndex);
        }

        [Test]
        public void SetLength_Read()
        {
            var recordContent = GetRandomByteArray(2 * SizeConstants.SegmentData);
            var record = CreateRecordWithContent("record", recordContent);
            record.SetLength(SizeConstants.SegmentData);
            Array.Resize(ref recordContent, SizeConstants.SegmentData);
            var recordContentReadResult = new byte[recordContent.Length];
            record.Read(recordContentReadResult, 0, recordContentReadResult.Length);
            Assert.IsTrue(ByteArray.IsEqual(recordContent, recordContentReadResult));
        }

        [Test]
        public void SetLength_CheckSegments()
        {
            var recordContent = GetRandomByteArray(3 * SizeConstants.SegmentData);
            var record = CreateRecordWithContent("record", recordContent);
            record.SetLength(SizeConstants.SegmentData);
            record.Dispose();
            var allSegments = GetAllSegments();
            Assert.AreEqual(3, allSegments.Count);
            Assert.AreEqual(SegmentState.Last, allSegments[0].State);
            Assert.AreEqual(SegmentState.Free, allSegments[1].State);
            Assert.AreEqual(SegmentState.Free, allSegments[2].State);
        }

        [Test]
        public void OpenReadAndTrySetLength_Error()
        {
            var recordContent = GetRandomByteArray(SizeConstants.SegmentData);
            CreateRecordWithContent("record", recordContent);
            _memoryStream.Dispose();
            _memoryStream.Open(Access.Read);
            var storageForRead = new Storage(_memoryStream);
            var record = storageForRead.OpenRecord("record");
            try
            {
                record.SetLength(100);
                Assert.Fail();
            }
            catch (Exception exp)
            {
                Assert.AreEqual("Stream is opened with Read access and cannot be modified.", exp.Message);
            }
        }
    }
}
