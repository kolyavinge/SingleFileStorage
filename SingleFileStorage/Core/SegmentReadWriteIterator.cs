using System;
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
            var iterator = new SegmentIterator(_storageFileStream, _segmentBuffer, _startSegment);
            TotalIteratedBytes = 0;
            RemainingBytes = _bytesCount;
            while (RemainingBytes > 0)
            {
                int segmentAvailableBytes;
                if (RemainingBytes <= iterator.Current.DataEndPosition - _storageFileStream.Position)
                {
                    segmentAvailableBytes = (int)RemainingBytes;
                    RemainingBytes -= segmentAvailableBytes;
                    iterationFunc(iterator.Current, segmentAvailableBytes, TotalIteratedBytes);
                    TotalIteratedBytes += segmentAvailableBytes;
                }
                else
                {
                    segmentAvailableBytes = (int)(iterator.Current.DataEndPosition - _storageFileStream.Position);
                    RemainingBytes -= segmentAvailableBytes;
                    iterationFunc(iterator.Current, segmentAvailableBytes, TotalIteratedBytes);
                    TotalIteratedBytes += segmentAvailableBytes;
                    if (!iterator.MoveNext()) break;
                }
            }
            LastIteratedSegment = iterator.Current;
        }
    }
}
