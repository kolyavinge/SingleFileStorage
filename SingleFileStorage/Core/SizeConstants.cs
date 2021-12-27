using System;
using System.Collections.Generic;
using System.Text;

namespace SingleFileStorage.Core
{
    internal static class SizeConstants
    {
        public const int RecordState = 1;

        public const int RecordName = 256;

        public const int RecordFirstSegmentIndex = 4;

        public const int RecordLastSegmentIndex = 4;

        public const int RecordLength = 4;

        public const int RecordDescription = RecordState + RecordName + RecordFirstSegmentIndex + RecordLastSegmentIndex + RecordLength;

        public const int MaxRecordsCount = 1000;

        public const int StorageDescription = RecordDescription * MaxRecordsCount;

        public const int SegmentState = 1;

        public const int SegmentNextIndexOrDataLength = 4;

        public const int SegmentData = 8 * 1024;

        public const int Segment = SegmentState + SegmentNextIndexOrDataLength + SegmentData;
    }
}
