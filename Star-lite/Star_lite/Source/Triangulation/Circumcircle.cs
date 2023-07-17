using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Starlite.Triangulation
{
    public class Circumcircle
    {
        public Vertex c;
        public float r;
        public Circumcircle(Vertex v0, Vertex v1, Vertex v2)
        {
            float d = calcD(v0, v1, v2);
            float x = calcX(v0, v1, v2, d);
            float y = calcY(v0, v1, v2, d);
            r = (float)Math.Sqrt(Math.Pow(v0.x - x, 2) + Math.Pow(v0.y - y, 2));
            c = new Vertex(x, y);
        }


        private float calcD(Vertex v0, Vertex v1, Vertex v2)
        {
            return (float)((v0.x * (v1.y - v2.y) + v1.x * (v2.y - v0.y) + v2.x * (v0.y - v1.y)) * 2);
        }
        private float calcX(Vertex v0, Vertex v1, Vertex v2, float d)
        {
            float f = (float)((1 / d) * ((Math.Pow(v0.x, 2) + Math.Pow(v0.y, 2)) * (v1.y - v2.y) + (Math.Pow(v1.x, 2) + Math.Pow(v1.y, 2)) * (v2.y - v0.y) +
                   (Math.Pow(v2.x, 2) + Math.Pow(v2.y, 2)) * (v0.y - v1.y)));
            return f;
        }

        private float calcY(Vertex v0, Vertex v1, Vertex v2, float d)
        {
            float f = (float)((1 / d) * ((Math.Pow(v0.x, 2) + Math.Pow(v0.y, 2)) * (v2.x - v1.x) + (Math.Pow(v1.x, 2) + Math.Pow(v1.y, 2)) * (v0.x - v2.x) +
                   (Math.Pow(v2.x, 2) + Math.Pow(v2.y, 2)) * (v1.x - v0.x)));
            return f;
        }
    }
}
