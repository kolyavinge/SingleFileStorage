using System.IO;
using SingleFileStorage.Infrastructure;

namespace SingleFileStorage.Core
{
    static class SegmentPositionIterator
    {
        public static Segment IterateAndGetLastSegment(StorageFileStream storageFileStream, SegmentBuffer segmentBuffer, Segment startSegment, long position)
        {
            long remainingBytes = position;
            var segment = startSegment;
            long storageFileStreamPosition = storageFileStream.Position;
            while (remainingBytes > 0)
            {
                if (remainingBytes <= segment.EndPosition - storageFileStreamPosition)
                {
                    storageFileStream.Seek(storageFileStreamPosition + (int)remainingBytes, SeekOrigin.Begin);
                    break;
                }
                else
                {
                    remainingBytes -= (int)(segment.EndPosition - storageFileStreamPosition);
                    var nextSegment = SegmentIterator.GetNextSegment(storageFileStream, segmentBuffer, segment);
                    if (nextSegment == null) break;
                    segment = nextSegment;
                    storageFileStreamPosition = segment.DataStartPosition;
                }
            }

            return segment;
        }
    }
}
