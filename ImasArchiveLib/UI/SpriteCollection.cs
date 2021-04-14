using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Imas.UI
{
    [SerialisationDerivedType(10)]
    public class SpriteCollection : Control
    {
        [SerialiseField(100)]
        public int childSpriteGroupCount;
        [SerialiseField(101)]
        public int e1;
        [SerialiseField(102)]
        public int e2;
        [SerialiseField(103, CountField = nameof(childSpriteGroupCount))]
        public List<SpriteGroup> childSpriteGroups;

        protected override void Deserialise(Stream stream)
        {
            type = 10;
            base.Deserialise(stream);

            int childCount = Binary.ReadInt32(stream, true);
            e1 = Binary.ReadInt32(stream, true);
            e2 = Binary.ReadInt32(stream, true);

            childSpriteGroups = new List<SpriteGroup>(childCount);
            for (int i = 0; i < childCount; i++)
            {
                SpriteGroup spriteGroup = CreateFromStream<SpriteGroup>(subcomponent, this, stream);
                spriteGroup.myVisible = false;
                childSpriteGroups.Add(spriteGroup);
            }
        }
        public override void Serialise(Stream stream)
        {
            base.Serialise(stream);

            Binary.WriteInt32(stream, true, childSpriteGroups.Count);
            Binary.WriteInt32(stream, true, e1);
            Binary.WriteInt32(stream, true, e2);
            foreach (SpriteGroup spriteGroup in childSpriteGroups)
            {
                spriteGroup.Serialise(stream);
            }
        }
        public override void Draw(Graphics g, Matrix transform, ColorMatrix color)
        {
            base.Draw(g, transform, color); // this changes the matrix transform but not the color
            ColorMatrix newColor = ScaleMatrix(color, alpha, red, green, blue);
            foreach (SpriteGroup spriteGroup in childSpriteGroups)
            {
                using Matrix childTransform = transform.Clone();
                spriteGroup.DrawIfVisible(g, childTransform, newColor);
            }
        }
    }
}
