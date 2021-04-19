namespace Imas.UI
{
    [SerialisationDerivedType(6)]
    public class ScaleAnimation : Animation
    {
        [SerialiseField(100)]
        public float startTime;
        [SerialiseField(101)]
        public float endTime;

        [SerialiseField(102)]
        public int a1;
        [SerialiseField(103)]
        public int a2;

        [SerialiseField(104)]
        public float startXScale;
        [SerialiseField(105)]
        public float startYScale;
        [SerialiseField(106)]
        public float endXScale;
        [SerialiseField(107)]
        public float endYScale;
    }
}
