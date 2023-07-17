using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Starlite.Input;
using Starlite.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Starlite.Entities
{
	public abstract class Entity : Sprite
	{
		protected Scene scene;
		protected float attackRate;
		protected float damage;
		protected float projectileSpeed;
		
		protected Rectangle[] hitboxes;
		protected int maxHealth, flashTime;
		protected float time;
		protected Rectangle healthBar, healthFrame, healthBarBackground;
		public int Health { get; protected set; }
		public bool IsDead { get; protected set; }
		public bool IsInvincible { get; set; }

		public Bullet recent;
		public Entity(
			Scene scene,
			Texture2D texture,
			Rectangle frame,
			float scale,
			int layer,
			Vector2 position,
			float rotation,
			float attackRate,
			float damage,
			float projectileSpeed) : base(texture, frame, scale, layer, position, rotation)
		{
			this.scene = scene;
			this.maxHealth = 100;
			Health = maxHealth;
			this.attackRate = attackRate;
			this.damage = damage;
			this.projectileSpeed = projectileSpeed;
		}
		
		public virtual void Update(float deltaTime)
		{
			this.time += deltaTime;
		}

		public virtual void Damage(float damage)
		{
			this.Health -= (int) damage;
			if (Health <= 0.0f)
				this.Die();
		}
		public virtual void UpdateHealthbar()
		{
			healthFrame = new Rectangle((int)Position.X + 15, (int)Position.Y - 10, 30, 10);
			healthBar = new Rectangle(healthFrame.X + 3, healthFrame.Y + 3, (int)((healthFrame.Width - 6) * ((float)Health / maxHealth)), healthFrame.Height - 6);
		}

		public virtual void Die()
		{
			this.IsDead = true;
		}

		protected abstract void Attack();
	}
}
