using Microsoft.Xna.Framework;

using PolyOne.Collision;


namespace AntiGrave.Platforms
{
    public abstract class Empty : Platform
    {
        public Vector2 ActualPosition
        {
            get
            {
                return this.Position;
            }
        }

        public Empty(Vector2 position, int width, int height)
            : base(position)
        {
            this.Tag((int)GameTags.Empty);
            this.Collider = new Hitbox((float)width, (float)height, 0.0f, 0.0f);
        }

        public Empty()
            : base(Vector2.Zero)
        {
            this.Tag((int)GameTags.Empty);
        }
    }
}
