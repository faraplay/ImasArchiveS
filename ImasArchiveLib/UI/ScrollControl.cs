using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Imas.UI
{
    class ScrollControl : GroupControl
    {
        public float e1, e2, e3, e4;
        public uint f1;

        protected override void Deserialise(Stream stream)
        {
            base.Deserialise(stream);
            type = 6;

            e1 = Binary.ReadFloat(stream, true);
            e2 = Binary.ReadFloat(stream, true);
            e3 = Binary.ReadFloat(stream, true);
            e4 = Binary.ReadFloat(stream, true);
            f1 = Binary.ReadUInt32(stream, true);
        }
        public override void Serialise(Stream stream)
        {
            base.Serialise(stream);
            Binary.WriteFloat(stream, true, e1);
            Binary.WriteFloat(stream, true, e2);
            Binary.WriteFloat(stream, true, e3);
            Binary.WriteFloat(stream, true, e4);
            Binary.WriteUInt32(stream, true, f1);
        }

    }
}
