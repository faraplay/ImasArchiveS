namespace Imas.UI
{
    [SerialisationDerivedType(8)]
    public class SpriteAnimation : Animation
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
        public int SpriteIndex { get; set; }
    }
}
