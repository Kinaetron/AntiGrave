using System;

using Microsoft.Xna.Framework;

using PolyOne.Engine;
using PolyOne.ScreenManager;


namespace AntiGrave.Screens
{
    class LoadingScreen : GameScreen
    {
        bool loadingIsSlow;
        bool otherScreensAreGone;

        GameScreen[] screensToLoad;

        private LoadingScreen(ScreenManager screenManager, bool loadingIsSlow,
                             GameScreen[] screensToLoad)
        {
            this.loadingIsSlow = loadingIsSlow;
            this.screensToLoad = screensToLoad;

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
        }

        public static void Load(ScreenManager screenManager, bool loadingIsSlow,
                              PlayerIndex? controllingPlayer,
                              params GameScreen[] screensToLoad)
        {
            foreach (GameScreen screen in screenManager.GetScreens())
                screen.ExitScreen();

            LoadingScreen loadingScreen = new LoadingScreen(screenManager,
                                                            loadingIsSlow,
                                                            screensToLoad);

            screenManager.AddScreen(loadingScreen, controllingPlayer);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                    bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (otherScreensAreGone)
            {
                ScreenManager.RemoveScreen(this);

                foreach (GameScreen screen in screensToLoad)
                {
                    if (screen != null)
                    {
                        ScreenManager.AddScreen(screen, ControllingPlayer);
                    }
                }

                ScreenManager.Game.ResetElapsedTime();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if ((ScreenState == ScreenState.Active) &&
                (ScreenManager.GetScreens().Length == 1))
            {
                otherScreensAreGone = true;
            }

            if (loadingIsSlow)
            {
                Vector2 loadingPosition = new Vector2(Engine.VirtualWidth - 16, Engine.VirtualHeight - 16);
                Color color = Color.White * TransitionAlpha;
            }
        }
    }
}
