using Microsoft.Xna.Framework;

using PolyOne.Collision;

namespace AntiGrave.Platforms
{
    public abstract class Solid : Platform
    {
        public Vector2 ActualPosition
        {
            get
            {
                return this.Position;
            }
        }

        public Solid(Vector2 position, int width, int height)
            : base(position)
        {
            this.Tag((int)GameTags.Solid);
            this.Collider = new Hitbox((float)width, (float)height, 0.0f, 0.0f);
        }

        public Solid()
            : base(Vector2.Zero)
        {
            this.Tag((int)GameTags.Solid);
        }
    }
}
