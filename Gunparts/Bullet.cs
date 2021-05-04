using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using PolyOne;
using PolyOne.Engine;
using PolyOne.Scenes;
using PolyOne.Collision;
using PolyOne.Components;

namespace AntiGrave
{
    public class Bullet : Entity
    {
        private Level level;

        private float rotation;
        private const float speed = 20.0f;
        private Vector2 remainder;

        private Texture2D debugTexture;
        private Texture2D bulletTexture;
        private Vector2 direction;
        private Vector2 velocity;

        private PlayerIndex playerIndex;

        CounterSet<string> counters = new CounterSet<string>();

        private Vector2 origin = new Vector2(6, 6);

        public Bullet(Vector2 position, Vector2 stickDirection, PlayerIndex playerIndexParam)
        : base(position)
        {

            playerIndex = playerIndexParam;
            this.Tag((int)GameTags.Bullet);
            this.Collider = new Hitbox(12.0f, 12.0f, -6.0f, -6.0f);
            direction = stickDirection;
            this.Visible = true;

            bulletTexture = Engine.Instance.Content.Load<Texture2D>("BulletTexture");
            debugTexture = Engine.Instance.Content.Load<Texture2D>("Tiles/Red");

            this.Add(counters);

            counters["bulletBuff"] = 66.8f;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (base.Scene is Level) {
                this.level = (base.Scene as Level);
            }
        }

        public override void Update()
        {
            base.Update();

            direction.Normalize();
            velocity = direction * speed;
            rotation = (float)Math.Atan2(direction.Y, direction.X);

            MovementHorizontal(velocity.X);
            MovementVerical(velocity.Y);
        }

        private void MovementHorizontal(float amount)
        {
            remainder.X += amount;
            int move = (int)Math.Round((double)remainder.X);

            if (move != 0)
            {
                remainder.X -= move;
                int sign = Math.Sign(move);

                while (move != 0)
                {
                    Vector2 newPosition = Position + new Vector2(sign, 0);

                    if (this.CollideFirst((int)GameTags.Solid, newPosition) != null)
                    {
                        this.RemoveSelf();
                        break;
                    }

                    if (this.CollideFirst((int)GameTags.ShieldOne, newPosition) != null)
                    {
                        if(level.playerOne.Shield.Activated == true)
                        {
                            direction = -direction;
                            break;
                        }
                    }

                    if (this.CollideFirst((int)GameTags.ShieldTwo, newPosition) != null)
                    {
                        if (level.playerTwo.Shield.Activated == true)
                        {
                            direction = -direction;
                            break;
                        }
                    }

                    if (this.CollideFirst((int)GameTags.PlayerOne, newPosition) != null &&
                        (counters["bulletBuff"] <= 0 || playerIndex == PlayerIndex.Two))
                    {
                        if (level.playerOne.HitPoints < level.playerTwo.HitLimit &&
                            level.playerOne.IsAlive == true)
                        {
                            level.playerTwo.HitPoints++;
                            level.playerOne.HitAction();
                            this.RemoveSelf();
                            break;
                        }
                    }

                    if (this.CollideFirst((int)GameTags.PlayerTwo, newPosition) != null &&
                       (counters["bulletBuff"] <= 0 || playerIndex == PlayerIndex.One))
                    {
                        if (level.playerTwo.HitPoints < level.playerTwo.HitLimit && 
                            level.playerTwo.IsAlive == true)
                        {
                            level.playerOne.HitPoints++;
                            level.playerTwo.HitAction();
                            this.RemoveSelf();
                            break;
                        }

                    }
                    Position.X += sign;
                    move -= sign;
                }
            }
        }

