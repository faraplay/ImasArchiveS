using System.Collections.Generic;
using System.Linq;

namespace Imas.UI
{
    [SerialisationDerivedType(4)]
    public class PositionAnimation : StartEndAnimation
    {
        [SerialiseProperty(200)]
		[Listed(200)]
        public int FCount
        {
            get => OtherF.Count;
            set
            {
                if (value == OtherF.Count)
                    return;
                if (value < OtherF.Count && value >= 0)
                {
                    OtherF.RemoveRange(value, OtherF.Count - value);
                    return;
                }
                if (value > OtherF.Count)
                {
                    OtherF.AddRange(Enumerable.Repeat<int>(0, value - OtherF.Count));
                    return;
                }
            }
        }
        [SerialiseProperty(201)]
		[Listed(201)]
        public int PointCount
        {
            get => Points.Count;
            set
            {
                if (value == Points.Count)
                    return;
                if (value < Points.Count && value >= 0)
                {
                    Points.RemoveRange(value, Points.Count - value);
                    return;
                }
                if (value > Points.Count)
                {
                    Points.AddRange(Enumerable.Repeat<Point>(null, value - Points.Count));
                    return;
                }
            }
        }
        [SerialiseProperty(202)]
		[Listed(202)]
        public float C1 { get; set; }
        [SerialiseProperty(203)]
		[Listed(203)]
        public int D1 { get; set; }

        [SerialiseProperty(204)]
		[Listed(204)]
        public int E1 { get; set; }
        [SerialiseProperty(205)]
        [Listed(205)]
        public List<int> OtherF { get; set; } = new List<int>();

        [SerialiseProperty(206)]
        [Listed(206)]
        public List<Point> Points { get; set; } = new List<Point>();
    }

    public class Point
    {
        [SerialiseProperty(0)]
		[Listed(0)]
        public float X { get; set; }
        [SerialiseProperty(1)]
		[Listed(1)]
        public float Y { get; set; }
    }
}
