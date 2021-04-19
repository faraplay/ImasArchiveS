namespace Imas.UI
{
    [SerialisationDerivedType(9)]
    public class ColorAnimation : Animation
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
        public uint startColor;
        [SerialiseField(106)]
        public uint endColor;
    }
}
