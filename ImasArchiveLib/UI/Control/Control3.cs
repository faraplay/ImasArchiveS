namespace Imas.UI
{
    [SerialisationDerivedType(3)]
    public class Control3 : Control
    {
        [SerialiseProperty(200)]
        [Listed(201)]
        public float E1 { get; set; }

        [SerialiseProperty(101)]
        public SpriteGroup OtherSprite { get; set; }

        [SerialiseProperty(202)]
        [Listed(202)]
        public float E2 { get; set; }
    }
}
