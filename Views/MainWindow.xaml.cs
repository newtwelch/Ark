﻿using Ark.Models.BibleLibrary;
using Ark.Models.Helpers;
using Ark.Models.SongLibrary;
using Ark.ViewModels;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Navigation;

namespace Ark.Views
{
    public partial class MainWindow : Window
    {
        //! View Model
        private MainWindowViewModel _viewModel;

        //! Routed Command
        public static RoutedCommand CloseSecondDisplay = new RoutedCommand(),
                                    ToSongLibraryTab = new RoutedCommand(),
                                    ToBibleLibraryTab = new RoutedCommand(),
                                    ToHistoryTab = new RoutedCommand();
        //!? UserControl Commands
        public static RoutedCommand SearchFocus = new RoutedCommand(),
                                    SpecificSearchFocus = new RoutedCommand();

        //! Command Events for other UserControls
        public static event Action CloseDisplayEvent;
        public static event Action SearchFocusEvent;
        public static event Action SpecificSearchFocusEvent;

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

            HistoryViewModel.HistoryObjectSelected += HistoryObjectSelectedMethod;

            //!? ====================================================
            //!? COMMANDS: startup code goes here
            //!? ====================================================
            CloseSecondDisplay.InputGestures.Add(new KeyGesture(Key.Escape));
            ToSongLibraryTab.InputGestures.Add(new KeyGesture(Key.Z, ModifierKeys.Alt));
            ToBibleLibraryTab.InputGestures.Add(new KeyGesture(Key.X, ModifierKeys.Alt));
            ToHistoryTab.InputGestures.Add(new KeyGesture(Key.H, ModifierKeys.Alt));
            SearchFocus.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Alt));
            SpecificSearchFocus.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Alt | ModifierKeys.Control));
        }

        //! ====================================================
        //! [+] RADIO BUTTON TABS: switching tabs 
        //! ====================================================
        private void HistoryObjectSelectedMethod(Object o)
        {
            if (o is SongData)
                SongLibraryTab.IsChecked = true;
            else if (o is VerseData)
                BibleLibraryTab.IsChecked = true;
        }
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
        private void ContentFrame_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e) =>
            e.Cancel = e.NavigationMode.Equals(NavigationMode.Forward) || e.NavigationMode.Equals(NavigationMode.Back);

        #region Window Maximize Fix
        //! ====================================================
        //! [+] SOURCE INITIALIZE: should fix the maximizing issue
        //! ====================================================
        private void MainWindow_SourceInitialized(object? sender, EventArgs e)
        {
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

        #region Window Alt
        //!? DLL Stuff
        private const int GWL_STYLE = -16; //WPF's Message code for Title Bar's Style 
        private const int WS_SYSMENU = 0x80000; //WPF's Message code for System Menu
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        #endregion

        //! ====================================================
        //! [+] WINDOW LOADED
        //! ====================================================
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //!? Window Alt Key Context Menu Fix
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }

        //? =============================[COMMAND METHODS]==============================

        private void CloseSecondDisplayMethod(object sender, ExecutedRoutedEventArgs e) => CloseDisplayEvent?.Invoke();
        private void ToSongLibraryTabMethod(object sender, ExecutedRoutedEventArgs e) => SongLibraryTab.IsChecked = true;
        private void ToBibleLibraryTabMethod(object sender, ExecutedRoutedEventArgs e) => BibleLibraryTab.IsChecked = true;
        private void ToHistoryTabMethod(object sender, ExecutedRoutedEventArgs e) => HistoryTab.IsChecked = true;
        private void SearchFocusMethod(object sender, ExecutedRoutedEventArgs e) => SearchFocusEvent?.Invoke();
        private void SpecificSearchFocusMethod(object sender, ExecutedRoutedEventArgs e) => SpecificSearchFocusEvent?.Invoke();

    }
}