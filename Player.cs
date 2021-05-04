using System;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using PolyOne;
using PolyOne.Collision;
using PolyOne.Scenes;
using PolyOne.Engine;
using PolyOne.Input;
using PolyOne.Animation;
using PolyOne.Components;
using PolyOne.Utility;

namespace AntiGrave
{
    public enum PlayerDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public class Player : Entity
    {
        private Level level;

        PlayerIndex playerIndex;

        private int signX;
        private int signY;
        private int previousSignX;
        private int previousSignY;
        private const float deadZone = 0.2f;
        private const float threshHold = 0.1f;
        private const float runAccel = 0.8f;
        private const float turnMul = 1.0f;
        private const float normMaxHorizSpeed = 3.0f;


        private bool shootButton;
        private bool deflectButton;
        private bool jumpButtonHeld;
        private bool jumpButtonPressed;
        private bool jumpButtonReleased;
        private const float airDragXLimit = 0.2f;
        private const float hangCutDown = 0.8f;
        private const float initialJumpHeight = 5.0f;
        private const float halfJumpHeight = 2.5f;
        private const float airDrag = 0.95f;

        private const float gravityUp = 0.25f;
        private const float gravityDown = 0.19f;
        private const float fallspeed = 4.0f;

        private const float graceTime = 66.9f;
        private const float graceTimePush = 66.9f;

        private const int wallDistanceOffSet = 8;

        private bool isOnGround;

        private bool isNearTop;
        private bool isNearBottom;
        private bool isNearLeft;
        private bool isNearRight;

        private bool isOnTop;
        private bool isOnBottom;
        private bool isOnLeft;
        private bool isOnRight;

        private Vector2 remainder;
        private Vector2 velocity;

        private Texture2D playerTexture;
        private const float rotationSpeed = 0.4f;
        private float currentRotation;
        private float playerRotation;

        private CounterSet<string> counters = new CounterSet<string>();

        private bool controllerMode;
        private bool keyboardMode;


        private List<Keys> keyList = new List<Keys>(new Keys[] { Keys.W, Keys.A, Keys.S, Keys.D, Keys.Up,
                                                                 Keys.Down, Keys.Left, Keys.Right ,Keys.Space });


        private const float kickbackVel = 0.5f;
        private const float kickbackTime = 100.2f;

        private StateMachine state;

        private const float resetTime = 2000.0f;
        private Texture2D hitTexture;
        public int HitPoints { get; set; }
        public int HitLimit { get { return 5; } }

        Gun gun;

        public PlayerDirection Direction { get; private set; }

        private static Random rand = new Random();

        public bool IsAlive { get; private set; }

        public Shield Shield { get; private set; }

        public Player(Vector2 position, PlayerIndex playerIndexParam)
        : base(position)
        {
            IsAlive = true;
            Direction = PlayerDirection.Right;
            playerRotation = 0;
            playerIndex = playerIndexParam;

            if (playerIndex == PlayerIndex.One) {
                this.Tag((int)GameTags.PlayerOne);
            }
            else if(playerIndex == PlayerIndex.Two) {
                this.Tag((int)GameTags.PlayerTwo);
            }

            this.Collider = new Hitbox((float)16.0f, (float)16.0f, -8.0f, -8.0f);

            playerTexture = Engine.Instance.Content.Load<Texture2D>("PlayerTexture");
            hitTexture = Engine.Instance.Content.Load<Texture2D>("HealthPointTexture");
            Visible = true;

            state = new StateMachine(4);
            state.SetCallbacks(0, new Func<int>(BottomUpdate), null, new Action(BottomEnter), new Action(BottomLeave));
            state.SetCallbacks(1, new Func<int>(TopUpdate), null, new Action(TopEnter), new Action(TopLeave));
            state.SetCallbacks(2, new Func<int>(LeftUpdate), null, new Action(LeftEnter), new Action(LeftLeave));
            state.SetCallbacks(3, new Func<int>(RightUpdate), null, new Action(RightEnter), new Action(RightLeave));

            this.Add(state);
            this.Add(counters);

            gun = new Gun(playerIndex, Vector2.Zero);
            Shield = new Shield(Position, playerIndex);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (base.Scene is Level) {
                this.level = (base.Scene as Level);
            }
            this.Scene.Add(gun);
            gun.Added(this.Scene);

            this.Scene.Add(Shield);
            Shield.Added(this.Scene);
        }

