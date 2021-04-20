namespace Imas.UI
{
    [SerialisationDerivedType(5)]
    public class Icon : GroupControl
    {
        [SerialiseField(200)]
        public float angle;
        [Listed(200)]
        public float Angle { get => angle; set => angle = value; }

        [SerialiseField(201)]
        public float e2;
        [Listed(201)]
        public float E2 { get => e2; set => e2 = value; }
        [SerialiseField(201)]
        public float e3;
        [Listed(202)]
        public float E3 { get => e3; set => e3 = value; }

        //public override void Draw(Graphics g, Matrix transform, ColorMatrix color)
        //{
        //    transform.RotateAt(-angle * (180 / (float)Math.PI), new PointF(xpos, ypos));
        //    base.Draw(g, transform, color);
        //}

    }
}
