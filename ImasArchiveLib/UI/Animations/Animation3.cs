namespace Imas.UI
{
    [SerialisationDerivedType(3)]
    public class Animation3 : Animation
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

        [SerialiseField(105)]
        public int c1;
        [SerialiseField(106)]
        public float d1;
        [SerialiseField(107)]
        public float d2;
        [SerialiseField(108)]
        public int e1;
        [SerialiseField(109)]
        public float f1;
        [SerialiseField(110)]
        public float f2;
    }
}
