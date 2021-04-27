namespace Imas.UI
{
    public class StartEndAnimation : Animation
    {
        [SerialiseProperty(100)]
        [Listed(100)]
        public float StartTime { get; set; } = 0;
        [SerialiseProperty(101)]
        [Listed(101)]
        public float EndTime { get; set; } = 1;

        [SerialiseProperty(102)]
        [Listed(102)]
        public int A1 { get; set; } = 1;
        [SerialiseProperty(103)]
        [Listed(103)]
        public int A2 { get; set; } = 0;
        public bool A1IsTwo => A1 == 2;
        [SerialiseProperty(104, ConditionProperty = nameof(A1IsTwo))]
        [Listed(104, ConditionProperty = nameof(A1IsTwo))]
        public float B1 { get; set; } = 0;
    }
}
