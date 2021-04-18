using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Imas.Gtf
{
    public partial class GTF : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        private readonly IntPtr pixelDataPtr;
        private readonly int[] pixelDataArray;
        public int Type { get; }
        public int Width { get; }
        public int Height { get; }
        public int Stride { get; }

        public IntPtr BitmapPtr => pixelDataPtr;

        public int[] BitmapArray => pixelDataArray;

        private GTF(Bitmap bitmap, IntPtr bitmapPtr, int[] bitmapArray, int type, int width, int height, int stride)
        {
            Bitmap = bitmap;
            this.pixelDataPtr = bitmapPtr;
            this.pixelDataArray = bitmapArray;
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
            Marshal.FreeHGlobal(pixelDataPtr);
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