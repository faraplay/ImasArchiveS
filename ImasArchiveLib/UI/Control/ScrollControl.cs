namespace Imas.UI
{
    [SerialisationDerivedType(6)]
    public class ScrollControl : GroupControl
    {
        [SerialiseField(200)]
        public float e1;
        [Listed(200)]
        public float E1 { get => e1; set => e1 = value; }
        [SerialiseField(201)]
        public float e2;
        [Listed(201)]
        public float E2 { get => e2; set => e2 = value; }
        [SerialiseField(202)]
        public float e3;
        [Listed(202)]
        public float E3 { get => e3; set => e3 = value; }
        [SerialiseField(203)]
        public float e4;
        [Listed(203)]
        public float E4 { get => e4; set => e4 = value; }

        [SerialiseField(204)]
        public uint f1;
        [Listed(204)]
        public uint F1 { get => f1; set => f1 = value; }
    }
}
