using System;
using System.IO;
using SingleFileStorage.Infrastructure;

namespace SingleFileStorage.Core
{
    class SegmentReadWriteIterator
    {
        public delegate void IterationDelegate(Segment currentSegment, int segmentAvailableBytes, long totalIteratedBytes);

        private readonly StorageFileStream _storageFileStream;
        private readonly SegmentBuffer _segmentBuffer;
        private readonly Segment _startSegment;
        private readonly long _bytesCount;

        public Segment LastIteratedSegment;
        public long RemainingBytes;
        public long TotalIteratedBytes;

        public SegmentReadWriteIterator(StorageFileStream storageFileStream, SegmentBuffer segmentBuffer, Segment startSegment, long bytesCount)
        {
            _storageFileStream = storageFileStream;
            _segmentBuffer = segmentBuffer;
            _startSegment = startSegment;
            _bytesCount = bytesCount;
        }

        public void Iterate(IterationDelegate iterationFunc)
        {
            TotalIteratedBytes = 0;
            RemainingBytes = _bytesCount;
            var segment = _startSegment;
            while (RemainingBytes > 0)
            {
                int segmentAvailableBytes;
                if (RemainingBytes <= segment.DataEndPosition - _storageFileStream.Position)
                {
                    segmentAvailableBytes = (int)RemainingBytes;
                    RemainingBytes -= segmentAvailableBytes;
                    iterationFunc(segment, segmentAvailableBytes, TotalIteratedBytes);
                    TotalIteratedBytes += segmentAvailableBytes;
                }
                else
                {
                    segmentAvailableBytes = (int)(segment.DataEndPosition - _storageFileStream.Position);
                    RemainingBytes -= segmentAvailableBytes;
                    iterationFunc(segment, segmentAvailableBytes, TotalIteratedBytes);
                    TotalIteratedBytes += segmentAvailableBytes;
                    var nextSegment = SegmentIterator.GetNextSegment(_storageFileStream, _segmentBuffer, segment);
                    if (nextSegment == null) break;
                    segment = nextSegment;
                    _storageFileStream.Seek(segment.DataStartPosition, SeekOrigin.Begin);
                }
            }
            LastIteratedSegment = segment;
        }
    }
}
