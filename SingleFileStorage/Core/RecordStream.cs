using System;
using System.IO;
using System.Linq;
using SingleFileStorage.Infrastructure;

namespace SingleFileStorage.Core
{
    internal class RecordStream : Stream
    {
        private readonly StorageFileStream _storageFileStream;
        private readonly RecordDescription _recordDescription;
        private readonly bool _canModify;
        private readonly SegmentBuffer _segmentBuffer;
        private readonly Segment _firstSegment;
        private Segment _currentSegment;
        private long _position;
        private long _lastStorageFileStreamPosition;
        private readonly SegmentReadWriteIterator _readIterator;
        private readonly SegmentReadWriteIterator _writeIterator;

        public override bool CanRead => true;
        public override bool CanWrite { get; }
        public override bool CanSeek => true;
        public override long Length => _recordDescription.RecordLength;

        public override long Position
        {
            get => _position;
            set => Seek(value, SeekOrigin.Begin);
        }

        public RecordStream(StorageFileStream storageFileStream, RecordDescription recordDescription)
        {
            _storageFileStream = storageFileStream;
            _recordDescription = recordDescription;
            _canModify = storageFileStream.AccessMode == Access.Modify; // для оптимизации ThrowErrorIfNotModified()
            CanWrite = _canModify;
            _firstSegment = Segment.GotoSegmentStartPositionAndCreate(_storageFileStream, _recordDescription.FirstSegmentIndex);
            _lastStorageFileStreamPosition = _firstSegment.DataStartPosition;
            _currentSegment = _firstSegment;
            _segmentBuffer = new SegmentBuffer();
            _segmentBuffer.Add(_firstSegment);
            _readIterator = new SegmentReadWriteIterator(_storageFileStream, _segmentBuffer);
            _writeIterator = new SegmentReadWriteIterator(_storageFileStream, _segmentBuffer);
        }

        public override int ReadByte()
        {
            return base.ReadByte();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_storageFileStream.Position != _lastStorageFileStreamPosition)
            {
                _storageFileStream.Seek(_lastStorageFileStreamPosition, SeekOrigin.Begin);
            }
            int totalReaded = 0;
            _readIterator.Iterate(_currentSegment, count, (segment, segmentAvailableBytes, totalIteratedBytes) =>
            {
                int maxBytesToRead = (int)Math.Min(segment.DataLength, segmentAvailableBytes);
                totalReaded += _storageFileStream.ReadByteArray(buffer, offset + (int)totalIteratedBytes, maxBytesToRead);
            });
            _currentSegment = _readIterator.LastIteratedSegment;
            _position += totalReaded;
            _lastStorageFileStreamPosition = _storageFileStream.Position;

            return totalReaded;
        }

