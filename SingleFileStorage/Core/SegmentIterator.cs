using System.IO;

namespace SingleFileStorage.Core
{
    internal class SegmentIterator
    {
        private readonly IStorageFileStream _storageFileStream;

        public Segment Current { get; private set; }

        public SegmentIterator(IStorageFileStream storageFileStream, Segment startSegment)
        {
            _storageFileStream = storageFileStream;
            Current = startSegment;
        }

        public bool MoveNext()
        {
            if (!SegmentState.IsLast(Current.State))
            {
                var nextSegment = Segment.GotoSegmentStartPositionAndCreate(_storageFileStream, Current.NextSegmentIndex);
                _storageFileStream.Seek(nextSegment.DataStartPosition, SeekOrigin.Begin);
                Current = nextSegment;

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
