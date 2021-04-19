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

        [SerialiseField(104, IsCountOf = nameof(otherF))]
        public int fCount;
        [SerialiseField(105, IsCountOf = nameof(points))]
        public int pointCount;
        [SerialiseField(106)]
        public float c1;
        [SerialiseField(107)]
        public int d1;

        [SerialiseField(108)]
        public int e1;
        [SerialiseField(109, CountField = nameof(fCount))]
        public int[] otherF;

        [SerialiseField(110, CountField = nameof(pointCount))]
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
