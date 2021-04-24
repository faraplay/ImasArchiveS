namespace Imas.UI
{
    public class Sprite : UIElement
    {
        [SerialiseProperty(0, FixedCount = 9)]
        // (9*4 bytes of 0s)
        public uint[] Start { get; set; } = new uint[9];
        [Listed(0)]
        public uint Start0 { get => Start[0]; set => Start[0] = value; }
        [Listed(1)]
        public uint Start1 { get => Start[1]; set => Start[1] = value; }
        [Listed(2)]
        public uint Start2 { get => Start[2]; set => Start[2] = value; }
        [Listed(3)]
        public uint Start3 { get => Start[3]; set => Start[3] = value; }
        [Listed(4)]
        public uint Start4 { get => Start[4]; set => Start[4] = value; }
        [Listed(5)]
        public uint Start5 { get => Start[5]; set => Start[5] = value; }
        [Listed(6)]
        public uint Start6 { get => Start[6]; set => Start[6] = value; }
        [Listed(7)]
        public uint Start7 { get => Start[7]; set => Start[7] = value; }
        [Listed(8)]
        public uint Start8 { get => Start[8]; set => Start[8] = value; }

        [SerialiseProperty(9)]
        [Listed(9)]
        public float Xpos { get; set; } = 0;
        [SerialiseProperty(10)]
        [Listed(10)]
        public float Ypos { get; set; } = 0;
        [SerialiseProperty(11)]
        [Listed(11)]
        public float Width { get; set; } = 64;
        [SerialiseProperty(12)]
        [Listed(12)]
        public float Height { get; set; } = 64;

        [SerialiseProperty(13)]
        [Listed(13)]
        public int A1 { get; set; } = 0;
        [SerialiseProperty(14)]
        [Listed(14)]
        public int A2 { get; set; } = 0;
        [SerialiseProperty(15)]
        [Listed(15)]
        public float B1 { get; set; } = 10000;
        [SerialiseProperty(16)]
        [Listed(16)]
        public float B2 { get; set; } = 10000;
        [SerialiseProperty(17)]
        [Listed(17)]
        public float B3 { get; set; } = 1;
        [SerialiseProperty(18)]
        [Listed(18)]
        public float B4 { get; set; } = 0;
        [SerialiseProperty(19)]
        [Listed(19)]
        public int SrcImageID { get; set; }

        [SerialiseProperty(20)]
        [Listed(20)]
        public byte Alpha { get; set; } = 0xFF;
        [SerialiseProperty(21)]
        [Listed(21)]
        public byte Red { get; set; } = 0xFF;
        [SerialiseProperty(22)]
        [Listed(22)]
        public byte Green { get; set; } = 0xFF;
        [SerialiseProperty(23)]
        [Listed(23)]
        public byte Blue { get; set; } = 0xFF;

        [SerialiseProperty(24)]
        public float SrcFracLeft { get; set; } = 0;
        [SerialiseProperty(25)]
        public float SrcFracTop { get; set; } = 0;
        [SerialiseProperty(26)]
        public float SrcFracRight { get; set; } = 1;
        [SerialiseProperty(27)]
        public float SrcFracBottom { get; set; } = 1;
    }
}
