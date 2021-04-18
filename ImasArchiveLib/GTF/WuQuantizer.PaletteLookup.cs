using System;
using System.Collections.Generic;
using System.Linq;

namespace Imas.Gtf
{
    partial class WuQuantizer {
        private class PaletteLookup
        {
            private uint mMask;
            private Dictionary<uint, LookupNode[]> mLookup;
            private LookupNode[] Palette;

            public PaletteLookup(uint[] palette)
            {
                Palette = new LookupNode[palette.Length];
                for (int paletteIndex = 0; paletteIndex < palette.Length; paletteIndex++)
                {
                    Palette[paletteIndex] = new LookupNode { Pixel = palette[paletteIndex], PaletteIndex = (byte)paletteIndex };
                }
                BuildLookup(palette);
            }

            public byte GetPaletteIndex(uint pixel)
            {
                uint pixelKey = pixel & mMask;
                if (!mLookup.TryGetValue(pixelKey, out LookupNode[] bucket))
                {
                    bucket = Palette;
                }

                if (bucket.Length == 1)
                {
                    return bucket[0].PaletteIndex;
                }

                int bestDistance = int.MaxValue;
                byte bestMatch = 0;
                foreach (var lookup in bucket)
                {
                    uint lookupPixel = lookup.Pixel;

                    int deltaAlpha = GetAlpha(pixel) - GetAlpha(lookupPixel);
                    int distance = deltaAlpha * deltaAlpha;

                    var deltaRed = GetRed(pixel) - GetRed(lookupPixel);
                    distance += deltaRed * deltaRed;

                    var deltaGreen = GetGreen(pixel) - GetGreen(lookupPixel);
                    distance += deltaGreen * deltaGreen;

                    var deltaBlue = GetBlue(pixel) - GetBlue(lookupPixel);
                    distance += deltaBlue * deltaBlue;

                    if (distance >= bestDistance)
                        continue;

                    bestDistance = distance;
                    bestMatch = lookup.PaletteIndex;
                }

                if ((bucket == Palette) && (pixelKey != 0))
                {
                    mLookup[pixelKey] = new LookupNode[] { bucket[bestMatch] };
                }

                return bestMatch;
            }

            private void BuildLookup(uint[] palette)
            {
                uint mask = GetMask(palette);
                Dictionary<uint, List<LookupNode>> tempLookup = new Dictionary<uint, List<LookupNode>>();
                foreach (LookupNode lookup in Palette)
                {
                    uint pixelKey = lookup.Pixel & mask;

                    List<LookupNode> bucket;
                    if (!tempLookup.TryGetValue(pixelKey, out bucket))
                    {
                        bucket = new List<LookupNode>();
                        tempLookup[pixelKey] = bucket;
                    }
                    bucket.Add(lookup);
                }

                mLookup = new Dictionary<uint, LookupNode[]>(tempLookup.Count);
                foreach (var key in tempLookup.Keys)
                {
                    mLookup[key] = tempLookup[key].ToArray();
                }
                mMask = mask;
            }

            private static uint GetMask(uint[] palette)
            {
                IEnumerable<byte> alphas = from pixel in palette
                                           select GetAlpha(pixel);
                byte maxAlpha = alphas.Max();
                int uniqueAlphas = alphas.Distinct().Count();

                IEnumerable<byte> reds = from pixel in palette
                                         select GetRed(pixel);
                byte maxRed = reds.Max();
                int uniqueReds = reds.Distinct().Count();

                IEnumerable<byte> greens = from pixel in palette
                                           select GetGreen(pixel);
                byte maxGreen = greens.Max();
                int uniqueGreens = greens.Distinct().Count();

                IEnumerable<byte> blues = from pixel in palette
                                          select GetBlue(pixel);
                byte maxBlue = blues.Max();
                int uniqueBlues = blues.Distinct().Count();

                double totalUniques = uniqueAlphas + uniqueReds + uniqueGreens + uniqueBlues;

                double AvailableBits = 1.0 + Math.Log(uniqueAlphas * uniqueReds * uniqueGreens * uniqueBlues);

                byte alphaMask = ComputeBitMask(maxAlpha, Convert.ToInt32(Math.Round(uniqueAlphas / totalUniques * AvailableBits)));
                byte redMask = ComputeBitMask(maxRed, Convert.ToInt32(Math.Round(uniqueReds / totalUniques * AvailableBits)));
                byte greenMask = ComputeBitMask(maxGreen, Convert.ToInt32(Math.Round(uniqueGreens / totalUniques * AvailableBits)));
                byte blueMask = ComputeBitMask(maxAlpha, Convert.ToInt32(Math.Round(uniqueBlues / totalUniques * AvailableBits)));

                uint maskedPixel =
                    ((uint)alphaMask << 24) ^
                    ((uint)redMask << 16) ^
                    ((uint)greenMask << 8) ^
                    ((uint)blueMask << 0);
                return maskedPixel;
            }

            private static byte ComputeBitMask(byte max, int bits)
            {
                byte mask = 0;

                if (bits != 0)
                {
                    byte highestSetBitIndex = HighestSetBitIndex(max);


                    for (int i = 0; i < bits; i++)
                    {
                        mask <<= 1;
                        mask++;
                    }

                    for (int i = 0; i <= highestSetBitIndex - bits; i++)
                    {
                        mask <<= 1;
                    }
                }
                return mask;
            }

            private static byte HighestSetBitIndex(byte value)
            {
                byte index = 0;
                for (int i = 0; i < 8; i++)
                {
                    if (0 != (value & 1))
                    {
                        index = (byte)i;
                    }
                    value >>= 1;
                }
                return index;
            }

            private struct LookupNode
            {
                public uint Pixel;
                public byte PaletteIndex;
            }

            private static byte GetAlpha(uint p) => (byte)((p & 0xFF000000) >> 24);
            private static byte GetRed(uint p) => (byte)((p & 0x00FF0000) >> 16);
            private static byte GetGreen(uint p) => (byte)((p & 0x0000FF00) >> 8);
            private static byte GetBlue(uint p) => (byte)((p & 0x000000FF) >> 0);
        }
    }
}
