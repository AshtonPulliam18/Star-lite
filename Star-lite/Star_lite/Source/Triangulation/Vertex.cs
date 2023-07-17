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
    public class Vertex
    {
        public static Random rand = new Random();
        public float x, y;
        public List<Edge> edges;
        public int cost;
        public Vertex(float x, float y)
        {
            this.x = x;
            this.y = y;
            edges = new List<Edge>();
        }

        public Vertex(int x, int y)
        {
            this.x = (float)x;
            this.y = (float)y;
        }
        public override bool Equals(object obj)
        {
            Vertex other = (Vertex)obj;
            return this.x == other.x && this.y == other.y;
        }

        public float Distance(Vertex other)
        {
            return (float)Math.Sqrt(Math.Pow(other.x - x, 2) + Math.Pow(other.y - y, 2));
        }

        public void Draw(SpriteBatch batch, Texture2D t)
        {
            batch.Draw(t, new Rectangle((int)x, (int)y, 5, 5), Color.Red);
        }
        public void Draw(SpriteBatch batch, Texture2D t, Color color)
        {
            batch.Draw(t, new Rectangle((int)x, (int)y, 5, 5), color);
        }
        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }

        public override int GetHashCode()
        {
            int hashCode = 1502939027;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            return hashCode;
        }
    }
}
