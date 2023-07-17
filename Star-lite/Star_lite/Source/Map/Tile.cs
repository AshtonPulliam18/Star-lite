using Microsoft.Xna.Framework;
using Starlite.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Starlite.Map
{
	public class Tile : Sprite
	{
		//public const int TileSize = 32;
		public static Texture2D TestTex;
		public int Score, HeuristicScore;
		public bool Visited, Locked;
		public Tile TileToRoute;

		private Animator animator;
		private SfxManager sfxManager;
		public bool openDoor, openingDoor, sideDoorLeft, sideDoorRight;
		public TileType TileType { get; set; }
		public int PositionX { get; }
		public int PositionY { get; }

		public Tile(TileType tileType, Texture2D texture, Rectangle frame, float scale, int layer, Vector2 position, float rotation) : base(texture, frame, scale, layer, position, rotation)
        {
			TileType = tileType;
			TileToRoute = null;
			Score = HeuristicScore = int.MaxValue;
			Visited = false;
			PositionX = (int)position.X;
			PositionY = (int)position.Y;
		}

		public Tile(TileType tileType, Vector2 position) : base(TestTex, position)
		{
			TileType = tileType;
			TileToRoute = null;
			Score = HeuristicScore = int.MaxValue;
			Visited = false;
			PositionX = (int)position.X;
			PositionY = (int)position.Y;
		}

		public Rectangle GetBox()
		{
			int xMod = 0;
			int wMod = 1;
			if (sideDoorLeft)
				xMod = MapGenerator.TileSize / 2;
			else if (sideDoorRight)
				wMod = 2;
			return new Rectangle(PositionX + xMod, PositionY, MapGenerator.TileSize / wMod,
				MapGenerator.TileSize);
		}
		public void SetFrame(Rectangle r) {
				Frame = r;
		}
		public void SetTexture(Texture2D t)
        {
			Texture = t;
        }
		
		public Texture2D GetTexture()
        {
			return Texture;
        }
		public void SetEffect(SpriteEffects e)
        {
			Effect = e;
        }
		public void LoadDoorAnimations()
        {
			Rectangle[][] animations = new Rectangle[3][];
			animations[0] = new Rectangle[8];
			animations[1] = new Rectangle[8];
			animations[2] = new Rectangle[] { new Rectangle(0, 0, 32, 32) };
			if (TileType == TileType.Door)
            {
				for (int i = 0; i < 2; i++)
					for (int j = 0; j < 4; j++)
					{
						animations[0][j + i * 4] = new Rectangle(j * 32, i * 32, 32, 32);
						animations[1][7 - (j + i * 4)] = new Rectangle(j * 32, i * 32, 32, 32);
					}
			}
			else
            {
				for (int i = 0; i < 8; i++)
                {
					animations[0][i] = new Rectangle(i * 32, 0, 32, 32);
					animations[1][7 - i] = new Rectangle(i * 32, 0, 32, 32);
				}
            }

			animator = new Animator(new string[] { "open", "close", "idle"}, animations, new int[] { 3, 3, 3 });
			Frame = animator.UpdateFrame("idle");
		}

		public void LoadSound(SoundEffect open, SoundEffect close)
        {
			sfxManager = new SfxManager(new string[] { "open", "close"},  new SoundEffect[] { open, close }, new float[] { 0.7f, 0.5f});
        }

		public void PlayOpenFX(float pitch)
        {
			sfxManager.UpdateSfxSingle("open", pitch);
        }

		public void PlayCloseFX(float pitch)
		{
			sfxManager.UpdateSfxSingle("close", pitch);
		}



		public bool OpenDoor()
		{
			openingDoor = true;
			Rectangle frame = animator.UpdateFrameSingle("open");
			if (frame == Rectangle.Empty)
			{
				return true;
			}
			else
				Frame = frame;
			return false;
		}
		public bool CloseDoor()
        {

			Rectangle frame = animator.UpdateFrameSingle("close");
			if (frame == Rectangle.Empty)
				return true;
			else
				Frame = frame;
			return false;
		}
        public override bool Equals(object obj)
        {
            return obj is Tile tile &&
                   Position.Equals(tile.Position);
        }

        public override int GetHashCode()
        {
            return -425505606 + Position.GetHashCode();
        }
    }
}
