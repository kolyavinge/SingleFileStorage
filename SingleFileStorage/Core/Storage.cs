using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SingleFileStorage.Core
{
    internal class Storage : IStorage
    {
        internal static void InitDescription(IStorageFileStream fileStream)
        {
            var emptyDescriptionBytes = new byte[SizeConstants.StorageDescription];
            fileStream.WriteByteArray(emptyDescriptionBytes, 0, emptyDescriptionBytes.Length);
        }

        private readonly IStorageFileStream _fileStream;

        public Storage(IStorageFileStream fileStream)
        {
            _fileStream = fileStream ?? throw new ArgumentNullException(nameof(fileStream));
        }

        public void Dispose()
        {
            _fileStream.Dispose();
        }

        public void CreateRecord(string recordName)
        {
            RecordName.ThrowErrorIfInvalid(recordName);
            _fileStream.Seek(0, SeekOrigin.Begin);
            var recordDescription = RecordDescription.FindByName(_fileStream, recordName);
            if (recordDescription != null) throw new IOException($"Record '{recordName}' already exists.");
            _fileStream.Seek(0, SeekOrigin.Begin);
            RecordDescription.FindFree(_fileStream);
            RecordDescription.WriteName(_fileStream, recordName);
            long recordDescriptionFirstSegmentIndexPosition = _fileStream.Position;
            _fileStream.Seek(SizeConstants.StorageDescription, SeekOrigin.Begin);
            uint firstSegmentIndex = Segment.FindNextFreeSegmentIndex(_fileStream);
            if (firstSegmentIndex != Segment.NullValue)
            {
                Segment.WriteState(_fileStream, SegmentState.UsedAndLast);
            }
            else
            {
                Segment.AppendEmptySegment(_fileStream, SegmentState.UsedAndLast);
                firstSegmentIndex = Segment.GetSegmentsCount(_fileStream.Length) - 1;
            }
            _fileStream.Seek(recordDescriptionFirstSegmentIndexPosition, SeekOrigin.Begin);
            RecordDescription.WriteFirstSegmentIndex(_fileStream, firstSegmentIndex);
            RecordDescription.WriteLastSegmentIndex(_fileStream, firstSegmentIndex);
            RecordDescription.WriteLength(_fileStream, 0);
        }

        public Stream OpenRecord(string recordName)
        {
            RecordName.ThrowErrorIfInvalid(recordName);
            _fileStream.Seek(0, SeekOrigin.Begin);
            var recordDescription = RecordDescription.FindByName(_fileStream, recordName);
            if (recordDescription == null) throw new IOException($"Record '{recordName}' does not exist.");

            return new RecordStream(_fileStream, recordDescription);
        }

        public bool IsRecordExist(string recordName)
        {
            RecordName.ThrowErrorIfInvalid(recordName);
            _fileStream.Seek(0, SeekOrigin.Begin);
            var recordDescription = RecordDescription.FindByName(_fileStream, recordName);

            return recordDescription != null;
        }

        public void RenameRecord(string oldRecordName, string newRecordName)
        {
            RecordName.ThrowErrorIfInvalid(oldRecordName);
            RecordName.ThrowErrorIfInvalid(newRecordName);
            _fileStream.Seek(0, SeekOrigin.Begin);
            var oldRecordDescription = RecordDescription.FindByName(_fileStream, oldRecordName);
            if (oldRecordDescription == null) throw new IOException($"Record '{oldRecordName}' does not exist.");
            _fileStream.Seek(0, SeekOrigin.Begin);
            var newRecordDescription = RecordDescription.FindByName(_fileStream, newRecordName);
            if (newRecordDescription != null) throw new IOException($"Record '{newRecordName}' already exists.");
            _fileStream.Seek(oldRecordDescription.StartPosition + SizeConstants.RecordState, SeekOrigin.Begin);
            RecordDescription.WriteName(_fileStream, newRecordName);
        }

        public void DeleteRecord(string recordName)
        {
            RecordName.ThrowErrorIfInvalid(recordName);
            _fileStream.Seek(0, SeekOrigin.Begin);
            var recordDescription = RecordDescription.FindByName(_fileStream, recordName);
            if (recordDescription == null) throw new IOException($"Record '{recordName}' does not exist.");
            _fileStream.Seek(recordDescription.StartPosition, SeekOrigin.Begin);
            RecordDescription.WriteState(_fileStream, RecordState.Free);
            var firstSegment = Segment.GotoSegmentStartPositionAndCreate(_fileStream, recordDescription.FirstSegmentIndex);
            var segmentIterator = new SegmentIterator(_fileStream, firstSegment);
            segmentIterator.ForEach(s => Segment.WriteState(_fileStream, SegmentState.Free));
        }

        public List<string> GetAllRecordNames()
        {
            var result = new List<string>();
            _fileStream.Seek(0, SeekOrigin.Begin);
            for (int recordNumber = 0; recordNumber < SizeConstants.MaxRecordsCount; recordNumber++)
            {
                byte recordState = RecordDescription.ReadState(_fileStream);
                if (!RecordState.IsFree(recordState))
                {
                    var nameBytes = new byte[SizeConstants.RecordName];
                    _fileStream.ReadByteArray(nameBytes, 0, SizeConstants.RecordName);
                    var name = Encoding.UTF8.GetString(nameBytes, 0, Array.IndexOf<byte>(nameBytes, 0));
                    result.Add(name);
                    _fileStream.Seek(SizeConstants.RecordFirstSegmentIndex + SizeConstants.RecordLastSegmentIndex + SizeConstants.RecordLength, SeekOrigin.Current);
                }
                else
                {
                    _fileStream.Seek(SizeConstants.RecordDescription - SizeConstants.RecordState, SeekOrigin.Current);
                }
            }

            return result;
        }
    }
}
