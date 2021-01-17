using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Imas.UI
{
    class Control3 : Control
    {
        public SpriteGroup otherSprite;
        public float e1, e2;

        protected override void Deserialise(Stream stream)
        {
            type = 3;
            base.Deserialise(stream);
            e1 = Binary.ReadFloat(stream, true);
            otherSprite = CreateFromStream<SpriteGroup>(subcomponent, this, stream);
            e2 = Binary.ReadFloat(stream, true);

        }
        public override void Serialise(Stream stream)
        {
            base.Serialise(stream);
            Binary.WriteFloat(stream, true, e1);
            otherSprite.Serialise(stream);
            Binary.WriteFloat(stream, true, e2);
        }
    }
}
