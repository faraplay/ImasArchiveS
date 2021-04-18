namespace Imas.Gtf
{
    partial class WuQuantizer
    {
        private class Box
        {
            public byte aMin;
            public byte aMax;
            public byte rMin;
            public byte rMax;
            public byte gMin;
            public byte gMax;
            public byte bMin;
            public byte bMax;
            public bool CanBeCut =>
                (aMax - aMin > 1) ||
                (rMax - rMin > 1) ||
                (gMax - gMin > 1) ||
                (bMax - bMin > 1);
            public Box Clone()
                => new Box()
                {
                    aMax = aMax,
                    aMin = aMin,
                    rMax = rMax,
                    rMin = rMin,
                    gMax = gMax,
                    gMin = gMin,
                    bMax = bMax,
                    bMin = bMin,
                };
        }

        private ColorMoment Volume(Box box) =>
                + moments[box.aMax, box.rMax, box.gMax, box.bMax]
                - moments[box.aMax, box.rMax, box.gMax, box.bMin]
                - moments[box.aMax, box.rMax, box.gMin, box.bMax]
                + moments[box.aMax, box.rMax, box.gMin, box.bMin]
                - moments[box.aMax, box.rMin, box.gMax, box.bMax]
                + moments[box.aMax, box.rMin, box.gMax, box.bMin]
                + moments[box.aMax, box.rMin, box.gMin, box.bMax]
                - moments[box.aMax, box.rMin, box.gMin, box.bMin]
                - moments[box.aMin, box.rMax, box.gMax, box.bMax]
                + moments[box.aMin, box.rMax, box.gMax, box.bMin]
                + moments[box.aMin, box.rMax, box.gMin, box.bMax]
                - moments[box.aMin, box.rMax, box.gMin, box.bMin]
                + moments[box.aMin, box.rMin, box.gMax, box.bMax]
                - moments[box.aMin, box.rMin, box.gMax, box.bMin]
                - moments[box.aMin, box.rMin, box.gMin, box.bMax]
                + moments[box.aMin, box.rMin, box.gMin, box.bMin];

        private ColorMoment Top(Box box, int direction, int position) => direction switch
        {
            Alpha =>
                + moments[position, box.rMax, box.gMax, box.bMax]
                - moments[position, box.rMax, box.gMax, box.bMin]
                - moments[position, box.rMax, box.gMin, box.bMax]
                + moments[position, box.rMax, box.gMin, box.bMin]
                - moments[position, box.rMin, box.gMax, box.bMax]
                + moments[position, box.rMin, box.gMax, box.bMin]
                + moments[position, box.rMin, box.gMin, box.bMax]
                - moments[position, box.rMin, box.gMin, box.bMin],
            Red =>
                + moments[box.aMax, position, box.gMax, box.bMax]
                - moments[box.aMax, position, box.gMax, box.bMin]
                - moments[box.aMax, position, box.gMin, box.bMax]
                + moments[box.aMax, position, box.gMin, box.bMin]
                - moments[box.aMin, position, box.gMax, box.bMax]
                + moments[box.aMin, position, box.gMax, box.bMin]
                + moments[box.aMin, position, box.gMin, box.bMax]
                - moments[box.aMin, position, box.gMin, box.bMin],
            Green =>
                + moments[box.aMax, box.rMax, position, box.bMax]
                - moments[box.aMax, box.rMax, position, box.bMin]
                - moments[box.aMax, box.rMin, position, box.bMax]
                + moments[box.aMax, box.rMin, position, box.bMin]
                - moments[box.aMin, box.rMax, position, box.bMax]
                + moments[box.aMin, box.rMax, position, box.bMin]
                + moments[box.aMin, box.rMin, position, box.bMax]
                - moments[box.aMin, box.rMin, position, box.bMin],
            Blue =>
                + moments[box.aMax, box.rMax, box.gMax, position]
                - moments[box.aMax, box.rMax, box.gMin, position]
                - moments[box.aMax, box.rMin, box.gMax, position]
                + moments[box.aMax, box.rMin, box.gMin, position]
                - moments[box.aMin, box.rMax, box.gMax, position]
                + moments[box.aMin, box.rMax, box.gMin, position]
                + moments[box.aMin, box.rMin, box.gMax, position]
                - moments[box.aMin, box.rMin, box.gMin, position],
            _ => new ColorMoment()
        };

        private ColorMoment Bottom(Box box, int direction)
            => Top(box,
                    direction,
                    direction switch
                    {
                        Alpha => box.aMin,
                        Red => box.rMin,
                        Green => box.gMin,
                        Blue => box.bMin,
                        _ => 0
                    });

        private float BoxVariance(Box box)
            => Volume(box).Variance;
    }
}
