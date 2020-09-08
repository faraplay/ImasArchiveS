using System;
using System.IO;
using System.Threading.Tasks;

namespace Imas.Archive
{
    public abstract class ContainerEntry : IDisposable
    {
        protected MemoryStream _newData;
        protected readonly long _originalLength;
        protected readonly long _originalOffset;

        public string FileName { get; }
        internal bool UsesMemoryStream => _newData != null;
        internal long Offset { get; set; }
        public long Length { get => UsesMemoryStream ? _newData.Length : _originalLength; }

        #region Constructors

        protected ContainerEntry(string fileName, long originalLength, long originalOffset)
        {
            FileName = fileName;
            _originalLength = originalLength;
            _originalOffset = originalOffset;
            Offset = originalOffset;
        }

        #endregion Constructors

        public abstract Task<Stream> GetData();

        public abstract Task SetData(Stream stream);

        #region IDisposable

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                _newData?.Dispose();
            }
            disposed = true;
        }

        #endregion IDisposable
    }
}