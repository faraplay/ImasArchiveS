using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;

namespace Imas.UI
{
    class SpriteGroup
    {
        public int spriteCount;
        public List<Sprite> sprites = new List<Sprite>();

        internal static SpriteGroup CreateFromStream(Stream stream)
        {
            SpriteGroup spriteGroup = new SpriteGroup();
            spriteGroup.Deserialise(stream);
            return spriteGroup;
        }
        private void Deserialise(Stream stream)
        {
            spriteCount = Binary.ReadInt32(stream, true);
            for (int i = 0; i < spriteCount; i++)
            {
                sprites.Add(Sprite.CreateFromStream(stream));
            }
        }

        public void Draw(Graphics g, ImageSource imageSource, Matrix transform)
        {
            foreach (Sprite sprite in sprites)
            {
                using Matrix childTransform = transform.Clone();
                sprite.Draw(g, imageSource, childTransform);
            }
        }
    }
}
