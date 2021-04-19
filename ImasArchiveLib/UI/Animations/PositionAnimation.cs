namespace Imas.UI
{
    [SerialisationDerivedType(4)]
    public class PositionAnimation : Animation
    {
        [SerialiseField(100)]
        public float startTime;
        [SerialiseField(101)]
        public float endTime;

        [SerialiseField(102)]
        public int a1;
        [SerialiseField(103)]
        public int a2;
        public bool A1IsTwo => a1 == 2;
        [SerialiseField(104, ConditionProperty = nameof(A1IsTwo))]
        public float b1;

        [SerialiseField(105, IsCountOf = nameof(otherF))]
        public int fCount;
        [SerialiseField(106, IsCountOf = nameof(points))]
        public int pointCount;
        [SerialiseField(107)]
        public float c1;
        [SerialiseField(108)]
        public int d1;

        [SerialiseField(109)]
        public int e1;
        [SerialiseField(110, CountField = nameof(fCount))]
        public int[] otherF;

        [SerialiseField(111, CountField = nameof(pointCount))]
        public Point[] points;
    }

    public class Point
    {
        [SerialiseField(0)]
        public float x;
        [SerialiseField(1)]
        public float y;
    }
}
