namespace Imas.UI
{
    [SerialisationDerivedType(6)]
    public class ScrollControl : GroupControl
    {
        [SerialiseProperty(200)]
        [Listed(200)]
        public float E1 { get; set; }
        [SerialiseProperty(201)]
        [Listed(201)]
        public float E2 { get; set; }
        [SerialiseProperty(202)]
        [Listed(202)]
        public float E3 { get; set; }
        [SerialiseProperty(203)]
        [Listed(203)]
        public float E4 { get; set; }

        [SerialiseProperty(204)]
        [Listed(204)]
        public uint F1 { get; set; }
    }
}
