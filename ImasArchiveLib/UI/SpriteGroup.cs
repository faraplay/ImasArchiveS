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
        public List<Sprite> sprites = new List<Sprite>();

        internal static SpriteGroup CreateFromStream(UISubcomponent parent, Stream stream)
        {
            SpriteGroup spriteGroup = new SpriteGroup
            {
                parent = parent
            };
            spriteGroup.Deserialise(stream);
            return spriteGroup;
        }
        private void Deserialise(Stream stream)
        {
            int spriteCount = Binary.ReadInt32(stream, true);
            for (int i = 0; i < spriteCount; i++)
            {
                sprites.Add(Sprite.CreateFromStream(parent, stream));
            }
        }
        public void Serialise(Stream stream)
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
                sprite.Draw(g, childTransform, color);
            }
        }
    }
}
