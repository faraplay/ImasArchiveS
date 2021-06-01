namespace Imas.UI
{
    [SerialisationDerivedType(0)]
    public class Animation0 : Animation
    {
        [SerialiseProperty(100)]
        public float Time { get; set; }
        [Listed(100)]
        public float Frame
        {
            get => Time * 60f;
            set => Time = value / 60f;
        }
    }
}
