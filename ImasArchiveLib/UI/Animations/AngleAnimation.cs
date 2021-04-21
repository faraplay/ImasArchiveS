namespace Imas.UI
{
    [SerialisationDerivedType(7)]
    public class AngleAnimation : Animation
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
        [Listed(104)]
        public float B1 { get; set; }

        [SerialiseProperty(105)]
        [Listed(105)]
        public float StartAngle { get; set; }
        [SerialiseProperty(106)]
        [Listed(106)]
        public float EndAngle { get; set; }
    }
}
