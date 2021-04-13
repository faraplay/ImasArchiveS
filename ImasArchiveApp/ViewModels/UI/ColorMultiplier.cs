namespace ImasArchiveApp
{
    public struct ColorMultiplier
    {
        public double r;
        public double g;
        public double b;

        public static ColorMultiplier One() => new ColorMultiplier
        {
            r = 1.0,
            g = 1.0,
            b = 1.0
        };

        public void Scale(double red, double green, double blue)
        {
            r *= red;
            g *= green;
            b *= blue;
        }
    }
}
