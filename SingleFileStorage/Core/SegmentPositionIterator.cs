﻿using System.IO;
using SingleFileStorage.Infrastructure;

namespace SingleFileStorage.Core;

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
                if (segment.State == SegmentState.Last) break;
                remainingBytes -= (int)(segment.EndPosition - storageFileStreamPosition);
                var nextSegment = segment.NextSegment ?? SegmentIterator.GetNextSegment(storageFileStream, segmentBuffer, segment);
                if (nextSegment is null) break;
                segment = nextSegment;
                storageFileStreamPosition = segment.DataStartPosition;
            }
        }

        return segment;
    }
}