        private void BottomEnter()
        {
            isOnBottom = true;
            currentRotation = 0;
            
        }

        private void BottomLeave()
        {
            isOnBottom = false;
          
        }

        private int BottomUpdate()
        {
            if (IsAlive == false) {
                return 0;
            }

            isOnGround = base.CollideCheck((int)GameTags.Solid, this.Position + Vector2.UnitY);

            bool isOnLeft = base.CollideCheck((int)GameTags.Solid, this.Position - Vector2.UnitX);
            bool isOnRight = base.CollideCheck((int)GameTags.Solid, this.Position + Vector2.UnitX);

            playerRotation = MathHelper.Lerp(playerRotation, currentRotation, rotationSpeed);


            if (graceTimePush > 0 && velocity.Y < 0) {
                velocity.Y += gravityUp;
            }
            else
            {
                if (jumpButtonHeld == true && Math.Abs(velocity.Y) < 1) {
                    velocity.Y += gravityDown * hangCutDown;
                }
                else {
                    velocity.Y += gravityDown;
                }
            }

            if (velocity.Y < 0 && velocity.Y > -halfJumpHeight)
            {
                if (Math.Abs(velocity.X) > airDragXLimit) {
                    velocity.X *= airDrag;
                }
            }
            velocity.Y = MathHelper.Clamp(velocity.Y, -initialJumpHeight, fallspeed);

            if (isOnGround == true && isOnLeft == true && signY <= -1) {
                return 2;
            }

            if (isOnGround == true && isOnRight == true && signY <= -1) {
                return 2;
            }

            if (isOnGround == false  && isNearTop == true && signY <= -1) {
                return 1;
            }

            if(isOnGround == false && isNearLeft == true && signX <= -1) {
                return 2;
            }

            if (isOnGround == false && isNearRight == true && signX >= 1) {
                return 3;
            }

            return 0;
        }

        private void TopEnter()
        {
            isOnTop = true;
            currentRotation = MathHelper.Pi;
        }

        private void TopLeave()
        {
            isOnTop = false;
        }

        private int TopUpdate()
        {
            if (IsAlive == false) {
                return 0;
            }

            isOnGround = base.CollideCheck((int)GameTags.Solid, this.Position - Vector2.UnitY);

            bool isOnLeft = base.CollideCheck((int)GameTags.Solid, this.Position - Vector2.UnitX);
            bool isOnRight = base.CollideCheck((int)GameTags.Solid, this.Position + Vector2.UnitX);

            playerRotation = MathHelper.Lerp(playerRotation, currentRotation, rotationSpeed);

            if (graceTimePush > 0 && velocity.Y > 0) {
                velocity.Y += -gravityUp;
            }
            else
            {
                if (jumpButtonHeld == true && Math.Abs(velocity.Y) < 1) {
                    velocity.Y += -gravityDown * hangCutDown;
                }
                else {
                    velocity.Y += -gravityDown;
                }
            }

            if (velocity.Y > 0 && velocity.Y < halfJumpHeight)
            {
                if (Math.Abs(velocity.X) > airDragXLimit) {
                    velocity.X *= airDrag;
                }
            }

            velocity.Y = MathHelper.Clamp(velocity.Y, -fallspeed, initialJumpHeight);


            if (isOnGround == true && isOnLeft == true && signY >= 1) {
                return 2;
            }

            if (isOnGround == true && isOnRight == true && signY >= 1) {
                return 2;
            }

            if (isOnGround == false && isNearBottom == true && signY >= 1) {
                return 0;
            }

            if (isOnGround == false && isNearLeft == true && signX <= -1) {
                return 2;
            }

            if (isOnGround == false && isNearRight == true && signX >= 1) {
                return 3;
            }

            return 1;
        }

    

        private void LeftEnter()
        {
            isOnLeft = true;
            currentRotation = MathHelper.PiOver2;
        }

        private void LeftLeave()
        {
            isOnLeft = false;
        }

