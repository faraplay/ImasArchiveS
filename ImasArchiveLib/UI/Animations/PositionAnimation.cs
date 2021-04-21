namespace Imas.UI
{
    [SerialisationDerivedType(4)]
    public class PositionAnimation : Animation
    {
        [SerialiseProperty(100)]
		[Listed(100)]
        public float StartTime { get; set; }
        [SerialiseProperty(101)]
		[Listed(101)]
        public float EndTime { get; set; }

        [SerialiseProperty(102)]
		[Listed(102)]
        public int A1 { get; set; }
        [SerialiseProperty(103)]
		[Listed(103)]
        public int A2 { get; set; }
        public bool A1IsTwo => A1 == 2;
        [SerialiseProperty(104, ConditionProperty = nameof(A1IsTwo))]
		[Listed(104, ConditionProperty = nameof(A1IsTwo))]
        public float B1 { get; set; }

        [SerialiseProperty(105, IsCountOf = nameof(OtherF))]
		[Listed(105)]
        public int FCount { get; set; }
        [SerialiseProperty(106, IsCountOf = nameof(Points))]
		[Listed(106)]
        public int PointCount { get; set; }
        [SerialiseProperty(107)]
		[Listed(107)]
        public float C1 { get; set; }
        [SerialiseProperty(108)]
		[Listed(108)]
        public int D1 { get; set; }

        [SerialiseProperty(109)]
		[Listed(109)]
        public int E1 { get; set; }
        [SerialiseProperty(110, CountProperty = nameof(FCount))]
		[Listed(110)]
        public int[] OtherF { get; set; }

        [SerialiseProperty(111, CountProperty = nameof(PointCount))]
		[Listed(111)]
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
