namespace Imas.UI
{
    [SerialisationDerivedType(7)]
    public class AngleAnimation : Animation
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
        public float startAngle;
        [SerialiseField(105)]
        public float endAngle;
    }
}
