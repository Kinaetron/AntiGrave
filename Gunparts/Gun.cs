using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using PolyOne;
using PolyOne.Input;
using PolyOne.Engine;
using PolyOne.Scenes;
using PolyOne.Utility;
using PolyOne.Components;


namespace AntiGrave
{
    public class Gun : Entity
    {
        private Level level;

        private const float deadZone = 0.2f;
        private const float threshHold = 0.1f;

        private bool reloadButton;
        private bool reloadTry;
        private float reloadSpeed;
        private Texture2D reloadLineTexture;
        private Texture2D reloadTimeLineTexture;
        private Texture2D reloadQuickTexture;
        private Vector2 reloadTimePosition;
        private bool reloadTimeUp = false;
        private const int clipSize = 4;
        private const float reloadQuickBegin = 500.0f;
        private const float reloadQuickEnd = 250.0f;
        private const float reloadTime = 1000.0f;
        private const float bulletBuffer = 160.0f;
        private float rightStickAngle;
        private const float PiOver8 = MathHelper.PiOver4 / 2;
        private Vector2 reloadActual;

        private bool isOnTile;
        private bool isOnTarget;
        private Vector2 target = Vector2.Zero;
        private Vector2 previousTarget = new Vector2(1, 0);

        private float scopeToOpponent;
        private float rayDistance = 500.0f;
        private const float viewSpeedNormal = 0.3f;
        private const float viewSpeedOnTarget = 0.1f;
        private Vector2 view = Vector2.Zero;
        private Vector2 previousView;

        private Texture2D pixel;
        private Vector2 stick;
        private Texture2D scopeTexture;
        private const float scopeSize = 25.0f;
        private Vector2 scopeOrigin = new Vector2(3, 3);

        private bool movementMode;
        private bool stickMode;

        private PlayerIndex playerIndex;

        private CounterSet<string> counters = new CounterSet<string>();
        Vector2 playerCentre = Vector2.Zero;
        Vector2 opponentCentre = Vector2.Zero;

        private List<Texture2D> bulletPoints = new List<Texture2D>();

        private bool died = false;

        public bool BulletInClip
        {
            get
            {
               if(bulletPoints.Count > 0) {
                    return true;
                }
               else {
                    return false;
                }
            }
        }

        public Gun(PlayerIndex playerIndex, Vector2 position)
            :base(position)
        {
            movementMode = true;
            stickMode = false;

            this.playerIndex = playerIndex;
            scopeTexture = Engine.Instance.Content.Load<Texture2D>("Scope");
            reloadLineTexture = Engine.Instance.Content.Load<Texture2D>("ReloadLine");
            reloadTimeLineTexture = Engine.Instance.Content.Load<Texture2D>("ReloadTimeLine");
            reloadQuickTexture = Engine.Instance.Content.Load<Texture2D>("QuickReloadTexture");
            pixel = Engine.Instance.Content.Load<Texture2D>("pixelTexture");

            for (int i = 0; i < clipSize; i++) {
                bulletPoints.Add(Engine.Instance.Content.Load<Texture2D>("BulletPointTexture"));
            }
          
            this.Visible = true;

            this.Add(counters);
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

           if (level.playerOne.IsAlive == false && playerIndex == PlayerIndex.One)
           {
                died = true;
                reloadTry = false;
                reloadSpeed = 0.0f;
                reloadTimeUp = false;
                return;
           }
           else if(level.playerOne.IsAlive == true && 
                  playerIndex == PlayerIndex.One && bulletPoints.Count < clipSize && died == true)
           {
                died = false;
                Reload();
           }

           if (level.playerTwo.IsAlive == false && playerIndex == PlayerIndex.Two)
           {
                died = true;
                reloadTry = false;
                reloadSpeed = 0.0f;
                reloadTimeUp = false;
                return;
           }
            else if (level.playerTwo.IsAlive == true &&
                     playerIndex == PlayerIndex.Two && bulletPoints.Count < clipSize && died == true)
            {
                died = false;
                Reload();
            }

            AimMode();

            reloadButton = false;

            if (PolyInput.GamePads[(int)playerIndex].LeftTriggerPressed(0.5f) == true) {
                reloadButton = true;
            }

            if (playerIndex == PlayerIndex.One)
            {
                playerCentre = level.playerOne.Centre;
                opponentCentre = level.playerTwo.Centre;
            }
            else if(playerIndex == PlayerIndex.Two)
            {
                playerCentre = level.playerTwo.Centre;
                opponentCentre = level.playerOne.Centre;
            }

            if ((bulletPoints.Count <= 0 || reloadButton == true && bulletPoints.Count < clipSize) && 
                counters["reloadTimer"] <= 0 && reloadTimeUp == false)
            {

                reloadButton = false;
                reloadTimeUp = true;
                counters["reloadTimer"] = reloadTime;
            }

            if(reloadTimeUp == true && counters["reloadTimer"] <= reloadQuickBegin &&
               counters["reloadTimer"] >= reloadQuickEnd && reloadTry == false && reloadButton == true)
            {
                reloadTry = true;
                counters["reloadTimer"] = 0;
            }
            else if((counters["reloadTimer"] > reloadQuickBegin || 
                    counters["reloadTimer"] < reloadQuickEnd) && 
                    reloadTimeUp == true && reloadButton == true && reloadTry == false)
            {
                reloadTry = true;
            }

            if(reloadTimeUp == true && counters["reloadTimer"] <= 0)
            {
                reloadTry = false;
                reloadSpeed = 0.0f;
                reloadTimeUp = false;
                Reload();
            }

            if (Target().LengthSquared() > 0.3f * 0.3f)
            {
                stick = View();
                stick *= scopeSize;
            }

            if (counters["reloadTimer"] > 0)
            {
                reloadSpeed += reloadLineTexture.Width / (reloadTime / Engine.DeltaTime);

                reloadTimePosition = new Vector2((playerCentre.X - 10) + reloadSpeed, playerCentre.Y - 12);
                reloadActual = new Vector2((playerCentre.X - 10), playerCentre.Y - 12);
            }
        }

