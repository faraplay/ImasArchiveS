using System;

namespace Imas.Gtf
{
    public partial class Font
    {
        private class CharData : IComparable<CharData>
        {
            public ushort key;
            public byte datawidth;
            public byte dataheight;
            public short datax;
            public short datay;
            public short offsetx;
            public short offsety;
            public short width;
            public short blank;

            public int index;
            public int left;
            public int right;

            public ushort isEmoji;

            public int CompareTo(CharData other) => key.CompareTo(other.key);
        }
    }

}