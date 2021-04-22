namespace Imas.UI
{
    [SerialisationDerivedType(5)]
    public class OpacityAnimation : StartEndAnimation
    {
        [SerialiseProperty(200)]
		[Listed(200)]
        public float StartTransparency { get; set; }
        [SerialiseProperty(201)]
		[Listed(201)]
        public float EndTransparency { get; set; }
    }
}
