using System;
using System.IO;

namespace SingleFileStorage.Core
{
    internal class SegmentPositionIterator
    {
        public static Segment IterateAndGetLastSegment(IStorageFileStream storageFileStream, Segment startSegment, long position)
        {
            var iterator = new SegmentPositionIterator(storageFileStream, startSegment, position);
            iterator.Iterate();
            return iterator.LastIteratedSegment;
        }

        private readonly IStorageFileStream _storageFileStream;
        private readonly Segment _startSegment;
        private readonly long _position;

        private Segment _lastIteratedSegment;
        public Segment LastIteratedSegment
        {
            get
            {
                if (_lastIteratedSegment == null) throw new InvalidOperationException("Iteration has not been finished");
                return _lastIteratedSegment;
            }
        }

        public SegmentPositionIterator(IStorageFileStream storageFileStream, Segment startSegment, long position)
        {
            _storageFileStream = storageFileStream;
            _startSegment = startSegment;
            _position = position;
        }

        public void Iterate()
        {
            long remainingBytes = _position;
            var currentSegment = _startSegment;
            while (remainingBytes > 0)
            {
                if (remainingBytes <= currentSegment.DataEndPosition - _storageFileStream.Position)
                {
                    _storageFileStream.Seek((int)remainingBytes, SeekOrigin.Current);
                    break;
                }
                else
                {
                    remainingBytes -= (int)(currentSegment.DataEndPosition - _storageFileStream.Position);
                    if (currentSegment.NextSegmentIndex != Segment.NullValue)
                    {
                        var nextSegmentStartPosition = Segment.GetSegmentStartPosition(currentSegment.NextSegmentIndex);
                        _storageFileStream.Seek(nextSegmentStartPosition, SeekOrigin.Begin);
                        var nextSegment = Segment.CreateFromCurrentPosition(_storageFileStream);
                        _storageFileStream.Seek(nextSegment.DataStartPosition, SeekOrigin.Begin);
                        currentSegment = nextSegment;
                    }
                    else break;
                }
            }
            _lastIteratedSegment = currentSegment;
        }
    }
}
