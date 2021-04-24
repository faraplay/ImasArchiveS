using System;
using System.Collections.Generic;
using System.Linq;

namespace Imas.Gtf
{
    partial class WuQuantizer
    {
        private const int Alpha = 3;
        private const int Red = 2;
        private const int Green = 1;
        private const int Blue = 0;
        private const int MaxSideIndex = 32;
        private const int SideSize = 33; // hypercube arrays need to store cumulative data

        private ColorMoment[,,,] moments;

        public static (byte[] indexedData, uint[] palette) QuantizeImage(int[] imageData, int maxColors)
        {
            WuQuantizer quantizer = new WuQuantizer();
            quantizer.BuildHistogram(imageData);
            quantizer.CalculateCumulativeMoments();
            var boxes = quantizer.SplitData(maxColors);
            var palette = quantizer.BuildPalette(boxes).ToArray();
            var fullPalette = new uint[256];
            Array.Copy(palette, fullPalette, palette.Length);
            return (LookupPixels(imageData, palette), fullPalette);
        }

        private void BuildHistogram(int[] imageData)
        {
            moments = new ColorMoment[SideSize, SideSize, SideSize, SideSize];
            foreach (int i in imageData)
            {
                uint p = (uint)i;
                int a = (int)((p & 0xFF000000) >> 24);
                int r = (int)((p & 0x00FF0000) >> 16);
                int g = (int)((p & 0x0000FF00) >> 8);
                int b = (int)((p & 0x000000FF) >> 0);
                moments[
                    (a >> 3) + 1,
                    (r >> 3) + 1,
                    (g >> 3) + 1,
                    (b >> 3) + 1].Add(p);
            }
        }

        private void CalculateCumulativeMoments()
        {
            var vol = new ColorMoment[SideSize, SideSize];
            var area = new ColorMoment[SideSize];
            ColorMoment line;
            for (int aIndex = 1; aIndex < SideSize; aIndex++)
            {
                Array.Clear(vol, 0, SideSize * SideSize);
                for (int rIndex = 1; rIndex < SideSize; rIndex++)
                {
                    Array.Clear(area, 0, SideSize);
                    for (int gIndex = 1; gIndex < SideSize; gIndex++)
                    {
                        line = new ColorMoment();
                        for (int bIndex = 1; bIndex < SideSize; bIndex++)
                        {
                            line.AddFast(ref moments[aIndex, rIndex, gIndex, bIndex]);
                            area[bIndex].AddFast(ref line);
                            vol[gIndex, bIndex].AddFast(ref area[bIndex]);
                            moments[aIndex, rIndex, gIndex, bIndex] = moments[aIndex - 1, rIndex, gIndex, bIndex];
                            moments[aIndex, rIndex, gIndex, bIndex] += vol[gIndex, bIndex];
                        }
                    }
                }
            }
        }

        private (byte?, float) MaxCutInDirection(Box box, int direction, byte first, byte last, ColorMoment boxWholeMoment)
        {
            ColorMoment bottomMoment = Bottom(box, direction);
            float maxScore = 0.0f;
            byte? cutPoint = null;

            for (byte position = first; position < last; ++position)
            {
                var lowerHalfMoment = Top(box, direction, position) - bottomMoment;
                if (lowerHalfMoment.Count == 0)
                    continue;
                var upperHalfMoment = boxWholeMoment - lowerHalfMoment;
                if (upperHalfMoment.Count == 0)
                    continue;
                var score = lowerHalfMoment.SquaredSumsOverCount + upperHalfMoment.SquaredSumsOverCount;
                // score is boxWholeMoment.Moment - (lowerHalfMoment.Variance + upperHalfMoment.Variance) 
                // to minimise variance, we want to maximise this
                if (score > maxScore)
                {
                    maxScore = score;
                    cutPoint = position;
                }
            }
            return (cutPoint, maxScore);
        }

