namespace Imas.UI
{
    [SerialisationDerivedType(8)]
    public class SpriteAnimation : Animation
    {
        [SerialiseField(100)]
        public float time;

        [SerialiseField(101)]
        public int spriteIndex;
    }
}
