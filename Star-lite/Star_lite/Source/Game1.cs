using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Starlite.Entities;
using Starlite.Input;
using Starlite.Map;
using System;
using System.Collections.Generic;

namespace Starlite
{
	public class Game1 : Game
	{
		public static Vector2 WindowSize;
		public static Texture2D BossHeadTexture;

		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;
		private Scene scene;

		private MouseState mouse;
		private Texture2D bigText, smallText, floatingMan, cursor;
		private Rectangle[] fgStars, bgStars;
		private Rectangle bigTextRect, smallTextRect, mouseRect, mouseOutlineRect, cursorFrame, cursorOutlineFrame;
		private Vector2 floatingPosition, floatingTarget, floatingSpeed;
		private Color smallTextColor, darkStar, farStar;
		private Random rand;
		private GameState gameState;
		private float floatingRotation;
		private bool mouseClicked, floatingScreen;
		private Color quitGameColor = Color.White;

		private RenderTarget2D renderTarget;
		private Effect vignetteEffect;

		public static SpriteFont headerFont;
		public static SpriteFont subheaderFont;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			graphics.PreferredBackBufferWidth = 700;
			graphics.PreferredBackBufferHeight = 500;
		}

		public void UpdateCursor()
		{
			mouse = Mouse.GetState();
			mouseClicked = mouse.LeftButton == ButtonState.Pressed;

			mouseRect.X = mouse.X;
			mouseRect.Y = mouse.Y;

			if (mouseClicked)
			{
				mouseOutlineRect.X = mouseRect.X - 1;
				mouseOutlineRect.Y = mouseRect.Y - 1;
				mouseOutlineRect.Width = 18;
				mouseOutlineRect.Height = 18;
			}
			else if (mouseOutlineRect.X == mouseRect.X - 1)
			{
				mouseOutlineRect.X = mouseRect.X - 2;
				mouseOutlineRect.Y = mouseRect.Y - 2;
				mouseOutlineRect.Width = 20;
				mouseOutlineRect.Height = 20;
			}
			else
			{
				mouseOutlineRect.X = mouseRect.X - 2;
				mouseOutlineRect.Y = mouseRect.Y - 2;
			}
		}
		public bool VectorOnScreen(Vector2 v)
		{
			if (v.X > -200 && v.X < graphics.PreferredBackBufferWidth + 200)
			{
				if (v.Y > -200 && v.Y < graphics.PreferredBackBufferHeight + 200)
				{
					return true;
				}
			}
			return false;
		}

		public enum GameState
		{
			StartMenu,
			Game,
			WinScreen,
			LoseScreen
		}

