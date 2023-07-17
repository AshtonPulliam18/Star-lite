using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Starlite.Entities;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Starlite.Map
{
    public class Room
    {
        public static Texture2D TileSet, DoorSet, SideDoorSet;
        public static SoundEffect openDoorFx, closeDoorFx;
        public static Rectangle rightSide, leftSide, door;
        public static Rectangle[] topWalls, botWalls, floors, dmgdFloors, glassPanels, doorFrames;
        public static Random rand;

        public Rectangle Rectangle;
        public Tile centerTile, oldDoor;

        public Tile[,] Tiles { get; set; }
        public List<Tile> doors, underDoors, openDoors, doorsToClose;
        public bool Locked;

        public bool openDoor, wasDiff;
        public float pitch;
        public int alienCount;
        public Room(Tile[,] tiles)
        {
            this.Rectangle = new Rectangle((int)tiles[0, 0].Position.X, (int)tiles[0, 0].Position.Y,
                tiles.GetLength(0) * MapGenerator.TileSize, tiles.GetLength(1) * MapGenerator.TileSize);
            this.Tiles = tiles;
            oldDoor = null;
            openDoors = new List<Tile>();
            doorsToClose = new List<Tile>();
            pitch = (float)(rand.NextDouble() * 0.4);
        }

        public Vector2 GetRandomPos()
        {
            int x = rand.Next(2, Tiles.GetLength(0) - 2);
            int y = rand.Next(2, Tiles.GetLength(1) - 2);
            return Tiles[x, y].Position;
        }

		public void AddAliens(int num, Texture2D tex, Scene scene, List<Entity> entities, List<ICollidable> collidables, Player p)
		{
			for (int i = 0; i < num; i++)
			{
				Enemy enemy = new Enemy(scene, tex, new Rectangle(0, 0, 48, 51), 1f, 0, GetRandomPos(), 0f, 3f, 5f, 3f, 48, 3, 200, p, this);
				entities.Add(enemy);
				collidables.Add(enemy);
                alienCount++;
            }
		}
		public bool InDoor(Vector2 position)
        {
            foreach (Tile t in doors)
                if (Math.Abs(Vector2.Distance(t.Position, position)) <= MapGenerator.TileSize)
                    return true;
            return false;
        }

        public bool InRoom(Rectangle r)
        {
            Rectangle intersection = Rectangle.Intersect(Rectangle, r);
            return r.Width == intersection.Width && r.Height == intersection.Height;
        }

        public bool InRoom(Vector2 position)
        {
            return position.X > Rectangle.X + MapGenerator.TileSize  / 2 &&
                   position.X + MapGenerator.TileSize / 2 < Rectangle.Right - MapGenerator.TileSize / 2 &&
                   position.Y > Rectangle.Y + MapGenerator.TileSize / 2 && position.Y + MapGenerator.TileSize / 2 <
                   Rectangle.Bottom - MapGenerator.TileSize / 2;
        }

        public void Update(Player p)
        {
            if (!Locked)
            {
                Tile nearDoor = PlayerNearDoors(p);
                if (nearDoor != null && !openDoors.Contains(nearDoor) && !doorsToClose.Contains(nearDoor))
                {
                    nearDoor.TileType = TileType.Passable;
                    openDoors.Add(nearDoor);
                    nearDoor.PlayOpenFX(pitch);
                }


                for (int i = openDoors.Count - 1; i >= 0; i--)
                {
                    bool done = openDoors[i].OpenDoor();
                    if (done && openDoors[i] != nearDoor && !doorsToClose.Contains(openDoors[i]))
                    {
                        doorsToClose.Add(openDoors[i]);
                        openDoors[i].PlayCloseFX(pitch);
                        openDoors[i].TileType = TileType.Impassable;
                        openDoors.RemoveAt(i);
                    }
                }

                for (int i = doorsToClose.Count - 1; i >= 0; i--)
                {
                    bool done = doorsToClose[i].CloseDoor();
                    if (done)
                        doorsToClose.RemoveAt(i);
                }
            }
            else
                foreach (Tile t in doors)
                {
                    t.CloseDoor();
                    t.TileType = TileType.Impassable;
                }
        }

        public virtual void FillTiles()
        {
            foreach (Tile tile in Tiles)
            {
                tile.TileType = TileType.Room;
                tile.SetTexture(TileSet);
            }

            Tiles[0, 0].TileType = TileType.RoomCorner;
            Tiles[0, Tiles.GetLength(1) - 1].TileType = TileType.RoomCorner;
            Tiles[Tiles.GetLength(0) - 1, 0].TileType = TileType.RoomCorner;
            Tiles[Tiles.GetLength(0) - 1, Tiles.GetLength(1) - 1].TileType = TileType.RoomCorner;
            centerTile = Tiles[Tiles.GetLength(0) / 2, Tiles.GetLength(1) / 2];
        }

        public static void SetFrames()
        {
            rand = new Random();

            leftSide = new Rectangle(112, 0, 32, 32);
            rightSide = new Rectangle(128, 0, 32, 32);
            door = new Rectangle(0, 0, 32, 32);

            topWalls = new Rectangle[3];
            for (int i = 0; i < 3; i++)
                topWalls[i] = new Rectangle(32 * i, 0, 32, 32);
            botWalls = new Rectangle[2];
            for (int i = 0; i < 2; i++)
                botWalls[i] = new Rectangle(176 + 32 * i, 0, 32, 32);
            floors = new Rectangle[9];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    floors[j + 3 * i] = new Rectangle(j * 32, 64 + 32 * i, 32, 32);
                }
            }

            dmgdFloors = new Rectangle[9];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    dmgdFloors[j + 3 * i] = new Rectangle(128 + j * 32, 64 + 32 * i, 32, 32);
                }
            }

            glassPanels = new Rectangle[9];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    glassPanels[j + 3 * i] = new Rectangle(j * 32, 192 + 32 * i, 32, 32);
                }
            }
        }

        private void middleGlass()
        {
            int midX = (Tiles.GetLength(0) - 1) / 2;
            int midY = (Tiles.GetLength(1) - 1) / 2;

            Tiles[midX - 1, midY - 1].SetFrame(glassPanels[0]);
            Tiles[midX, midY - 1].SetFrame(glassPanels[1]);
            Tiles[midX + 1, midY - 1].SetFrame(glassPanels[2]);

            Tiles[midX - 1, midY].SetFrame(glassPanels[3]);
            Tiles[midX, midY].SetFrame(glassPanels[4]);
            Tiles[midX + 1, midY].SetFrame(glassPanels[5]);

            Tiles[midX - 1, midY + 1].SetFrame(glassPanels[6]);
            Tiles[midX, midY + 1].SetFrame(glassPanels[7]);
            Tiles[midX + 1, midY + 1].SetFrame(glassPanels[8]);
        }

        private void glassStrips()
        {
            for (int i = 2; i < Tiles.GetLength(0) - 2; i++)
            {
                for (int j = 2; j < Tiles.GetLength(1) - 2; j += 2)
                {
                    if (i == 2)
                        Tiles[i, j].SetFrame(glassPanels[3]);
                    else if (i == Tiles.GetLength(0) - 3)
                        Tiles[i, j].SetFrame(glassPanels[5]);
                    else
                        Tiles[i, j].SetFrame(glassPanels[4]);
                }
            }
        }

        public void SetTextures()
        {
            rand = new Random();
            int topInd, botInd;
            topInd = botInd = 0;
            Rectangle[] floorType;
            for (int i = 0; i < Tiles.GetLength(0); i++)
            {
                for (int j = 0; j < Tiles.GetLength(1); j++)
                {
                    if (Tiles[i, j].TileType != TileType.Door)
                    {
                        floorType = rand.Next(10) > 0 ? floors : dmgdFloors;
                        if (j == 0 && i != 0 && i != Tiles.GetLength(0) - 1 && Tiles[i, j].TileType != TileType.Door)
                        {
                            if (i == Tiles.GetLength(0) - 2)
                                Tiles[i, j].SetFrame(topWalls[2]);
                            else if (rand.Next(10) > 0)
                                Tiles[i, j].SetFrame(topWalls[0]);
                            else
                                Tiles[i, j].SetFrame(topWalls[1]);
                        }
                        else if (j == Tiles.GetLength(1) - 1 && i != 0 && i != Tiles.GetLength(0) - 1 &&
                                 Tiles[i, j].TileType != TileType.Door)
                        {
                            Tiles[i, j].SetFrame(botWalls[botInd]);
                            botInd++;
                            if (botInd > botWalls.Length - 1)
                                botInd = 0;
                        }
                        else if (j == 1)
                        {
                            // top left corner floor
                            if (i == 1)
                            {
                                Tiles[i, j].SetFrame(floorType[0]);
                            } // top right corner floor
                            else if (i == Tiles.GetLength(0) - 2)
                            {
                                Tiles[i, j].SetFrame(floorType[2]);
                            } // top row floors
                            else
                            {
                                Tiles[i, j].SetFrame(floorType[1]);
                            }
                        }
                        else if (j == Tiles.GetLength(1) - 2)
                        {
                            // top left corner floor
                            if (i == 1)
                            {
                                Tiles[i, j].SetFrame(floorType[6]);
                            } // top right corner floor
                            else if (i == Tiles.GetLength(0) - 2)
                            {
                                Tiles[i, j].SetFrame(floorType[8]);
                            } // top row floors
                            else
                            {
                                Tiles[i, j].SetFrame(floorType[7]);
                            }
                        }
                        else
                        {
                            if (i == 1)
                            {
                                Tiles[i, j].SetFrame(floorType[3]);
                            } // top right corner floor
                            else if (i == Tiles.GetLength(0) - 2)
                            {
                                Tiles[i, j].SetFrame(floorType[5]);
                            } // top row floors
                            else
                            {
                                Tiles[i, j].SetFrame(floorType[4]);
                            }
                        }

                        if (i == 0)
                            Tiles[i, j].SetFrame(leftSide);
                        else if (i == Tiles.GetLength(0) - 1)
                            Tiles[i, j].SetFrame(rightSide);
                    }
                }
            }

            if (rand.Next(10) < 4)
            {
                if (rand.Next(10) < 5)
                    middleGlass();
                else
                    glassStrips();
            }
        }

        public void SetDoors(Tile[,] mapTiles)
        {
            doors = new List<Tile>();
            underDoors = new List<Tile>();
            foreach (Tile t in Tiles)
            {
                Tile above = tileAbove(t, mapTiles);
                Tile below = tileBelow(t, mapTiles);
                Tile left = tileLeft(t, mapTiles);
                Tile right = tileRight(t, mapTiles);
                if (above != null)
                {
                    if (above.TileType == TileType.Hallway)
                    {
                        if (validDoor(above, mapTiles))
                        {
                            t.TileType = TileType.Door;
                            t.SetTexture(DoorSet);
                            doors.Add(t);
                            underDoors.Add(new Tile(TileType.Passable, TileSet, floors[1], 1f, 0, t.Position, 0f));
                        }
                    }
                }

                if (below != null)
                {
                    if (below.TileType == TileType.Hallway)
                    {
                        if (validDoor(below, mapTiles))
                        {
                            t.TileType = TileType.Door;
                            t.SetTexture(DoorSet);
                            doors.Add(t);
                            underDoors.Add(new Tile(TileType.Passable, TileSet, floors[1], 1f, 0, t.Position, 0f));
                        }
                    }
                }

                if (left != null)
                {
                    if (left.TileType == TileType.Hallway)
                    {
                        if (validDoor(left, mapTiles))
                        {
                            t.TileType = TileType.SideDoor;
                            t.sideDoorLeft = true;
                            t.SetTexture(SideDoorSet);
                            t.SetEffect(SpriteEffects.FlipHorizontally);
                            doors.Add(t);
                            underDoors.Add(new Tile(TileType.Passable, TileSet, floors[1], 1f, 0, t.Position, 0f));
                        }
                    }
                }

                if (right != null)
                {
                    if (right.TileType == TileType.Hallway)
                    {
                        if (validDoor(right, mapTiles))
                        {
                            t.TileType = TileType.SideDoor;
                            t.sideDoorRight = true;
                            t.SetTexture(SideDoorSet);
                            doors.Add(t);
                            underDoors.Add(new Tile(TileType.Passable, TileSet, floors[1], 1f, 0, t.Position, 0f));
                        }
                    }
                }
            }

            foreach (Tile door in doors)
            {
                door.LoadDoorAnimations();
                door.LoadSound(openDoorFx, closeDoorFx);
            }

            foreach (Tile t in Tiles)
            {
                if (t.TileType != TileType.SideDoor && t.TileType != TileType.Door)
                    if (t.Frame == leftSide || t.Frame == rightSide || topWalls.Contains(t.Frame) ||
                        botWalls.Contains(t.Frame))
                        t.TileType = TileType.Impassable;
            }
        }

        public float DistanceToRoom(Room other)
        {
            return Math.Abs(Vector2.Distance(new Vector2(Rectangle.X, Rectangle.Y),
                new Vector2(other.Rectangle.X, other.Rectangle.Y)));
        }

        public Tile PlayerNearDoors(Player p)
        {
            //int x = (int)p.GetWorldPosition().X;
            //int y = (int)p.GetWorldPosition().Y;
            //Rectangle vicinity = new Rectangle(x - MapGenerator.TileSize, y - MapGenerator.TileSize, MapGenerator.TileSize * 2, MapGenerator.TileSize * 2);

            foreach (Tile door in doors)
            {
                //door.IdleDoor();
                if (Math.Abs(Vector2.Distance(p.GetWorldPosition(), door.Position)) <= 96)
                    return door;
            }

            return null;
        }

        private bool validDoor(Tile t, Tile[,] mapTiles)
        {
            Tile above = tileAbove(t, mapTiles);
            Tile below = tileBelow(t, mapTiles);
            Tile left = tileLeft(t, mapTiles);
            Tile right = tileRight(t, mapTiles);
            int hallNeighbors = 0;
            if (above != null)
                if (above.TileType == TileType.Hallway)
                    hallNeighbors++;
            if (below != null)
                if (below.TileType == TileType.Hallway)
                    hallNeighbors++;
            if (left != null)
                if (left.TileType == TileType.Hallway)
                    hallNeighbors++;
            if (right != null)
                if (right.TileType == TileType.Hallway)
                    hallNeighbors++;
            return hallNeighbors < 2;
        }

        public bool Intersects(Room other)
        {
            int padding = MapGenerator.TileSize;
            Rectangle paddedRect = new Rectangle(this.Rectangle.X - padding, this.Rectangle.Y - padding,
                this.Rectangle.Width + padding * 2, this.Rectangle.Height + padding * 2);
            return paddedRect.Intersects(other.Rectangle);
        }

        private static Tile tileAbove(Tile t, Tile[,] tiles)
        {
            if (t.PositionY / MapGenerator.TileSize - 1 > -1)
                return tiles[t.PositionX / MapGenerator.TileSize, t.PositionY / MapGenerator.TileSize - 1];
            return null;
        }

        private static Tile tileBelow(Tile t, Tile[,] tiles)
        {
            if (t.PositionY / MapGenerator.TileSize + 1 < tiles.GetLength(1))
                return tiles[t.PositionX / MapGenerator.TileSize, t.PositionY / MapGenerator.TileSize + 1];
            return null;
        }

        private static Tile tileLeft(Tile t, Tile[,] tiles)
        {
            if (t.PositionX / MapGenerator.TileSize - 1 > -1)
                return tiles[t.PositionX / MapGenerator.TileSize - 1, t.PositionY / MapGenerator.TileSize];
            return null;
        }

        private static Tile tileRight(Tile t, Tile[,] tiles)
        {
            if (t.PositionX / MapGenerator.TileSize + 1 < tiles.GetLength(0))
                return tiles[t.PositionX / MapGenerator.TileSize + 1, t.PositionY / MapGenerator.TileSize];
            return null;
        }
    }
}