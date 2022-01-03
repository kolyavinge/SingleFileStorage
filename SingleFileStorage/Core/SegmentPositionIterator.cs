using System;
using System.IO;
using SingleFileStorage.Infrastructure;

namespace SingleFileStorage.Core
{
    internal class SegmentPositionIterator
    {
        public static Segment IterateAndGetLastSegment(StorageFileStream storageFileStream, SegmentBuffer segmentBuffer, Segment startSegment, long position)
        {
            var iterator = new SegmentPositionIterator(storageFileStream, segmentBuffer, startSegment, position);
            iterator.Iterate();
            return iterator.LastIteratedSegment;
        }

        private readonly StorageFileStream _storageFileStream;
        private readonly SegmentBuffer _segmentBuffer;
        private readonly Segment _startSegment;
        private readonly long _position;

        public Segment LastIteratedSegment;

        public SegmentPositionIterator(StorageFileStream storageFileStream, SegmentBuffer segmentBuffer, Segment startSegment, long position)
        {
            _storageFileStream = storageFileStream;
            _segmentBuffer = segmentBuffer;
            _startSegment = startSegment;
            _position = position;
        }

        public void Iterate()
        {
            var iterator = new SegmentIterator(_storageFileStream, _segmentBuffer, _startSegment);
            long remainingBytes = _position;
            while (remainingBytes > 0)
            {
                if (remainingBytes <= iterator.Current.DataEndPosition - _storageFileStream.Position)
                {
                    _storageFileStream.Seek((int)remainingBytes, SeekOrigin.Current);
                    break;
                }
                else
                {
                    remainingBytes -= (int)(iterator.Current.DataEndPosition - _storageFileStream.Position);
                    if (!iterator.MoveNext()) break;
                }
            }
            LastIteratedSegment = iterator.Current;
        }
    }
}
