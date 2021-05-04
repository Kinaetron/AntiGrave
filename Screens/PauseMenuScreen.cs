
namespace AntiGrave.Screens
{
    class PauseMenuScreen : MenuScreen
    {
        public PauseMenuScreen()
            : base("Paused")
        {
            // Create our menu entries.
            MenuEntry resumeGameMenuEntry = new MenuEntry("Resume");
            MenuEntry optionsMenuEntry = new MenuEntry("Options");
            MenuEntry quitGameMenuEntry = new MenuEntry("Quit");

            // Hook up menu event handlers.
            resumeGameMenuEntry.Selected += OnCancel;
            optionsMenuEntry.Selected += OptionsMenuEntrySelected;
            quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;

            // Add entries to the menu.
            MenuEntries.Add(resumeGameMenuEntry);
            MenuEntries.Add(optionsMenuEntry);
            MenuEntries.Add(quitGameMenuEntry);
        }

        void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(),
                                                          new MainMenuScreen());
        }

        void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new OptionsMenuScreen(), e.PlayerIndex);
        }
    }
}
