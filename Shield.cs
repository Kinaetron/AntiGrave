using Microsoft.Xna.Framework.Graphics;

using PolyOne;
using PolyOne.Engine;
using PolyOne.Animation;
using PolyOne.Scenes;
using PolyOne.Collision;
using Microsoft.Xna.Framework;

namespace AntiGrave
{
    public class Shield : Entity
    {
        private Level level;
        private AnimationPlayer animationPlayer;
        private AnimationData shieldData;
        private Texture2D shieldTexture;

        private bool shieldDisplayed;

        private PlayerIndex index;

        public bool Activated { get; private set; }
        public bool On { get; private set; }

        public Shield(Vector2 playerPos, PlayerIndex index) :
            base(playerPos)
        {
            shieldDisplayed = false;
            shieldTexture = Engine.Instance.Content.Load<Texture2D>("DeflectionShield");
            shieldData = new AnimationData(shieldTexture, 80, 10, false);
            this.Collider = new Hitbox(10.0f, 10.0f, -5.0f, -5.0f);
            this.index = index;


            if (index == PlayerIndex.One) {
                this.Tag((int)GameTags.ShieldOne);
            }
            else if(index == PlayerIndex.Two) {
                this.Tag((int)GameTags.ShieldTwo);
            }
           

            animationPlayer = new AnimationPlayer();
            animationPlayer.PlayAnimation(shieldData);

            Visible = true;
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

            if(index == PlayerIndex.One) {
                Position = level.playerOne.Position;
            }
            else if(index == PlayerIndex.Two) {
                Position = level.playerTwo.Position;
            }

            if (animationPlayer.FrameIndex >= 2) {
                Activated = true;
            }
            else {
                Activated = false;
            }

        }

        public void ActivateShield()
        {
            On = true;
            shieldDisplayed = true;
            animationPlayer.PlayAnimation(shieldData);
        }

        public override void Draw()
        {
            base.Draw();

            if((index == PlayerIndex.One && level.playerOne.IsAlive) == true ||
               (index == PlayerIndex.Two && level.playerTwo.IsAlive == true))
            {
                if (shieldDisplayed == true)
                {
                    animationPlayer.Draw(Position, 0, SpriteEffects.None);

                    if (shieldData.AnimationFinished == true)
                    {
                        On = false;
                        animationPlayer.ResetAnimation();
                        shieldDisplayed = false;
                    }
                }
            }
            else if((index == PlayerIndex.One && level.playerOne.IsAlive) == false ||
                    (index == PlayerIndex.Two && level.playerTwo.IsAlive ==  false))
            {
                On = false;
                animationPlayer.ResetAnimation();
                shieldDisplayed = false;
            }
        }
    }
}
