namespace Imas
{
    partial class WuQuantizer
    {
        struct ColorMoment
        {
            public long SumA;
            public long SumR;
            public long SumG;
            public long SumB;
            public int Count;
            public float SumSquares;

            public static ColorMoment operator +(ColorMoment c1, ColorMoment c2)
            {
                c1.SumA += c2.SumA;
                c1.SumR += c2.SumR;
                c1.SumG += c2.SumG;
                c1.SumB += c2.SumB;
                c1.Count += c2.Count;
                c1.SumSquares += c2.SumSquares;
                return c1;
            }

            public static ColorMoment operator -(ColorMoment c1, ColorMoment c2)
            {
                c1.SumA -= c2.SumA;
                c1.SumR -= c2.SumR;
                c1.SumG -= c2.SumG;
                c1.SumB -= c2.SumB;
                c1.Count -= c2.Count;
                c1.SumSquares -= c2.SumSquares;
                return c1;
            }
            public static ColorMoment operator +(ColorMoment c1) => c1;

            public static ColorMoment operator -(ColorMoment c1)
            {
                c1.SumA = -c1.SumA;
                c1.SumR = -c1.SumR;
                c1.SumG = -c1.SumG;
                c1.SumB = -c1.SumB;
                c1.Count = -c1.Count;
                c1.SumSquares = -c1.SumSquares;
                return c1;
            }

            public void Add(uint p)
            {
                int a = (int)((p & 0xFF000000) >> 24);
                int r = (int)((p & 0x00FF0000) >> 16);
                int g = (int)((p & 0x0000FF00) >> 8);
                int b = (int)((p & 0x000000FF) >> 0);
                SumA += a;
                SumR += r;
                SumG += g;
                SumB += b;
                Count++;
                SumSquares += a * a + r * r + g * g + b * b;
            }

            public void AddFast(ref ColorMoment c2)
            {
                SumA += c2.SumA;
                SumR += c2.SumR;
                SumG += c2.SumG;
                SumB += c2.SumB;
                Count += c2.Count;
                SumSquares += c2.SumSquares;
            }

            public byte MeanA => (Count != 0) ? (byte)(SumA / Count) : (byte)0;
            public byte MeanR => (Count != 0) ? (byte)(SumR / Count) : (byte)0;
            public byte MeanG => (Count != 0) ? (byte)(SumG / Count) : (byte)0;
            public byte MeanB => (Count != 0) ? (byte)(SumB / Count) : (byte)0;

            public long SquaredSums => SumA * SumA + SumR * SumR + SumG * SumG + SumB * SumB;

            public long SquaredSumsOverCount => SquaredSums / Count;

            public float Variance
            { 
                get
                {
                    var result = SumSquares - (float)SquaredSums / Count;
                    return float.IsNaN(result) ? 0.0f : result;
                }
            }
        }
    }
}
