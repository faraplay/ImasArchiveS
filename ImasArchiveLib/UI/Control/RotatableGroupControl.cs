namespace Imas.UI
{
    [SerialisationDerivedType(5)]
    public class RotatableGroupControl : GroupControl
    {
        [SerialiseProperty(200)]
        [Listed(200)]
        public float Angle { get; set; }

        [SerialiseProperty(201)]
        [Listed(201)]
        public float E2 { get; set; }
        [SerialiseProperty(201)]
        [Listed(202)]
        public float E3 { get; set; }

    }
}
