namespace Imas.UI
{
    [SerialisationDerivedType(6)]
    public class ScaleAnimation : StartEndAnimation
    {
        [SerialiseProperty(200)]
        [Listed(200)]
        public float StartXScale { get; set; } = 1;
        [SerialiseProperty(201)]
        [Listed(201)]
        public float StartYScale { get; set; } = 1;
        [SerialiseProperty(202)]
        [Listed(202)]
        public float EndXScale { get; set; } = 1;
        [SerialiseProperty(203)]
        [Listed(203)]
        public float EndYScale { get; set; } = 1;
    }
}
