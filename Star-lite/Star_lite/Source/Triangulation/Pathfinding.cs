using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Starlite.Triangulation;
using Starlite.Map;

namespace Starlite.Triangulation
{
    public static class Pathfinding
    {
        public static HashSet<Tile> justVisited = new HashSet<Tile>();

        public static List<Hallway> PathFind(Edge[] edges, Tile[,] tiles)
        {
            List<Hallway> halls = new List<Hallway>();
            Dictionary<Tile, HashSet<Tile>> graph = GridToGraph(tiles);
            foreach (Edge e in edges)
            {
                List<Tile> path = AStarAlgo(e.tiles[0], e.tiles[2], graph, tiles);
                ResetPath(justVisited);
                justVisited = new HashSet<Tile>();
                Hallway hall = new Hallway(ListToMatrix(path));
                hall.FillTiles();
                halls.Add(hall);
            }
            //foreach (Hallway hall in halls)
            //    hall.FillTiles();
            return halls;
        }

        public static List<Tile> AStarAlgo(Tile start, Tile end, Dictionary<Tile, HashSet<Tile>> graph, Tile[,] grid)
        {
            start.Score = start.HeuristicScore = 0;
            Tile current = null;
            while (true) {
               //if (current == null)
                    current = MinimalNode(grid);
                //else
                //    current = MinimalNode(current, graph);
                current.Visited = true;
                justVisited.Add(current);
                //Vertex currentVertex = new Vertex((int)current.Position.X, (int)current.Position.Y);
                foreach (Tile next in graph[current])
                {
                    if (!next.Visited)
                    {
                        int newScore = CalcScore(current, next);
                        if (newScore < next.Score)
                        {
                            next.Score = newScore;
                            next.HeuristicScore = newScore + CalcHeuristic(next, end);
                            next.TileToRoute = current;
                            justVisited.Add(next);
                        }
                    }
                }
                if (current.Equals(end))
                    return CreatePath(end);
                if (MinimalNode(current, graph).Score == int.MaxValue)
                    return null;
            }
        }

        private static Tile[,] ListToMatrix(List<Tile> tiles)
        {
            Tile[,] matrix = new Tile[1, tiles.Count];
            for (int i = 0; i < matrix.GetLength(1); i++)
                matrix[0, i] = tiles[i];
            return matrix;
        }
        private static void ResetPath(HashSet<Tile> path)
        {
            foreach (Tile t in path)
            {
                t.TileToRoute = null;
                t.Score = t.HeuristicScore = int.MaxValue;
                t.Visited = false;
            }
        }
        private static void ResetGrid(Tile[,] grid)
        {
            foreach (Tile t in grid)
            {
                t.TileToRoute = null;
                t.Score = t.HeuristicScore = int.MaxValue;
                t.Visited = false;
            }
        }

        private static List<Tile> CreatePath(Tile end)
        {
            List<Tile> path = new List<Tile>();
            Tile current = end;
            while (current != null)
            {
                path.Add(current);
                current = current.TileToRoute;
            }
            return path;  
        }
        private static int CalcScore(Tile current, Tile next)
        {
            int stepCost = (int)Vector2.Distance(current.Position, next.Position);
            if (next.TileType == TileType.Hallway || next.TileType == TileType.Room)
            {
                return current.Score + stepCost / 10;
            }
            return current.Score + stepCost;
        }
        private static int CalcHeuristic(Tile next, Tile end)
        {
            if (next.TileType == TileType.Hallway || next.TileType == TileType.Room)
            {
                return (int)Vector2.Distance(next.Position, end.Position) / 10;
            }
            return (int)Vector2.Distance(next.Position, end.Position);
        }

        private static Tile MinimalNode(Tile[,] grid)
        {
            Tile result = null;
            foreach (Tile t in grid)
            {
                if (t.TileType != TileType.RoomCorner)
                {
                    if (result == null)
                        result = t;
                    if (!t.Visited && t.HeuristicScore < result.HeuristicScore)
                        result = t;
                }
            }
            return result;
        }
        private static Tile MinimalNode(Tile current, Dictionary<Tile, HashSet<Tile>> graph)
        {
            Tile result = null;
            foreach (Tile t in graph[current])
            {
                if (t.TileType != TileType.RoomCorner)
                {
                    if (result == null)
                        result = t;
                    if (!t.Visited && t.HeuristicScore < result.HeuristicScore)
                        result = t;
                }
            }
            return result;
        }
        private static Dictionary<Tile, HashSet<Tile>> GridToGraph(Tile[,] grid)
        {
            Dictionary<Tile, HashSet<Tile>> graph = new Dictionary<Tile, HashSet<Tile>>();

            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    Tile tile = grid[i, j];
                    if (tile.TileType != TileType.RoomCorner)
                    {
                        graph.Add(tile, new HashSet<Tile>());
                        if (i + 1 < grid.GetLength(0))
                            graph[tile].Add(grid[i + 1, j]);
                        if (i - 1 > -1)
                            graph[tile].Add(grid[i - 1, j]);
                        if (j + 1 < grid.GetLength(1))
                            graph[tile].Add(grid[i, j + 1]);
                        if (j - 1 > -1)
                            graph[tile].Add(grid[i, j - 1]);
                    }
                }
            }
            return graph;
        }
    }
}
