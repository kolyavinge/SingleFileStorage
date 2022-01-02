using System;
using System.IO;

namespace SingleFileStorage.Core
{
    internal class RecordDescription
    {
        public static byte ReadState(IStorageFileStream storageFileStream)
        {
            return storageFileStream.ReadByte();
        }

        public static uint ReadFirstSegmentIndex(IStorageFileStream storageFileStream)
        {
            return storageFileStream.ReadUInt32();
        }

        public static uint ReadLastSegmentIndex(IStorageFileStream storageFileStream)
        {
            return storageFileStream.ReadUInt32();
        }

        public static uint ReadLength(IStorageFileStream storageFileStream)
        {
            return storageFileStream.ReadUInt32();
        }

        public static void WriteState(IStorageFileStream storageFileStream, byte state)
        {
            storageFileStream.WriteByte(state);
        }

        public static void WriteFirstSegmentIndex(IStorageFileStream storageFileStream, uint firstSegmentIndex)
        {
            storageFileStream.WriteUInt32(firstSegmentIndex);
        }

        public static void WriteLastSegmentIndex(IStorageFileStream storageFileStream, uint lastSegmentIndex)
        {
            storageFileStream.WriteUInt32(lastSegmentIndex);
        }

        public static void WriteLength(IStorageFileStream storageFileStream, uint length)
        {
            storageFileStream.WriteUInt32(length);
        }

        public static void WriteName(IStorageFileStream storageFileStream, string recordName)
        {
            var nameBytes = RecordName.GetBytes(recordName);
            storageFileStream.WriteByteArray(nameBytes, 0, nameBytes.Length);
        }

        public static void FindFree(IStorageFileStream storageFileStream)
        {
            for (int recordNumber = 0; recordNumber < SizeConstants.MaxRecordsCount; recordNumber++)
            {
                byte recordState = ReadState(storageFileStream);
                if (RecordState.IsFree(recordState))
                {
                    storageFileStream.Seek(-SizeConstants.RecordState, SeekOrigin.Current);
                    RecordState.SetUsed(ref recordState);
                    WriteState(storageFileStream, recordState);
                    return;
                }
                else
                {
                    storageFileStream.Seek(SizeConstants.RecordDescription - SizeConstants.RecordState, SeekOrigin.Current);
                }
            }

            throw new IOException("Cannot find any free record description.");
        }

        public static RecordDescription FindByName(IStorageFileStream storageFileStream, string recordName)
        {
            var recordNameBytes = RecordName.GetBytes(recordName);
            for (int recordNumber = 0; recordNumber < SizeConstants.MaxRecordsCount; recordNumber++)
            {
                byte recordState = ReadState(storageFileStream);
                if (!RecordState.IsFree(recordState))
                {
                    var currentRecordNameBytes = new byte[SizeConstants.RecordName];
                    storageFileStream.ReadByteArray(currentRecordNameBytes, 0, SizeConstants.RecordName);
                    if (RecordName.IsEqual(recordNameBytes, currentRecordNameBytes))
                    {
                        return MakeStartFromFirstSegmentIndex(storageFileStream);
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

        private static RecordDescription MakeStartFromFirstSegmentIndex(IStorageFileStream storageFileStream)
        {
            return new RecordDescription
            {
                StartPosition = storageFileStream.Position - (SizeConstants.RecordState + SizeConstants.RecordName),
                FirstSegmentIndex = ReadFirstSegmentIndex(storageFileStream),
                LastSegmentIndex = ReadLastSegmentIndex(storageFileStream),
                RecordLength = ReadLength(storageFileStream)
            };
        }

        public long StartPosition;
        public uint FirstSegmentIndex;
        public uint LastSegmentIndex;
        public uint RecordLength;

        public long LastSegmentIndexPosition
        {
            get
            {
                return StartPosition + SizeConstants.RecordState + SizeConstants.RecordName + SizeConstants.RecordFirstSegmentIndex;
            }
        }

        public long RecordLengthStartPosition
        {
            get
            {
                return StartPosition + SizeConstants.RecordState + SizeConstants.RecordName + SizeConstants.RecordFirstSegmentIndex + SizeConstants.RecordLastSegmentIndex;
            }
        }
    }
}
