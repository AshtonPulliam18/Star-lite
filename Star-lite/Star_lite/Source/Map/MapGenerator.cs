using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Starlite.Triangulation;
using Starlite.Entities;

namespace Starlite.Map
{
	public class MapGenerator
	{
		// constant, static, public, private
		public const int TileSize = 32;

		public static Texture2D[] space;
		public static Texture2D meleeAlien, boss;
		public Tile[,] Tiles;
		public List<Room> Rooms = new List<Room>();


		private Rectangle[] fgStars, bgStars;
		private Rectangle debris, farPlanets, ringPlanet, closePlanet, rightRoom, leftRoom, topRoom, bottomRoom;
		private int[] bgRotations;
		private Color lightStar, darkStar;


		private Edge[] paths;
		private List<Hallway> halls;
		private List<int> roomDimensions;
		private Random rand;

		public MapGenerator(int xLength, int yLength)
		{
			rand = new Random();

			fgStars = new Rectangle[xLength + yLength];
			for (int i = 0; i < fgStars.Length; i ++)
				fgStars[i] = new Rectangle(rand.Next(xLength * TileSize), rand.Next(yLength * TileSize), rand.Next(5, 7), rand.Next(5, 7));
			bgStars = new Rectangle[xLength + yLength];
			for (int i = 0; i < bgStars.Length; i++)
				bgStars[i] = new Rectangle(rand.Next(xLength * TileSize), rand.Next(yLength * TileSize), rand.Next(2, 4), rand.Next(2, 4));
			lightStar = new Color(255, 255, 255);
			darkStar = new Color(220, 220, 200, 200);

			bgRotations = new int[4];

			debris = new Rectangle(rand.Next(Scene.ScreenWidth, Scene.ScreenWidth * 2), rand.Next(-Scene.ScreenHeight / 3, Scene.ScreenHeight - Scene.ScreenHeight / 3),
								   Scene.ScreenWidth / 2, Scene.ScreenHeight / 2);
			farPlanets = new Rectangle(rand.Next(Scene.ScreenWidth, Scene.ScreenWidth * 4), rand.Next(0, Scene.ScreenHeight - Scene.ScreenHeight / 5),
								   Scene.ScreenWidth / 7, Scene.ScreenHeight / 7);
			ringPlanet = new Rectangle(rand.Next(Scene.ScreenWidth, Scene.ScreenWidth * 4), rand.Next(0, Scene.ScreenHeight - Scene.ScreenHeight / 3),
								   Scene.ScreenWidth / 30, Scene.ScreenHeight / 10);
			closePlanet = new Rectangle(rand.Next(Scene.ScreenWidth, Scene.ScreenWidth * 4), rand.Next(0, Scene.ScreenHeight - Scene.ScreenHeight / 5),
								   Scene.ScreenWidth / 9, Scene.ScreenHeight / 7);
			bgRotations[2] = rand.Next(0, 50);
			roomDimensions = new List<int>();
			for (int i = 0; i < xLength; i++)
            {
				if (i % 2 != 0 && (i > xLength / 10 && i < xLength / 2)) {
					if (roomDimensions.Count < 10)
						roomDimensions.Add(i);
					else
						break;
				}
            }
			this.Tiles = new Tile[xLength, yLength];
			for (int i = 0; i < Tiles.GetLength(0); i++)
			{
				for (int j = 0; j < Tiles.GetLength(1); j++)
				{
					Tiles[i, j] = new Tile(TileType.Impassable, new Vector2(i * MapGenerator.TileSize, j * MapGenerator.TileSize));
				}
			}
			Room.SetFrames();
			CreateRooms(8);
			SortPaths();
			halls = Pathfinding.PathFind(paths, Tiles);
			foreach (Hallway h in halls)
				h.Transform(Tiles);
			foreach (Room r in Rooms)
				r.SetTextures();
			foreach (Room r in Rooms)
				r.SetDoors(Tiles);

			rightRoom = bottomRoom = Rectangle.Empty;
			leftRoom = topRoom = new Rectangle(1000, 1000, 0, 0);
			foreach (Room r in Rooms)
            {
				rightRoom = r.Rectangle.Right > rightRoom.Right ? r.Rectangle : rightRoom;
				leftRoom = r.Rectangle.X < leftRoom.X ? r.Rectangle : leftRoom;
				topRoom = r.Rectangle.Y <  topRoom.Y ? r.Rectangle : topRoom;
				bottomRoom = r.Rectangle.Bottom > bottomRoom.Bottom ? r.Rectangle : bottomRoom;
			}
		}