        public void Shoot()
        {
            if(bulletPoints.Count <= 0) {
                return;
            }

            if(playerIndex == PlayerIndex.One) {
                bulletPoints.RemoveAt(0);
            }

            if(playerIndex == PlayerIndex.Two) {
                bulletPoints.RemoveAt(0);
            }

            Bullet bullet;

            if (counters["bulletBufferTimer"] > 0) {
                return;
            }

            if(stickMode == true)
            {
                bullet = new Bullet(playerCentre, Target(), playerIndex);
                this.Scene.Add(bullet);
                bullet.Added(this.Scene);

                level.Camera.CameraPullBack(GeneralDirection());
            }
            else if(movementMode == true)
            {
                bullet = new Bullet(playerCentre, MovementDirection(), playerIndex);
                this.Scene.Add(bullet);
                bullet.Added(this.Scene);

                level.Camera.CameraPullBack(MovementDirection());
            }
            
            counters["bulletBufferTimer"] = bulletBuffer;
           
        }


        private void Reload()
        {
            bulletPoints.Clear();

            for (int i = 0; i < clipSize; i++) {
                bulletPoints.Add(Engine.Instance.Content.Load<Texture2D>("BulletPointTexture"));
            }
        }

        private Vector2 Target()
        {
            rightStickAngle = (float)Math.Atan2(PolyInput.GamePads[(int)playerIndex].GetRightStick().Y,
                                                PolyInput.GamePads[(int)playerIndex].GetRightStick().X);

            Vector2 currentDirection = PolyInput.GamePads[(int)playerIndex].GetRightStick(0.2f);

            if (currentDirection != Vector2.Zero)
            {
                target = new Vector2(currentDirection.X, -currentDirection.Y);
                previousTarget = target;
                return target;
            }
            else
            {
                target = previousTarget;
                return target;
            }
        }

        private void AimMode()
        {
            if (PolyInput.GamePads[(int)playerIndex].GetRightStick(deadZone).
                         LengthSquared() > threshHold)
            {
                stickMode = true;
                movementMode = false;
            }
            else
            {
                movementMode = true;
                stickMode = false;
            }
        }

        public Vector2 GunDirection()
        {
            if (stickMode == true) {
                return GeneralDirection();
            }
            else if (movementMode == true) {
                return MovementDirection();
            }
            else {
                return Vector2.Zero;
            }
        }

        private Vector2 MovementDirection()
        {
            Vector2 direction = Vector2.Zero;

            if(playerIndex == PlayerIndex.One)
            {
                if (level.playerOne.Direction == PlayerDirection.Right) {
                    direction = new Vector2(1, 0);
                }

                if (level.playerOne.Direction == PlayerDirection.Left) {
                    direction = new Vector2(-1, 0);
                }

                if (level.playerOne.Direction == PlayerDirection.Up) {
                    direction = new Vector2(0, -1);
                }

                if (level.playerOne.Direction == PlayerDirection.Down) {
                    direction = new Vector2(0, 1);
                }
            }


            if(playerIndex == PlayerIndex.Two)
            {
                if (level.playerTwo.Direction == PlayerDirection.Right) {
                    direction = new Vector2(1, 0);
                }

                if (level.playerTwo.Direction == PlayerDirection.Left) {
                    direction = new Vector2(-1, 0);
                }

                if (level.playerTwo.Direction == PlayerDirection.Up) {
                    direction = new Vector2(0, -1);
                }

                if (level.playerTwo.Direction == PlayerDirection.Down) {
                    direction = new Vector2(0, 1);
                }

            }
            return direction;
        }