        private void MovementVerical(float amount)
        {
            remainder.Y += amount;
            int move = (int)Math.Round((double)remainder.Y);

            if (move < 0)
            {
                remainder.Y -= move;
                while (move != 0)
                {
                    Vector2 newPosition = Position + new Vector2(0, -1.0f);
                    if (this.CollideFirst((int)GameTags.Solid, newPosition) != null)
                    {
                        this.RemoveSelf();
                        break;
                    }

                    if (this.CollideFirst((int)GameTags.ShieldOne, newPosition) != null)
                    {
                        if (level.playerOne.Shield.Activated == true)
                        {
                            direction = -direction;
                            break;
                        }
                    }

                    if (this.CollideFirst((int)GameTags.ShieldTwo, newPosition) != null)
                    {
                        if (level.playerTwo.Shield.Activated == true)
                        {
                            direction = -direction;
                            break;
                        }
                    }

                    if (this.CollideFirst((int)GameTags.PlayerOne, newPosition) != null &&
                       (counters["bulletBuff"] <= 0 || playerIndex == PlayerIndex.Two))
                    {
                        if (level.playerOne.HitPoints < level.playerTwo.HitLimit &&
                            level.playerOne.IsAlive == true)
                        {
                            level.playerTwo.HitPoints++;
                            level.playerOne.HitAction();
                            this.RemoveSelf();
                            break;
                        }
                    }

                    if (this.CollideFirst((int)GameTags.PlayerTwo, newPosition) != null &&
                       (counters["bulletBuff"] <= 0 || playerIndex == PlayerIndex.One))
                    {
                        if (level.playerTwo.HitPoints < level.playerTwo.HitLimit &&
                            level.playerTwo.IsAlive == true)
                        {
                            level.playerOne.HitPoints++;
                            level.playerTwo.HitAction();
                            this.RemoveSelf();
                            break;
                        }
                    }
                    Position.Y += -1.0f;
                    move -= -1;
                }
            }
            else if (move > 0)
            {
                remainder.Y -= move;
                while (move != 0)
                {
                    Vector2 newPosition = Position + new Vector2(0, 1.0f);
                    if (this.CollideFirst((int)GameTags.Solid, newPosition) != null)
                    {
                        this.RemoveSelf();
                        break;
                    }

                    if (this.CollideFirst((int)GameTags.ShieldOne, newPosition) != null)
                    {
                        if (level.playerOne.Shield.Activated == true)
                        {
                            direction = -direction;
                            break;
                        }
                    }

                    if (this.CollideFirst((int)GameTags.ShieldTwo, newPosition) != null)
                    {
                        if (level.playerTwo.Shield.Activated == true)
                        {
                            direction = -direction;
                            break;
                        }
                    }

                    if (this.CollideFirst((int)GameTags.PlayerOne, newPosition) != null &&
                       (counters["bulletBuff"] <= 0 || playerIndex == PlayerIndex.Two))
                    {
                        if (level.playerOne.HitPoints < level.playerTwo.HitLimit &&
                            level.playerOne.IsAlive == true)
                        {
                            level.playerTwo.HitPoints++;
                            level.playerOne.HitAction();
                            this.RemoveSelf();
                            break;
                        }
                    }

                    if (this.CollideFirst((int)GameTags.PlayerTwo, newPosition) != null &&
                       (counters["bulletBuff"] <= 0 || playerIndex == PlayerIndex.One))
                    {
                        if (level.playerTwo.HitPoints < level.playerTwo.HitLimit &&
                            level.playerTwo.IsAlive == true)
                        {
                            level.playerOne.HitPoints++;
                            level.playerTwo.HitAction();
                            this.RemoveSelf();
                            break;
                        }
                    }
                    Position.Y += 1.0f;
                    move -= 1;
                }
            }
        }

        public override void Draw()
        {
            //Engine.SpriteBatch.Draw(debugTexture, new Rectangle((int)Position.X, (int)Position.Y, (int)Collider.Width, (int)Collider.Height), Color.White);
            Engine.SpriteBatch.Draw(bulletTexture, Position, null, Color.White, rotation, origin, 1.0f, SpriteEffects.None, 0.0f);
            base.Draw();
        }
    }
}
