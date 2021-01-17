using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Imas.UI
{
    public class SpriteCollection : Control
    {
        public int e1, e2;
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
                childSpriteGroups.Add(CreateFromStream<SpriteGroup>(subcomponent, this, stream));
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
    }
}
