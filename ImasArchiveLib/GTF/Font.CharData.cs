using System;

namespace Imas.Gtf
{
    public partial class Font
    {
        private class CharData : IComparable<CharData>
        {
            public ushort key;
            public byte paddedBbWidth;
            public byte paddedBbHeight;
            public short dataX;
            public short dataY;
            public short bearingX;
            public short bearingY;
            public short advance;
            public short blank;

            public int index;
            public int left;
            public int right;

            public ushort isEmoji;

            public int CompareTo(CharData other) => key.CompareTo(other.key);
        }
    }

}