        private Vector2 GeneralDirection()
        {
            float angle = (float)Math.Atan2(PolyInput.GamePads[(int)playerIndex].GetRightStick(0.2f).Y,
                                            PolyInput.GamePads[(int)playerIndex].GetRightStick(0.2f).X);

            Vector2 direction = Vector2.Zero;

            //Top
            if (angle <= MathHelper.PiOver2 + PiOver8 &&
                angle >= MathHelper.PiOver2 - PiOver8)
            {
                direction = new Vector2(0, -1);
            }

            // Right
            if (angle <= PiOver8 && rightStickAngle >= -PiOver8)
            {
                direction = new Vector2(1, 0);
            }

            // Left
            if (angle >= MathHelper.Pi - PiOver8 ||
                angle <= -MathHelper.Pi + PiOver8)
            {
                direction = new Vector2(-1, 0);
            }

            // Bottom
            if (angle <= -MathHelper.PiOver2 + PiOver8 &&
                angle >= -MathHelper.PiOver2 - PiOver8)
            {
                direction = new Vector2(0, 1);
            }

            // Top Left
            if (angle <= MathHelper.PiOver2 + MathHelper.PiOver4 + PiOver8 &&
               angle >= MathHelper.PiOver2 + MathHelper.PiOver4 - PiOver8)
            {
                direction = new Vector2(-1, -1);
            }

            // Top Right 
            if (angle <= MathHelper.PiOver4 + PiOver8 &&
                angle >= MathHelper.PiOver4 - PiOver8)
            {
                direction = new Vector2(1, -1);
            }


            // Bottom Left
            if (angle <= -MathHelper.PiOver2 - MathHelper.PiOver4 - PiOver8 ||
               angle <= -MathHelper.PiOver2 - MathHelper.PiOver4 + PiOver8)
            {
                direction = new Vector2(-1, 1);
            }

            // Bottom Right
            if (angle <= -MathHelper.PiOver4 + PiOver8 &&
               angle >= -MathHelper.PiOver4 - PiOver8)
            {
                direction = new Vector2(1, 1);
            }

            return direction;
        }

        private Vector2 View()
        {
            scopeToOpponent = Vector2.Distance(playerCentre - scopeOrigin, opponentCentre);

            isOnTile = level.tilesSolid.CollideLine(playerCentre - scopeOrigin,
                                                   playerCentre - scopeOrigin + (view * scopeToOpponent));


            if (playerIndex == PlayerIndex.One) {
                isOnTarget = level.playerTwo.CollideLine(playerCentre - scopeOrigin,
                                                         playerCentre - scopeOrigin + (view * rayDistance));
            }
            else if(playerIndex == PlayerIndex.Two) {
                isOnTarget = level.playerOne.CollideLine(playerCentre - scopeOrigin,
                                                            playerCentre - scopeOrigin + (view * rayDistance));
            }

            if (isOnTarget == true && isOnTile == false) {
                view = Vector2.Lerp(view, Target(), viewSpeedOnTarget);
                view = Vector2.Normalize(view);
            }
            else {
                view = Vector2.Lerp(view, Target(), viewSpeedNormal);
                view = Vector2.Normalize(view);
            }

            previousView = view;
            return view;
        }

        public override void Draw()
        {
            base.Draw();

            if (playerIndex == PlayerIndex.One)
            {
                for (int i = 0; i < bulletPoints.Count; i++) {
                    Engine.SpriteBatch.Draw(bulletPoints[i], new Vector2(21, 60 + (i * -10)));
                }
            }

            if (playerIndex == PlayerIndex.Two)
            {
                for (int i = 0; i < bulletPoints.Count; i++) {
                    Engine.SpriteBatch.Draw(bulletPoints[i], new Vector2(485, 60 + (i * -10)));
                }
            }

            if((level.playerOne.IsAlive == true && playerIndex == PlayerIndex.One) ||
               (level.playerTwo.IsAlive == true && playerIndex == PlayerIndex.Two))
            {
                if (counters["reloadTimer"] > 0)
                {
                    Engine.SpriteBatch.Draw(reloadQuickTexture, new Vector2(reloadActual.X + 15, reloadActual.Y), Color.White);
                    Engine.SpriteBatch.Draw(reloadTimeLineTexture, reloadTimePosition, Color.White);
                    Engine.SpriteBatch.Draw(reloadLineTexture, new Vector2(playerCentre.X - 10, playerCentre.Y - 10), Color.White);

                }

                if (PolyInput.GamePads[(int)playerIndex].GetRightStick(deadZone).
                             LengthSquared() > threshHold)
                {
                    Engine.SpriteBatch.Draw(scopeTexture, new Vector2(playerCentre.X + stick.X - scopeOrigin.X,
                                                                      playerCentre.Y + stick.Y - scopeOrigin.Y), Color.White);
                }
            }
        }

        public void DrawLine(Vector2 begin, Vector2 end, int width = 1)
        {
            Rectangle r = new Rectangle((int)begin.X, (int)begin.Y, (int)(end - begin).Length() + width, width);
            Vector2 v = Vector2.Normalize(begin - end);
            float angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
            if (begin.Y > end.Y) angle = MathHelper.TwoPi - angle;
            Engine.SpriteBatch.Draw(pixel, r, null, Color.White, angle, Vector2.Zero, SpriteEffects.None, 0);
        }
    }
}