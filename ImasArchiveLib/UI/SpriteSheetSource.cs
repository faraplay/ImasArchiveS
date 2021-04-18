using Imas.Archive;
using Imas.Gtf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace Imas.UI
{
    public class SpriteSheetSource
    {
        private readonly ParFile gtfSource;
        private List<GTF> gtfs = new List<GTF>();
        private List<string> filenames = new List<string>();

        public IReadOnlyList<string> Filenames => filenames;

        private SpriteSheetSource(ParFile gtfSource) { this.gtfSource = gtfSource; }
        public static async Task<SpriteSheetSource> CreateImageSource(ParFile parFile)
        {
            SpriteSheetSource imageSource = new SpriteSheetSource(parFile);
            foreach (ParEntry entry in parFile.Entries)
            {
                try
                {
                    imageSource.filenames.Add(entry.FileName);
                    GTF gtf = GTF.CreateFromGtfStream(await entry.GetData());
                    imageSource.gtfs.Add(gtf);
                }
                catch (Exception)
                {

                    throw;
                }
            }
            return imageSource;
        }

        public GTF this[int i] => gtfs[i];

        public int Count => gtfs.Count;

        public async Task ReplaceGTF(int index, Bitmap bitmap)
        {
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException();
            int encodingType = gtfs[index].Type;
            gtfs[index].Dispose();
            using MemoryStream ms = new MemoryStream();
            await GTF.WriteGTF(ms, bitmap, encodingType);
            ms.Seek(0, SeekOrigin.Begin);
            gtfs[index] = GTF.CreateFromGtfStream(ms);
            ms.Seek(0, SeekOrigin.Begin);
            await gtfSource.Entries[index].SetData(ms);
        }
    }
}
