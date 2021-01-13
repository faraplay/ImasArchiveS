using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;

namespace Imas.UI
{
    class GroupControl : Control
    {
        public int childCount;
        public List<Control> childControls;

        internal static GroupControl CreateFromStream(Stream stream)
        {
            GroupControl groupControl = new GroupControl();
            groupControl.Deserialise(stream);
            return groupControl;
        }

        protected override void Deserialise(Stream stream)
        {
            type = 4;
            base.Deserialise(stream);

            childCount = Binary.ReadInt32(stream, true);
            childControls = new List<Control>(childCount);
            for (int i = 0; i < childCount; i++)
            {
                childControls.Add(Control.Create(stream));
            }
        }

        public override void Draw(Graphics g, ImageSource imageSource, Matrix transform)
        {
            base.Draw(g, imageSource, transform); // this changes the matrix transform
            foreach (Control childControl in childControls)
            {
                using Matrix childTransform = transform.Clone();
                childControl.Draw(g, imageSource, childTransform);
            }
        }
    }
}
