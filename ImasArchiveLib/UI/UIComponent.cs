using Imas.Archive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imas.UI
{
    public class UIComponent : IDisposable
    {
        private ParFile parFile;
        private List<string> _subNames;

        public IReadOnlyList<string> SubcomponentNames => _subNames;

        public UIComponent(Stream parStream)
        {
            parFile = new ParFile(parStream);
            _subNames = new List<string>();
            foreach (string entryName in parFile.Entries.Select(entry => entry.FileName).Where(name => name.EndsWith(".pau")))
            {
                _subNames.Add(entryName[..^4]);
            }
        }

        public async Task<UISubcomponent> CreateComponent(string name)
        {
            return await UISubcomponent.CreateComponent(parFile, name);
        }

        public async Task<UISubcomponent> CreateComponent(int index)
        {
            return await CreateComponent(SubcomponentNames[index]);
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
            if (!disposed)
            {
                parFile?.Dispose();
            }
            disposed = true;
        }

        #endregion IDisposable
    }
}
