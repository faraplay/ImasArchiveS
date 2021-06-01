namespace Imas.UI
{
    [SerialisationDerivedType(1)]
    public class VisibilityAnimation : Animation
    {
        [SerialiseProperty(100)]
        public float Time { get; set; }
        [Listed(100)]
        public float Frame
        {
            get => Time * 60f;
            set => Time = value / 60f;
        }

        [SerialiseProperty(101)]
        [Listed(101)]
        public int Visibility { get; set; } = 3;
    }
}
