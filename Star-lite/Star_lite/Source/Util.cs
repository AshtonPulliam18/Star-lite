using Microsoft.Xna.Framework;
using System;

namespace Starlite
{
    public static class Util
    {
        public static int Clamp(int input, int a, int b)
        {
            if (input < a)
                return a;
            if (input > b)
                return b;

            return input;
        }

        public static float Clamp(float input, float a, float b)
        {
            if (input < a)
                return a;
            if (input > b)
                return b;

            return input;
        }

        public static Vector2 Normalized(this Vector2 input)
        {
            if (input.X == 0.0f)
                return Vector2.UnitY * Math.Sign(input.Y);
            if (input.Y == 0.0f)
                return Vector2.UnitX * Math.Sign(input.X);
            return Vector2.Normalize(input);
        }

        public static float EaseInOutSine(this float x)
        {
            return (float) -(Math.Cos(Math.PI * (double) x) - 1) / 2;
        }

        public static float EaseOutVignette(float t)
        {
            const float decreaseRate = 5.0f;
            const float falloffTime = 0.5f;

            if (t > 3.0f)
                t = 3.0f;

            var shifted = t - 2.0f;

            
            return t < 2.0f
                ? 0.0f
                : shifted * shifted;
        }

        public static float Lerp(float a, float b, float t)
        {
            if (t < 0.0f)
                t = 0.0f;
            else if (t > 1.0f)
                t = 1.0f;
            
            return a * (1.0f - t) + b * t;
        }
        public static int[] ColorDist(Color c1, Color c2, float mod)
        {
            int r = (int)((c2.R - c1.R) * mod);
            int g = (int)((c2.G - c1.G) * mod);
            int b = (int)((c2.B - c1.B) * mod);
            return new int[] { r, g, b };
        }

        public static Color ColorAdd(Color c1, int[] rgb)
        {
            return new Color(c1.R + rgb[0], c1.G + rgb[1], c1.B + rgb[2]);
        }

        public static Rectangle RecPlusOff(Rectangle r, Vector2 offset)
        {
            return new Rectangle(r.X + (int)offset.X, r.Y + (int)offset.Y, r.Width, r.Height);
        }

        public static Rectangle RecMinusOff(Rectangle r, Vector2 offset)
        {
            return new Rectangle(r.X - (int)offset.X, r.Y - (int)offset.Y, r.Width, r.Height);
        }
    }
}