using System;
using System.Collections.Generic;
using System.Text;

namespace SingleFileStorage.Core
{
    static class SegmentState
    {
        public const byte Free = 0;

        public const byte Chained = 1;

        public const byte Last = 2;
    }
}
