using System.IO;
using SingleFileStorage.Infrastructure;

namespace SingleFileStorage.Core
{
    internal class RecordDescription
    {
        public static byte ReadState(IReadableStream stream)
        {
            return stream.ReadByte();
        }

        public static uint ReadFirstSegmentIndex(IReadableStream stream)
        {
            return stream.ReadUInt32();
        }

        public static uint ReadLastSegmentIndex(IReadableStream stream)
        {
            return stream.ReadUInt32();
        }

        public static uint ReadLength(IReadableStream stream)
        {
            return stream.ReadUInt32();
        }

        public static void WriteState(IWriteableStream stream, byte state)
        {
            stream.WriteByte(state);
        }

        public static void WriteFirstSegmentIndex(IWriteableStream stream, uint firstSegmentIndex)
        {
            stream.WriteUInt32(firstSegmentIndex);
        }

        public static void WriteLastSegmentIndex(IWriteableStream stream, uint lastSegmentIndex)
        {
            stream.WriteUInt32(lastSegmentIndex);
        }

        public static void WriteLength(IWriteableStream stream, uint length)
        {
            stream.WriteUInt32(length);
        }

        public static void WriteName(IWriteableStream stream, string recordName)
        {
            var nameBytes = RecordName.GetBytes(recordName);
            stream.WriteByteArray(nameBytes, 0, nameBytes.Length);
        }

        public static long GetFreeStartPosition(IReadableStream storageDescriptionStream)
        {
            storageDescriptionStream.Seek(0, SeekOrigin.Begin);
            for (int recordNumber = 0; recordNumber < SizeConstants.MaxRecordsCount; recordNumber++)
            {
                byte recordState = ReadState(storageDescriptionStream);
                if (recordState == RecordState.Free)
                {
                    return storageDescriptionStream.Seek(-SizeConstants.RecordState, SeekOrigin.Current);
                }
                else
                {
                    storageDescriptionStream.Seek(SizeConstants.RecordDescription - SizeConstants.RecordState, SeekOrigin.Current);
                }
            }

            throw new IOException("Cannot find any free record description.");
        }

        public static RecordDescription FindByName(IReadableStream storageDescriptionStream, string recordName)
        {
            storageDescriptionStream.Seek(0, SeekOrigin.Begin);
            var recordNameBytes = RecordName.GetBytes(recordName);
            for (int recordNumber = 0; recordNumber < SizeConstants.MaxRecordsCount; recordNumber++)
            {
                byte recordState = ReadState(storageDescriptionStream);
                if (recordState == RecordState.Used)
                {
                    var currentRecordNameBytes = new byte[SizeConstants.RecordName];
                    storageDescriptionStream.ReadByteArray(currentRecordNameBytes, 0, SizeConstants.RecordName);
                    if (RecordName.IsEqual(recordNameBytes, currentRecordNameBytes))
                    {
                        return new RecordDescription(storageDescriptionStream, recordState);
                    }
                    storageDescriptionStream.Seek(SizeConstants.RecordFirstSegmentIndex + SizeConstants.RecordLastSegmentIndex + SizeConstants.RecordLength, SeekOrigin.Current);
                }
                else
                {
                    storageDescriptionStream.Seek(SizeConstants.RecordDescription - SizeConstants.RecordState, SeekOrigin.Current);
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

        private RecordDescription(IReadableStream stream, byte state)
        {
            long position = stream.Seek(0, SeekOrigin.Current);
            StartPosition = position - (SizeConstants.RecordState + SizeConstants.RecordName);
            State = state;
            FirstSegmentIndex = ReadFirstSegmentIndex(stream);
            LastSegmentIndex = ReadLastSegmentIndex(stream);
            RecordLength = ReadLength(stream);
            LastSegmentIndexPosition = StartPosition + SizeConstants.RecordState + SizeConstants.RecordName + SizeConstants.RecordFirstSegmentIndex;
            RecordLengthStartPosition = StartPosition + SizeConstants.RecordState + SizeConstants.RecordName + SizeConstants.RecordFirstSegmentIndex + SizeConstants.RecordLastSegmentIndex;
        }
    }
}
