﻿using Ark.Models.BibleLibrary;
using Ark.Models.SongLibrary;
using Ark.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
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

            SongLibraryViewModel.OnSelectedLyricChanged += ChangeText;
            BibleLibraryViewModel.OnSelectedVerseChanged += ChangeText;

            //!? ====================================================
            //!? SETUP WINDOW: set window position, width and height
            //!? ====================================================
            SetWindow();
        }

        //! ====================================================
        //! [+] CHANGE TEXT: change the display text
        //! ====================================================
        void ChangeText(object o)
        {
            DisplayBibleTextBox.Visibility = Visibility.Collapsed;
            BibleDataTextBox.Visibility = Visibility.Collapsed;
            DisplaySongTextBox.Visibility = Visibility.Collapsed;

            if (o is LyricData lyric)
            {
                DisplaySongTextBox.Visibility = Visibility.Visible;
                DisplaySongTextBox.Text = lyric.Text;
            }
            else if (o is VerseData verse)
            {
                DisplayBibleTextBox.Visibility = Visibility.Visible;
                BibleDataTextBox.Visibility = Visibility.Visible;

                DisplayBibleTextBox.Text = verse.Text;
                BibleDataTextBox.Text = $"{verse.FromBook} {verse.FromChapter}:{verse.ID}";
            }
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

        //! ====================================================
        //! [+] WINDOW CLOSING: cancel the closing and set visibility to collapsed
        //! ====================================================
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            e.Cancel = true;
        }

        //! ====================================================
        //! [+] CLOSE DISPLAY
        //! ====================================================
        public void CloseDisplayMethod()
        {
            DisplayBibleTextBox.HighlightPhrase = "";
            Close();
        }

        //! ====================================================
        //! [+] VISIBLE CHANGED
        //! ====================================================
        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) => SetWindow();

        private void SetWindow()
        {
            //!? ====================================================
            //!? COUNT SCREENS: make a count of the screen displays
            //!? ====================================================
            _screensList = new List<Screen>();
            foreach (Screen? screen in Screen.AllScreens)
            {
                _screensList.Add(screen);
            }

            //!? ====================================================
            //!? SET WHICH SCREEN TO USE
            //!? ====================================================
            // check if the list has 2 or more screens
            bool hasManyScreens = _screensList.Count >= 2;

            // Set the window position depending on screenlist size, set the width adn height as well
            SetWindowPosition(this, WindowPositions.TopLeft, hasManyScreens ? _screensList[1].Bounds : _screensList[0].Bounds);
            Width = hasManyScreens ? _screensList[1].Bounds.Width : Screen.PrimaryScreen.Bounds.Width;
            Height = hasManyScreens ? _screensList[1].Bounds.Height : Screen.PrimaryScreen.Bounds.Height;
        }

        //? =============================[LOADED & UNLOADED]==============================

        private void Window_Loaded(object sender, RoutedEventArgs e) => MainWindow.CloseDisplayEvent += CloseDisplayMethod;

        private void Window_Unloaded(object sender, RoutedEventArgs e) => MainWindow.CloseDisplayEvent -= CloseDisplayMethod;

        //!? ====================================================
        //!? DISABLES FOCUS ON THIS WINDOW
        //!? ====================================================
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_MOUSEACTIVATE)
            {
                handled = true;
                return new IntPtr(MA_NOACTIVATE);
            }
            else return IntPtr.Zero;
        }
        private const int WM_MOUSEACTIVATE = 0x0021;
        private const int MA_NOACTIVATE = 0x0003;


    }
}
