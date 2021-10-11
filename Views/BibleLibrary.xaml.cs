using Ark.Models.Helpers;
using Ark.ViewModels;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Ark.Views
{
    public partial class BibleLibrary : UserControl
    {
        private BibleLibraryViewModel _viewModel;

        TypeAssistant chapterAssistant, verseAssistant;

        public BibleLibrary()
        {
            _viewModel = new BibleLibraryViewModel();
            DataContext = _viewModel;
            InitializeComponent();

            EnglishRadioTab.IsChecked = true;

            //!? Delay Event for the search
            chapterAssistant = new TypeAssistant();
            chapterAssistant.Idled += chapterTextBoxIdled;

            verseAssistant = new TypeAssistant(250);
            verseAssistant.Idled += verseTextBoxIdled;
        }

        //! ====================================================
        //! [+] TEXT CHANGED: handle search stuff here
        //! ====================================================
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = e.Source as TextBox;

            switch (tb.Name)
            {
                //!? BOOK SEARCH
                //!? ____________________________________________
                case "BookSearchTextBox":
                    //!? Refresh ViewModel Collection
                    _viewModel.SearchBookText = tb.Text;
                    _viewModel.BooksView.Refresh();

                    //!? Reset the other searches
                    ChapterSearchTextBox.Text = "";
                    VerseSearchTextBox.Text = "";

                    if (tb.IsFocused)
                        WideVerseSearchTextBox.Text = "";

                    //!? If only one is remaining select and focus on next search
                    if (BookListBox.Items.Count == 1)
                    {
                        BookListBox.SelectedIndex = 0;
                        ChapterSearchTextBox.Focus();
                    }
                    break;

                //!? CHAPTER SEARCH
                //!? ____________________________________________
                case "ChapterSearchTextBox":

                    //!? Reset the other searches
                    VerseSearchTextBox.Text = "";
                    if (tb.IsFocused)
                        WideVerseSearchTextBox.Text = "";

                    //!? If only one is remaining select and focus on next search
                    if (ChapterListBox.Items.Count == 1)
                    {
                        ChapterListBox.SelectedIndex = 0;
                        VerseSearchTextBox.Focus();
                    }
                    else
                        //!? if there is more item then do this fancy waiting
                        chapterAssistant.TextChanged();
                    break;

                //!? VERSE SEARCH
                //!? ____________________________________________
                case "VerseSearchTextBox":

                    if (tb.IsFocused)
                        WideVerseSearchTextBox.Text = "";
                    verseAssistant.TextChanged();

                    break;

                //!? WIDE VERSE SEARCH
                //!? ____________________________________________
                case "WideVerseSearchTextBox":

                    //!? If the textbox is not null
                    if (!String.IsNullOrWhiteSpace(WideVerseSearchTextBox.Text))
                    {
                        //!? Show whether text starts with a dot or not
                        bool startsWithDot = WideVerseSearchTextBox.Text.StartsWith(".");
                        WideVerseListBox.Visibility = startsWithDot ? Visibility.Visible : Visibility.Collapsed;
                        VerseListBox.Visibility = startsWithDot ? Visibility.Collapsed : Visibility.Visible;

                        //!? Reset the other searches
                        BookSearchTextBox.Text = "";
                        ChapterSearchTextBox.Text = "";
                        VerseSearchTextBox.Text = "";
                    }
                    else //!? If it Is Null
                    {
                        //!? then Show the default Verses
                        WideVerseListBox.Visibility = Visibility.Collapsed;
                        VerseListBox.Visibility = Visibility.Visible;
                    }

                    break;
            }
        }

        //! ====================================================
        //! [+] CHAPTER TEXT BOX IDLED: chapter search delayer
        //! ====================================================
        void chapterTextBoxIdled(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                //!? Select the chapter number provided in the search then move on to Verse Search
                if (!String.IsNullOrWhiteSpace(ChapterSearchTextBox.Text))
                {
                    ChapterListBox.SelectedIndex = Int32.Parse(ChapterSearchTextBox.Text) - 1;
                    ChapterListBox.ScrollIntoView(ChapterListBox.SelectedItem);
                    VerseSearchTextBox.Focus();
                }
            });
        }

        //! ====================================================
        //! [+] VERSE TEXT BOX IDLED: verse search delayer
        //! ====================================================
        void verseTextBoxIdled(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                //!? Select the Searched Verse
                if (!String.IsNullOrWhiteSpace(VerseSearchTextBox.Text))
                    VerseListBox.SelectedIndex = Int32.Parse(VerseSearchTextBox.Text) - 1;

                VerseListBox.ScrollIntoView(VerseListBox.SelectedItem);
            });
        }

        //! ====================================================
        //! [+] PREVIEW TEXT INPUT: only allow numeric input
        //! ====================================================
        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^[0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void English_Checked(object sender, RoutedEventArgs e)
        {
            if (e.Source is RadioButton rb)
            {
                switch (rb.Name)
                {
                    case "EnglishRadioTab":
                        _viewModel.Language = "English";
                        break;
                    case "TagalogRadioTab":
                        _viewModel.Language = "Tagalog";
                        break;
                }
            }
        }

        //? =============================[METHODS]==============================

        //! ====================================================
        //! [+] SEARCH FOCUS METHOD: hotkey stuff
        //! ====================================================
        public void SearchFocusMethod()
        {
            BookSearchTextBox.Text = "";
            ChapterSearchTextBox.Text = "";
            VerseSearchTextBox.Text = "";
            WideVerseSearchTextBox.Text = "";
            BookSearchTextBox.Focus();
        }

        //! ====================================================
        //! [+] SPECIFIC SEARCH FOCUS METHOD: hotkey stuff
        //! ====================================================
        public void SpecificSearchFocusMethod()
        {
            SearchFocusMethod();
            WideVerseSearchTextBox.Focus();
        }

        //? =============================[LOADED & UNLOADED]==============================

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.SearchFocusEvent += SearchFocusMethod;
            MainWindow.SpecificSearchFocusEvent += SpecificSearchFocusMethod;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            MainWindow.SearchFocusEvent -= SearchFocusMethod;
            MainWindow.SpecificSearchFocusEvent -= SpecificSearchFocusMethod;
        }
    }
}
