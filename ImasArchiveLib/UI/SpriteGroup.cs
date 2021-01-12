using System;
using System.Collections.Generic;
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
    }
}
