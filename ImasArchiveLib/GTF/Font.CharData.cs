using System;
using System.Drawing;

namespace Imas.Gtf
{
    public partial class Font
    {
        private class CharData : IComparable<CharData>, IDisposable
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

            public Bitmap bitmap;

            public int CompareTo(CharData other) => key.CompareTo(other.key);

            #region IDisposable

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    bitmap?.Dispose();
                    bitmap = null;
                }
            }

            ~CharData()
            {
                Dispose(false);
            }

            #endregion IDisposable
        }
    }

}