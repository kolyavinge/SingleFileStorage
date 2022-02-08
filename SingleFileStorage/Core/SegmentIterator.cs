using System;
using System.Collections.Generic;
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
            storageFileStream.Seek(current.StartPosition, SeekOrigin.Begin);
            while (true)
            {
                action(current);
                current = GetNextSegment(storageFileStream, segmentBuffer, current);
                if (current == null) return;
                storageFileStream.Seek(current.StartPosition, SeekOrigin.Begin);
            }
        }

        public static List<Segment> ForEachExceptFirst(StorageFileStream storageFileStream, SegmentBuffer segmentBuffer, Segment segment, Action<Segment> action)
        {
            var iteratedSegments = new List<Segment>();
            var current = GetNextSegment(storageFileStream, segmentBuffer, segment);
            if (current == null) return iteratedSegments;
            storageFileStream.Seek(current.StartPosition, SeekOrigin.Begin);
            while (true)
            {
                action(current);
                iteratedSegments.Add(current);
                current = GetNextSegment(storageFileStream, segmentBuffer, current);
                if (current == null) return iteratedSegments;
                storageFileStream.Seek(current.StartPosition, SeekOrigin.Begin);
            }
        }
    }
}
