using Imas.Archive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Imas.UI
{
    public class UIComponent : IDisposable
    {
        private Stream pauStream;
        private ParFile ptaFile;
        public Control control;
        internal ImageSource imageSource;

        private void ReadPauStream()
        {
            if (Binary.ReadUInt32(pauStream, true) != 0x50415505) // "PAU\05"
                throw new InvalidDataException("Unrecognised PAU header");
            control = Control.Create(this, pauStream);
        }

        private async Task LoadImageSource()
        {
            imageSource = await ImageSource.CreateImageSource(ptaFile);
        }

        public static async Task<UIComponent> CreateComponent(ParFile componentPar, string name)
        {
            UIComponent component = new UIComponent();
            component.pauStream = await componentPar.GetEntry(name + ".pau").GetData();
            component.ReadPauStream();
            component.ptaFile = new ParFile(await componentPar.GetEntry(name + ".pta").GetData());
            await component.LoadImageSource();
            return component;
        }

        public static async Task<UIComponent> CreateComponent(Stream parStream, string name)
        {
            return await CreateComponent(new ParFile(parStream), name);
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
                pauStream?.Dispose();
                ptaFile?.Dispose();
            }
            disposed = true;
        }

        #endregion IDisposable
    }
}
