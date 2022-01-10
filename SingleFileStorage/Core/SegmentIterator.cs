using System;
using System.IO;
using SingleFileStorage.Infrastructure;

namespace SingleFileStorage.Core
{
    static class SegmentIterator
    {
        public static Segment GetNextSegment(StorageFileStream storageFileStream, SegmentBuffer segmentBuffer, Segment segment)
        {
            if (segment.NextSegment != null)
            {
                return segment.NextSegment;
            }

            if (segment.State != SegmentState.Last)
            {
                var nextSegment = segmentBuffer.GetByIndex(storageFileStream, segment.NextSegmentIndex);
                segment.NextSegment = nextSegment;
                return nextSegment;
            }

            return null;
        }

        public static void ForEach(StorageFileStream storageFileStream, SegmentBuffer segmentBuffer, Segment segment, Action<Segment> action)
        {
            var current = segment;
            while (true)
            {
                action(current);
                current = GetNextSegment(storageFileStream, segmentBuffer, current);
                if (current == null) return;
                storageFileStream.Seek(current.DataStartPosition, SeekOrigin.Begin);
            }
        }

        public static void ForEachExceptFirst(StorageFileStream storageFileStream, SegmentBuffer segmentBuffer, Segment segment, Action<Segment> action)
        {
            var current = GetNextSegment(storageFileStream, segmentBuffer, segment);
            if (current == null) return;
            while (true)
            {
                action(current);
                current = GetNextSegment(storageFileStream, segmentBuffer, current);
                if (current == null) return;
                storageFileStream.Seek(current.DataStartPosition, SeekOrigin.Begin);
            }
        }
    }
}
