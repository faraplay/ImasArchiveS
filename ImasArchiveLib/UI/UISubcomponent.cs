using Imas.Archive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Imas.UI
{
    public class UISubcomponent : IDisposable
    {
        private Stream pauStream;
        private ParFile ptaFile;
        private ParFile animationsPar;

        public Control rootControl;
        public SpriteSheetSource imageSource;
        public List<AnimationGroup> animationGroups;

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
            rootControl = (Control)Deserialiser.Deserialise(new Binary(pauStream, true), typeof(Control));
        }

        private async Task ReadAnimations()
        {
            animationGroups = new List<AnimationGroup>();
            foreach (var entry in animationsPar.Entries)
            {
                AnimationGroup animationGroup = (AnimationGroup)Deserialiser.Deserialise(
                                        new Binary(await entry.GetData(), true), typeof(AnimationGroup));
                animationGroup.FileName = entry.FileName;
                animationGroups.Add(animationGroup);
            }
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
            var animationsEntry = componentPar.GetEntry(name + ".par");
            if (animationsEntry != null)
            {
                component.animationsPar = new ParFile(await animationsEntry.GetData());
                await component.ReadAnimations();
            }
            return component;
        }

        public void WritePauStream(Stream stream)
        {
            Binary.WriteUInt32(stream, true, 0x50415505);
            Serialiser.Serialise(new Binary(stream, true), rootControl);
        }

        public async Task WritePtaStream(Stream stream)
        {
            await ptaFile.SaveTo(stream);
        }

        public async Task WritePaaStream(Stream stream)
        {
            using MemoryStream memStream = new MemoryStream();
            foreach (var entry in animationsPar.Entries)
            {
                AnimationGroup animationGroup = animationGroups.Find(group => group.FileName == entry.FileName);
                memStream.SetLength(0);
                Serialiser.Serialise(new Binary(memStream, true), animationGroup);
                memStream.Seek(0, SeekOrigin.Begin);
                await entry.SetData(memStream);
            }
            await animationsPar.SaveTo(stream);
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
