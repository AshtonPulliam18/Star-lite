using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Starlite.Input;
using Starlite.Map;
using Starlite.Rendering;
using System;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input;

namespace Starlite.Entities
{
    public class Player : Entity, ICollidable
    {
        public Texture2D CannonTexture;

        private Color outlineColor, fullOxyCol, lowOxyCol;
        private Vector2 outlinePos;
        private Rectangle playerOutline;
        private Rectangle outlineFrame;

        private const float TimeToRemoveOxygenPoint = 2.0f;
        private const float RespawnDuration = 0.5f;
        private const float DashDuration = 0.1f;
        public int Oxygen { get; private set; } = 100;
        public bool PermanentlyDead { get; set; }
        private float timeSinceOxygenPointRemoved;

        private bool isAttackingPrimary, isAttackingSecondary, isWalking;

        protected Rectangle[] afterImages = new Rectangle[2];
        protected Rectangle oxygenBar, oxygenBarBackground, oxygenFrame;

        private Animator animator;
        private String[] actions;
        private String action;
        private Rectangle[] frames = new Rectangle[4];
        private SpriteEffects GunEffect;

        private SfxManager sfxManager;
        private String[] playerFX;
        private bool LoadedSounds;

        private Vector3 hurtColor = Vector3.Zero;
        private float healthRedFlashTimer;

        private bool isRespawning;
        private Vector2 initialDeadPosition;
        private float respawnTimer;

        //private float time;
        private float speed = 5.0f;
        private int afterImageTime;
        public Player(Scene scene, Texture2D texture, Rectangle frame, float scale, int layer, Vector2 position,
                      float rotation, float attackRate, float damage, float projectileSpeed) : base(scene, texture, frame, scale, layer, position, rotation, attackRate,
            damage, projectileSpeed)
        {
            for (int i = 0; i < 4; i++)
            {
                frames[i] = new Rectangle(32 * i, 0, 32, 32);
            }

            actions = new String[] { "walking", "idle" };
            action = "idle";
            animator = new Animator(actions,
                new Rectangle[][] { frames, new Rectangle[] { new Rectangle(0, 0, 32, 32) } }, new int[] { 10, 10 });

            playerFX = new string[] { "walking" };
            afterImageTime = 0;
        }

        public void LoadSounds(SoundEffect[] playerSounds)
        {
            sfxManager = new SfxManager(playerFX, playerSounds, new float[] { 0.8f });
            LoadedSounds = true;
        }
        public void LoadTextures(Texture2D player, Texture2D cannon)
        {
            // rewrite textures to be loaded similar to how FX is handled
            Texture = player;
            CannonTexture = cannon;

            fullOxyCol = new Color(10, 0, 0);
            lowOxyCol = new Color(89, 6, 3);
            playerOutline = new Rectangle((int)this.Position.X - 1, (int)this.Position.Y - 1, this.Texture.Width / 4 + 1, this.Texture.Height / 2 + 1);
        }

        public Color GetOutlineColor(int oxygen)
        {
            int[] difference = Util.ColorDist(lowOxyCol, fullOxyCol, oxygen / (float)100);
            return Util.ColorAdd(lowOxyCol, difference);
        }

        public bool InRoom(Room r)
        {
            if (r.InDoor(Position))
                return false;
            Rectangle playerRect = GetBoundingBox();
            Rectangle intersection = Rectangle.Intersect(playerRect, r.Rectangle);
            return playerRect.Width == intersection.Height && playerRect.Height == intersection.Height;
        }


        public Vector2 GetWorldPosition() => this.Position;
        public void Depentrate(Vector2 displacement) => this.Position += displacement;

        public Rectangle GetBoundingBox(Vector2 position = new Vector2()) => new Rectangle((int)this.Position.X, (int)this.Position.Y,
            this.Frame.Width, this.Frame.Height);

        private Vector2 mouseDirection;

        private bool isDashing;
        private float dashTimer;
        private Vector2 dashDirection;
        private float dashSpeed;
        private float dashCooldown;

        private float secondaryAttackCharge;
        private float timeSinceLastHurt;

        private void Dash(float dashSpeed, Vector2 direction, bool isPlayerDash)
        {
            if (isPlayerDash)
            {
                if (this.dashCooldown > 0.0f) return;

                this.dashCooldown = 2.0f;
            }

            this.isDashing = true;
            this.dashTimer = DashDuration;
            this.dashSpeed = dashSpeed;
            this.dashDirection = direction;
            scene.Shake(3, 0.1f);
        }