		protected override void Initialize()
		{
			this.renderTarget = new RenderTarget2D(this.GraphicsDevice, this.GraphicsDevice.PresentationParameters.BackBufferWidth, this.GraphicsDevice.PresentationParameters.BackBufferHeight);
			this.vignetteEffect = Content.Load<Effect>("Assets/FullscreenEffect");
			this.vignetteEffect.Parameters["Tint"].SetValue(new Vector3(0.0f, 0.0f, 0.0f));
			Tile.TestTex = Content.Load<Texture2D>("Assets/MapTiles/WhiteTile");
			Texture2D[] space = new Texture2D[5];
			for (int i = 0; i < 4; i++)
			{
				space[i] = Content.Load<Texture2D>("Assets/BG/" + i + "");
			}

			BossHeadTexture = Content.Load<Texture2D>("Assets/Enemies/BossHead");

			Scene.ScreenWidth = graphics.PreferredBackBufferWidth;
			Scene.ScreenHeight = graphics.PreferredBackBufferHeight;

			//Texture2D tileSet, Texture2D door, Texture2D sideDoor, Texture2D meleeAlien, SoundEffect doorOpenSound, SoundEffect doorCloseSound
			Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
			Dictionary<string, SoundEffect> sounds = new Dictionary<string, SoundEffect>();
			textures.Add("tileSet", Content.Load<Texture2D>("Assets/MapTiles/TileSet1"));
			textures.Add("door", Content.Load<Texture2D>("Assets/MapTiles/Door"));
			textures.Add("sideDoor", Content.Load<Texture2D>("Assets/MapTiles/SideDoor"));
			textures.Add("alien1Attack", Content.Load<Texture2D>("Assets/Enemies/Alien1Attack"));
			textures.Add("meleeAlien", Content.Load<Texture2D>("Assets/Enemies/Alien1"));
			sounds.Add("doorOpenSound", Content.Load<SoundEffect>("Assets/Sounds/DoorOpenSound"));
			sounds.Add("doorCloseSound", Content.Load<SoundEffect>("Assets/Sounds/DoorCloseSound"));

			scene = new Scene(space, textures, sounds);

			Dictionary<String, Texture2D> entityTextures = new Dictionary<String, Texture2D>();
			entityTextures.Add("boss", Content.Load<Texture2D>("Assets/Enemies/AlienBoss"));
			entityTextures.Add("player", Content.Load<Texture2D>("Assets/Player/Astronaut"));
			entityTextures.Add("cannon", Content.Load<Texture2D>("Assets/Weapons/Cannon"));
			entityTextures.Add("cannonBullet", Content.Load<Texture2D>("Assets/Projectiles/CannonBullet"));
			scene.LoadTextures(entityTextures);

			SoundEffect[] sceneSounds = new SoundEffect[4];
			for (int i = 0; i < 4; i++)
				sceneSounds[i] = Content.Load<SoundEffect>("Assets/Songs/" + i + "");
			SoundEffect[] playerSounds = new SoundEffect[1];
			playerSounds[0] = Content.Load<SoundEffect>("Assets/Sounds/PlayerWalking");
			SoundEffect[] mapSounds = new SoundEffect[2];
			//mapSounds[0] = Content.Load<SoundEffect>("Assets/Sounds/DoorSound");
			mapSounds[1] = Content.Load<SoundEffect>("Assets/Sounds/CannonSound");
			scene.LoadSounds(sceneSounds, playerSounds, mapSounds);
			scene.LoadEffects(this.vignetteEffect);


			rand = new Random();


			floatingMan = Content.Load<Texture2D>("Assets/Misc/FloatingMan");
			int x = rand.Next(10) > 4 ? rand.Next(-1000, -100) : rand.Next(graphics.PreferredBackBufferWidth + 100, graphics.PreferredBackBufferWidth + 1000);
			int y = rand.Next(10) > 4 ? rand.Next(-1000, -100) : rand.Next(graphics.PreferredBackBufferHeight + 100, graphics.PreferredBackBufferHeight + 1000);
			floatingPosition = new Vector2(x, y);
			floatingTarget = new Vector2(rand.Next(50, graphics.PreferredBackBufferWidth), rand.Next(50, graphics.PreferredBackBufferHeight));
			x = floatingPosition.X > floatingTarget.X ? rand.Next(-5, -2) : rand.Next(2, 5);
			y = floatingPosition.Y > floatingTarget.Y ? rand.Next(-5, -2) : (floatingPosition.Y == floatingTarget.Y ? 0 : rand.Next(2, 5));
			floatingSpeed = new Vector2(x, y);
			floatingRotation = 0;

			//floatingManRect
			fgStars = new Rectangle[50];
			for (int i = 0; i < fgStars.Length; i++)
				fgStars[i] = new Rectangle(rand.Next(graphics.PreferredBackBufferWidth), rand.Next(graphics.PreferredBackBufferHeight), rand.Next(5, 7), rand.Next(5, 7));
			bgStars = new Rectangle[100];
			for (int i = 0; i < bgStars.Length; i++)
			{
				int width, height;
				width = height = i < 49 ? rand.Next(2, 4) : rand.Next(1, 2);
				bgStars[i] = new Rectangle(rand.Next(graphics.PreferredBackBufferWidth), rand.Next(graphics.PreferredBackBufferHeight), width, height);
			}
			darkStar = new Color(220, 220, 200, 200);
			farStar = new Color(200, 200, 200, 180);

			bigText = Content.Load<Texture2D>("Assets/Menu/STARLITE");
			smallText = Content.Load<Texture2D>("Assets/Menu/StartGame");

			headerFont = Content.Load<SpriteFont>("Assets/Fonts/HeaderFont");
			subheaderFont = Content.Load<SpriteFont>("Assets/Fonts/SubheaderFont");

			cursor = Content.Load<Texture2D>("Assets/Misc/Cursor");
			cursorFrame = new Rectangle(0, 0, 16, 16);
			cursorOutlineFrame = new Rectangle(16, 0, 16, 16);
			mouseRect = new Rectangle(0, 0, 16, 16);
			mouseOutlineRect = new Rectangle(0, 0, 20, 20);

			bigTextRect = new Rectangle(170, 190, 360, 50);
			smallTextRect = new Rectangle(300, 290, 100, 10);

			smallTextColor = Color.White;
			gameState = (int)GameState.StartMenu;
			base.Initialize();

			WindowSize = new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			UpdateCursor();
			if (gameState == GameState.Game)
				this.scene.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
			else
			{
				scene.UpdateFX((float)gameTime.ElapsedGameTime.TotalSeconds);
				for (int i = 0; i < fgStars.Length; i++)
				{
					fgStars[i].X -= 4;
					if (fgStars[i].Right < 0)
						fgStars[i] = new Rectangle(rand.Next(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferWidth + 1000), rand.Next(graphics.PreferredBackBufferHeight), rand.Next(5, 7), rand.Next(5, 7));
					bgStars[i].X -= 2;
					if (bgStars[i].Right < 0)
						bgStars[i] = new Rectangle(rand.Next(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferWidth + 1000), rand.Next(graphics.PreferredBackBufferHeight), rand.Next(2, 4), rand.Next(2, 4));
				}

				floatingPosition += floatingSpeed;
				floatingRotation += (float)gameTime.ElapsedGameTime.TotalSeconds;
				if (!floatingScreen)
				{
					floatingScreen = VectorOnScreen(floatingPosition);
				}
				else if (!VectorOnScreen(floatingPosition))
				{
					int x = rand.Next(10) > 4 ? rand.Next(-1000, -100) : rand.Next(graphics.PreferredBackBufferWidth + 100, graphics.PreferredBackBufferWidth + 1000);
					int y = rand.Next(10) > 4 ? rand.Next(-1000, -100) : rand.Next(graphics.PreferredBackBufferHeight + 100, graphics.PreferredBackBufferHeight + 1000);
					floatingPosition = new Vector2(x, y);
					floatingTarget = new Vector2(rand.Next(50, graphics.PreferredBackBufferWidth), rand.Next(50, graphics.PreferredBackBufferHeight));
					x = floatingPosition.X > floatingTarget.X ? rand.Next(-5, -2) : rand.Next(2, 5);
					y = floatingPosition.Y > floatingTarget.Y ? rand.Next(-5, -2) : floatingPosition.Y == floatingTarget.Y ? 0 : rand.Next(2, 5);
					floatingSpeed = new Vector2(x, y);
					floatingScreen = false;
				}
				if (mouseRect.Intersects(smallTextRect))
				{

					smallTextColor = Color.Gray;
					if (mouseClicked)
						gameState = GameState.Game;
				}
				else
					smallTextColor = Color.White;
			}

			if (this.gameState == GameState.WinScreen || this.gameState == GameState.LoseScreen)
			{
				var mousePos = InputManager.GetMousePosition();
				if (new Rectangle(25, 25, 150, 50).Contains((int)mousePos.X, (int)mousePos.Y))
				{
					this.quitGameColor = Color.Gray;

					if (InputManager.LMBHeldDown())
						this.quitGameColor = Color.DarkSlateGray;
					if (InputManager.LMBReleased())
						this.Exit();
				}
				else
					this.quitGameColor = Color.White;
			}

			if (this.scene.player.PermanentlyDead)
				this.gameState = GameState.LoseScreen;
			else if (this.scene.boss.IsDead)
				this.gameState = GameState.WinScreen;

				// Make sure this is the last call in `Update`
			InputManager.SwapStates();
		}

