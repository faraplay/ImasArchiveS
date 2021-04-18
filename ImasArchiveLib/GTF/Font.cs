using System;
using System.IO;

namespace Imas.Gtf
{
    public partial class Font : IDisposable
    {
        private CharData[] chars;
        private int root;

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
                ClearBigBitmap();
                foreach (CharData c in chars)
                {
                    c?.Dispose();
                }
            }
        }

        ~Font()
        {
            Dispose(false);
        }

        #endregion IDisposable
    }

}