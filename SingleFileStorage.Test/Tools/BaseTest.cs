using System;
using System.Collections.Generic;
using System.IO;
using SingleFileStorage.Core;

namespace SingleFileStorage.Test.Tools;

internal abstract class BaseTest
{
    protected Storage _storage;
    protected MemoryStorageFileStream _memoryStream;

    public void InitStorage()
    {
        _memoryStream = new MemoryStorageFileStream();
        _memoryStream.Open(Access.Modify);
        Storage.InitDescription(_memoryStream);
    }

    public void OpenStorage()
    {
        _storage = new Storage(_memoryStream);
    }

    public void DisposeStorage()
    {
        _storage.Dispose();
        _storage = null;
    }

    public Stream CreateEmptyRecord(string recordName)
    {
        _storage.CreateRecord(recordName);
        var record = _storage.OpenRecord(recordName);

        return record;
    }

    public Stream CreateRecordWithContent(string recordName, byte[] recordContent)
    {
        _storage.CreateRecord(recordName);
        var record = _storage.OpenRecord(recordName);
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

    public List<Segment> GetAllRecordSegments(string name)
    {
        var result = new List<Segment>();
        var position = _memoryStream.Position;
        _memoryStream.Seek(0, SeekOrigin.Begin);
        var recordDescription = RecordDescription.FindByName(_memoryStream, name);
        var segment = Segment.GotoSegmentStartPositionAndCreate(_memoryStream, recordDescription.FirstSegmentIndex);
        result.Add(segment);
        while (segment.NextSegmentIndex != Segment.NullValue)
        {
            segment = Segment.GotoSegmentStartPositionAndCreate(_memoryStream, segment.NextSegmentIndex);
            result.Add(segment);
        }
        _memoryStream.Seek(position, SeekOrigin.Begin);

        return result;
    }

    public List<Segment> GetAllSegments()
    {
        var result = new List<Segment>();
        var position = _memoryStream.Position;
        _memoryStream.Seek(0, SeekOrigin.Begin);
        uint segmentsCount = Segment.GetSegmentsCount(_memoryStream.Length);
        for (uint segmentIndex = 0; segmentIndex < segmentsCount; segmentIndex++)
        {
            var segment = Segment.GotoSegmentStartPositionAndCreate(_memoryStream, segmentIndex);
            result.Add(segment);
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
