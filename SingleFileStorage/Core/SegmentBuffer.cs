﻿using System.Collections.Generic;
using SingleFileStorage.Infrastructure;

namespace SingleFileStorage.Core;

class SegmentBuffer
{
    private readonly Dictionary<uint, Segment> _segments;

    public SegmentBuffer()
    {
        _segments = new Dictionary<uint, Segment>();
    }

    public Segment GetByIndex(StorageFileStream storageFileStream, uint segmentIndex)
    {
        if (_segments.TryGetValue(segmentIndex, out var segment))
        {
            return segment;
        }
        else
        {
            segment = Segment.GotoSegmentStartPositionAndCreate(storageFileStream, segmentIndex);
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
