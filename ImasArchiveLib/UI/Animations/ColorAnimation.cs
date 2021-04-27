namespace Imas.UI
{
    [SerialisationDerivedType(9)]
    public class ColorAnimation : StartEndAnimation
    {
        [SerialiseProperty(200)]
        [Listed(200)]
        public uint StartColor { get; set; } = 0xFFFFFFFF;
        [SerialiseProperty(201)]
        [Listed(201)]
        public uint EndColor { get; set; } = 0xFFFFFFFF;
    }
}
