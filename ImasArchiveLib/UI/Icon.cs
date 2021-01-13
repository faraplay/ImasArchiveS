using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;

namespace Imas.UI
{
    class Icon : GroupControl
    {
        public float angle, e2, e3;

        internal static Icon CreateFromStream(Stream stream)
        {
            Icon icon = new Icon();
            icon.Deserialise(stream);
            return icon;
        }

        protected override void Deserialise(Stream stream)
        {
            base.Deserialise(stream);
            type = 5;

            angle = Binary.ReadFloat(stream, true);
            e2 = Binary.ReadFloat(stream, true);
            e3 = Binary.ReadFloat(stream, true);
            if (e2 != 0 || e3 != 0)
            {
                angle = angle;
            }
        }
        public override void Draw(Graphics g, ImageSource imageSource, Matrix transform)
        {
            transform.RotateAt(-angle * (180 / (float)Math.PI), new PointF(xpos, ypos));
            base.Draw(g, imageSource, transform);
        }

    }
}