		// <summary>
		// Allows every tile on the Map to be moved. Used for player movement and camera effects
		public Room SelectBossRoom(Room playerRoom)
        {
			Room bossRoom = Rooms[Rooms.Count - 1];
			foreach (Room r in Rooms)
            {
	            bossRoom = r.Tiles.Length > bossRoom.Tiles.Length && bossRoom.DistanceToRoom(playerRoom) < r.DistanceToRoom(playerRoom) ? r : bossRoom;
            }
			return bossRoom;
        }

		public Room SelectPlayerRoom()
		{
			Room small = Rooms[0];
			foreach (Room r in Rooms)
			{
				small = r.Tiles.Length < small.Tiles.Length ? r : small;
			}
			return small;
		}

		public void FillWithAliens(int min, int max, Scene scene, List<Entity> entities, List<ICollidable> collidables, Player p, Room playerRoom, Room bossRoom = null)
        {
	        //playerRoom.AddAliens(1, meleeAlien, scene, entities, collidables, p);
	        foreach (Room room in Rooms)
            {
                if (room != playerRoom)
                {
                    int num = rand.Next((int)(room.Tiles.Length *.01), (int)(room.Tiles.Length * .05                                         ));
                    room.AddAliens(num, meleeAlien, scene, entities, collidables, p);
                }
            }
	        bossRoom?.AddAliens(10, meleeAlien, scene, entities, collidables, p);
        }

		public void CreateRooms(int numOfRooms)
        {
			rand = new Random();
			for (int k = 0; k < numOfRooms; k++)
			{
				int width = roomDimensions[rand.Next(roomDimensions.Count)];
				int height = roomDimensions[rand.Next(roomDimensions.Count)];
				int x = rand.Next(0, Tiles.GetLength(0) - width);
				int y = rand.Next(0, Tiles.GetLength(1) - height);
				Tile[,] tiles = new Tile[width, height];
				for (int i = 0; i < tiles.GetLength(0); i++)
				{
					for (int j = 0; j < tiles.GetLength(1); j++)
					{
						tiles[i, j] = Tiles[x + i, y + j];
					}
				}
				int attempts = 0;
				Room room = new Room(tiles);
				while (IntersectsRooms(room))
                {
					width = roomDimensions[rand.Next(roomDimensions.Count)];
					height = roomDimensions[rand.Next(roomDimensions.Count)];
					x = rand.Next(0, Tiles.GetLength(0) - width);
					y = rand.Next(0, Tiles.GetLength(1) - height);
					tiles = new Tile[width, height];
					for (int i = 0; i < tiles.GetLength(0); i++)
					{
						for (int j = 0; j < tiles.GetLength(1); j++)
						{
							tiles[i, j] = Tiles[x + i, y + j];
						}
					}	
					room = new Room(tiles);
					attempts++;
					if (attempts > 100000)
					{
						Rooms.Clear();
						CreateRooms(numOfRooms);
						return;
					}

				}
				Rooms.Add(room);
			}
		}

		public void CreateRoom(int x, int y, int width, int height)
        {
			Tile[,] tiles = new Tile[width, height];
			for (int i = 0; i < tiles.GetLength(0); i++)
			{
				for (int j = 0; j < tiles.GetLength(1); j++)
				{
					tiles[i, j] = Tiles[x + i, y + j];
				}
			}
			Room room = new Room(tiles);
			Rooms.Add(room);
		}
		public bool IntersectsRooms(Room room)
        {
			foreach (Room r in Rooms)
				if (r.Intersects(room))
					return true;
			return false;
        }