        private int LeftUpdate()
        {
            if (IsAlive == false) {
                return 0;
            }

            isOnGround = base.CollideCheck((int)GameTags.Solid, this.Position - Vector2.UnitX);

            bool isOnTop = base.CollideCheck((int)GameTags.Solid, this.Position - Vector2.UnitY);
            bool isOnBottom = base.CollideCheck((int)GameTags.Solid, this.Position + Vector2.UnitY);

            playerRotation = MathHelper.Lerp(playerRotation, currentRotation, rotationSpeed);

            if (graceTimePush > 0 && velocity.X > 0) {
                velocity.X += -gravityUp;
            }
            else
            {
                if (jumpButtonHeld == true && Math.Abs(velocity.X) < 1) {
                    velocity.X += -gravityDown * hangCutDown;
                }
                else {
                    velocity.X += -gravityDown;
                }
            }

            if (velocity.X > 0 && velocity.X < halfJumpHeight)
            {
                if (Math.Abs(velocity.Y) > airDragXLimit) {
                    velocity.Y *= airDrag;
                }
            }

            velocity.X = MathHelper.Clamp(velocity.X, -fallspeed, initialJumpHeight);

            if (isOnGround == true && isOnTop == true && signX >= 1) {
                return 1;
            }

            if (isOnGround == true && isOnBottom == true && signX >= 1) {
                return 0;
            }

            if (isOnGround == false && isNearBottom == true && signY >= 1) {
                return 0;
            }

            if (isOnGround == false && isNearTop == true && signY <= -1) {
                return 1;
            }

            if (isOnGround == false && isNearLeft == true && signX <= -1) {
                return 2;
            }

            if (isOnGround == false && isNearRight == true && signX >= 1) {
                return 3;
            }

            return 2;
        }

        private void RightEnter()
        {
            isOnRight = true;
            currentRotation = -MathHelper.PiOver2;
        }

        private void RightLeave()
        {
            isOnRight = false;
        }

        private int RightUpdate()
        {
            if (IsAlive == false) {
                return 0;
            }

            isOnGround = base.CollideCheck((int)GameTags.Solid, this.Position + Vector2.UnitX);

            bool isOnTop = base.CollideCheck((int)GameTags.Solid, this.Position - Vector2.UnitY);
            bool isOnBottom = base.CollideCheck((int)GameTags.Solid, this.Position + Vector2.UnitY);

            playerRotation = MathHelper.Lerp(playerRotation, currentRotation, rotationSpeed);

            if (graceTimePush > 0 && velocity.X < 0) {
                velocity.X += gravityUp;
            }
            else
            {
                if (jumpButtonHeld == true && Math.Abs(velocity.X) < 1) {
                    velocity.X += gravityDown * hangCutDown;
                }
                else {
                    velocity.X += gravityDown;
                }
            }

            if (velocity.X < 0 && velocity.X > -halfJumpHeight)
            {
                if (Math.Abs(velocity.Y) > airDragXLimit) {
                    velocity.Y *= airDrag;
                }
            }
            velocity.X = MathHelper.Clamp(velocity.X, -initialJumpHeight, fallspeed);

            if (isOnGround == true && isOnTop == true && signX <= -1) {
                return 1;
            }

            if (isOnGround == true && isOnBottom == true && signX <= -1) {
                return 0;
            }

            if (isOnGround == false && isNearBottom == true && signY >= 1) {
                return 0;
            }

            if (isOnGround == false && isNearTop == true && signY <= -1) {
                return 1;
            }

            if (isOnGround == false && isNearLeft == true && signX <= -1) {
                return 2;
            }

            return 3;
        }

        public override void Update()
        {
            ResetPlayer();

            base.Update();

            if (IsAlive == false) {
                return;
            }

            isNearBottom = base.CollideCheck((int)GameTags.Solid, this.Position + new Vector2(0, wallDistanceOffSet));
            isNearTop = base.CollideCheck((int)GameTags.Solid, this.Position - new Vector2(0, wallDistanceOffSet));
            isNearRight = base.CollideCheck((int)GameTags.Solid, this.Position + new Vector2(wallDistanceOffSet, 0));
            isNearLeft = base.CollideCheck((int)GameTags.Solid, this.Position - new Vector2(wallDistanceOffSet, 0));

            PlayerDirectMethod();
            InputTypeCheck();
            InputReading();

            MovementPhysics();
            KickbackPhysics();
            JumpPhysics();

            if(shootButton == true) {
                gun.Shoot();
                counters["kickbackTimer"] = kickbackTime;
            }

            if(deflectButton == true) {
                Shield.ActivateShield();
            }

            if(isOnTop == true)
            {
                MovementHorizontalX(velocity.X);
                MovementVericalYTop(velocity.Y);
            }
            else if(isOnBottom == true)
            {
                MovementHorizontalX(velocity.X);
                MovementVericalYBottom(velocity.Y);
            }
            else if(isOnRight == true)
            {
                MovementHorizontalY(velocity.Y);
                MovementVericalXRight(velocity.X);
            }
            else if(isOnLeft)
            {
                MovementHorizontalY(velocity.Y);
                MovementVericalXLeft(velocity.X);
            }
        }

