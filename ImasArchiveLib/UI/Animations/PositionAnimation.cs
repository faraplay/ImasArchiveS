namespace Imas.UI
{
    [SerialisationDerivedType(4)]
    public class PositionAnimation : StartEndAnimation
    {
        [SerialiseProperty(200, IsCountOf = nameof(OtherF))]
		[Listed(200)]
        public int FCount { get; set; }
        [SerialiseProperty(201, IsCountOf = nameof(Points))]
		[Listed(201)]
        public int PointCount { get; set; }
        [SerialiseProperty(202)]
		[Listed(202)]
        public float C1 { get; set; }
        [SerialiseProperty(203)]
		[Listed(203)]
        public int D1 { get; set; }

        [SerialiseProperty(204)]
		[Listed(204)]
        public int E1 { get; set; }
        [SerialiseProperty(205, CountProperty = nameof(FCount))]
		[Listed(205)]
        public int[] OtherF { get; set; }

        [SerialiseProperty(206, CountProperty = nameof(PointCount))]
		[Listed(206)]
        public Point[] Points { get; set; }
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
