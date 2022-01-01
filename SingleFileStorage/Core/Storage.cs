using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SingleFileStorage.Core
{
    internal class Storage : IStorage
    {
        private readonly IStorageFileStream _fileStream;

        public Storage(IStorageFileStream fileStream)
        {
            _fileStream = fileStream ?? throw new ArgumentNullException(nameof(fileStream));
        }

        public void InitDescription()
        {
            try
            {
                _fileStream.BeginReadWrite();
                var emptyDescriptionBytes = new byte[SizeConstants.StorageDescription];
                _fileStream.WriteByteArray(emptyDescriptionBytes, 0, emptyDescriptionBytes.Length);
            }
            finally
            {
                _fileStream.EndReadWrite();
            }
        }

        public void CreateRecord(string recordName)
        {
            try
            {
                RecordName.ThrowErrorIfInvalid(recordName);
                _fileStream.BeginReadWrite();
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
            finally
            {
                _fileStream.EndReadWrite();
            }
        }

        public Stream OpenRecord(string recordName, RecordAccess recordAccess)
        {
            RecordName.ThrowErrorIfInvalid(recordName);
            if (recordAccess == RecordAccess.Read) _fileStream.BeginRead();
            else if (recordAccess == RecordAccess.ReadWrite) _fileStream.BeginReadWrite();
            else throw new ArgumentException(nameof(recordAccess));
            _fileStream.Seek(0, SeekOrigin.Begin);
            var recordDescription = RecordDescription.FindByName(_fileStream, recordName);

            return new RecordStream(_fileStream, recordAccess, recordDescription);
        }

        public void RenameRecord(string oldRecordName, string newRecordName)
        {
            RecordName.ThrowErrorIfInvalid(oldRecordName);
            RecordName.ThrowErrorIfInvalid(newRecordName);
        }

        public void DeleteRecord(string recordName)
        {
            RecordName.ThrowErrorIfInvalid(recordName);
        }
    }
}
