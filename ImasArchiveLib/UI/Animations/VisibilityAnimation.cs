namespace Imas.UI
{
    [SerialisationDerivedType(1)]
    public class VisibilityAnimation : Animation
    {
        [SerialiseProperty(100)]
        [Listed(100)]
        public float Time { get; set; }

        [SerialiseProperty(101)]
        [Listed(101)]
        public int Visibility { get; set; } = 3;
    }
}
