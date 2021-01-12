using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Imas.UI
{
    class Icon : Control
    {
        public int childCount;
        public List<Control> childControls;
        public int e1, e2, e3;

        internal static Icon CreateFromStream(Stream stream)
        {
            Icon icon = new Icon();
            icon.Deserialise(stream);
            return icon;
        }

        protected override void Deserialise(Stream stream)
        {
            type = 5;
            base.Deserialise(stream);

            childCount = Binary.ReadInt32(stream, true);
            childControls = new List<Control>(childCount);
            for (int i = 0; i < childCount; i++)
            {
                childControls.Add(Control.Create(stream));
            }

            e1 = Binary.ReadInt32(stream, true);
            e2 = Binary.ReadInt32(stream, true);
            e3 = Binary.ReadInt32(stream, true);
        }

    }
}