        private Box TryCut(Box box) // attempts to cut box and put higher half in 'second'
        {
            var whole = Volume(box);
            (byte? cutAPosition, float cutAScore) = MaxCutInDirection(box, Alpha, (byte)(box.aMin + 1), box.aMax, whole);
            (byte? cutRPosition, float cutRScore) = MaxCutInDirection(box, Red, (byte)(box.rMin + 1), box.rMax, whole);
            (byte? cutGPosition, float cutGScore) = MaxCutInDirection(box, Green, (byte)(box.gMin + 1), box.gMax, whole);
            (byte? cutBPosition, float cutBScore) = MaxCutInDirection(box, Blue, (byte)(box.bMin + 1), box.bMax, whole);
            Box newBox = box.Clone(); // copy
            if (cutAScore >= cutRScore && cutAScore >= cutGScore && cutAScore >= cutBScore)
            {
                if (cutAPosition.HasValue)
                    newBox.aMin = box.aMax = cutAPosition.Value;
                else
                    newBox = null;
            }
            else if (cutRScore >= cutGScore && cutRScore >= cutBScore)
            {
                if (cutRPosition.HasValue)
                    newBox.rMin = box.rMax = cutRPosition.Value;
                else
                    newBox = null;
            }
            else if (cutGScore >= cutBScore)
            {
                if (cutGPosition.HasValue)
                    newBox.gMin = box.gMax = cutGPosition.Value;
                else
                    newBox = null;
            }
            else
            {
                if (cutBPosition.HasValue)
                    newBox.bMin = box.bMax = cutBPosition.Value;
                else
                    newBox = null;
            }
            return newBox;
        }

        private List<Box> SplitData(int maxColors)
        {
            int colorCount = maxColors - 1;
            int splitBoxIndex = 0;
            var boxes = new List<(Box box, float var)>(maxColors) // var = 0.0f if box cannot be split
            {
                (new Box
                {
                    aMax = MaxSideIndex,
                    rMax = MaxSideIndex,
                    gMax = MaxSideIndex,
                    bMax = MaxSideIndex
                }, 0)
            };
            while (boxes.Count <= colorCount)
            {
                Box firstBox = boxes[splitBoxIndex].box;
                Box newBox = TryCut(firstBox);
                if (newBox != null)
                {
                    boxes[splitBoxIndex] = (firstBox, firstBox.CanBeCut ? BoxVariance(firstBox) : 0.0f);
                    boxes.Add((newBox, newBox.CanBeCut ? BoxVariance(newBox) : 0.0f));
                }
                else
                {
                    boxes[splitBoxIndex] = (firstBox, 0.0f);
                    //boxes[boxCount] = new Box();
                }

                splitBoxIndex = 0;
                float maxVar = boxes[splitBoxIndex].var;
                for (int i = 1; i < boxes.Count; ++i)
                {
                    if (boxes[i].var > maxVar)
                    {
                        maxVar = boxes[i].var;
                        splitBoxIndex = i;
                    }
                }

                if (maxVar == 0.0f)
                {
                    break;
                }
            }
            return boxes.Select(tuple => tuple.box).ToList();
        }

        private List<uint> BuildPalette(List<Box> boxes)
        {
            var palette = new List<uint>(boxes.Count);
            foreach (Box box in boxes)
            {
                var vol = Volume(box);
                if (vol.Count == 0)
                    continue;
                palette.Add(
                    ((uint)vol.MeanA << 24) ^
                    ((uint)vol.MeanR << 16) ^
                    ((uint)vol.MeanG << 8) ^
                    ((uint)vol.MeanB << 0));
            }
            return palette;
        }

        private static byte[] LookupPixels(int[] imageData, uint[] palette)
        {
            byte[] indexedData = new byte[imageData.Length];
            PaletteLookup paletteLookup = new PaletteLookup(palette);
            for (int i = 0; i < indexedData.Length; ++i)
            {
                indexedData[i] = paletteLookup.GetPaletteIndex((uint)imageData[i]);
            }
            return indexedData;
        }
    }
}
