namespace Imas.Gtf
{
    public partial class GTF
    {
        private class Order
        {
            private readonly int width;
            private readonly int height;
            private readonly uint xmax;
            private readonly uint ymax;
            private int x, y;
            private readonly bool zOrder;

            public Order(int width, int height) :
                this(width, height, IsPow2(width) && IsPow2(height))
            { }

            public Order(int width, int height, bool isZOrder)
            {
                this.width = width;
                this.height = height;
                xmax = (uint)(width - 1);
                ymax = (uint)(height - 1);
                x = 0;
                y = 0;
                zOrder = isZOrder;
            }

            public (int, int) GetXY()
            {
                (int, int) result = (x, y);
                if (zOrder)
                {
                    uint xTrail1 = (uint)((x - width) ^ (x - width + 1));
                    uint yTrail1 = (uint)((y - height) ^ (y - height + 1));
                    x ^= (int)(xTrail1 & yTrail1 & xmax);
                    y ^= (int)((xTrail1 >> 1) & yTrail1 & ymax);
                }
                else
                {
                    x++;
                    if (x >= width)
                    {
                        x = 0;
                        y++;
                    }
                }
                return result;
            }
        }
    }
}