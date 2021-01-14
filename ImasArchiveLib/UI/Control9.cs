using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Imas.UI
{
    class Control9 : GroupControl
    {
        public int e1;

        protected override void Deserialise(Stream stream)
        {
            base.Deserialise(stream);
            type = 9;
            e1 = Binary.ReadInt32(stream, true);
        }
    }
}
