using System.Collections.Generic;
using System.Windows;
using WpfScreenHelper;
using WpfScreenHelper.Enum;

namespace Ark.Views
{
    public partial class DisplayWindow : Window
    {
        //! Singleton Insatance
        public static DisplayWindow Instance { get; private set; }
        static DisplayWindow() => Instance = new DisplayWindow();

        //! List of Screens
        private List<Screen> _screensList;

        //! ====================================================
        //! [+] DISPLAY WINDOW: everything starts here
        //! ====================================================
        private DisplayWindow()
        {
            //!? ====================================================
            //!? INITIALIZE: start stuff here
            //!? ====================================================
            InitializeComponent();
            _screensList = new List<Screen>();

            //!? ====================================================
            //!? COUNT SCREENS: make a count of the screen displays
            //!? ====================================================
            foreach (Screen? screen in Screen.AllScreens)
            {
                _screensList.Add(screen);
            }

            //!? ====================================================
            //!? SETUP WINDOW: set window position, width and height
            //!? ====================================================
            // check if the list has 2 or more screens
            bool hasManyScreens = _screensList.Count >= 2;

            // Set the window position depending on screenlist size, set the width adn height as well
            SetWindowPosition(this, WindowPositions.TopLeft, hasManyScreens ? _screensList[1].Bounds : _screensList[0].Bounds);
            Width = hasManyScreens ? _screensList[1].Bounds.Width : Screen.PrimaryScreen.Bounds.Width;
            Height = hasManyScreens ? _screensList[1].Bounds.Height : Screen.PrimaryScreen.Bounds.Height;

        }

        //! ====================================================
        //! [+] SET WINDOW POSITION: using WPFScreenHelper so I don't have to use WinForms
        //! ====================================================
        private static void SetWindowPosition(Window wnd, WindowPositions pos, Rect bounds)
        {
            var x = 0.0d;
            var y = 0.0d;

            switch (pos)
            {
                case WindowPositions.Center:
                    x = bounds.X + (bounds.Width - wnd.Width) / 2.0;
                    y = bounds.Y + (bounds.Height - wnd.Height) / 2.0;
                    break;
                case WindowPositions.Left:
                    x = bounds.X;
                    y = bounds.Y + (bounds.Height - wnd.Height) / 2.0;
                    break;
                case WindowPositions.Top:
                    x = bounds.X + (bounds.Width - wnd.Width) / 2.0;
                    y = bounds.Y;
                    break;
                case WindowPositions.Right:
                    x = bounds.X + (bounds.Width - wnd.Width);
                    y = bounds.Y + (bounds.Height - wnd.Height) / 2.0;
                    break;
                case WindowPositions.Bottom:
                    x = bounds.X + (bounds.Width - wnd.Width) / 2.0;
                    y = bounds.Y + (bounds.Height - wnd.Height);
                    break;
                case WindowPositions.TopLeft:
                    x = bounds.X;
                    y = bounds.Y;
                    break;
                case WindowPositions.TopRight:
                    x = bounds.X + (bounds.Width - wnd.Width);
                    y = bounds.Y;
                    break;
                case WindowPositions.BottomRight:
                    x = bounds.X + (bounds.Width - wnd.Width);
                    y = bounds.Y + (bounds.Height - wnd.Height);
                    break;
                case WindowPositions.BottomLeft:
                    x = bounds.X;
                    y = bounds.Y + (bounds.Height - wnd.Height);
                    break;
            }

            wnd.Left = x;
            wnd.Top = y;
        }
    }
}
