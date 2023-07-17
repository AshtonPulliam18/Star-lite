using System.Net.Mime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Starlite.Rendering
{
    public abstract class Sprite
    {
        protected Texture2D Texture;
        public Rectangle Frame;
        protected float Scale;
        protected int Layer;
        protected Color Color;
        protected int Brightness; 
        protected SpriteEffects Effect;
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }

        public Sprite(Texture2D texture, Rectangle frame, float scale, int layer, Vector2 position, float rotation)
        {
            this.Texture = texture;
            this.Frame = frame;
            this.Scale = scale;
            this.Layer = layer;
            this.Position = position;
            this.Rotation = rotation;
            this.Color = Color.White;
            this.Effect = SpriteEffects.None;
        }
        public Sprite(Texture2D texture, Vector2 position)
        {
            this.Texture = texture;
            this.Frame = new Rectangle(0, 0, 32, 32);
            this.Scale = 1f;
            this.Layer = 1;
            this.Position = position;
            this.Rotation = 0f;
            this.Color = Color.White;
            this.Effect = SpriteEffects.None;
        }

        public virtual void Draw(SpriteBatch batch, Vector2 offset)
        {
            batch.Draw(this.Texture, this.Position - offset, this.Frame, Color, this.Rotation, Vector2.Zero, Scale, Effect, Layer);
            //batch.Draw(this.Texture, new Rectangle((int)(Position.X + offset.X), (int)(Position.Y + offset.Y), 32, 32), Frame, Color.White, 0f, Vector2.Zero, SpriteEffects.None, Layer);
        }
    }
}