using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Starlite.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Starlite.Entities
{
	public class Bullet : Sprite, ICollidable
	{
		public static Texture2D BulletTexture;

		public float damage;
		public ICollidable ignore;

		public SoundEffectInstance bulletFx;
		private Vector2 direction;
		private float speed;
		public bool penetrating, bossBullet;
		public Bullet(float damage, Vector2 direction, float speed, ICollidable ignore, Rectangle frame, float scale, int layer, Vector2 position, float rotation, SoundEffectInstance bulletFX, bool penetrating) : base(BulletTexture, frame, scale, layer, position, rotation)
		{
			this.damage = damage;
			this.direction = direction;
			this.speed = speed;
			this.ignore = ignore;
			this.penetrating = penetrating;
			this.bulletFx = bulletFX;
			this.bulletFx.Volume = 0.4f;
			this.bulletFx.Play();
		}

		public Bullet(float damage, Vector2 direction, float speed, ICollidable ignore, Rectangle frame, float scale, int layer, Vector2 position, float rotation, Color color) : base(BulletTexture, frame, scale, layer, position, rotation)
		{
			Color = color;
			this.damage = damage;
			this.direction = direction;
			this.speed = speed;
			this.ignore = ignore;
			bossBullet = true;
		}

		public void Update(float deltaTime)
		{
			this.Position += this.direction * this.speed;
		}

		void ICollidable.Depentrate(Vector2 displacement) => this.Position += displacement;

		public Rectangle GetBoundingBox(Vector2 position) => new Rectangle((int)this.Position.X, (int)this.Position.Y,
			this.Frame.Width, this.Frame.Height);

		Vector2 ICollidable.GetWorldPosition() => this.Position;

        public Rectangle[] GetHitboxes()
        {
            return new Rectangle[] { GetBoundingBox(Vector2.Zero) };
        }
    }
}
