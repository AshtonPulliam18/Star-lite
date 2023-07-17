using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Starlite.Triangulation
{
    public class Prim
    {
        public static List<Edge> RemoveDuplicates(List<Edge> edges)
        {
            List<Edge> removes = new List<Edge>();
            bool keep = false;
            for (int i = edges.Count - 1; i >= 0; i--)
            {
                for (int j = edges.Count - 1; j >= 0; j--)
                {
                    keep = false;
                    if (i != j && edges[i].Equals(edges[j]))
                    {
                        foreach (Edge edge in removes)
                            if (edge.Equals(edges[j]))
                            {
                                keep = true;
                                break;
                            }
                        if (!keep)
                            removes.Add(edges[j]);
                    }
                }
            }

            foreach (Edge e in removes)
                edges.Remove(e);
            return edges;
        }
        public static Dictionary<Vertex, List<Vertex>> MapToGraph(List<Triangle> triangles, List<Vertex> centers, List<Edge> edges)
        {
            Dictionary<Vertex, List<Vertex>> graph = new Dictionary<Vertex, List<Vertex>>();
            foreach (Triangle t in triangles)
                foreach (Edge e in t.edges)
                    edges.Add(e);

           edges = RemoveDuplicates(edges);

            foreach (Edge edge in edges)
                edge.AssignVertices();

            foreach (Vertex center in centers)
            {
                graph.Add(center, new List<Vertex>());
                foreach (Edge e in center.edges)
                    foreach (Vertex other in centers)
                        if (!other.Equals(center))
                            if (e.HasVertex(other))
                                graph[center].Add(other);
            }

            return graph;
        }
        public static void RelaxTree(Vertex v, Dictionary<Vertex, List<Vertex>> graph, HashSet<Vertex> mst)
        {
            foreach (Vertex vertex in graph[v])
                if (!mst.Contains(vertex))
                    vertex.cost = (int)vertex.Distance(v);
        }

        public static Vertex GreedySelection(Vertex v, Dictionary<Vertex, List<Vertex>> graph, HashSet<Vertex> mst)
        {
            Vertex low = new Vertex(0, 0);
            low.cost = int.MaxValue;
            foreach (Vertex vertex in graph[v])
                if (!mst.Contains(vertex))
                {
                    if (vertex.cost < low.cost)
                    {
                        low = vertex;
                    }
                }
            if (low.cost == int.MaxValue)
                return low;
            return low;
        }
        public static Edge[] PrimsPath(List<Triangle> triangles, List<Vertex> centers, Vertex start)
        {
            List<Edge> edges = new List<Edge>();
            Dictionary<Vertex, List<Vertex>> graph = MapToGraph(triangles, centers, edges);
       
            HashSet<Vertex> mst = new HashSet<Vertex>();
            HashSet<Edge> path = new HashSet<Edge>();
            Vertex current = start;
            mst.Add(current);
            for (int i = 0; i < graph.Keys.Count - 1; i++)
            {
                RelaxTree(current, graph, mst);
                current = GreedySelection(current, graph, mst);
                if (current.cost == int.MaxValue)
                    return null;
                mst.Add(current);
            }
            Random rand = new Random();
            for (int i = 0; i < mst.Count - 1; i++)
                foreach (Edge edge in edges)
                {
                    if (edge.HasVertex(mst.ElementAt(i)) && edge.HasVertex(mst.ElementAt(i + 1)))
                        path.Add(edge);
                    else if (rand.Next(100) < 13)
                        path.Add(edge);
                }
            Edge[] pathAr = new Edge[path.Count];
            for (int i = 0; i < path.Count; i++)
                pathAr[i] = path.ElementAt(i);
            return pathAr;
        }
    }
}
