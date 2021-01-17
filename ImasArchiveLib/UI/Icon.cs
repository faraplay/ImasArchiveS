using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Imas.UI
{
    class Icon : GroupControl
    {
        public float angle, e2, e3;

        protected override void Deserialise(Stream stream)
        {
            base.Deserialise(stream);
            type = 5;

            angle = Binary.ReadFloat(stream, true);
            e2 = Binary.ReadFloat(stream, true);
            e3 = Binary.ReadFloat(stream, true);
        }
        public override void Serialise(Stream stream)
        {
            base.Serialise(stream);
            Binary.WriteFloat(stream, true, angle);
            Binary.WriteFloat(stream, true, e2);
            Binary.WriteFloat(stream, true, e3);
        }
        public override void Draw(Graphics g, Matrix transform, ColorMatrix color)
        {
            transform.RotateAt(-angle * (180 / (float)Math.PI), new PointF(xpos, ypos));
            base.Draw(g, transform, color);
        }

    }
}
