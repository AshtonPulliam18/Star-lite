using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Starlite.Rendering;
using Starlite.Map;

namespace Starlite.Entities
{
    public class Boss : Enemy
    {
        private Rectangle[] frames = new Rectangle[4];

        private Rectangle acidBall, healthBar, healthFrame;
        private String[] bossFX;
        private int longRangeTime, meleeRadius, attackTempo, attackLimit;
        private bool LoadedSounds, isShooting;
        private bool canUnlock = false;

        public Boss(Scene scene, Texture2D texture, Rectangle frame, float scale, int layer, Vector2 position,
            float rotation, float attackRate, float damage, float projectileSpeed, int radius, int speed, int health,
            Player player, Room bossRoom) : base(scene, texture, frame, scale, layer, position, rotation, attackRate,
            damage, projectileSpeed, radius, speed, health, player, bossRoom)
        {
            Room.Locked = false;

            //maxHealth = Health;
            hitboxes = new Rectangle[4];
            hitboxes[0] = new Rectangle((int)this.Position.X + 35, (int)this.Position.Y + 30, 60, 55);
            hitboxes[1] = new Rectangle((int)this.Position.X + 25, (int)this.Position.Y + 40, 20, 50);
            hitboxes[2] = new Rectangle((int)this.Position.X + 5, (int)this.Position.Y + 50, 20, 65);
            hitboxes[3] = new Rectangle((int)this.Position.X + 85, (int)this.Position.Y + 50, 25, 60);
            healthBar = new Rectangle(70, Scene.ScreenHeight - 55, Scene.ScreenWidth - 140, 50);
            this.healthBarBackground = new Rectangle(70, Scene.ScreenHeight - 55, Scene.ScreenWidth - 140, 50);
            healthFrame = new Rectangle(65, Scene.ScreenHeight - 60, Scene.ScreenWidth - 130, 60);


            attackTempo = 180;
            attackLimit = 3;
            meleeRadius = 64;
            Boss = this;
        }


        public void LoadTextures(Texture2D boss, Texture2D attackAni = null)
        {
            Texture = boss;
        }

        public override void UpdateAnimation()
        {
            if (isWalking)
                action = "walking";
            else if (isAttacking)
                action = "idle";
            else
                action = "idle";

            Frame = animator.UpdateFrame(action);
        }


        public virtual bool PlayerInMeleeRadius()
        {
            return Math.Abs(Vector2.Distance(Player.Position, Position)) <= meleeRadius;
        }

        protected bool Attack()
        {
            isWalking = false;
            isAttacking = true;
            return LongRangedAttack();
        }

        public bool LongRangedAttack()
        {
            if (!isShooting)
            {
                isShooting = true;
                acidBall = Rectangle.Empty;
                longRangeTime = 0;
            }
            else
            {
                int offsetX = 0;
                float bulletSpeed = longRangeTime > 60 ? projectileSpeed : 0;
                float scale = 3 * ((float)longRangeTime / 60);
                time = longRangeTime > 60 ? 0 : time;
                isShooting = longRangeTime > 60 ? false : true;
                if (Effect == SpriteEffects.FlipHorizontally)
                    offsetX = 15;

                int size = (int)(14 * scale);
                int x = ((int)Position.X + 50 + offsetX) - size / 2;
                int y = ((int)Position.Y + 100) - size / 2;
                acidBall = new Rectangle(x, y, size, size);
                // acidBall = new Bullet(damage, PlayerDirection(), bulletSpeed, this, new Rectangle(0, 0, 14, 14), scale, 0, new Vector2(50 + offsetX, 100), );
                longRangeTime = longRangeTime > 60 ? 0 : longRangeTime + 1;
                if (longRangeTime == 0)
                {
                    Bullet acidBullet = new Bullet(damage, PlayerDirection(), projectileSpeed, this,
                        new Rectangle(0, 0, 14, 14), scale, 0, new Vector2(x, y), 0f, Color.LightGreen);
                    scene.SpawnBullet(acidBullet);
                    scene.Shake(2, 0.2f);
                    return true;
                }
            }

            return false;
        }

        public override void UpdateHealthbar()
        {
            healthBar.Width = (int)((Scene.ScreenWidth - 140) * ((float)Health / maxHealth));
        }

        public override void Update(float deltaTime)
        {
            hitboxes[0] = new Rectangle((int)this.Position.X + 35, (int)this.Position.Y + 30, 60, 55);
            hitboxes[1] = new Rectangle((int)this.Position.X + 25, (int)this.Position.Y + 40, 20, 50);
            hitboxes[2] = new Rectangle((int)this.Position.X + 5, (int)this.Position.Y + 50, 20, 65);
            hitboxes[3] = new Rectangle((int)this.Position.X + 85, (int)this.Position.Y + 50, 25, 60);
            
            if (Player.InRoom(Room) && !Room.Locked)
            {
                Room.Locked = true;
                scene.SetTrack("action");
            }
            else if (Player.IsDead)
                Room.Locked = false;

            isWalking = false;
            // TODO CHECK IF PLAYER IS IN RADIUS

            IsInvincible = Room.InDoor(Player.Position) || !Room.InRoom(Player.GetBoundingBox());

            if (!isAttacking)
            {
                currentDestination = GetDestination();
                Vector2 direction = GetDirectionToPoint(currentDestination);
                if (direction != Vector2.Zero)
                    isWalking = true;
                Position = new Vector2(Position.X + Speed * direction.X, Position.Y + Speed * direction.Y);
            }

            if (PlayerInSenseRadius())
            {
                if (!PlayerNearby)
                    PlayerNearby = true;
                Effect = Player.Position.X > Position.X ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                if (PlayerInAttackRadius() && time >= 0 || isAttacking)
                {
                    if (attackTempo == 0 && attackLimit > 0)
                    {
                        if (Attack())
                            attackLimit--;
                    }
                    else if (attackTempo == 0 && attackLimit == 0)
                    {
                        attackTempo = 180;
                        if (Health > maxHealth * .75)
                            attackLimit = 3;
                        else if (Health > maxHealth * .5)
                            attackLimit = 4;
                        else if (Health > maxHealth * .25)
                            attackLimit = 5;
                        else
                            attackLimit = 6;
                        isAttacking = false;
                    }
                    else if (attackTempo != 0 && attackLimit > 0)
                        attackTempo--;
                }
            }

            IsDead = Health <= 0;
            UpdateHealthbar();
            UpdateAnimation();
        }

        public override void Die()
        {
            Room.Locked = false;
            IsDead = true;
        }
        public override Rectangle[] GetHitboxes()
        {
            return hitboxes;
        }

        public override void Draw(SpriteBatch batch, Vector2 offset)
        {
            base.Draw(batch, offset);
            if (PlayerInSenseRadius())
            {
                if (isShooting)
                    batch.Draw(Bullet.BulletTexture, Util.RecMinusOff(acidBall, offset), new Rectangle(0, 0, 14, 14), Color.LightGreen);
                
                batch.Draw(Tile.TestTex, healthFrame, new Color(27,25,39));
                batch.Draw(Tile.TestTex, this.healthBarBackground, new Color(117,32,32));
                batch.Draw(Tile.TestTex, healthBar, new Color(34,32,52));
                batch.Draw(Game1.BossHeadTexture, new Rectangle(75, 400, 35, 35), Color.White);
                batch.DrawString(Game1.subheaderFont, "Arachnox the Voracious", new Vector2(125, 410), Color.White);
            }
        }
        
    }
}