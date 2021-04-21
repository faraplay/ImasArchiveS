namespace Imas.UI
{
    [SerialisationDerivedType(3)]
    public class Animation3 : Animation
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
        public int C1 { get; set; }
        [SerialiseProperty(106)]
		[Listed(106)]
        public float D1 { get; set; }
        [SerialiseProperty(107)]
		[Listed(107)]
        public float D2 { get; set; }
        [SerialiseProperty(108)]
		[Listed(108)]
        public int E1 { get; set; }
        [SerialiseProperty(109)]
		[Listed(109)]
        public float F1 { get; set; }
        [SerialiseProperty(110)]
		[Listed(110)]
        public float F2 { get; set; }
    }
}