		private void DrawStars()
		{
			for (int i = 0; i < bgStars.Length; i++)
			{
				if (i < 49)
					spriteBatch.Draw(Tile.TestTex, bgStars[i], darkStar);
				else
					spriteBatch.Draw(Tile.TestTex, bgStars[i], farStar);
			}
			foreach (Rectangle star in fgStars)
				spriteBatch.Draw(Tile.TestTex, star, Color.White);
		}

		private void WinScreen()
		{
			this.DrawStars();

			this.spriteBatch.DrawString(headerFont, "WINNER!", new Vector2(250, 200), Color.White);
			this.spriteBatch.DrawString(subheaderFont, "You have successfully destroyed\n\nthe enemy overloard,\n\nArachnox the Voracious,\n\nand decided to retreat back home!\n\nUntil next time, hero!", new Vector2(150, 300), Color.White);
			this.spriteBatch.DrawString(subheaderFont, "Quit Game", new Vector2(50, 50), this.quitGameColor);
		}

		private void LoseScreen()
		{
			this.DrawStars();

			this.spriteBatch.DrawString(headerFont, "LOSER!", new Vector2(250, 200), Color.White);
			this.spriteBatch.DrawString(subheaderFont, "You have been defeated\n\nby the enemy overlord,\n\nArachnox the Voracious!\n\nHe has taken control of your ship\n\nand used it to dominate the world...", new Vector2(150, 300), Color.White);
			this.spriteBatch.DrawString(subheaderFont, "Quit Game", new Vector2(50, 50), this.quitGameColor);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.SetRenderTarget(renderTarget);

			GraphicsDevice.Clear(new Color(52, 28, 44));

			spriteBatch.Begin();
			switch (gameState)
			{
				case GameState.StartMenu:
					this.DrawStars();
					spriteBatch.Draw(floatingMan, floatingPosition, null, Color.White, floatingRotation, new Vector2(floatingMan.Width / 2, floatingMan.Height / 2), 1f, SpriteEffects.None, 0f);
					spriteBatch.Draw(bigText, bigTextRect, Color.White);
					spriteBatch.Draw(smallText, smallTextRect, smallTextColor);
					break;

				case GameState.Game:
					scene.Draw(spriteBatch);
					break;
				case GameState.WinScreen:
					this.WinScreen();
					break;
				case GameState.LoseScreen:
					this.LoseScreen();
					break;
			}

			spriteBatch.Draw(cursor, mouseOutlineRect, cursorOutlineFrame, Color.Gray);
			spriteBatch.Draw(cursor, mouseRect, cursorFrame, Color.DarkRed);
			spriteBatch.End();

			GraphicsDevice.SetRenderTarget(null);

			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullNone, this.vignetteEffect);
			spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
			spriteBatch.End();


			base.Draw(gameTime);
		}
	}
}