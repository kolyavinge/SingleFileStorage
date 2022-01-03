using System;
using System.Collections.Generic;
using System.Text;
using SingleFileStorage.Infrastructure;

namespace SingleFileStorage.Core
{
    class SegmentBuffer
    {
        private Dictionary<uint, Segment> _segments;

        public SegmentBuffer()
        {
            _segments = new Dictionary<uint, Segment>();
        }

        public Segment GetByIndex(StorageFileStream storageFileStream, uint segmentIndex)
        {
            if (_segments.ContainsKey(segmentIndex))
            {
                return _segments[segmentIndex];
            }
            else
            {
                var segment = Segment.GotoSegmentStartPositionAndCreate(storageFileStream, segmentIndex);
                _segments.Add(segmentIndex, segment);

                return segment;
            }
        }

        public IEnumerable<Segment> GetAll()
        {
            return _segments.Values;
        }

        public void Add(Segment segment)
        {
            _segments.Add(segment.Index, segment);
        }
    }
}
