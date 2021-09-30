using Ark.Models.Helpers;
using Ark.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Navigation;

namespace Ark.Views
{
    public partial class MainWindow : Window
    {
        //! View Model
        private MainWindowViewModel _viewModel;

        //! Display Window
        private DisplayWindow displayWindow;

        //! ====================================================
        //! [+] MAIN WINDOW: everything starts here
        //! ====================================================
        public MainWindow()
        {
            //!? ====================================================
            //!? INITIALIZE: startup code goes here
            //!? ====================================================
            SourceInitialized += MainWindow_SourceInitialized;                                              // Initialize the complicated window maximize problem thingy
            InitializeComponent();                                                                          // Initialize UI Components?
            _viewModel = new MainWindowViewModel();                                                         // Initialize ViewModel    
            SongLibraryTab.IsChecked = true;

            //!? ====================================================
            //!? INIT: Window Buttons
            //!? ====================================================
            HideButton.Click += (s, e) => WindowState = WindowState.Minimized;                              // EVENT: Hide App
            MinMaxButton.Click += (s, e) => WindowState = WindowState ==                                    // EVENT: Minimize if Max.
                            WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;             // Maximize if Min.
            CloseButton.Click += (s, e) => Close();                                                         // EVENT: Close App

            //!? ====================================================
            //!? INIT: Display Window
            //!? ====================================================
            displayWindow = _viewModel.DisplayWindow;                                                       // Inititialize Display Window
            displayWindow.InitHwnd();                                                                       // Open it

        }

        #region Window Maximize Fix
        //! ====================================================
        //! [+] SOURCE INITIALIZE: should fix the maximizing issue
        //! ====================================================
        private void MainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            //!? ====================================================
            //!? WINDOWPROC: use this as callback method to process native stuff
            //!? ====================================================
            IntPtr handle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(handle)?.AddHook(WindowProc);
        }
        //! ====================================================
        //! [+] INTPTR WINDOWPROC: I have no idea :(
        //! ====================================================
        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WindowMaximizeHelper.WmGetMinMaxInfo(hwnd, lParam, (int)MinWidth, (int)MinHeight);
                    handled = true;
                    break;
            }

            return (IntPtr)0;
        }
        #endregion

        //! ====================================================
        //! [+] RADIO BUTTON TABS: switching tabs 
        //! ====================================================
        private void RadioTab_Checked(object sender, RoutedEventArgs e)
        {
            if (e.Source is RadioButton rb)
            {
                switch (rb.Name)
                {
                    case "SongLibraryTab":
                        ContentFrame.Content = _viewModel.SongLibrary;
                        break;
                    case "BibleLibraryTab":
                        ContentFrame.Content = _viewModel.BibleLibrary;
                        break;
                    case "HistoryTab":
                        ContentFrame.Content = _viewModel.History;
                        break;
                }
            }
        }

        //! ====================================================
        //! [+] CONTENTFRAME NAVIGATING: prevent default navigation
        //! ====================================================
        private void ContentFrame_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Forward ||
                e.NavigationMode == NavigationMode.Back)
                e.Cancel = true;
        }
    }
}
