namespace Imas.UI
{
    [SerialisationDerivedType(6)]
    public class ScaleAnimation : StartEndAnimation
    {
        [SerialiseProperty(200)]
		[Listed(200)]
        public float StartXScale { get; set; }
        [SerialiseProperty(201)]
		[Listed(201)]
        public float StartYScale { get; set; }
        [SerialiseProperty(202)]
		[Listed(202)]
        public float EndXScale { get; set; }
        [SerialiseProperty(203)]
		[Listed(203)]
        public float EndYScale { get; set; }
    }
}
