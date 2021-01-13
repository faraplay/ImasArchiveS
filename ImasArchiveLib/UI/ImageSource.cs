using Imas.Archive;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace Imas.UI
{
    class ImageSource
    {
        private List<GTF> gtfs = new List<GTF>();

        private ImageSource() { }
        public static async Task<ImageSource> CreateImageSource(ParFile parFile)
        {
            ImageSource imageSource = new ImageSource();
            foreach (ParEntry entry in parFile.Entries)
            {
                try
                {
                    GTF gtf = GTF.ReadGTF(await entry.GetData());
                    imageSource.gtfs.Add(gtf);
                }
                catch (Exception)
                {

                    throw;
                }
            }
            return imageSource;
        }

        public Bitmap this[int i] => gtfs[i].Bitmap;

        public int Count => gtfs.Count;
    }
}
