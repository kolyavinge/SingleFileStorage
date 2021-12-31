﻿using System;
using System.Collections.Generic;
using System.IO;
using SingleFileStorage.Core;

namespace SingleFileStorage.Test.Tools
{
    internal abstract class BaseTest
    {
        protected Storage _storage;
        protected MemoryFileStream _memoryStream;

        public Stream CreateEmptyRecord(string recordName)
        {
            _storage.CreateRecord(recordName);
            var record = _storage.OpenRecord(recordName, RecordAccess.ReadWrite);

            return record;
        }

        public Stream CreateRecordWithContent(string recordName, byte[] recordContent)
        {
            _storage.CreateRecord(recordName);
            var record = _storage.OpenRecord(recordName, RecordAccess.ReadWrite);
            record.Write(recordContent, 0, recordContent.Length);

            return record;
        }

        public RecordDescription GetRecordDescription(string name)
        {
            var position = _memoryStream.Position;
            _memoryStream.Seek(0, SeekOrigin.Begin);
            var recordDescription = RecordDescription.FindByName(_memoryStream, name);
            _memoryStream.Seek(position, SeekOrigin.Begin);

            return recordDescription;
        }

        public List<Segment> GetAllSegments(string name)
        {
            var result = new List<Segment>();

            var position = _memoryStream.Position;

            _memoryStream.Seek(SizeConstants.StorageDescription, SeekOrigin.Begin);
            var recordDescription = RecordDescription.FindByName(_memoryStream, name);
            var segmentPosition = Segment.GetSegmentStartPosition(recordDescription.FirstSegmentIndex);
            _memoryStream.Seek(segmentPosition, SeekOrigin.Begin);
            var segment = Segment.CreateFromCurrentPosition(_memoryStream);
            result.Add(segment);

            segmentPosition = segment.NextSegmentIndex;
            while (segmentPosition != Segment.NullValue)
            {
                segmentPosition = Segment.GetSegmentStartPosition(segmentPosition);
                _memoryStream.Seek(segmentPosition, SeekOrigin.Begin);
                segment = Segment.CreateFromCurrentPosition(_memoryStream);
                result.Add(segment);
                segmentPosition = segment.NextSegmentIndex;
            }

            _memoryStream.Seek(position, SeekOrigin.Begin);

            return result;
        }

        public byte[] GetRandomByteArray(int size)
        {
            var result = new byte[size];
            var rand = new Random();
            rand.NextBytes(result);

            return result;
        }

        public byte[] GetEmptyArray(int size)
        {
            return new byte[size];
        }
    }
}
