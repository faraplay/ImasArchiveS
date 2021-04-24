using System.Drawing;

namespace Imas.Gtf
{
    public partial class GTF
    {
        private static class ColorHelp
        {
            public static Color From565(ushort x)
            {
                int r0 = (x >> 11) & 0x1F;
                r0 = (r0 << 3) ^ (r0 >> 2);
                int g0 = (x >> 5) & 0x3F;
                g0 = (g0 << 2) ^ (g0 >> 4);
                int b0 = x & 0x1F;
                b0 = (b0 << 3) ^ (b0 >> 2);
                return Color.FromArgb(r0, g0, b0);
            }

            public static Color From1555(ushort x)
            {
                int a0 = (x >> 15) * 255;
                int r0 = (x >> 10) & 0x1F;
                r0 = (r0 << 3) ^ (r0 >> 2);
                int g0 = (x >> 5) & 0x1F;
                g0 = (g0 << 3) ^ (g0 >> 2);
                int b0 = x & 0x1F;
                b0 = (b0 << 3) ^ (b0 >> 2);
                return Color.FromArgb(a0, r0, g0, b0);
            }

            public static Color From4444(int x)
            {
                int b = (x & 15) * 17;
                x >>= 4;
                int g = (x & 15) * 17;
                x >>= 4;
                int r = (x & 15) * 17;
                x >>= 4;
                int a = (x & 15) * 17;
                return Color.FromArgb(a, r, g, b);
            }

            public static ushort To4444(Color color)
            {
                return (ushort)(
                    ((color.A >> 4) << 12)
                    + ((color.R >> 4) << 8)
                    + ((color.G >> 4) << 4)
                    + (color.B >> 4));
            }

            public static ushort To565(Color color)
            {
                return (ushort)(
                    ((color.R >> 3) << 11)
                    + ((color.G >> 2) << 5)
                    + (color.B >> 3));
            }

            public static Color MixRatio(Color c0, Color c1, int m0, int m1)
            {
                int r = (m0 * c0.R + m1 * c1.R) / (m0 + m1);
                int g = (m0 * c0.G + m1 * c1.G) / (m0 + m1);
                int b = (m0 * c0.B + m1 * c1.B) / (m0 + m1);
                return Color.FromArgb(r, g, b);
            }
        }
    }
}