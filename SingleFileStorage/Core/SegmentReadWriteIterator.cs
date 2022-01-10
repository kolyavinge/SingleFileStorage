using System.IO;
using SingleFileStorage.Infrastructure;

namespace SingleFileStorage.Core
{
    class SegmentReadWriteIterator
    {
        public delegate void IterationDelegate(Segment currentSegment, int segmentAvailableBytes, long totalIteratedBytes);

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
                int segmentAvailableBytes;
                if (RemainingBytes <= segment.EndPosition - _storageFileStream.Position)
                {
                    segmentAvailableBytes = (int)RemainingBytes;
                    RemainingBytes -= segmentAvailableBytes;
                    iterationFunc(segment, segmentAvailableBytes, TotalIteratedBytes);
                    TotalIteratedBytes += segmentAvailableBytes;
                }
                else
                {
                    segmentAvailableBytes = (int)(segment.EndPosition - _storageFileStream.Position);
                    RemainingBytes -= segmentAvailableBytes;
                    iterationFunc(segment, segmentAvailableBytes, TotalIteratedBytes);
                    TotalIteratedBytes += segmentAvailableBytes;
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
}
