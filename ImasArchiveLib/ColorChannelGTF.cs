using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Imas
{
    public class ColorChannelGTF : IDisposable
    {
        public GTF Gtf { get; } // ColorChannelGTF does not own this gtf

        private readonly IntPtr[] intPtrs = new IntPtr[7];

        public IntPtr PtrWhite
        {
            get
            {
                if (intPtrs[0] == IntPtr.Zero)
                {
                    intPtrs[0] = CreatePixelData(0x00FFFFFF);
                }
                return intPtrs[0];
            }
        }
        public IntPtr PtrYellow
        {
            get
            {
                if (intPtrs[1] == IntPtr.Zero)
                {
                    intPtrs[1] = CreatePixelData(0x00FFFF00);
                }
                return intPtrs[1];
            }
        }
        public IntPtr PtrMagenta
        {
            get
            {
                if (intPtrs[2] == IntPtr.Zero)
                {
                    intPtrs[2] = CreatePixelData(0x00FF00FF);
                }
                return intPtrs[2];
            }
        }
        public IntPtr PtrRed
        {
            get
            {
                if (intPtrs[3] == IntPtr.Zero)
                {
                    intPtrs[3] = CreatePixelData(0x00FF0000);
                }
                return intPtrs[3];
            }
        }
        public IntPtr PtrCyan
        {
            get
            {
                if (intPtrs[4] == IntPtr.Zero)
                {
                    intPtrs[4] = CreatePixelData(0x0000FFFF);
                }
                return intPtrs[4];
            }
        }
        public IntPtr PtrGreen
        {
            get
            {
                if (intPtrs[5] == IntPtr.Zero)
                {
                    intPtrs[5] = CreatePixelData(0x0000FF00);
                }
                return intPtrs[5];
            }
        }
        public IntPtr PtrBlue
        {
            get
            {
                if (intPtrs[6] == IntPtr.Zero)
                {
                    intPtrs[6] = CreatePixelData(0x000000FF);
                }
                return intPtrs[6];
            }
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

            foreach (IntPtr ptr in intPtrs)
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
            disposed = true;
        }

        ~ColorChannelGTF()
        {
            Dispose(false);
        }

        #endregion IDisposable

        public ColorChannelGTF(GTF gtf)
        {
            Gtf = gtf;
        }

        private IntPtr CreatePixelData(uint pixelMask)
        {
            int[] newData = (int[])Gtf.BitmapArray.Clone();
            for (int i = 0; i < newData.Length; i++)
            {
                newData[i] = (int)((newData[i] & pixelMask) | 0xFF000000);
            }
            IntPtr ptr = Marshal.AllocHGlobal(4 * Gtf.Stride * Gtf.Height);
            Marshal.Copy(newData, 0, ptr, Gtf.Stride * Gtf.Height);
            return ptr;
        }
    }
}
