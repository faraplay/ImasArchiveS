namespace Imas.UI
{
    [SerialisationDerivedType(3)]
    public class Animation3 : StartEndAnimation
    {
        [SerialiseProperty(200)]
		[Listed(200)]
        public int C1 { get; set; }
        [SerialiseProperty(201)]
		[Listed(201)]
        public float D1 { get; set; }
        [SerialiseProperty(202)]
		[Listed(202)]
        public float D2 { get; set; }
        [SerialiseProperty(203)]
		[Listed(203)]
        public int E1 { get; set; }
        [SerialiseProperty(204)]
		[Listed(204)]
        public float F1 { get; set; }
        [SerialiseProperty(205)]
		[Listed(205)]
        public float F2 { get; set; }
    }
}
