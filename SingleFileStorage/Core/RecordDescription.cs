using System.IO;
using SingleFileStorage.Infrastructure;

namespace SingleFileStorage.Core
{
    internal class RecordDescription
    {
        public static byte ReadState(StorageFileStream storageFileStream)
        {
            return storageFileStream.ReadByte();
        }

        public static uint ReadFirstSegmentIndex(StorageFileStream storageFileStream)
        {
            return storageFileStream.ReadUInt32();
        }

        public static uint ReadLastSegmentIndex(StorageFileStream storageFileStream)
        {
            return storageFileStream.ReadUInt32();
        }

        public static uint ReadLength(StorageFileStream storageFileStream)
        {
            return storageFileStream.ReadUInt32();
        }

        public static void WriteState(StorageFileStream storageFileStream, byte state)
        {
            storageFileStream.WriteByte(state);
        }

        public static void WriteFirstSegmentIndex(StorageFileStream storageFileStream, uint firstSegmentIndex)
        {
            storageFileStream.WriteUInt32(firstSegmentIndex);
        }

        public static void WriteLastSegmentIndex(StorageFileStream storageFileStream, uint lastSegmentIndex)
        {
            storageFileStream.WriteUInt32(lastSegmentIndex);
        }

        public static void WriteLength(StorageFileStream storageFileStream, uint length)
        {
            storageFileStream.WriteUInt32(length);
        }

        public static void WriteName(StorageFileStream storageFileStream, string recordName)
        {
            var nameBytes = RecordName.GetBytes(recordName);
            storageFileStream.WriteByteArray(nameBytes, 0, nameBytes.Length);
        }

        public static void FindFree(StorageFileStream storageFileStream)
        {
            for (int recordNumber = 0; recordNumber < SizeConstants.MaxRecordsCount; recordNumber++)
            {
                byte recordState = ReadState(storageFileStream);
                if (recordState == RecordState.Free)
                {
                    storageFileStream.Seek(-SizeConstants.RecordState, SeekOrigin.Current);
                    WriteState(storageFileStream, RecordState.Used);
                    return;
                }
                else
                {
                    storageFileStream.Seek(SizeConstants.RecordDescription - SizeConstants.RecordState, SeekOrigin.Current);
                }
            }

            throw new IOException("Cannot find any free record description.");
        }

        public static RecordDescription FindByName(StorageFileStream storageFileStream, string recordName)
        {
            var recordNameBytes = RecordName.GetBytes(recordName);
            for (int recordNumber = 0; recordNumber < SizeConstants.MaxRecordsCount; recordNumber++)
            {
                byte recordState = ReadState(storageFileStream);
                if (recordState == RecordState.Used)
                {
                    var currentRecordNameBytes = new byte[SizeConstants.RecordName];
                    storageFileStream.ReadByteArray(currentRecordNameBytes, 0, SizeConstants.RecordName);
                    if (RecordName.IsEqual(recordNameBytes, currentRecordNameBytes))
                    {
                        return new RecordDescription(storageFileStream, recordState);
                    }
                    storageFileStream.Seek(SizeConstants.RecordFirstSegmentIndex + SizeConstants.RecordLastSegmentIndex + SizeConstants.RecordLength, SeekOrigin.Current);
                }
                else
                {
                    storageFileStream.Seek(SizeConstants.RecordDescription - SizeConstants.RecordState, SeekOrigin.Current);
                }
            }

            return null;
        }

        public readonly long StartPosition;
        public byte State;
        public readonly uint FirstSegmentIndex;
        public uint LastSegmentIndex;
        public uint RecordLength;
        public readonly long LastSegmentIndexPosition;
        public readonly long RecordLengthStartPosition;
        public bool IsModified;

        private RecordDescription(StorageFileStream storageFileStream, byte state)
        {
            StartPosition = storageFileStream.Position - (SizeConstants.RecordState + SizeConstants.RecordName);
            State = state;
            FirstSegmentIndex = ReadFirstSegmentIndex(storageFileStream);
            LastSegmentIndex = ReadLastSegmentIndex(storageFileStream);
            RecordLength = ReadLength(storageFileStream);
            LastSegmentIndexPosition = StartPosition + SizeConstants.RecordState + SizeConstants.RecordName + SizeConstants.RecordFirstSegmentIndex;
            RecordLengthStartPosition = StartPosition + SizeConstants.RecordState + SizeConstants.RecordName + SizeConstants.RecordFirstSegmentIndex + SizeConstants.RecordLastSegmentIndex;
        }
    }
}
