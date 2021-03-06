﻿namespace EA4S.UI
{
    public enum MenuButtonType
    {
        Unset = 0,
        PauseToggle = 1,
        Continue = 2,
        Back = 3,
        MusicToggle = 4,
        FxToggle = 5,
        Restart = 6,
        Credits = 7,
        EnglishToggle = 8
    }

    /// <summary>
    /// A button used in a menu.
    /// </summary>
    public class MenuButton : UIButton
    {
        public MenuButtonType Type;
    }
}