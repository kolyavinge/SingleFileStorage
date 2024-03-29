﻿using System;
using System.Collections.Generic;
using System.IO;
using SingleFileStorage.Infrastructure;

namespace SingleFileStorage.Core;

internal class Storage : IStorage
{
    internal static void InitDescription(StorageFileStream fileStream)
    {
        var emptyDescriptionBytes = new byte[SizeConstants.StorageDescription];
        fileStream.WriteByteArray(emptyDescriptionBytes, 0, emptyDescriptionBytes.Length);
    }

    private readonly StorageFileStream _fileStream;
    private uint _createRecordStartSegmentIndex;

    public Storage(StorageFileStream fileStream)
    {
        _fileStream = fileStream;
    }

    public Access AccessMode => _fileStream.AccessMode;

    public void Dispose()
    {
        _fileStream.Dispose();
    }

    public void CreateRecord(string recordName)
    {
        ThrowErrorIfNotModified();
        RecordName.ThrowErrorIfInvalid(recordName);
        var storageDescriptionStream = StorageDescription.GetStorageDescription(_fileStream);
        var recordDescription = RecordDescription.FindByName(storageDescriptionStream, recordName);
        if (recordDescription is not null) throw new IOException($"Record '{recordName}' already exists.");
        long freeRecordDescriptionStartPosition = RecordDescription.GetFreeStartPosition(storageDescriptionStream);
        _fileStream.Seek(freeRecordDescriptionStartPosition, SeekOrigin.Begin);
        RecordDescription.WriteState(_fileStream, RecordState.Used);
        RecordDescription.WriteName(_fileStream, recordName);
        long recordDescriptionFirstSegmentIndexPosition = _fileStream.Position;
        _fileStream.Seek(Segment.GetSegmentStartPosition(_createRecordStartSegmentIndex), SeekOrigin.Begin);
        uint firstSegmentIndex = Segment.FindNextFreeSegmentIndex(_fileStream);
        if (firstSegmentIndex != Segment.NullValue)
        {
            Segment.WriteState(_fileStream, SegmentState.Last);
        }
        else
        {
            Segment.AppendEmptySegment(_fileStream, SegmentState.Last);
            firstSegmentIndex = Segment.GetSegmentsCount(_fileStream.Length) - 1;
        }
        _createRecordStartSegmentIndex = firstSegmentIndex + 1;
        _fileStream.Seek(recordDescriptionFirstSegmentIndexPosition, SeekOrigin.Begin);
        RecordDescription.WriteFirstSegmentIndex(_fileStream, firstSegmentIndex);
        RecordDescription.WriteLastSegmentIndex(_fileStream, firstSegmentIndex);
        RecordDescription.WriteLength(_fileStream, 0);
    }

    public Stream OpenRecord(string recordName)
    {
        RecordName.ThrowErrorIfInvalid(recordName);
        var storageDescriptionStream = StorageDescription.GetStorageDescription(_fileStream);
        var recordDescription = RecordDescription.FindByName(storageDescriptionStream, recordName);
        if (recordDescription is null) throw new IOException($"Record '{recordName}' does not exist.");

        return new RecordStream(_fileStream, recordDescription);
    }

    public bool IsRecordExist(string recordName)
    {
        RecordName.ThrowErrorIfInvalid(recordName);
        var storageDescriptionStream = StorageDescription.GetStorageDescription(_fileStream);
        var recordDescription = RecordDescription.FindByName(storageDescriptionStream, recordName);

        return recordDescription is not null;
    }

    public void RenameRecord(string oldRecordName, string newRecordName)
    {
        ThrowErrorIfNotModified();
        RecordName.ThrowErrorIfInvalid(oldRecordName);
        RecordName.ThrowErrorIfInvalid(newRecordName);
        var storageDescriptionStream = StorageDescription.GetStorageDescription(_fileStream);
        var oldRecordDescription = RecordDescription.FindByName(storageDescriptionStream, oldRecordName);
        if (oldRecordDescription is null) throw new IOException($"Record '{oldRecordName}' does not exist.");
        var newRecordDescription = RecordDescription.FindByName(storageDescriptionStream, newRecordName);
        if (newRecordDescription is not null) throw new IOException($"Record '{newRecordName}' already exists.");
        _fileStream.Seek(oldRecordDescription.StartPosition + SizeConstants.RecordState, SeekOrigin.Begin);
        RecordDescription.WriteName(_fileStream, newRecordName);
    }

    public void DeleteRecord(string recordName)
    {
        ThrowErrorIfNotModified();
        RecordName.ThrowErrorIfInvalid(recordName);
        var storageDescriptionStream = StorageDescription.GetStorageDescription(_fileStream);
        var recordDescription = RecordDescription.FindByName(storageDescriptionStream, recordName);
        if (recordDescription is null) throw new IOException($"Record '{recordName}' does not exist.");
        _fileStream.Seek(recordDescription.StartPosition, SeekOrigin.Begin);
        RecordDescription.WriteState(_fileStream, RecordState.Free);
        var firstSegment = Segment.GotoSegmentStartPositionAndCreate(_fileStream, recordDescription.FirstSegmentIndex);
        SegmentIterator.ForEach(_fileStream, new SegmentBuffer(), firstSegment, s => Segment.WriteState(_fileStream, SegmentState.Free));
        _createRecordStartSegmentIndex = Math.Min(_createRecordStartSegmentIndex, firstSegment.Index);
    }

    public List<string> GetAllRecordNames()
    {
        var result = new List<string>();
        var storageDescriptionStream = StorageDescription.GetStorageDescription(_fileStream);
        for (int recordNumber = 0; recordNumber < SizeConstants.MaxRecordsCount; recordNumber++)
        {
            byte recordState = RecordDescription.ReadState(storageDescriptionStream);
            if (recordState == RecordState.Used)
            {
                var nameBytes = new byte[SizeConstants.RecordName];
                storageDescriptionStream.ReadByteArray(nameBytes, 0, SizeConstants.RecordName);
                result.Add(RecordName.GetString(nameBytes));
                storageDescriptionStream.Seek(SizeConstants.RecordFirstSegmentIndex + SizeConstants.RecordLastSegmentIndex + SizeConstants.RecordLength, SeekOrigin.Current);
            }
            else
            {
                storageDescriptionStream.Seek(SizeConstants.RecordDescription - SizeConstants.RecordState, SeekOrigin.Current);
            }
        }

        return result;
    }

    private void ThrowErrorIfNotModified()
    {
        if (_fileStream.AccessMode != Access.Modify) throw new InvalidOperationException("Storage cannot be modified.");
    }
}