        public override void Update(float deltaTime)
        {
            Vector2 displacement =
                new Vector2(-InputManager.GetHorizontalAxis(), InputManager.GetVerticalAxis()).Normalized() * speed;
            this.Position -= displacement;

            UpdateAnimation();
            UpdateHealthbar();
            UpdateFX(deltaTime);
            outlineFrame = new Rectangle(this.Frame.X, 32, this.Frame.Width, this.Frame.Height);
            playerOutline.X = (int)this.Position.X;
            playerOutline.Y = (int)this.Position.Y;

            this.mouseDirection = (InputManager.GetMousePosition() - Game1.WindowSize / 2.0f).Normalized();

            this.timeSinceLastHurt += deltaTime;

            Vector2 movement = new Vector2(InputManager.GetHorizontalAxis(), -InputManager.GetVerticalAxis());
            

            if (InputManager.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Space))
                this.Dash(15.0f, movement, true);

            this.dashCooldown -= deltaTime;

            if (this.isDashing)
            {
                this.dashTimer -= deltaTime;

                if (this.dashTimer <= 0.0f)
                    this.isDashing = false;

                this.Position += dashDirection * dashSpeed;
            }

            if (this.isRespawning)
            {
                this.respawnTimer += deltaTime;

                if (this.respawnTimer >= RespawnDuration)
                {
                    IsDead = false;
                    this.isRespawning = false;
                    this.respawnTimer = 0.0f;
                }
                else
                {
                    var t = (respawnTimer / RespawnDuration).EaseInOutSine();
                    this.Position = new Vector2(Util.Lerp(this.initialDeadPosition.X, Scene.RespawnPoint.X, t), Util.Lerp(this.initialDeadPosition.Y, Scene.RespawnPoint.Y, t));
                }
            }

            Effect = this.mouseDirection.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if ((int)movement.X != 0 || (int)movement.Y != 0)
            {
                isWalking = true;
            }
            else
                isWalking = false;

            if (InputManager.LMBPressed())
            {
                this.isAttackingPrimary = true;
                this.isAttackingSecondary = false;
            }

            if (InputManager.RMBPressed())
            {
                this.isAttackingSecondary = true;
                this.isAttackingPrimary = false;
            }

            if (InputManager.LMBReleased())
                this.isAttackingPrimary = false;

            if (InputManager.RMBReleased())
                this.isAttackingSecondary = false;


            if (this.isAttackingSecondary)
            {
                this.secondaryAttackCharge += deltaTime;
                this.speed = 2.0f;
            }
            else
            {
                this.secondaryAttackCharge = 0.0f;
                this.speed = 5.0f;
            }

            if (isAttackingPrimary && time >= this.attackRate)
            {
                this.Attack();
                time = 0.0f;
            }
            else if (isAttackingSecondary && time >= this.attackRate * 5.0f && secondaryAttackCharge > 0.5f)
            {
                this.AttackSecondary();
                time = 0.0f;

                this.secondaryAttackCharge = 0.0f;
                this.isAttackingSecondary = false;
            }

            this.timeSinceOxygenPointRemoved += deltaTime;

            if (this.timeSinceOxygenPointRemoved > TimeToRemoveOxygenPoint)
            {
                this.Oxygen--;
                this.timeSinceOxygenPointRemoved = 0.0f;
            }

            if (Oxygen <= 0.0f)
			{
                this.PermanentlyDead = true;
			}
            
            // if (isDashing)
            // {
            //     afterImages[0] = new Rectangle((int)(Position.X - movement.X * speed * ),
            //         (int)(Position.Y - movement.Y * speed * 3), Frame.Width, Frame.Height);
            //     afterImages[1] = new Rectangle((int)(Position.X - movement.X * speed * 6),
            //         (int)(Position.Y - movement.Y * speed * 6), Frame.Width, Frame.Height);
            //     // afterImages[2] = new Rectangle((int)(Position.X - movement.X * speed * 9),
            //     //     (int)(Position.Y - movement.Y * speed * 9), Frame.Width, Frame.Height);
            //     // afterImages[3] = new Rectangle((int)(Position.X - movement.X * speed * 12),
            //     //     (int)(Position.Y - movement.Y * speed * 12), Frame.Width, Frame.Height);
            //     if (afterImageTime == 0)
            //         afterImageTime = 10;
            // }
            
            outlineColor = GetOutlineColor(this.Oxygen);
            base.Update(deltaTime);
        }


		public override void UpdateHealthbar()
		{
            healthFrame = new Rectangle(15, 10, 200, 30);
            healthBar = new Rectangle(healthFrame.X + 3, healthFrame.Y + 3, (int)((healthFrame.Width - 6) * ((float)Health / maxHealth)), healthFrame.Height - 6);
            
            healthBarBackground = new Rectangle(healthFrame.X + 3, healthFrame.Y + 3, this.healthFrame.Width - 6, healthFrame.Height - 6);

            oxygenFrame = new Rectangle(15, 45, 200, 20);
            oxygenBarBackground = new Rectangle(oxygenFrame.X + 3, oxygenFrame.Y + 3, (int)((oxygenFrame.Width - 6)), oxygenFrame.Height - 6);
            oxygenBar = new Rectangle(oxygenFrame.X + 3, oxygenFrame.Y + 3, (int)((oxygenFrame.Width - 6) * ((float)Oxygen / 100.0f)), oxygenFrame.Height - 6);
        }

