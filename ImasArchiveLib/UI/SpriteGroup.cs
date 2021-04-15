using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Imas.UI
{
    public class SpriteGroup : UIElement
    {
        [SerialiseField(0, IsCountOf = nameof(sprites))]
        public int spriteCount;
        [SerialiseField(1, CountField = nameof(spriteCount))]
        public List<Sprite> sprites = new List<Sprite>();

        protected override void Deserialise(Stream stream)
        {
            int spriteCount = Binary.ReadInt32(stream, true);
            for (int i = 0; i < spriteCount; i++)
            {
                sprites.Add(CreateFromStream<Sprite>(subcomponent, this, stream));
            }
        }
        public override void Serialise(Stream stream)
        {
            Binary.WriteInt32(stream, true, sprites.Count);
            foreach (Sprite sprite in sprites)
            {
                sprite.Serialise(stream);
            }
        }

        public override void Draw(Graphics g, Matrix transform, ColorMatrix color)
        {
            foreach (Sprite sprite in sprites)
            {
                using Matrix childTransform = transform.Clone();
                sprite.DrawIfVisible(g, childTransform, color);
            }
        }
    }
}
