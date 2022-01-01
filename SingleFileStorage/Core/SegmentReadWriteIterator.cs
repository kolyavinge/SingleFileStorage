using System;

namespace SingleFileStorage.Core
{
    class SegmentReadWriteIterator
    {
        public delegate void IterationDelegate(Segment currentSegment, int segmentAvailableBytes, long totalIteratedBytes);

        private readonly IStorageFileStream _storageFileStream;
        private readonly Segment _startSegment;
        private readonly long _bytesCount;

        private Segment _lastIteratedSegment;
        public Segment LastIteratedSegment
        {
            get
            {
                if (_lastIteratedSegment == null) throw new InvalidOperationException("Iteration has not been finished");
                return _lastIteratedSegment;
            }
        }

        public long RemainingBytes { get; private set; }

        public long TotalIteratedBytes { get; private set; }

        public SegmentReadWriteIterator(IStorageFileStream storageFileStream, Segment startSegment, long bytesCount)
        {
            _storageFileStream = storageFileStream;
            _startSegment = startSegment;
            _bytesCount = bytesCount;
        }

        public void Iterate(IterationDelegate iterationFunc)
        {
            var iterator = new SegmentIterator(_storageFileStream, _startSegment);
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
            _lastIteratedSegment = iterator.Current;
        }
    }
}
