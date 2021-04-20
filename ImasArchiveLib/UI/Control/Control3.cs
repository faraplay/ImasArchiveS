namespace Imas.UI
{
    [SerialisationDerivedType(3)]
    public class Control3 : Control
    {
        [SerialiseField(200)]
        public float e1;
        [Listed(201)]
        public float E1 { get => e1; set => e1 = value; }

        [SerialiseField(101)]
        public SpriteGroup otherSprite;

        [SerialiseField(202)]
        public float e2;
        [Listed(202)]
        public float E2 { get => e2; set => e2 = value; }
    }
}
