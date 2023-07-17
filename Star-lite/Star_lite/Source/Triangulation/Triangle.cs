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
    public class Triangle
    {
        public Vertex v0, v1, v2;
        public Edge[] edges;
        public Circumcircle circle;
        public Triangle(Vertex v0, Vertex v1, Vertex v2)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;

            edges = new Edge[3];
            edges[0] = new Edge(v0, v1);
            edges[1] = new Edge(v1, v2);
            edges[2] = new Edge(v2, v0);

            circle = new Circumcircle(v0, v1, v2);
        }

        public bool HasVertex(Vertex v)
        {
            foreach (Edge e in edges)
                if (e.HasVertex(v))
                    return true;
            return false;
        }

        public bool HasEdge(Edge e)
        {
            foreach (Edge edge in edges)
                if (edge.Equals(e))
                    return true;
            return false;
        }
        public bool InCircle(Vertex v)
        {
            float dx = circle.c.x - v.x;
            float dy = circle.c.y - v.y;
            return Math.Sqrt(dx * dx + dy * dy) <= circle.r;
        }

        public void Draw(SpriteBatch batch, Texture2D t)
        {
            foreach (Edge edge in edges)
                edge.Draw(batch, t);
        }
    }
}
