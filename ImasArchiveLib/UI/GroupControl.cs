﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Imas.UI
{
    public class GroupControl : Control
    {
        public int childCount;
        public List<Control> childControls;

        protected override void Deserialise(Stream stream)
        {
            type = 4;
            base.Deserialise(stream);

            childCount = Binary.ReadInt32(stream, true);
            childControls = new List<Control>(childCount);
            for (int i = 0; i < childCount; i++)
            {
                childControls.Add(Control.Create(parent, stream));
            }
        }

        public override void Draw(Graphics g, Matrix transform, ColorMatrix color)
        {
            base.Draw(g, transform, color); // this changes the matrix transform but not the color
            ColorMatrix newColor = ScaleMatrix(color, alpha, red, green, blue);
            foreach (Control childControl in childControls)
            {
                using Matrix childTransform = transform.Clone();
                childControl.Draw(g, childTransform, newColor);
            }
        }
    }
}