using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Imas.UI
{
    class SpriteCollection : Control
    {
        public int childCount;
        public int e1, e2;
        public List<SpriteGroup> childControls;

        protected SpriteCollection(UIComponent parent) : base(parent) { }

        internal static SpriteCollection CreateFromStream(UIComponent parent, Stream stream)
        {
            SpriteCollection spriteCollection = new SpriteCollection(parent);
            spriteCollection.Deserialise(stream);
            return spriteCollection;
        }

        protected override void Deserialise(Stream stream)
        {
            type = 10;
            base.Deserialise(stream);

            childCount = Binary.ReadInt32(stream, true);
            e1 = Binary.ReadInt32(stream, true);
            e2 = Binary.ReadInt32(stream, true);

            childControls = new List<SpriteGroup>(childCount);
            for (int i = 0; i < childCount; i++)
            {
                childControls.Add(SpriteGroup.CreateFromStream(parent, stream));
            }
        }
    }
}
