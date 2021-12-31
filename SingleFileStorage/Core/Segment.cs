using System;
using System.IO;

namespace SingleFileStorage.Core
{
    internal class Segment
    {
        public const uint NullValue = UInt32.MaxValue;

        public static byte ReadState(IStorageFileStream storageFileStream)
        {
            return storageFileStream.ReadByte();
        }

        public static void WriteState(IStorageFileStream storageFileStream, byte state)
        {
            storageFileStream.WriteByte(state);
        }

        public static uint ReadNextSegmentIndexOrDataLength(IStorageFileStream storageFileStream)
        {
            return storageFileStream.ReadUInt32();
        }

        public static void WriteNextSegmentIndexOrDataLength(IStorageFileStream storageFileStream, uint value)
        {
            storageFileStream.WriteUInt32(value);
        }

        private static void WriteData(IStorageFileStream storageFileStream, byte[] buffer, int offset, int count)
        {
            if (count > SizeConstants.SegmentData) throw new ArgumentException($"Count must be less or equal {SizeConstants.SegmentData}");
            storageFileStream.WriteByteArray(buffer, offset, count);
            if (count < SizeConstants.SegmentData)
            {
                var empty = new byte[SizeConstants.SegmentData - count];
                storageFileStream.WriteByteArray(empty, 0, empty.Length);
            }
        }

        public static void AppendEmptySegment(IStorageFileStream storageFileStream, byte state)
        {
            Segment.WriteState(storageFileStream, state);
            Segment.WriteNextSegmentIndexOrDataLength(storageFileStream, 0);
            var emptySegmentDataBytes = new byte[SizeConstants.SegmentData];
            Segment.WriteData(storageFileStream, emptySegmentDataBytes, 0, emptySegmentDataBytes.Length);
        }

        public static void AppendSegment(IStorageFileStream storageFileStream, byte state, uint nextSegmentIndexOrDataLength, byte[] buffer, int offset, int count)
        {
            Segment.WriteState(storageFileStream, state);
            Segment.WriteNextSegmentIndexOrDataLength(storageFileStream, nextSegmentIndexOrDataLength);
            Segment.WriteData(storageFileStream, buffer, offset, count);
        }

        public static void AppendSegment(IStorageFileStream storageFileStream, byte state, uint nextSegmentIndexOrDataLength, byte[] buffer, int offset, int count, out Segment appendedSegment)
        {
            uint index = GetSegmentIndex(storageFileStream.Position);
            AppendSegment(storageFileStream, state, nextSegmentIndexOrDataLength, buffer, offset, count);
            appendedSegment = new Segment(index, state, nextSegmentIndexOrDataLength);
        }

        public static uint FindNextFreeSegmentIndex(IStorageFileStream storageFileStream)
        {
            uint segmentIndex = GetSegmentIndex(storageFileStream.Position);
            uint segmentsCount = GetSegmentsCount(storageFileStream.Length);
            while (segmentIndex < segmentsCount)
            {
                byte segmentState = ReadState(storageFileStream);
                if (SegmentState.IsFree(segmentState))
                {
                    storageFileStream.Seek(-SizeConstants.SegmentState, SeekOrigin.Current);
                    return segmentIndex;
                }
                storageFileStream.Seek(SizeConstants.Segment - SizeConstants.SegmentState, SeekOrigin.Current);
                segmentIndex++;
            }

            return NullValue;
        }

        public static uint GetSegmentIndex(long fileStreamPosition)
        {
            long correctedPosition = fileStreamPosition - SizeConstants.StorageDescription;
            long fullSegmentsCount = correctedPosition / SizeConstants.Segment;
            long lastSegmentOffset = correctedPosition % SizeConstants.Segment;
            uint currentSegmentIndex = (uint)fullSegmentsCount;
            if (lastSegmentOffset > 0) currentSegmentIndex++;

            return currentSegmentIndex;
        }

        public static uint GetSegmentsCount(long fileStreamLength)
        {
            return (uint)((fileStreamLength - SizeConstants.StorageDescription) / SizeConstants.Segment);
        }

        public static uint GetSegmentStartPosition(uint segmentIndex)
        {
            return SizeConstants.StorageDescription + SizeConstants.Segment * segmentIndex;
        }

        public static Segment CreateFromCurrentPosition(IStorageFileStream storageFileStream)
        {
            uint index = GetSegmentIndex(storageFileStream.Position);
            byte state = ReadState(storageFileStream);
            uint nextSegmentIndexOrDataLength = ReadNextSegmentIndexOrDataLength(storageFileStream);
            var segment = new Segment(index, state, nextSegmentIndexOrDataLength);

            return segment;
        }

        public uint Index;
        public byte State;
        public uint NextSegmentIndex;
        public uint DataLength;
        public long StartPosition;

        public long EndPosition => StartPosition + SizeConstants.Segment;
        public long DataStartPosition => StartPosition + SizeConstants.SegmentState + SizeConstants.SegmentNextIndexOrDataLength;
        public long DataEndPosition => DataStartPosition + SizeConstants.SegmentData;

        private Segment(uint index, byte state, uint nextSegmentIndexOrDataLength)
        {
            Index = index;
            State = state;
            if (SegmentState.IsLast(State))
            {
                NextSegmentIndex = NullValue;
                DataLength = nextSegmentIndexOrDataLength;
            }
            else
            {
                NextSegmentIndex = nextSegmentIndexOrDataLength;
                DataLength = SizeConstants.SegmentData;
            }
            StartPosition = SizeConstants.StorageDescription + SizeConstants.Segment * Index;
        }

        public bool Contains(long position)
        {
            return DataStartPosition <= position && position <= DataStartPosition + DataLength;
        }
    }
}
