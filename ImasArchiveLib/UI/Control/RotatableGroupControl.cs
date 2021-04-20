namespace Imas.UI
{
    [SerialisationDerivedType(5)]
    public class RotatableGroupControl : GroupControl
    {
        [SerialiseField(200)]
        public float angle;
        [Listed(200)]
        public float Angle { get => angle; set => angle = value; }

        [SerialiseField(201)]
        public float e2;
        [Listed(201)]
        public float E2 { get => e2; set => e2 = value; }
        [SerialiseField(201)]
        public float e3;
        [Listed(202)]
        public float E3 { get => e3; set => e3 = value; }

    }
}
