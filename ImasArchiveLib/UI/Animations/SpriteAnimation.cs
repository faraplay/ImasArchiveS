namespace Imas.UI
{
    [SerialisationDerivedType(8)]
    public class SpriteAnimation : Animation
    {
        [SerialiseProperty(100)]
		[Listed(100)]
        public float Time { get; set; }

        [SerialiseProperty(101)]
		[Listed(101)]
        public int SpriteIndex { get; set; }
    }
}
