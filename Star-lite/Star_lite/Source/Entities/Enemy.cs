using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Starlite.Map;
using Starlite.Rendering;
using System;

namespace Starlite.Entities
{
    public class Enemy : Entity, ICollidable
    {
        public static Enemy Boss;
        public static Random rand = new Random();
        public static bool PlayerNearby;
        public static Texture2D normal, attackAni;
        protected Room Room;
        protected Player Player;
        protected int Radius, Speed;
        
        protected Vector2 currentDestination = Vector2.Zero;
        protected Animator animator;
        protected String action;
        protected bool isWalking, isAttacking, playerHit;
        protected int movementCooldown, attackTime;
        public Enemy(Scene scene, Texture2D texture, Rectangle frame, float scale, int layer, Vector2 position,
                     float rotation, float attackRate, float damage, float projectileSpeed, int radius, int speed, int health, Player player, Room room) :
            base(scene, texture, frame, scale, layer, position, rotation, attackRate, damage,
                projectileSpeed)
        {
            if (texture != null && normal != texture)
                normal = texture;
            Health = health;
            maxHealth = health;
            Room = room;
            Player = player;
            Speed = speed;
            Radius = radius;
            Rectangle[] frames = new Rectangle[4];
            for (int i = 0; i < frames.Length; i++)
                frames[i] = new Rectangle(i * frame.Width, 0, frame.Width, frame.Height);
            Rectangle[] attackFrames = new Rectangle[8];
            for (int i = 0; i < attackFrames.Length; i++)
                attackFrames[i] = new Rectangle(i * 54, 0, 54, 48);
            animator = new Animator(new string[] { "walking", "attacking", "idle" }, new Rectangle[][] { frames, attackFrames,  new Rectangle[] { frame } }, new int[] { 10, 5, 10 });
            
            //movementCooldown = rand.Next(40, 120);
            time = attackRate;
            currentDestination = GetDestination();
        }
        
        public virtual void UpdateAnimation()
        {
            if (isWalking)
                action = "walking";
            else if (isAttacking)
                action = "attacking";
            else
                action = "idle";
            
            Frame = animator.UpdateFrame(action);
        }

        public Vector2 GetWorldPosition() => this.Position;
        public void Depentrate(Vector2 displacement) => this.Position += displacement;

        public virtual Rectangle GetBoundingBox(Vector2 position) => new Rectangle((int)this.Position.X, (int)this.Position.Y,
            this.Frame.Width, this.Frame.Height);
        public virtual Rectangle[] GetHitboxes()
        {
            return new Rectangle[] { GetBoundingBox(Vector2.Zero) };
        }

        public bool PlayerInSenseRadius()
        {
            return Player.GetBoundingBox().Intersects(Room.Rectangle);
        }

        public bool PlayerInAttackRadius()
        {
            return Math.Abs(Vector2.Distance(Player.Position, Position)) <= Radius;
        }

        public Vector2 PlayerDirection()
        {
            return (Player.Position - Position).Normalized();
        }
        public Vector2 GetDirectionToPoint(Vector2 point)
        {
            Vector2 direction = Vector2.Zero;
            int margin = Frame.Width;
            if (!(Math.Abs(point.X - Position.X) < margin))
                direction.X = point.X > Position.X ? 1 : -1;
            if (!(Math.Abs(point.Y - Position.Y) < margin))
                direction.Y = point.Y > Position.Y ? 1 : -1;
            return direction;
        }
        public Vector2 PositionNearPlayer()
        {
            Vector2 nearPosition = Player.Position;
            if (!Room.InRoom(Player.Position) || Room.InDoor(Player.Position))
                return Room.GetRandomPos();
            while (!Room.InRoom(nearPosition) || nearPosition == Player.Position) {
                nearPosition = Player.Position;
                int dx = rand.Next(-Radius,Radius);
                int dy = rand.Next(-Radius, Radius);
                nearPosition.X += dx;
                nearPosition.Y += dy;

            }
            return nearPosition;
        }
        public Vector2 GetDestination()
        {
            if (currentDestination == Vector2.Zero)
            {
                return Room.GetRandomPos();
            }
            else if (GetDirectionToPoint(currentDestination) == Vector2.Zero)
            {
                if (movementCooldown == 0)
                {
                    movementCooldown = rand.Next(40, 120);
                    int afterPlayer = rand.Next(3);
                    if (afterPlayer < 2 && PlayerInSenseRadius())
                    {
                        return PositionNearPlayer();
                    }
                    else
                        return Room.GetRandomPos();
                }
                else
                    movementCooldown--;
            }
            return currentDestination;
        }
        public override void Update(float deltaTime)
        {
           // PlayerNearby = false;
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
                if (PlayerInAttackRadius() && time >= attackRate / 2 || isAttacking)
                {
                    Attack();
                }
            }

            IsDead = Health <= 0;
            UpdateHealthbar();
            UpdateAnimation();
            base.Update(deltaTime);
        }

        // public override void Die()
        // {
        //     Room.alienCount--;
        //     base.Die();
        // }
        protected override void Attack()
        {
            // if (!PlayerInAttackRadius())
            //     attackTime = 41;
            if (attackTime == 0)
            {
                isAttacking = true;
                isWalking = false;
                Texture = attackAni;
                playerHit = false;
            }
            else if (attackTime < 40 && !playerHit && PlayerInAttackRadius())
            {
                Player.Damage(damage);
                playerHit = true;
            }
            else if (attackTime > 40)
            {
                isAttacking = false;
                Texture = normal;
                this.time = 0;
                attackTime = 0;
                return;
            }
            attackTime++;
        }

        public override void Draw(SpriteBatch batch, Vector2 offset)
        {
            base.Draw(batch, offset);
            batch.Draw(Tile.TestTex, Util.RecMinusOff(healthFrame, offset), Color.AntiqueWhite);
            batch.Draw(Tile.TestTex, Util.RecMinusOff(healthBar, offset), Color.Red);
        }
    }
}