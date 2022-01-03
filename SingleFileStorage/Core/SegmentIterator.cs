using System;
using System.IO;
using SingleFileStorage.Infrastructure;

namespace SingleFileStorage.Core
{
    internal class SegmentIterator
    {
        private readonly StorageFileStream _storageFileStream;
        private readonly SegmentBuffer _segmentBuffer;

        public Segment Current;

        public SegmentIterator(StorageFileStream storageFileStream, SegmentBuffer segmentBuffer, Segment startSegment)
        {
            _storageFileStream = storageFileStream;
            _segmentBuffer = segmentBuffer;
            Current = startSegment;
        }

        public bool MoveNext()
        {
            if (!SegmentState.IsLast(Current.State))
            {
                var nextSegment = _segmentBuffer.GetByIndex(_storageFileStream, Current.NextSegmentIndex);
                _storageFileStream.Seek(nextSegment.DataStartPosition, SeekOrigin.Begin);
                Current = nextSegment;

                return true;
            }
            else
            {
                return false;
            }
        }

        public void ForEach(Action<Segment> action)
        {
            action(Current);
            while (MoveNext()) action(Current);
        }

        public void ForEachExceptFirst(Action<Segment> action)
        {
            while (MoveNext()) action(Current);
        }
    }
}
