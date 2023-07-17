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
    public class Delaunay
    {
        public static Triangle super;
        public static List<Triangle> Triangulate(List<Vertex> vertices)
        {
            Triangle sT = GetSuper(vertices);
            super = sT;
            List<Triangle> triangles = new List<Triangle>() { sT };


            foreach (Vertex v in vertices)
            {
                triangles = AddVertex(v, triangles);
            }

            List<Triangle> filtered = new List<Triangle>();
            foreach (Triangle t in triangles)
            {
                if (!(t.v0 == sT.v0 || t.v0 == sT.v1 || t.v0 == sT.v2 ||
                      t.v1 == sT.v0 || t.v1 == sT.v1 || t.v1 == sT.v2 ||
                      t.v2 == sT.v0 || t.v2 == sT.v1 || t.v2 == sT.v2))
                {
                    filtered.Add(t);
                }
            }
            triangles = filtered;
            return triangles;
        }

        public static List<Triangle> AddVertex(Vertex v, List<Triangle> triangles)
        {
            List<Edge> edges = new List<Edge>();
            List<Triangle> filtered = new List<Triangle>();
            foreach (Triangle t in triangles)
            {
                if (t.InCircle(v))
                {
                    edges.Add(new Edge(t.v0, t.v1));
                    edges.Add(new Edge(t.v1, t.v2));
                    edges.Add(new Edge(t.v2, t.v0));
                }
                else
                    filtered.Add(t);
            }
            triangles = filtered;
            edges = UniqueEdges(edges);

            foreach (Edge e in edges)
                triangles.Add(new Triangle(e.v0, e.v1, v));

            return triangles;
        }

        public static List<Edge> UniqueEdges(List<Edge> edges)
        {
            List<Edge> uniqueEdges = new List<Edge>();
            for (int i = 0; i < edges.Count; i++)
            {
                bool isUnique = true;

                for (int j = 0; j < edges.Count; j++)
                {
                    if (i != j && edges[i].Equals(edges[j]))
                    {
                        isUnique = false;
                        break;
                    }
                }
                if (isUnique)
                    uniqueEdges.Add(edges[i]);
            }
            return uniqueEdges;
        }
        public static Triangle GetSuper(List<Vertex> vertices)
        {
            float minX, minY;
            float maxX, maxY;
            minX = minY = float.MaxValue;
            maxX = maxY = float.MinValue;
            foreach (Vertex v in vertices)
            {
                minX = Math.Min(minX, v.x);
                minY = Math.Min(minY, v.y);
                maxX = Math.Max(maxX, v.x);
                maxY = Math.Max(maxY, v.y);
            }

            float dx = (maxX - minX) * 10;
            float dy = (maxY - minY) * 10;

            Vertex v0, v1, v2;

            v0 = new Vertex(minX - dx, minY - dy * 3);
            v1 = new Vertex(minX - dx, maxY + dy);
            v2 = new Vertex(maxX + dx * 3, maxY + dy);
            return new Triangle(v0, v1, v2);
        }

    }
}