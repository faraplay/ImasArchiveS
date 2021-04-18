using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Imas.Gtf
{
    public partial class GTF : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        private readonly IntPtr bitmapDataPtr;
        private readonly int[] pixelData;
        public int Type { get; }
        public int Width { get; }
        public int Height { get; }
        public int Stride { get; }

        public IntPtr BitmapDataPtr => bitmapDataPtr;

        public int[] PixelData => pixelData;

        private GTF(int[] pixelData, int type, int width, int height, int stride)
        {
            bitmapDataPtr = Marshal.AllocHGlobal(4 * stride * height);
            Bitmap = new Bitmap(width, height, 4 * stride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, bitmapDataPtr);
            this.pixelData = pixelData;
            Type = type;
            Width = width;
            Height = height;
            Stride = stride;
        }

        #region IDisposable

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Bitmap?.Dispose();
            }
            Marshal.FreeHGlobal(bitmapDataPtr);
            disposed = true;
        }

        ~GTF()
        {
            Dispose(false);
        }

        #endregion IDisposable

        private static bool IsPow2(int n) => (n & (n - 1)) == 0;
    }
}