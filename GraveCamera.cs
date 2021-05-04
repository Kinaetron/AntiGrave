using Microsoft.Xna.Framework;
using PolyOne.Utility;

namespace AntiGrave
{
    public class GraveCamera : Camera
    {
        private Vector2 pullDirection;
        private Vector2 priorCameraPos = Vector2.Zero;
        private Vector2 camGotoPosition = Vector2.Zero;
        private float strength = 2.0f;

        private bool pullback = false;

        public GraveCamera()
        {
            Position = new Vector2(16, 16);
            priorCameraPos = Position;
        }

        public void CameraPullBack(Vector2 gunDirection)
        {
            if(pullback == false)
            {
                pullDirection = gunDirection;
                pullback = true;
            }
        }

        public void Update()
        {
            if(pullback == true)
            { 
                camGotoPosition = priorCameraPos + pullDirection * strength;

                Position = Vector2.Lerp(Position, camGotoPosition, 0.95f);

                if (Position == camGotoPosition) {
                    pullback = false;
                }
                
            }
            else {
                Position = Vector2.Lerp(priorCameraPos, Position, 0.6f);
            }
        }
    }
}