		private void UpdateAnimation()
        {
            // add more actions
            if (isWalking)
                action = "walking";
            else
                action = "idle";
            Frame = animator.UpdateFrame(action);
        }

        private void UpdateFX(float deltaTime)
        {
            if (InputManager.IsKeyPressed(Keys.Tab))
                Damage(10);
            
            if (this.healthRedFlashTimer > 0.0f)
            {
                this.hurtColor = new Vector3((healthRedFlashTimer * 10.0f).EaseInOutSine(), 0.0f, 0.0f);
                this.healthRedFlashTimer -= deltaTime;
            }
            else
            {
                this.hurtColor = new Vector3(Util.Lerp((1.0f - (float) this.Health / this.maxHealth) / 2.0f, 0.0f, Util.EaseOutVignette(this.timeSinceLastHurt)), 0.0f, 0.0f);
            }

            if (LoadedSounds)
            {
                if (action == "idle")
                    sfxManager.UpdateSfx("none", false, false);
                else
                    sfxManager.UpdateSfx(action, false, false);
            }
        }

        public override void Die()
        {
            IsDead = true;
            this.Health = this.maxHealth;

            this.initialDeadPosition = this.Position;
            this.isRespawning = true;
        }

        public override void Damage(float damage)
        {
            this.timeSinceLastHurt = 0.0f;
            this.healthRedFlashTimer = 0.2f;
            scene.Shake(2, 0.2f);
            base.Damage(damage);
        }

        protected override void Attack()
        {
            this.scene.SpawnBullet(this.damage, this.mouseDirection, this.projectileSpeed,
                this, new Rectangle(0, 0, 14, 14), new Vector2(8, 5) + this.mouseDirection * 50.0f, 1.0f, false);
        }

        private void AttackSecondary()
        {
            float offset = mouseDirection.Y > 0 ? 50.0f : 60f;
            this.scene.SpawnBullet(this.damage * 3, this.mouseDirection, this.projectileSpeed / 4.0f,
                this, new Rectangle(0, 0, 14, 14), new Vector2(8, 5) + this.mouseDirection * offset, 2.0f, true);

            this.Dash(5.0f, -mouseDirection, false);
        }

        public void Draw(SpriteBatch batch, Vector2 offset, Effect effect)
        {
            effect.Parameters["Tint"].SetValue(this.hurtColor);

            if (this.isRespawning) return;

            if (this.mouseDirection.X < 0)
                GunEffect = SpriteEffects.FlipVertically;
            else
                GunEffect = SpriteEffects.None;
            batch.Draw(CannonTexture, this.Position - offset + new Vector2(16, 10), //16 10 or 15 9
                new Rectangle(0, 0, 44, 18), // 44 18 or 66 27
                Color.White,
                (float)Math.Atan2(this.mouseDirection.Y, this.mouseDirection.X),
                new Vector2(5, 9), // 5 9 or 6 13
                Vector2.One,
                GunEffect,
                Layer);

            // if (afterImageTime > 0)
            // {
            //     for (var i = 0; i < afterImages.Length; i++)
            //     {
            //         var image = afterImages[i];
            //         int a = (int)(100 * ((float)afterImageTime/10) + i * 40);
            //         batch.Draw(Texture, Util.RecMinusOff(image, offset), Frame, new Color(200, 200, 255, (100 * ((float)afterImageTime/10)) + i * 20));
            //     }
            //
            //     afterImageTime--;
            // }

            //batch.Draw(this.Texture, Util.RecMinusOff(playerOutline, offset), outlineFrame, outlineColor, 0f, Vector2.Zero, Effect, 0f);
            batch.Draw(this.Texture, this.Position - offset, outlineFrame, outlineColor, this.Rotation, Vector2.Zero, Scale, Effect, Layer);
            base.Draw(batch, offset);

            batch.Draw(Tile.TestTex, healthFrame, Color.AntiqueWhite);
            batch.Draw(Tile.TestTex, this.healthBarBackground, new Color(240, 25, 20));
            batch.Draw(Tile.TestTex, healthBar, new Color(25, 240, 20));

            batch.Draw(Tile.TestTex, oxygenFrame, Color.AntiqueWhite);
            batch.Draw(Tile.TestTex, oxygenBarBackground, Color.Gray);
            batch.Draw(Tile.TestTex, oxygenBar, new Color(20, 230, 255));
        }

        public Rectangle[] GetHitboxes()
        {
            return new Rectangle[] { GetBoundingBox() };
        }
    }
}