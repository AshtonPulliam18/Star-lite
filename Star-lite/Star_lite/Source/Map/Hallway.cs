using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Starlite.Map
{
    public class Hallway : Room
    {
        private int botInd;
        public Hallway(Tile[,] tiles) : base(tiles)
        {
            this.Tiles = tiles;
        }

        public override void FillTiles()
        {
            List<Tile> roomless = new List<Tile>();
            foreach (Tile t in Tiles)
            {
                if (t.TileType != TileType.Room)
                {
                    roomless.Add(t);
                    t.TileType = TileType.Hallway;
                }
            }
            Tile[,] tiles = new Tile[1, roomless.Count];
            for (int i = 0; i < tiles.GetLength(1); i++)
                tiles[0, i] = roomless[i];
            Tiles = tiles;
        }

        public void Transform(Tile[,] mapTiles)
        {
            foreach (Tile t in Tiles)
            {
                t.SetTexture(TileSet);
                t.SetFrame(floors[1]);
            }
            //botInd = 0;
            //for (int i = 0; i < Tiles.GetLength(0); i++)
            //{
            //    for (int j = 0; j < Tiles.GetLength(1); j++)
            //    {
            //        Tiles[i, j].SetTexture(TileSet);
            //        Tiles[i, j].SetFrame(floors[1]);
            //        Tile above = tileAbove(Tiles[i, j], mapTiles);
            //        Tile below = tileBelow(Tiles[i, j], mapTiles);
            //        Tile left = tileLeft(Tiles[i, j], mapTiles);
            //        Tile right = tileRight(Tiles[i, j], mapTiles);
            //        TransformVertical(above, below, left, right);
            //        TransformHorizontal(above, below, left, right);
            //    }
            //}
            //Tile start = Tiles[0, 0];
            //Tile end = Tiles[Tiles.GetLength(0) - 1, Tiles.GetLength(1) - 1];

            //start.TileType = TileType.Door;
            //start.SetTexture(DoorSet);
            //start.SetFrame(door);
            //end.TileType = TileType.Door;
            //end.SetTexture(DoorSet);
            //end.SetFrame(door);
            //// start door
            ////if (tileAbove(start, mapTiles).TileType == TileType.Room)
            ////    tileAbove(start, mapTiles).TileType = TileType.Door;
            ////else if (tileBelow(start, mapTiles).TileType == TileType.Room)
            ////    tileBelow(start, mapTiles).TileType = TileType.Door;
            ////else if (tileLeft(start, mapTiles).TileType == TileType.Room)
            ////    tileLeft(start, mapTiles).TileType = TileType.Door;
            ////else if (tileRight(start, mapTiles).TileType == TileType.Room)
            ////    tileRight(start, mapTiles).TileType = TileType.Door;
            ////// end door
            ////if (tileAbove(end, mapTiles).TileType == TileType.Room)
            ////    tileAbove(end, mapTiles).TileType = TileType.Door;
            ////else if (tileBelow(end, mapTiles).TileType == TileType.Room)
            ////    tileBelow(end, mapTiles).TileType = TileType.Door;
            ////else if (tileLeft(end, mapTiles).TileType == TileType.Room)
            ////    tileLeft(end, mapTiles).TileType = TileType.Door;
            ////else if (tileRight(end, mapTiles).TileType == TileType.Room)
            ////    tileRight(end, mapTiles).TileType = TileType.Door;
        }
        private void TransformVertical(Tile above, Tile below, Tile left, Tile right)
        {
            Tile present = above ?? below ?? null;
            if (present != null)
            {
                if (left != null)
                    if (left.TileType == TileType.Impassable)
                    {
                        //left.TileType = TileType.Hallway;
                        left.SetTexture(TileSet);
                        left.SetFrame(leftSide);
                    }
                if (right != null)
                    if (right.TileType == TileType.Impassable)
                    {
                        //right.TileType = TileType.Hallway;
                        right.SetTexture(TileSet);
                        right.SetFrame(rightSide);
                    }
            }
        }

        private void TransformHorizontal(Tile above, Tile below, Tile left, Tile right)
        {
            Tile present = left ?? right ?? null;
            if (present != null)
            {
                if (above != null)
                    if (above.TileType == TileType.Impassable)
                    {
                       //above.TileType = TileType.Impassable;
                        //above.SetTexture(TileSet);
                       // above.SetFrame(topWalls[0]);
                    }
                if (below != null)
                    if (below.TileType == TileType.Impassable)
                    {
                        //below.TileType = TileType.Hallway;
                        //below.SetTexture(TileSet);
                        //below.SetFrame(botWalls[botInd]);
                      //  botInd++;
                       // if (botInd > 1)
                          //  botInd = 0;
                    }
            }
        }

        
    }
}
