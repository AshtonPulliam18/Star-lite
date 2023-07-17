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
using Starlite.Map;

namespace Starlite.Triangulation
{
    public class Edge
    {
        public Vertex v0, v1;
        public int cost;
        public Tile[] tiles;
        public static Random rand = new Random();
        public Edge(Vertex v0, Vertex v1)
        {
            this.v0 = v0;
            this.v1 = v1;
            cost = (int)v0.Distance(v1);
        }

        public void AssignVertices()
        {
            v0.edges.Add(this);
            v1.edges.Add(this);
        }

        public void AssignTiles(Tile[,] map)
        {
            tiles = new Tile[3];
            int x = (int)(this.v0.x / MapGenerator.TileSize);
            int y = (int)(this.v0.y / MapGenerator.TileSize);
            tiles[0] = map[x, y];
            x = (int)((this.v0.x + (this.v1.x - this.v0.x) / 2) / MapGenerator.TileSize);
            y = (int)((this.v0.y + (this.v1.y - this.v0.y) / 2) / MapGenerator.TileSize);
            tiles[1] = map[x, y];
            x = (int)(this.v1.x / MapGenerator.TileSize);
            y = (int)(this.v1.y / MapGenerator.TileSize);
            tiles[2] = map[x, y];
        }

        public override bool Equals(object obj)
        {
            Edge other = (Edge)obj;

            return (this.v0.Equals(other.v0) && this.v1.Equals(other.v1)) ||
                   (this.v0.Equals(other.v1) && this.v1.Equals(other.v0));
        }
        
        public int getMidX()
        {
            return (int)(v0.x + (v1.x - v0.x) / 2);

        }
        public int getMidY()
        {
            return (int)(v0.y + (v1.y - v0.y) / 2);

        }
        public bool HasVertex(Vertex v)
        {
            return v0.Equals(v) || v1.Equals(v);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public float GetCost(Edge other)
        {
            if (Equals(other))
                return float.MaxValue;
            return Distance(other);
        }

        public float Distance(Edge other)
        {
            return v0.Distance(other.v0);
        }
        public void Draw(SpriteBatch batch, Texture2D t)
        {
            DrawLine(new Vector2(v0.x, v0.y), new Vector2(v1.x, v1.y), Color.Gold, batch, t);
        }
        public void Draw(SpriteBatch batch, Texture2D t, Color color)
        {
            DrawLine(new Vector2(v0.x, v0.y), new Vector2(v1.x, v1.y), color, batch, t);
        }
        public void Draw(SpriteBatch batch, Texture2D t, Color color, Vector2 offset)
        {
            DrawLine(new Vector2(v0.x, v0.y), new Vector2(v1.x, v1.y), offset, color, batch, t);
        }
        public void DrawLine(Vector2 start, Vector2 end, Color color, SpriteBatch spriteBatch, Texture2D t)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);

            spriteBatch.Draw(t,
                new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), 1),
                null,
                color,
                angle,
                new Vector2(0, 0),
                SpriteEffects.None,
                0);
        }
        public void DrawLine(Vector2 start, Vector2 end, Vector2 offset, Color color, SpriteBatch spriteBatch, Texture2D t)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);

            spriteBatch.Draw(t,
                new Rectangle((int)start.X + (int)offset.X, (int)start.Y + (int)offset.Y, (int)edge.Length(), 1),
                null,
                color,
                angle,
                new Vector2(0, 0),
                SpriteEffects.None,
                0);
        }
    }
}