        private void InputTypeCheck()
        {
            foreach (Keys key in keyList)
            {
                if (PolyInput.Keyboard.Check(key) == true) {
                    controllerMode = false;
                    keyboardMode = true;
                }
            }

            if (PolyInput.GamePads[(int)playerIndex].ButtonCheck() == true) {
                controllerMode = true;
                keyboardMode = false;
            }

            if (controllerMode == false && keyboardMode == false) {
                keyboardMode = true;
            }
        }

        private void PlayerDirectMethod()
        {
            if(isOnBottom == true || isOnTop == true)
            {
                if(previousSignX > 0) {
                    Direction = PlayerDirection.Right;
                }
                else if(previousSignX < 0) {
                    Direction = PlayerDirection.Left;
                }
            }
            else if(isOnRight == true || isOnLeft == true)
            {
                if (previousSignY < 0) {
                    Direction = PlayerDirection.Up;
                }
                else if (previousSignY > 0) {
                    Direction = PlayerDirection.Down;
                }
            }
        }

        private void InputReading()
        {
            signX = 0;
            signY = 0;

            shootButton = false;
            deflectButton = false;
            jumpButtonHeld = false;
            jumpButtonPressed = false;
            jumpButtonReleased = false;

            if (controllerMode == true)
            {
                if (PolyInput.GamePads[(int)playerIndex].LeftStickVertical(deadZone) > threshHold ||
                       PolyInput.GamePads[(int)playerIndex].DPadUpCheck == true)
                {
                    signY = -1;
                }
                else if (PolyInput.GamePads[(int)playerIndex].LeftStickVertical(deadZone) < -threshHold ||
                         PolyInput.GamePads[(int)playerIndex].DPadDownCheck == true)
                {
                    signY = 1;
                }

                if (PolyInput.GamePads[(int)playerIndex].LeftStickHorizontal(deadZone) > threshHold ||
                      PolyInput.GamePads[(int)playerIndex].DPadRightCheck == true)
                {
                    signX = 1;
                }
                else if (PolyInput.GamePads[(int)playerIndex].LeftStickHorizontal(deadZone) < -threshHold ||
                         PolyInput.GamePads[(int)playerIndex].DPadLeftCheck == true)
                {
                    signX = -1;
                }

                if (PolyInput.GamePads[(int)playerIndex].Check(Buttons.A) == true) {
                    jumpButtonHeld = true;
                }

                if (PolyInput.GamePads[(int)playerIndex].Pressed(Buttons.A) == true) {
                    jumpButtonPressed = true;
                }

                if (PolyInput.GamePads[(int)playerIndex].Released(Buttons.A) == true) {
                    jumpButtonReleased = true;
                }

                if (PolyInput.GamePads[(int)playerIndex].RightTriggerPressed(0.5f) == true) {
                    shootButton = true;
                }

                if (PolyInput.GamePads[(int)playerIndex].Pressed(Buttons.B) == true) {
                    deflectButton = true;
                }
            }
            else if(keyboardMode == true && playerIndex == PlayerIndex.One)
            {
                if (PolyInput.Keyboard.Check(Keys.Right) ||
                    PolyInput.Keyboard.Check(Keys.D))
                {
                    signX = 1;
                }
                else if (PolyInput.Keyboard.Check(Keys.Left) ||
                        PolyInput.Keyboard.Check(Keys.A))
                {
                    signX = -1;
                }

                if (PolyInput.Keyboard.Check(Keys.Up) ||
                       PolyInput.Keyboard.Check(Keys.W))
                {
                    signY = -1;
                }
                else if (PolyInput.Keyboard.Check(Keys.Down) ||
                        PolyInput.Keyboard.Check(Keys.S))
                {
                    signY = 1;
                }

                if (PolyInput.Keyboard.Check(Keys.Space) == true) {
                    jumpButtonHeld = true;
                }

                if (PolyInput.Keyboard.Pressed(Keys.Space) == true) {
                    jumpButtonPressed = true;
                }

                if (PolyInput.Keyboard.Released(Keys.Space) == true) {
                    jumpButtonReleased = true;
                }
            }

            previousSignX = signX;
            previousSignY = signY;
        }

