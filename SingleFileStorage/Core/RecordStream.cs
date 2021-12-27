using System;
using System.IO;

namespace SingleFileStorage.Core
{
    internal class RecordStream : Stream
    {
        private readonly IStorageFileStream _storageFileStream;
        private readonly RecordDescription _recordDescription;
        private readonly Segment _firstSegment;
        private Segment _currentSegment;
        private long _position;

        public override bool CanRead => true;

        public override bool CanWrite { get; }

        public override bool CanSeek => true;

        public override long Length => throw new NotImplementedException();

        public override long Position
        {
            get => _position;
            set => Seek(value, SeekOrigin.Begin);
        }

        public RecordStream(IStorageFileStream storageFileStream, RecordAccess access, RecordDescription recordDescription)
        {
            _storageFileStream = storageFileStream ?? throw new ArgumentNullException(nameof(storageFileStream));
            _recordDescription = recordDescription;
            CanWrite = access == RecordAccess.ReadWrite;
            var firstSegmentStartPosition = Segment.GetSegmentStartPosition(_storageFileStream, _recordDescription.FirstSegmentIndex);
            _storageFileStream.Seek(firstSegmentStartPosition, SeekOrigin.Begin);
            _firstSegment = Segment.CreateFromSegmentIndex(_storageFileStream, _recordDescription.FirstSegmentIndex);
            _storageFileStream.Seek(_firstSegment.DataStartPosition, SeekOrigin.Begin);
            _currentSegment = _firstSegment;
            _position = 0;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalReaded = 0;
            var segmentIterator = new SegmentIterator(_storageFileStream, _currentSegment, count);
            segmentIterator.Iterate((currentSegment, segmentAvailableBytes, totalIteratedBytes) =>
            {
                int maxBytesToRead = (int)Math.Min(currentSegment.DataLength, segmentAvailableBytes);
                totalReaded += _storageFileStream.ReadByteArray(buffer, offset + (int)totalIteratedBytes, maxBytesToRead);
            });
            _currentSegment = segmentIterator.LastIteratedSegment;
            _position += totalReaded;

            return totalReaded;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var segmentIterator = new SegmentIterator(_storageFileStream, _currentSegment, count);
            segmentIterator.Iterate((currentSegment, segmentAvailableBytes, totalIteratedBytes) =>
            {
                _storageFileStream.WriteByteArray(buffer, offset + (int)totalIteratedBytes, segmentAvailableBytes);
            });
            _position += segmentIterator.TotalIteratedBytes + segmentIterator.RemainingBytes;
            _currentSegment = segmentIterator.LastIteratedSegment;
            if (segmentIterator.RemainingBytes == 0)
            {
                var currentStorageFileStreamPosition = _storageFileStream.Position;
                _currentSegment.DataLength = (uint)(_storageFileStream.Position - _currentSegment.DataStartPosition);
                _storageFileStream.Seek(_currentSegment.StartPosition + SizeConstants.SegmentState, SeekOrigin.Begin);
                Segment.WriteNextSegmentIndexOrDataLength(_storageFileStream, _currentSegment.DataLength);
                _storageFileStream.Seek(currentStorageFileStreamPosition, SeekOrigin.Begin);
            }
            else
            {
                uint lastSegmentIndex = Segment.GetSegmentsCount(_storageFileStream.Length) - 1;
                SegmentState.SetChained(ref _currentSegment.State);
                _currentSegment.DataLength = SizeConstants.SegmentData;
                _currentSegment.NextSegmentIndex = lastSegmentIndex + 1;
                _storageFileStream.Seek(_currentSegment.StartPosition, SeekOrigin.Begin);
                Segment.WriteState(_storageFileStream, _currentSegment.State);
                Segment.WriteNextSegmentIndexOrDataLength(_storageFileStream, _currentSegment.NextSegmentIndex);
                _storageFileStream.Seek(0, SeekOrigin.End);
                int currentOffset = offset + (int)segmentIterator.TotalIteratedBytes;
                long remainingBytes = segmentIterator.RemainingBytes;
                while (remainingBytes > SizeConstants.SegmentData)
                {
                    lastSegmentIndex++;
                    Segment.AppendSegment(_storageFileStream, SegmentState.UsedAndChained, lastSegmentIndex + 1, buffer, currentOffset, SizeConstants.SegmentData);
                    currentOffset += SizeConstants.SegmentData;
                    remainingBytes -= SizeConstants.SegmentData;
                }
                lastSegmentIndex++;
                Segment.AppendSegment(_storageFileStream, SegmentState.UsedAndLast, (uint)remainingBytes, buffer, currentOffset, (int)remainingBytes);
                _storageFileStream.Seek(-SizeConstants.Segment, SeekOrigin.End);
                _currentSegment = Segment.CreateFromSegmentIndex(_storageFileStream, lastSegmentIndex);
                _storageFileStream.Seek((int)remainingBytes, SeekOrigin.Current);
            }
            UpdateRecordDescription();
        }

        private void UpdateRecordDescription()
        {
            var currentStorageFileStreamPosition = _storageFileStream.Position;
            _storageFileStream.Seek(_recordDescription.RecordLengthStartPosition, SeekOrigin.Begin);
            RecordDescription.WriteLastSegmentIndex(_storageFileStream, _currentSegment.Index);
            RecordDescription.WriteLength(_storageFileStream, _currentSegment.DataLength);
            _storageFileStream.Seek(currentStorageFileStreamPosition, SeekOrigin.Begin);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
            {
                _currentSegment = _firstSegment;
                _storageFileStream.Seek(_currentSegment.DataStartPosition, SeekOrigin.Begin);
                long lastSegmentAvailableBytes = 0;
                var segmentIterator = new SegmentIterator(_storageFileStream, _currentSegment, offset);
                segmentIterator.Iterate((currentSegment, segmentAvailableBytes, totalIteratedBytes) =>
                {
                    lastSegmentAvailableBytes = segmentAvailableBytes;
                });
                if (lastSegmentAvailableBytes > 0)
                {
                    _storageFileStream.Seek(lastSegmentAvailableBytes, SeekOrigin.Current);
                }
                _position = offset;
            }
            else if (origin == SeekOrigin.Current && offset > 0)
            {
                _position += offset;
            }
            else if (origin == SeekOrigin.Current && offset < 0)
            {
                _position += offset;
            }
            else if (origin == SeekOrigin.End)
            {
                _position = Length - offset;
            }
            else throw new ArgumentException(nameof(origin));

            return _position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
            _storageFileStream.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            _storageFileStream.EndReadWrite();
            base.Dispose(disposing);
        }

        public override void Close()
        {
            _storageFileStream.EndReadWrite();
            base.Close();
        }
    }
}
