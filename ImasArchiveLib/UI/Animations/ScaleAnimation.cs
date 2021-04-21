namespace Imas.UI
{
    [SerialisationDerivedType(6)]
    public class ScaleAnimation : Animation
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

        [SerialiseProperty(105)]
		[Listed(105)]
        public float StartXScale { get; set; }
        [SerialiseProperty(106)]
		[Listed(106)]
        public float StartYScale { get; set; }
        [SerialiseProperty(107)]
		[Listed(107)]
        public float EndXScale { get; set; }
        [SerialiseProperty(108)]
		[Listed(108)]
        public float EndYScale { get; set; }
    }
}
