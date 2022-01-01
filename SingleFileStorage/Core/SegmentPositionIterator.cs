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
            var iterator = new SegmentIterator(_storageFileStream, _startSegment);
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
            _lastIteratedSegment = iterator.Current;
        }
    }
}
