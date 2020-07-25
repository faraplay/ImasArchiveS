using System;
using System.Threading.Tasks;

namespace ImasArchiveApp
{
    abstract class ContainerFileModel : FileModel, IDisposable
    {
        #region Properties
        public BrowserModel BrowserModel { get; set; }
        public abstract IFileModel FileModel { get; set; }
        #endregion
        #region Constructors
        protected ContainerFileModel(IReport parent, string fileName) : base(parent, fileName)
        { }
        #endregion
        public abstract Task LoadChildFileModel(string filename);

        #region IDisposable
        bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                FileModel?.Dispose();
            }
            disposed = true;
            base.Dispose(disposing);
        }
        #endregion


    }
}
