using System;

namespace Imas.UI
{
    [SerialisationBaseType]
    public abstract class Control : UIElement
    {
        [SerialiseProperty(0)]
        public byte[] NameBuffer { get; set; } = new byte[16];
        [Listed(0)]
        public string Name
        {
            get => ImasEncoding.Ascii.GetString(NameBuffer);
            set
            {
                Array.Clear(NameBuffer, 0, NameBuffer.Length);
                ImasEncoding.Ascii.GetBytes(value, NameBuffer);
            }
        }

        [SerialiseProperty(1)]
        [Listed(1)]
        public float Xpos { get; set; } = 0;
        [SerialiseProperty(2)]
        [Listed(2)]
        public float Ypos { get; set; } = 0;
        [SerialiseProperty(3)]
        [Listed(3)]
        public float Width { get; set; } = 64;
        [SerialiseProperty(4)]
        [Listed(4)]
        public float Height { get; set; } = 64;

        [SerialiseProperty(5)]
        [Listed(5)]
        public int A1 { get; set; } = 0;
        [SerialiseProperty(6)]
        [Listed(6)]
        public int A2 { get; set; } = 0;
        [SerialiseProperty(7)]
        [Listed(7)]
        public int A3 { get; set; } = 0;
        [SerialiseProperty(8)]
        [Listed(8)]
        public int A4 { get; set; } = 0;
        [SerialiseProperty(9)]
        [Listed(9)]
        public float B1 { get; set; } = 10000;
        [SerialiseProperty(10)]
        [Listed(10)]
        public float B2 { get; set; } = 10000;
        [SerialiseProperty(11)]
        [Listed(11)]
        public float B3 { get; set; } = 0;
        [SerialiseProperty(12)]
        [Listed(12)]
        public float B4 { get; set; } = 0;
        [SerialiseProperty(13)]
        [Listed(13)]
        public int C1 { get; set; } = 0;
        [SerialiseProperty(14)]
        [Listed(14)]
        public int C2 { get; set; } = 0;
        [SerialiseProperty(15)]
        [Listed(15)]
        public int C3 { get; set; } = 0;
        [SerialiseProperty(16)]
        [Listed(16)]
        public int C4 { get; set; } = 0;

        [SerialiseProperty(17)]
        [Listed(17)]
        public byte Alpha { get; set; } = 0xFF;
        [SerialiseProperty(18)]
        [Listed(18)]
        public byte Red { get; set; } = 0xFF;
        [SerialiseProperty(19)]
        [Listed(19)]
        public byte Green { get; set; } = 0xFF;
        [SerialiseProperty(20)]
        [Listed(20)]
        public byte Blue { get; set; } = 0xFF;

        [SerialiseProperty(21)]
        [Listed(21)]
        public float ScaleX { get; set; } = 1;
        [SerialiseProperty(22)]
        [Listed(22)]
        public float ScaleY { get; set; } = 1;
        [SerialiseProperty(23)]
        [Listed(23)]
        public float SourceRight { get; set; } = 0;
        [SerialiseProperty(24)]
        [Listed(24)]
        public float SourceBottom { get; set; } = 0;

        [SerialiseProperty(25)]
        [Listed(25)]
        public uint D1 { get; set; } = 7;

        [SerialiseProperty(26)]
        public SpriteGroup SpecialSprite { get; set; } = new SpriteGroup();
        [SerialiseProperty(27, ConditionProperty = nameof(HasExtData))]
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
