using System;

using Microsoft.Xna.Framework;

using PolyOne.Engine;
using PolyOne.Utility;

using AntiGrave.Screens;

namespace AntiGrave
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class AntiGrave : Engine
    {
        static readonly string[] preloadAssets =
        {
            "MenuAssets/gradient",
        };

        public AntiGrave()
            :base(480, 360, "AntiGrave", 2.0f, false)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            TileInformation.TileDiemensions(16, 16);

            screenManager.AddScreen(new BackgroundScreen(), null);
            screenManager.AddScreen(new MainMenuScreen(), null);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            foreach (string asset in preloadAssets)
            {
                Engine.Instance.Content.Load<object>(asset);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //[STAThread]
        static void Main(string[] args)
        {
            using (AntiGrave game = new AntiGrave())
            {
                game.Run();
            }
        }
    }
}
