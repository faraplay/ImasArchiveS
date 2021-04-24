namespace ImasArchiveApp
{
    public struct ColorMultiplier
    {
        public float r;
        public float g;
        public float b;

        public static ColorMultiplier One() => new ColorMultiplier
        {
            r = 1.0f,
            g = 1.0f,
            b = 1.0f
        };

        public void Scale(float red, float green, float blue)
        {
            r *= red;
            g *= green;
            b *= blue;
        }
    }
}
