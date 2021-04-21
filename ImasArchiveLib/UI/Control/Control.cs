namespace Imas.UI
{
    [SerialisationBaseType]
    public abstract class Control : UIElement
    {
        [SerialiseProperty(0, FixedCount = 16)]
        public byte[] NameBuffer { get; set; }
        [Listed(0)]
        public string Name
        {
            get => ImasEncoding.Ascii.GetString(NameBuffer);
            set => ImasEncoding.Ascii.GetBytes(value, NameBuffer);
        }

        [SerialiseProperty(1)]
        [Listed(1)]
        public float Xpos { get; set; }
        [SerialiseProperty(2)]
        [Listed(2)]
        public float Ypos { get; set; }
        [SerialiseProperty(3)]
        [Listed(3)]
        public float Width { get; set; }
        [SerialiseProperty(4)]
        [Listed(4)]
        public float Height { get; set; }

        [SerialiseProperty(5)]
        [Listed(5)]
        public int A1 { get; set; }
        [SerialiseProperty(6)]
        [Listed(6)]
        public int A2 { get; set; }
        [SerialiseProperty(7)]
        [Listed(7)]
        public int A3 { get; set; }
        [SerialiseProperty(8)]
        [Listed(8)]
        public int A4 { get; set; }
        [SerialiseProperty(9)]
        [Listed(9)]
        public float B1 { get; set; }
        [SerialiseProperty(10)]
        [Listed(10)]
        public float B2 { get; set; }
        [SerialiseProperty(11)]
        [Listed(11)]
        public float B3 { get; set; }
        [SerialiseProperty(12)]
        [Listed(12)]
        public float B4 { get; set; }
        [SerialiseProperty(13)]
        [Listed(13)]
        public int C1 { get; set; }
        [SerialiseProperty(14)]
        [Listed(14)]
        public int C2 { get; set; }
        [SerialiseProperty(15)]
        [Listed(15)]
        public int C3 { get; set; }
        [SerialiseProperty(16)]
        [Listed(16)]
        public int C4 { get; set; }

        [SerialiseProperty(17)]
        [Listed(17)]
        public byte Alpha { get; set; }
        [SerialiseProperty(18)]
        [Listed(18)]
        public byte Red { get; set; }
        [SerialiseProperty(19)]
        [Listed(19)]
        public byte Green { get; set; }
        [SerialiseProperty(20)]
        [Listed(20)]
        public byte Blue { get; set; }

        [SerialiseProperty(21)]
        [Listed(21)]
        public float ScaleX { get; set; }
        [SerialiseProperty(22)]
        [Listed(22)]
        public float ScaleY { get; set; }
        [SerialiseProperty(23)]
        [Listed(23)]
        public float SourceRight { get; set; }
        [SerialiseProperty(24)]
        [Listed(24)]
        public float SourceBottom { get; set; }

        [SerialiseProperty(25)]
        [Listed(25)]
        public uint D1 { get; set; }

        [SerialiseProperty(26)]
        public SpriteGroup SpecialSprite { get; set; }
        [SerialiseProperty(27, FixedCount = 4, ConditionProperty = nameof(HasExtData))]
        public uint[] ExtData { get; set; } = new uint[4];

        [Listed(28, ConditionProperty = nameof(HasExtData))]
        public uint ExtData0 { get => ExtData[0]; set => ExtData[0] = value; }
        [Listed(29, ConditionProperty = nameof(HasExtData))]
        public uint ExtData1 { get => ExtData[1]; set => ExtData[1] = value; }
        [Listed(30, ConditionProperty = nameof(HasExtData))]
        public uint ExtData2 { get => ExtData[2]; set => ExtData[2] = value; }
        [Listed(31, ConditionProperty = nameof(HasExtData))]
        public uint ExtData3 { get => ExtData[3]; set => ExtData[3] = value; }


        public bool HasExtData => (D1 & 0x04000000) != 0;
        public bool DefaultVisibility => (D1 & 0x04) != 0;

        public override string ToString() => Name;
    }
}
