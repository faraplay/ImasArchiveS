using Imas.Archive;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Imas.UI
{
    public class UISubcomponent : IDisposable
    {
        private Stream pauStream;
        private ParFile ptaFile;
        public Control control;
        public SpriteSheetSource imageSource;

        public byte version;

        public string Name { get; }

        private UISubcomponent(string name) { Name = name; }

        private void ReadPauStream()
        {
            if (pauStream.ReadByte() != 0x50 ||
                pauStream.ReadByte() != 0x41 ||
                pauStream.ReadByte() != 0x55)
                throw new InvalidDataException("Unrecognised PAU header");
            version = (byte)pauStream.ReadByte();
            control = (Control)Deserialiser.Deserialise(new Binary(pauStream, true), typeof(Control));
        }

        private async Task LoadImageSource()
        {
            imageSource = await SpriteSheetSource.CreateImageSource(ptaFile);
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
            Serialiser.Serialise(new Binary(stream, true), control);
        }

        public async Task WritePtaStream(Stream stream)
        {
            await ptaFile.SaveTo(stream);
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
