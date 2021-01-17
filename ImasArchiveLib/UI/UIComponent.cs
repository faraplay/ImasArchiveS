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
        private readonly ParFile parFile;
        private readonly List<string> _subNames;
        private readonly List<UISubcomponent> _subcomponents;

        public int Count { get; private set; }
        public IReadOnlyList<string> SubcomponentNames => _subNames;

        public UISubcomponent this[int i]
        {
            get
            {
                if (i < 0 || i >= Count)
                    throw new ArgumentOutOfRangeException();
                else
                    return _subcomponents[i];
            }
        }
        public UISubcomponent this[string key]
        {
            get
            {
                int index = _subNames.IndexOf(key);
                if (index == -1)
                    return null;
                else
                    return _subcomponents[index];
            }
        }

        private UIComponent(ParFile parFile)
        {
            this.parFile = parFile;
            var entryNames = parFile.Entries.Select(entry => entry.FileName).Where(name => name.EndsWith(".pau"));
            Count = entryNames.Count();
            _subNames = new List<string>(Count);
            _subcomponents = new List<UISubcomponent>(Count);
            foreach (string entryName in entryNames)
            {
                _subNames.Add(entryName[..^4]);
            }
        }

        private async Task CreateAllSubcomponents()
        {
            foreach (string entryName in _subNames)
            {
                _subcomponents.Add(await UISubcomponent.CreateComponent(parFile, entryName));
            }
        }

        public static async Task<UIComponent> CreateUIComponent(ParFile parFile)
        {
            UIComponent uiComponent = new UIComponent(parFile);
            await uiComponent.CreateAllSubcomponents();
            return uiComponent;
        }

        public static async Task<UIComponent> CreateUIComponent(Stream parStream)
        {
            return await CreateUIComponent(new ParFile(parStream));
        }

        public async Task SaveTo(Stream stream)
        {
            foreach (UISubcomponent subcomponent in _subcomponents)
            {
                var pauEntry = parFile.GetEntry(subcomponent.Name + ".pau");
                using MemoryStream memStream = new MemoryStream();
                subcomponent.WritePauStream(memStream);
                memStream.Seek(0, SeekOrigin.Begin);
                await pauEntry.SetData(memStream);

                var ptaEntry = parFile.GetEntry(subcomponent.Name + ".pta");
                memStream.SetLength(0);
                await subcomponent.WritePtaStream(memStream);
                memStream.Seek(0, SeekOrigin.Begin);
                await ptaEntry.SetData(memStream);
            }
            await parFile.SaveTo(stream);
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
