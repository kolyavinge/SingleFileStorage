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
        private long _lastStorageFileStreamPosition;

        public override bool CanRead => true;

        public override bool CanWrite { get; }

        public override bool CanSeek => true;

        public override long Length => _recordDescription.RecordLength;

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
            var firstSegmentStartPosition = Segment.GetSegmentStartPosition(_recordDescription.FirstSegmentIndex);
            _storageFileStream.Seek(firstSegmentStartPosition, SeekOrigin.Begin);
            _firstSegment = Segment.CreateFromCurrentPosition(_storageFileStream);
            _lastStorageFileStreamPosition = _firstSegment.DataStartPosition;
            _currentSegment = _firstSegment;
            _position = 0;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            _storageFileStream.Seek(_lastStorageFileStreamPosition, SeekOrigin.Begin);
            int totalReaded = 0;
            var iterator = new SegmentReadWriteIterator(_storageFileStream, _currentSegment, count);
            iterator.Iterate((currentSegment, segmentAvailableBytes, totalIteratedBytes) =>
            {
                int maxBytesToRead = (int)Math.Min(currentSegment.DataLength, segmentAvailableBytes);
                totalReaded += _storageFileStream.ReadByteArray(buffer, offset + (int)totalIteratedBytes, maxBytesToRead);
            });
            _currentSegment = iterator.LastIteratedSegment;
            _position += totalReaded;
            _lastStorageFileStreamPosition = _storageFileStream.Position;

            return totalReaded;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _storageFileStream.Seek(_lastStorageFileStreamPosition, SeekOrigin.Begin);
            var iterator = new SegmentReadWriteIterator(_storageFileStream, _currentSegment, count);
            iterator.Iterate((currentSegment, segmentAvailableBytes, totalIteratedBytes) =>
            {
                _storageFileStream.WriteByteArray(buffer, offset + (int)totalIteratedBytes, segmentAvailableBytes);
            });
            _currentSegment = iterator.LastIteratedSegment;
            _position += iterator.TotalIteratedBytes + iterator.RemainingBytes;
            if (iterator.RemainingBytes == 0)
            {
                _lastStorageFileStreamPosition = _storageFileStream.Position;
                if (SegmentState.IsLast(_currentSegment.State))
                {
                    var newDataLength = (uint)(_storageFileStream.Position - _currentSegment.DataStartPosition);
                    if (newDataLength > _currentSegment.DataLength)
                    {
                        _currentSegment.DataLength = newDataLength;
                        _storageFileStream.Seek(_currentSegment.StartPosition + SizeConstants.SegmentState, SeekOrigin.Begin);
                        Segment.WriteNextSegmentIndexOrDataLength(_storageFileStream, _currentSegment.DataLength);
                    }
                    if (_position > _recordDescription.RecordLength)
                    {
                        _recordDescription.RecordLength = (uint)_position;
                        _storageFileStream.Seek(_recordDescription.RecordLengthStartPosition, SeekOrigin.Begin);
                        RecordDescription.WriteLength(_storageFileStream, _recordDescription.RecordLength);
                    }
                }
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
                int currentOffset = offset + (int)iterator.TotalIteratedBytes;
                long remainingBytes = iterator.RemainingBytes;
                while (remainingBytes > SizeConstants.SegmentData)
                {
                    lastSegmentIndex++;
                    Segment.AppendSegment(_storageFileStream, SegmentState.UsedAndChained, lastSegmentIndex + 1, buffer, currentOffset, SizeConstants.SegmentData);
                    currentOffset += SizeConstants.SegmentData;
                    remainingBytes -= SizeConstants.SegmentData;
                }
                lastSegmentIndex++;
                Segment.AppendSegment(_storageFileStream, SegmentState.UsedAndLast, (uint)remainingBytes, buffer, currentOffset, (int)remainingBytes, out _currentSegment);
                _lastStorageFileStreamPosition = _currentSegment.DataStartPosition + (int)remainingBytes;
                _recordDescription.LastSegmentIndex = _currentSegment.Index;
                _recordDescription.RecordLength = (uint)_position;
                _storageFileStream.Seek(_recordDescription.LastSegmentIndexPosition, SeekOrigin.Begin);
                RecordDescription.WriteLastSegmentIndex(_storageFileStream, _recordDescription.LastSegmentIndex);
                RecordDescription.WriteLength(_storageFileStream, _recordDescription.RecordLength);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long newPosition = GetNewSeekPosition(offset, origin);
            ThrowErrorIfPositionIsInvalid(newPosition);
            if (origin == SeekOrigin.Begin)
            {
                _currentSegment = _firstSegment;
                if (_currentSegment.Contains(_currentSegment.DataStartPosition + offset))
                {
                    _storageFileStream.Seek(_currentSegment.DataStartPosition + offset, SeekOrigin.Begin);
                }
                else
                {
                    _storageFileStream.Seek(_currentSegment.DataStartPosition, SeekOrigin.Begin);
                    _currentSegment = SegmentPositionIterator.IterateAndGetLastSegment(_storageFileStream, _currentSegment, newPosition);
                }
            }
            else if (origin == SeekOrigin.Current && offset > 0)
            {
                _storageFileStream.Seek(_lastStorageFileStreamPosition, SeekOrigin.Begin);
                if (_currentSegment.Contains(_storageFileStream.Position + offset))
                {
                    _storageFileStream.Seek(offset, SeekOrigin.Current);
                }
                else
                {
                    _currentSegment = SegmentPositionIterator.IterateAndGetLastSegment(_storageFileStream, _currentSegment, offset);
                }
            }
            else if (origin == SeekOrigin.Current && offset < 0)
            {
                _storageFileStream.Seek(_lastStorageFileStreamPosition, SeekOrigin.Begin);
                if (_currentSegment.Contains(_storageFileStream.Position + offset))
                {
                    _storageFileStream.Seek(offset, SeekOrigin.Current);
                }
                else
                {
                    _currentSegment = _firstSegment;
                    _storageFileStream.Seek(_currentSegment.DataStartPosition, SeekOrigin.Begin);
                    _currentSegment = SegmentPositionIterator.IterateAndGetLastSegment(_storageFileStream, _currentSegment, newPosition);
                }
            }
            else if (origin == SeekOrigin.End)
            {
                if (!SegmentState.IsLast(_currentSegment.State))
                {
                    var lastSegmentStartPosition = Segment.GetSegmentStartPosition(_recordDescription.LastSegmentIndex);
                    _storageFileStream.Seek(lastSegmentStartPosition, SeekOrigin.Begin);
                    _currentSegment = Segment.CreateFromCurrentPosition(_storageFileStream);
                }
                if (_currentSegment.Contains(_currentSegment.DataStartPosition + _currentSegment.DataLength + offset))
                {
                    _storageFileStream.Seek(offset, SeekOrigin.End);
                }
                else
                {
                    _currentSegment = _firstSegment;
                    _storageFileStream.Seek(_currentSegment.DataStartPosition, SeekOrigin.Begin);
                    _currentSegment = SegmentPositionIterator.IterateAndGetLastSegment(_storageFileStream, _currentSegment, newPosition);
                }
            }
            else throw new ArgumentException(nameof(origin));

            _lastStorageFileStreamPosition = _storageFileStream.Position;
            _position = newPosition;

            return _position;
        }

        private long GetNewSeekPosition(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
            {
                return offset;
            }
            else if (origin == SeekOrigin.Current && offset > 0)
            {
                return _position + offset;
            }
            else if (origin == SeekOrigin.Current && offset < 0)
            {
                return _position + offset;
            }
            else if (origin == SeekOrigin.End)
            {
                return Length + offset; // offset is negative
            }
            else throw new ArgumentException(nameof(origin));
        }

        private void ThrowErrorIfPositionIsInvalid(long position)
        {
            if (position < 0 || position > Length)
            {
                throw new IOException("Seek position is invalid");
            }
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
