namespace HNZ.Utils
{
    public static class MathUtils
    {
        public static float PositiveMod(float a, float b)
        {
            return (a % b + b) % b;
        }
    }
}