        private void MovementPhysics()
        {
            if(isOnBottom == true || isOnTop == true)
            {
                velocity.X += runAccel * signX;
                velocity.X = MathHelper.Clamp(velocity.X, -normMaxHorizSpeed, normMaxHorizSpeed);

                float currentSign = Math.Sign(velocity.X);

                if (currentSign != 0 && currentSign != signX) {
                    velocity.X *= turnMul;
                }

                if (signX == 0 && counters["kickbackTimer"] <= 0) {
                    velocity.X = 0;
                }
            }

            if (isOnLeft == true || isOnRight == true)
            {
                velocity.Y += runAccel * signY;
                velocity.Y = MathHelper.Clamp(velocity.Y, -normMaxHorizSpeed, normMaxHorizSpeed);

                float currentSign = Math.Sign(velocity.Y);

                if (currentSign != 0 && currentSign != signY) {
                    velocity.Y *= turnMul;
                }

                if (signY == 0) {
                    velocity.Y = 0;
                }
            }
        }

        private void KickbackPhysics()
        {
            if(counters["kickbackTimer"] <= 0 || gun.BulletInClip == false) {
                return;
            }

            Vector2 direction = gun.GunDirection();
            direction.Normalize();

            if (isOnBottom == true || isOnTop == true) {
                velocity.X = kickbackVel * -direction.X;
            }

            if(isOnLeft == true || isOnRight == true) {
                velocity.Y = kickbackVel * -direction.Y;
            }
        }

        private void JumpPhysics()
        {
            if (isOnGround == true) {
                counters["graceTimer"] = graceTime;
            }

            if (jumpButtonPressed == true) {
                counters["graceTimerPush"] = graceTimePush;
            }

            if (counters["graceTimerPush"] > 0)
            {
                if (isOnGround == true || counters["graceTimer"] > 0)
                {
                    counters["graceTimerPush"] = 0.0f;

                    if (isOnBottom == true) {
                        velocity.Y = -initialJumpHeight;
                    }
                    else if (isOnTop == true) {
                        velocity.Y = initialJumpHeight;
                    }
                    else if (isOnRight == true) {
                        velocity.X = -initialJumpHeight;
                    }
                    else if (isOnLeft == true) {
                        velocity.X = initialJumpHeight;
                    }
                }
            }
            else if (jumpButtonReleased == true && velocity.Y < 0.0f &&
                     velocity.Y < -halfJumpHeight && isOnBottom == true)
            {
                counters["graceTimerPush"] = 0.0f;
                velocity.Y = -halfJumpHeight;
            }
            else if (jumpButtonReleased == true && velocity.Y > 0.0f &&
                    velocity.Y > halfJumpHeight && isOnTop == true)
            {
                counters["graceTimerPush"] = 0.0f;
                velocity.Y = halfJumpHeight;
            }
            else if (jumpButtonReleased == true && velocity.X < 0.0f &&
                    velocity.X < -halfJumpHeight && isOnRight == true)
            {
                counters["graceTimerPush"] = 0.0f;
                velocity.X = -halfJumpHeight;
            }
            else if (jumpButtonReleased == true && velocity.X > 0.0f &&
                    velocity.X > halfJumpHeight && isOnLeft == true)
            {
                counters["graceTimerPush"] = 0.0f;
                velocity.X = halfJumpHeight;
            }
        }

        public void HitAction()
        {
            IsAlive = false;
            counters["playerReset"] = resetTime;
            PolyInput.GamePads[(int)playerIndex].Rumble(0.6f, 500);
        }

        private void ResetPlayer()
        {
            if (IsAlive == false && counters["playerReset"] <= 0)
            {
                IsAlive = true;
                Position = level.SpawnPoints[rand.Next(level.SpawnPoints.Count)];
            }
        }

