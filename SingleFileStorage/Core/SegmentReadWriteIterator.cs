using System;
using System.IO;
using SingleFileStorage.Infrastructure;

namespace SingleFileStorage.Core;

class SegmentReadWriteIterator
{
    public delegate int IterationDelegate(Segment currentSegment, int readAvailableBytes, int writeAvailableBytes, long totalIteratedBytes);

    private readonly StorageFileStream _storageFileStream;
    private readonly SegmentBuffer _segmentBuffer;

    public Segment LastIteratedSegment;
    public long RemainingBytes;
    public long TotalIteratedBytes;

    public SegmentReadWriteIterator(StorageFileStream storageFileStream, SegmentBuffer segmentBuffer)
    {
        _storageFileStream = storageFileStream;
        _segmentBuffer = segmentBuffer;
    }

    public void Iterate(Segment startSegment, long bytesCount, IterationDelegate iterationFunc)
    {
        TotalIteratedBytes = 0;
        RemainingBytes = bytesCount;
        var segment = startSegment;
        while (RemainingBytes > 0)
        {
            if (RemainingBytes <= segment.EndPosition - _storageFileStream.Position)
            {
                int readAvailableBytes = (int)Math.Min(segment.DataStartPosition + segment.DataLength - _storageFileStream.Position, RemainingBytes);
                int writeAvailableBytes = (int)RemainingBytes;
                int iteratedBytes = iterationFunc(segment, readAvailableBytes, writeAvailableBytes, TotalIteratedBytes);
                if (iteratedBytes == 0) break;
                RemainingBytes -= iteratedBytes;
                TotalIteratedBytes += iteratedBytes;
            }
            else
            {
                int readAvailableBytes = (int)Math.Min(segment.DataStartPosition + segment.DataLength - _storageFileStream.Position, RemainingBytes);
                int writeAvailableBytes = (int)(segment.EndPosition - _storageFileStream.Position);
                int iteratedBytes = iterationFunc(segment, readAvailableBytes, writeAvailableBytes, TotalIteratedBytes);
                RemainingBytes -= iteratedBytes;
                TotalIteratedBytes += iteratedBytes;
                if (segment.State == SegmentState.Last) break;
                var nextSegment = segment.NextSegment ?? SegmentIterator.GetNextSegment(_storageFileStream, _segmentBuffer, segment);
                if (nextSegment == null) break;
                segment = nextSegment;
                _storageFileStream.Seek(segment.DataStartPosition, SeekOrigin.Begin);
            }
        }
        LastIteratedSegment = segment;
    }
}
