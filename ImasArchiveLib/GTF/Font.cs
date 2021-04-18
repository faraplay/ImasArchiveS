using System;
using System.IO;

namespace Imas.Gtf
{
    public partial class Font : IDisposable
    {
        public GTF Gtf { get; }
        private CharData[] chars;
        private int root;

        private Font(GTF gtf, CharData[] chars, int root)
        {
            Gtf = gtf;
            this.chars = chars;
            this.root = root;
        }

        #region IDisposable
        private bool disposed;
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
                Gtf.Dispose();
            }
            disposed = true;
        }
        ~Font()
        {
            Dispose(false);
        }
        #endregion IDisposable

        public void WriteJSON(TextWriter writer)
        {
            writer.WriteLine("const fontdata = [");

            foreach (CharData c in chars)
            {
                writer.Write("{");
                writer.Write("\"key\":\"{0}\", \"datawidth\":{1}, \"dataheight\":{2}, \"datax\":{3}, \"datay\":{4}, ",
                    c.key, c.datawidth, c.dataheight, c.datax, c.datay);
                writer.Write("\"offsetx\":{0}, \"offsety\":{1}, \"width\":{2}", c.offsetx, c.offsety, c.width);
                writer.Write("},\n");
            }

            writer.WriteLine("]");
        }
    }

}