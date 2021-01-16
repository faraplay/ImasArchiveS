using Imas.Archive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Imas.UI
{
    public class UISubcomponent : IDisposable
    {
        private Stream pauStream;
        private ParFile ptaFile;
        public Control control;
        internal ImageSource imageSource;

        public string Name { get; }

        private UISubcomponent(string name) { Name = name; }

        private void ReadPauStream()
        {
            uint header = Binary.ReadUInt32(pauStream, true);
            if (header != 0x50415505 && header != 0x50415504) // "PAU\x05", "PAU\x04"
                throw new InvalidDataException("Unrecognised PAU header");
            control = Control.Create(this, pauStream);
        }

        private async Task LoadImageSource()
        {
            imageSource = await ImageSource.CreateImageSource(ptaFile);
        }

        public static async Task<UISubcomponent> CreateComponent(ParFile componentPar, string name)
        {
            UISubcomponent component = new UISubcomponent(name)
            {
                pauStream = await componentPar.GetEntry(name + ".pau").GetData()
            };
            component.ReadPauStream();
            component.ptaFile = new ParFile(await componentPar.GetEntry(name + ".pta").GetData());
            await component.LoadImageSource();
            return component;
        }

        public void WritePauStream(Stream stream)
        {
            Binary.WriteUInt32(stream, true, 0x50415505);
            control.Serialise(stream);
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
