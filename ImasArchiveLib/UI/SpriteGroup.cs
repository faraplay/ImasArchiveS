using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;

namespace Imas.UI
{
    public class SpriteGroup
    {
        public UIComponent parent;
        public int spriteCount;
        public List<Sprite> sprites = new List<Sprite>();

        internal static SpriteGroup CreateFromStream(UIComponent parent, Stream stream)
        {
            SpriteGroup spriteGroup = new SpriteGroup();
            spriteGroup.parent = parent;
            spriteGroup.Deserialise(stream);
            return spriteGroup;
        }
        private void Deserialise(Stream stream)
        {
            spriteCount = Binary.ReadInt32(stream, true);
            for (int i = 0; i < spriteCount; i++)
            {
                sprites.Add(Sprite.CreateFromStream(parent, stream));
            }
        }

        public void Draw(Graphics g, Matrix transform)
        {
            foreach (Sprite sprite in sprites)
            {
                using Matrix childTransform = transform.Clone();
                sprite.Draw(g, childTransform);
            }
        }
    }
}