		public void SortPaths()
        {
			// legit carries
			List<Vertex> centers = new List<Vertex>();
			foreach (Room room in Rooms)
			{
				room.FillTiles();
				centers.Add(new Vertex(room.centerTile.Position.X, room.centerTile.Position.Y));
			}
			List<Triangle> triangles = Delaunay.Triangulate(centers);
			int start = rand.Next(centers.Count);
			paths = null;
			while (paths == null)
			{
				paths = Prim.PrimsPath(triangles, centers, centers[start]);
				start = rand.Next(centers.Count);
			}
			foreach (Edge e in paths)
				e.AssignTiles(Tiles);

		}

		public void Update(Player p)
        {
			foreach (Room r in Rooms)
				r.Update(p);
			for (int i = 0; i < fgStars.Length; i++)
			{
				fgStars[i].X -= 2;
				bgStars[i].X -= 1;
				if (fgStars[i].Right < leftRoom.X - Scene.ScreenHeight / 2)
                {
					fgStars[i].X = newX(50);
					fgStars[i].Y = newY();
                }
				if (bgStars[i].Right < leftRoom.X - Scene.ScreenHeight / 2)
				{
					
					bgStars[i].X = newX(50);
					bgStars[i].Y = newY();
				}
			}
			debris.X -= 4;
			if (debris.Right < leftRoom.X - Scene.ScreenHeight / 2)
            {
				debris.X = newX(0);
				debris.Y = newY();
				bgRotations[0] = rand.Next(360);
			}
			farPlanets.X -= 1;
			if (farPlanets.Right < leftRoom.X - Scene.ScreenHeight / 2)
            {
				farPlanets.X = newX(0);
				farPlanets.Y = newY();
				bgRotations[1] = rand.Next(360);
			}
			ringPlanet.X -= 2;
			if (ringPlanet.Right < leftRoom.X - Scene.ScreenHeight / 2)
			{
				ringPlanet.X = newX(0);
				ringPlanet.Y = newY();
				bgRotations[2] = rand.Next(0, 50);
			}
			closePlanet.X -= 1;
			if (closePlanet.Right < leftRoom.X - Scene.ScreenHeight / 2)
            {
				closePlanet.X = newX(0);
				closePlanet.Y = newY();
			}
		}


		private int newX(int extraDist)
        {
			return rightRoom.Right + Scene.ScreenWidth / 2 + extraDist;
        }

		private int newY()
        {
			return rand.Next(topRoom.Y - Scene.ScreenHeight / 2 - 50, bottomRoom.Bottom + Scene.ScreenHeight / 2);
		}
		public void Draw(SpriteBatch batch, Vector2 offset)
        {
			offset *= -1;
			batch.Draw(space[1], Util.RecPlusOff(farPlanets, offset), null, new Color(255, 255, 255, 60), 0, Vector2.Zero, SpriteEffects.None, 0f);
			batch.Draw(space[2], Util.RecPlusOff(ringPlanet, offset), null, new Color(255, 255, 255, 0), bgRotations[2], Vector2.Zero, SpriteEffects.None, 0f);
			for (int i = 0; i < bgStars.Length; i++)
				batch.Draw(Tile.TestTex, Util.RecPlusOff(bgStars[i], offset), darkStar);
			for (int i = 0; i < fgStars.Length; i++)
				batch.Draw(Tile.TestTex, Util.RecPlusOff(fgStars[i], offset), lightStar);
			batch.Draw(space[3], Util.RecPlusOff(closePlanet, offset), new Color(255, 255, 255, 60));
			batch.Draw(space[0], Util.RecPlusOff(debris, offset), null, Color.White, bgRotations[0], Vector2.Zero, SpriteEffects.None, 0f);

			foreach (Room room in Rooms)
				foreach (Tile t in room.underDoors)
					t.Draw(batch, offset * -1);
			foreach (Tile tile in Tiles)
			{
				if (tile.GetTexture() != Tile.TestTex)
					tile.Draw(batch, offset * -1);
			}
        }
	}
}