        public override void WriteByte(byte value)
        {
            base.WriteByte(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ThrowErrorIfNotModified();
            if (_storageFileStream.Position != _lastStorageFileStreamPosition)
            {
                _storageFileStream.Seek(_lastStorageFileStreamPosition, SeekOrigin.Begin);
            }
            _writeIterator.Iterate(_currentSegment, count, (segment, segmentAvailableBytes, totalIteratedBytes) =>
            {
                _storageFileStream.WriteByteArray(buffer, offset + (int)totalIteratedBytes, segmentAvailableBytes);
            });
            _currentSegment = _writeIterator.LastIteratedSegment;
            _position += _writeIterator.TotalIteratedBytes + _writeIterator.RemainingBytes;
            if (_writeIterator.RemainingBytes == 0)
            {
                _lastStorageFileStreamPosition = _storageFileStream.Position;
                if (_currentSegment.State == SegmentState.Last)
                {
                    var newDataLength = (uint)(_storageFileStream.Position - _currentSegment.DataStartPosition);
                    if (newDataLength > _currentSegment.DataLength)
                    {
                        _currentSegment.DataLength = newDataLength;
                        _currentSegment.IsModified = true;
                    }
                    if (_position > _recordDescription.RecordLength)
                    {
                        _recordDescription.RecordLength = (uint)_position;
                        _recordDescription.IsModified = true;
                    }
                }
            }
            else
            {
                uint lastSegmentIndex = Segment.GetSegmentsCount(_storageFileStream.Length) - 1;
                _currentSegment.State = SegmentState.Chained;
                _currentSegment.DataLength = SizeConstants.SegmentData;
                _currentSegment.NextSegmentIndex = lastSegmentIndex + 1;
                _currentSegment.IsModified = true;
                _storageFileStream.Seek(0, SeekOrigin.End);
                int currentOffset = offset + (int)_writeIterator.TotalIteratedBytes;
                long remainingBytes = _writeIterator.RemainingBytes;
                while (remainingBytes > SizeConstants.SegmentData)
                {
                    lastSegmentIndex++;
                    Segment.AppendSegment(_storageFileStream, SegmentState.Chained, lastSegmentIndex + 1, buffer, currentOffset, SizeConstants.SegmentData);
                    currentOffset += SizeConstants.SegmentData;
                    remainingBytes -= SizeConstants.SegmentData;
                }
                lastSegmentIndex++;
                Segment.AppendSegment(_storageFileStream, SegmentState.Last, (uint)remainingBytes, buffer, currentOffset, (int)remainingBytes, out _currentSegment);
                _currentSegment.IsModified = true;
                _lastStorageFileStreamPosition = _currentSegment.DataStartPosition + (int)remainingBytes;
                _recordDescription.LastSegmentIndex = _currentSegment.Index;
                _recordDescription.RecordLength = (uint)_position;
                _recordDescription.IsModified = true;
                _segmentBuffer.Add(_currentSegment);
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
                    _currentSegment = SegmentPositionIterator.IterateAndGetLastSegment(_storageFileStream, _segmentBuffer, _currentSegment, newPosition);
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
                    _currentSegment = SegmentPositionIterator.IterateAndGetLastSegment(_storageFileStream, _segmentBuffer, _currentSegment, offset);
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
                    _currentSegment = SegmentPositionIterator.IterateAndGetLastSegment(_storageFileStream, _segmentBuffer, _currentSegment, newPosition);
                }
            }
            else if (origin == SeekOrigin.End)
            {
                if (_currentSegment.State != SegmentState.Last)
                {
                    _currentSegment = _segmentBuffer.GetByIndex(_storageFileStream, _recordDescription.LastSegmentIndex);
                }
                if (_currentSegment.Contains(_currentSegment.DataStartPosition + _currentSegment.DataLength + offset))
                {
                    _storageFileStream.Seek(_currentSegment.DataStartPosition + _currentSegment.DataLength + offset, SeekOrigin.Begin);
                }
                else
                {
                    _currentSegment = _firstSegment;
                    _storageFileStream.Seek(_currentSegment.DataStartPosition, SeekOrigin.Begin);
                    _currentSegment = SegmentPositionIterator.IterateAndGetLastSegment(_storageFileStream, _segmentBuffer, _currentSegment, newPosition);
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
            ThrowErrorIfNotModified();
            if (value < 0) throw new ArgumentException(nameof(value));
            else if (value == Length) return;
            else if (value < Length)
            {
                _lastStorageFileStreamPosition = _storageFileStream.Seek(_firstSegment.DataStartPosition, SeekOrigin.Begin);
                _currentSegment = SegmentPositionIterator.IterateAndGetLastSegment(_storageFileStream, _segmentBuffer, _firstSegment, value);
                SegmentIterator.ForEachExceptFirst(_storageFileStream, _segmentBuffer, _currentSegment, s => Segment.WriteState(_storageFileStream, SegmentState.Free));
                _currentSegment.State = SegmentState.Last;
                _currentSegment.DataLength = (uint)(_storageFileStream.Position - _currentSegment.DataStartPosition);
                _currentSegment.NextSegmentIndex = Segment.NullValue;
                _storageFileStream.Seek(_currentSegment.StartPosition, SeekOrigin.Begin);
                Segment.WriteState(_storageFileStream, _currentSegment.State);
                Segment.WriteNextSegmentIndexOrDataLength(_storageFileStream, _currentSegment.DataLength);
                _recordDescription.LastSegmentIndex = _currentSegment.Index;
                _recordDescription.RecordLength = (uint)value;
                _storageFileStream.Seek(_recordDescription.LastSegmentIndexPosition, SeekOrigin.Begin);
                RecordDescription.WriteLastSegmentIndex(_storageFileStream, _recordDescription.LastSegmentIndex);
                RecordDescription.WriteLength(_storageFileStream, _recordDescription.RecordLength);
            }
            else
            {
                throw new ArgumentException("Hasn't realised yet");
            }
            _currentSegment = _firstSegment;
        }

        public override void Flush() { }

        public override void Close()
        {
            if (_recordDescription.IsModified)
            {
                _storageFileStream.Seek(_recordDescription.LastSegmentIndexPosition, SeekOrigin.Begin);
                RecordDescription.WriteLastSegmentIndex(_storageFileStream, _recordDescription.LastSegmentIndex);
                RecordDescription.WriteLength(_storageFileStream, _recordDescription.RecordLength);
            }

            foreach (var segment in _segmentBuffer.GetAll().Where(x => x.IsModified))
            {
                _storageFileStream.Seek(segment.StartPosition, SeekOrigin.Begin);
                Segment.WriteState(_storageFileStream, segment.State);
                if (segment.State == SegmentState.Last)
                {
                    Segment.WriteNextSegmentIndexOrDataLength(_storageFileStream, segment.DataLength);
                }
                else
                {
                    Segment.WriteNextSegmentIndexOrDataLength(_storageFileStream, segment.NextSegmentIndex);
                }
            }

            base.Close();
        }

        private void ThrowErrorIfNotModified()
        {
            if (!_canModify) throw new InvalidOperationException("Stream cannot be modified.");
        }
    }
}
