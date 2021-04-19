namespace Imas.UI
{
    [SerialisationBaseType]
    public abstract class Control : UIElement
    {
        [SerialiseField(0, FixedCount = 16)]
        public byte[] nameBuffer;
        [Listed(0)]
        public string Name
        {
            get => ImasEncoding.Ascii.GetString(nameBuffer);
            set => ImasEncoding.Ascii.GetBytes(value, nameBuffer);
        }

        [SerialiseField(1)]
        public float xpos;
        [Listed(1)]
        public float Xpos { get => xpos; set => xpos = value; }
        [SerialiseField(2)]
        public float ypos;
        [Listed(2)]
        public float Ypos { get => ypos; set => ypos = value; }
        [SerialiseField(3)]
        public float width;
        [Listed(3)]
        public float Width { get => width; set => width = value; }
        [SerialiseField(4)]
        public float height;
        [Listed(4)]
        public float Height { get => height; set => height = value; }

        [SerialiseField(5)]
        public int a1;
        [Listed(5)]
        public int A1 { get => a1; set => a1 = value; }
        [SerialiseField(6)]
        public int a2;
        [Listed(6)]
        public int A2 { get => a2; set => a2 = value; }
        [SerialiseField(7)]
        public int a3;
        [Listed(7)]
        public int A3 { get => a3; set => a3 = value; }
        [SerialiseField(8)]
        public int a4;
        [Listed(8)]
        public int A4 { get => a4; set => a4 = value; }
        [SerialiseField(9)]
        public float b1;
        [Listed(9)]
        public float B1 { get => b1; set => b1 = value; }
        [SerialiseField(10)]
        public float b2;
        [Listed(10)]
        public float B2 { get => b2; set => b2 = value; }
        [SerialiseField(11)]
        public float b3;
        [Listed(11)]
        public float B3 { get => b3; set => b3 = value; }
        [SerialiseField(12)]
        public float b4;
        [Listed(12)]
        public float B4 { get => b4; set => b4 = value; }
        [SerialiseField(13)]
        public int c1;
        [Listed(13)]
        public int C1 { get => c1; set => c1 = value; }
        [SerialiseField(14)]
        public int c2;
        [Listed(14)]
        public int C2 { get => c2; set => c2 = value; }
        [SerialiseField(15)]
        public int c3;
        [Listed(15)]
        public int C3 { get => c3; set => c3 = value; }
        [SerialiseField(16)]
        public int c4;
        [Listed(16)]
        public int C4 { get => c4; set => c4 = value; }

        [SerialiseField(17)]
        public byte alpha;
        [Listed(17)]
        public byte Alpha { get => alpha; set => alpha = value; }
        [SerialiseField(18)]
        public byte red;
        [Listed(18)]
        public byte Red { get => red; set => red = value; }
        [SerialiseField(19)]
        public byte green;
        [Listed(19)]
        public byte Green { get => green; set => green = value; }
        [SerialiseField(20)]
        public byte blue;
        [Listed(20)]
        public byte Blue { get => blue; set => blue = value; }

        [SerialiseField(21)]
        public float scaleX;
        [Listed(21)]
        public float ScaleX { get => scaleX; set => scaleX = value; }
        [SerialiseField(22)]
        public float scaleY;
        [Listed(22)]
        public float ScaleY { get => scaleY; set => scaleY = value; }
        [SerialiseField(23)]
        public float sourceRight;
        [Listed(23)]
        public float SourceRight { get => sourceRight; set => sourceRight = value; }
        [SerialiseField(24)]
        public float sourceBottom;
        [Listed(24)]
        public float SourceBottom { get => sourceBottom; set => sourceBottom = value; }

        [SerialiseField(25)]
        public uint d1;
        [Listed(25)]
        public uint D1 { get => d1; set => d1 = value; }

        [SerialiseField(26)]
        public SpriteGroup specialSprite;
        [SerialiseField(27, FixedCount = 4, ConditionProperty = nameof(HasExtData))]
        public int[] extData;


        public bool HasExtData => (d1 & 0x04000000) != 0;
        public bool DefaultVisibility => (d1 & 0x04) != 0;

        public override string ToString() => Name;
    }
}