        private void MovementHorizontalX(float amount)
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
                        velocity.X = 0.0f;
                        remainder.X = 0;
                        break;
                    }
                    Position.X += sign;
                    move -= sign;
                }
            }
        }

        private void MovementHorizontalY(float amount)
        {
            remainder.Y += amount;
            int move = (int)Math.Round((double)remainder.Y);

            if (move != 0)
            {
                remainder.Y -= move;
                int sign = Math.Sign(move);

                while (move != 0)
                {
                    Vector2 newPosition = Position + new Vector2(0, sign);

                    if (this.CollideFirst((int)GameTags.Solid, newPosition) != null)
                    {
                        velocity.Y = 0.0f;
                        remainder.Y = 0;
                        break;
                    }
                    Position.Y += sign;
                    move -= sign;
                }
            }
        }

        private void MovementVericalYBottom(float amount)
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
                        velocity.Y = 0.0f;
                        remainder.Y = 0;
                        break;
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
                        velocity.Y = 0.0f;
                        remainder.Y = 0;
                        break;
                    }
                    Position.Y += 1.0f;
                    move -= 1;
                }
            }
        }

        private void MovementVericalYTop(float amount)
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
                        velocity.Y = 0.0f;
                        remainder.Y = 0;
                        break;
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
                        velocity.Y = 0.0f;
                        remainder.Y = 0;
                        break;
                    }
                    Position.Y += 1.0f;
                    move -= 1;
                }
            }
        }

        private void MovementVericalXRight(float amount)
        {
            remainder.X += amount;
            int move = (int)Math.Round((double)remainder.X);

            if (move < 0)
            {
                remainder.X -= move;
                while (move != 0)
                {
                    Vector2 newPosition = Position + new Vector2(-1.0f, 0);
                    if (this.CollideFirst((int)GameTags.Solid, newPosition) != null)
                    {
                        velocity.X = 0.0f;
                        remainder.X = 0;
                        break;
                    }
                    Position.X += -1.0f;
                    move -= -1;
                }
            }
            else if (move > 0)
            {
                remainder.X -= move;
                while (move != 0)
                {
                    Vector2 newPosition = Position + new Vector2(1.0f, 0);
                    if (this.CollideFirst((int)GameTags.Solid, newPosition) != null)
                    {
                        velocity.X = 0.0f;
                        remainder.X = 0;
                        break;
                    }
                    Position.X += 1.0f;
                    move -= 1;
                }
            }
        }

        private void MovementVericalXLeft(float amount)
        {
            remainder.X += amount;
            int move = (int)Math.Round((double)remainder.X);

            if (move < 0)
            {
                remainder.X -= move;
                while (move != 0)
                {
                    Vector2 newPosition = Position + new Vector2(-1.0f, 0);
                    if (this.CollideFirst((int)GameTags.Solid, newPosition) != null)
                    {
                        velocity.X = 0.0f;
                        remainder.X = 0;
                        break;
                    }
                    Position.X += -1.0f;
                    move -= -1;
                }
            }
            else if (move > 0)
            {
                remainder.X -= move;
                while (move != 0)
                {
                    Vector2 newPosition = Position + new Vector2(1.0f, 0);
                    if (this.CollideFirst((int)GameTags.Solid, newPosition) != null)
                    {
                        velocity.X = 0.0f;
                        remainder.X = 0;
                        break;
                    }
                    Position.X += 1.0f;
                    move -= 1;
                }
            }
        }



        public override void Draw()
        {
            base.Draw();

            if (IsAlive == true) {
                Engine.SpriteBatch.Draw(playerTexture, Position, null, Color.White, playerRotation, new Vector2(8, 8), 1.0f, SpriteEffects.None, 0.0f);
            }

            if (playerIndex == PlayerIndex.One)
            {
                for (int i = 0; i < HitPoints; i++) {
                    Engine.SpriteBatch.Draw(hitTexture, new Vector2(40 + (i * 20), 21));
                }
            }

            if (playerIndex == PlayerIndex.Two)
            {
                for (int i = 0; i < HitPoints; i++) {
                    Engine.SpriteBatch.Draw(hitTexture, new Vector2(422 + (i * -20), 21));
                }
            }
        }
    }
}