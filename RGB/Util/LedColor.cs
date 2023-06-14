namespace RGB.Util
{
    public struct LedColor
    {
        public float R, G, B, W;
        public LedColor(float r, float g, float b, float w)
        {
            R = r; G = g; B = b; W = w;
        }

        public static LedColor operator *(LedColor a, LedColor b) => new LedColor(a.R * b.R, a.G * b.G, a.B * b.B, a.W * b.W);
        public static LedColor operator +(LedColor a, LedColor b) => new LedColor(a.R + b.R, a.G + b.G, a.B + b.B, a.W + b.W);
        public static LedColor operator /(LedColor a, int b) => new LedColor(a.R / b, a.G / b, a.B / b, a.W / b);
    }
}
