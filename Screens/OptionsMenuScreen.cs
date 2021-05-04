using Microsoft.Xna.Framework.Media;
using PolyOne.Utility;

namespace AntiGrave.Screens
{
    class OptionsMenuScreen : MenuScreen
    {
        MenuEntry fullscreenMenuEntry;
        MenuEntry volumeLevel;

        static bool fullscreen = Resolution.IsFullScreen;
        static float volume = MediaPlayer.Volume * 10;

        public OptionsMenuScreen()
            : base("Options")
        {
            // Create our menu entries.
            fullscreenMenuEntry = new MenuEntry(string.Empty);

            SetMenuEntryText();

            MenuEntry back = new MenuEntry("Back");

            // Hook up menu event handlers.
            fullscreenMenuEntry.Selected += FullscreenMenuEntrySelected;
            back.Selected += OnCancel;

            // Add entries to the menu.

            MenuEntries.Add(fullscreenMenuEntry);
            MenuEntries.Add(back);
        }

        void SetMenuEntryText()
        {
            fullscreenMenuEntry.Text = "Fullscreen: " + (fullscreen ? "on" : "off");
        }


        void FullscreenMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            fullscreen = !fullscreen;
            Resolution.IsFullScreen = fullscreen;
            SetMenuEntryText();
        }
    }
}
