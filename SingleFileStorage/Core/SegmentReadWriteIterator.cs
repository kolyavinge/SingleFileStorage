﻿using System;
using System.IO;

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
            TotalIteratedBytes = 0;
            RemainingBytes = _bytesCount;
            var currentSegment = _startSegment;
            while (RemainingBytes > 0)
            {
                int segmentAvailableBytes;
                if (RemainingBytes <= currentSegment.DataEndPosition - _storageFileStream.Position)
                {
                    segmentAvailableBytes = (int)RemainingBytes;
                    RemainingBytes -= segmentAvailableBytes;
                    iterationFunc(currentSegment, segmentAvailableBytes, TotalIteratedBytes);
                    TotalIteratedBytes += segmentAvailableBytes;
                }
                else
                {
                    segmentAvailableBytes = (int)(currentSegment.DataEndPosition - _storageFileStream.Position);
                    RemainingBytes -= segmentAvailableBytes;
                    iterationFunc(currentSegment, segmentAvailableBytes, TotalIteratedBytes);
                    TotalIteratedBytes += segmentAvailableBytes;
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