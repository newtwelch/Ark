using Ark.Models.BibleLibrary;
using Ark.Models.Helpers;
using Ark.ViewModels;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ark.Views
{
    public partial class BibleLibrary : UserControl
    {
        //! Viewmodel
        private BibleLibraryViewModel _viewModel;

        //! TypeAssistant
        TypeAssistant chapterAssistant, verseAssistant;

        //! Variables
        int storeVerse;
        bool fromHistory;

        //! ====================================================
        //! [+] BIBLE LIBRARY:
        //! ====================================================
        public BibleLibrary()
        {
            _viewModel = new BibleLibraryViewModel();
            DataContext = _viewModel;
            InitializeComponent();

            EnglishRadioTab.IsChecked = true;

            //!? =========================================
            //!? Event for HistoryObject Selection Changed
            HistoryViewModel.HistoryObjectSelected += HistoryObjectSelectedMethod;

            //!? Delay Event for the search
            chapterAssistant = new TypeAssistant();
            chapterAssistant.Idled += chapterTextBoxIdled;

            verseAssistant = new TypeAssistant();
            verseAssistant.Idled += verseTextBoxIdled;
        }

        //! ====================================================
        //! [+] TEXT CHANGED: handle search stuff here
        //! ====================================================
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (fromHistory) return;

            TextBox? tb = e.Source as TextBox;
            switch (tb.Name)
            {
                //!? BOOK SEARCH
                //!? ____________________________________________
                case "BookSearchTextBox":

                    //!? This deals with the books with first and second parts
                    string result = Regex.Replace(tb.Text, @"(?<=\d)(?=[^\d\s])", " ");
                    tb.Text = result;
                    tb.CaretIndex = tb.Text.Length;

                    //!? Refresh ViewModel Collection
                    _viewModel.SearchBookText = tb.Text;
                    _viewModel.BooksView.Refresh();

                    //!? Reset the other searches
                    ChapterSearchTextBox.Clear();
                    VerseSearchTextBox.Clear();
                    if (tb.IsFocused) WideVerseSearchTextBox.Clear();

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
                    VerseSearchTextBox.Clear();
                    if (tb.IsFocused) WideVerseSearchTextBox.Clear();

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

                    if (tb.IsFocused) WideVerseSearchTextBox.Clear();
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
                        BookSearchTextBox.Clear();
                        ChapterSearchTextBox.Clear();
                        VerseSearchTextBox.Clear();
                    }
                    else //!? If it Is Null
                    {
                        //!? then Show the default Verses
                        WideVerseListBox.Visibility = Visibility.Collapsed;
                        VerseListBox.Visibility = Visibility.Visible;
                        if (VerseListBox.SelectedItem is not null) FocusListBoxItem(VerseListBox);
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
                if (String.IsNullOrWhiteSpace(ChapterSearchTextBox.Text)) return;
                ChapterListBox.SelectedIndex = Int32.Parse(ChapterSearchTextBox.Text) - 1;
                VerseSearchTextBox.Focus();

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
                if (String.IsNullOrWhiteSpace(VerseSearchTextBox.Text)) return;
                VerseListBox.SelectedIndex = Int32.Parse(VerseSearchTextBox.Text) - 1;
                FocusListBoxItem(VerseListBox);
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

        //! ====================================================
        //! [+] TEXT BOX KEY DOWN: hotkeys for searches
        //! ====================================================
        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            TextBox? tb = sender as TextBox;

            //!? If enter is pressed then do the following
            if (e.Key != Key.Enter) return;

            switch (tb?.Name)
            {
                //!? BOOK SEARCH
                case "BookSearchTextBox":
                    if (!String.IsNullOrEmpty(tb.Text))
                        BookListBox.SelectedIndex = 0;

                    ChapterSearchTextBox.Focus();
                    break;

                //!? CHAPTER SEARCH
                case "ChapterSearchTextBox":
                    if (!String.IsNullOrEmpty(tb.Text))
                        ChapterListBox.SelectedIndex = Int32.Parse(tb.Text) - 1;

                    VerseSearchTextBox.Focus();

                    break;

                //!? VERSE SEARCH
                case "VerseSearchTextBox":
                    if (!String.IsNullOrEmpty(tb.Text))
                        VerseListBox.SelectedIndex = Int32.Parse(tb.Text) - 1;

                    VerseSearchTextBox.Focus();

                    break;
            }
        }

        //! ====================================================
        //! [+] LIST BOX KEY DOWN: language switching
        //! ====================================================
        private void ListBox_KeyDown(object sender, KeyEventArgs e)
        {
            ListBox? lb = sender as ListBox;
            switch (lb.Name)
            {
                case "VerseListBox":
                    //!? If Pressing up on first Portion, Go Previous Verse
                    if (e.Key == Key.Up && lb.SelectedIndex == 0 && _viewModel.SelectedChapter.ID != 1)
                        ChapterListBox.SelectedIndex--;

                    //!? If Pressing down on last Portion, Go Next Verse
                    if (e.Key == Key.Down && lb.SelectedIndex == lb.Items.Count - 1)
                        ChapterListBox.SelectedIndex++;

                    //!? Pressing enter focuses on Verse Portions
                    if (e.Key == Key.Enter)
                    {

                        if (lb.SelectedItem is null)
                        {
                            lb.SelectedIndex = storeVerse;
                            FocusListBoxItem(lb);
                        }
                        else
                        {
                            VersePortionsListBox.SelectedIndex = 0;
                            VersePortionsListBox.Focus();
                            FocusListBoxItem(VersePortionsListBox);
                        }
                    }

                    ChapterListBox.ScrollIntoView(ChapterListBox.SelectedItem);
                    break;

                case "VersePortionsListBox":
                    //!? If Pressing up on first Portion, Go Previous Verse
                    if (e.Key == Key.Up && lb.SelectedIndex == 0)
                        VerseListBox.SelectedIndex--;
                    //!? If Pressing down on last Portion, Go Next Verse
                    if (e.Key == Key.Down && lb.SelectedIndex == lb.Items.Count - 1)
                        VerseListBox.SelectedIndex++;

                    VerseListBox.ScrollIntoView(VerseListBox.SelectedItem);
                    break;
            }
        }

        //! ====================================================
        //! [+] RADIO BUTTON CHECKED: language switching
        //! ====================================================
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (((RadioButton)sender).Name == "EnglishRadioTab")
                _viewModel.Language = "English";
            else
                _viewModel.Language = "Tagalog";
        }

        //? =============================[METHODS]==============================

        //! ====================================================
        //! [+] SEARCH FOCUS METHOD: hotkey stuff
        //! ====================================================
        public void SearchFocusMethod()
        {
            BookSearchTextBox.Clear();
            ChapterSearchTextBox.Clear();
            VerseSearchTextBox.Clear();
            WideVerseSearchTextBox.Clear();
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

        //! ====================================================
        //! [+] CLOSE DISPLAY
        //! ====================================================
        public void CloseDisplayMethod()
        {
            storeVerse = VerseListBox.SelectedIndex;
            _viewModel.SelectedVerse = null;
            _viewModel.VerseHighlight = "";
            VerseListBox.Focus();
        }

        //! ====================================================
        //! [+] FOCUS LIST BOX ITEM
        //! ====================================================
        void FocusListBoxItem(ListBox lb)
        {
            if (lb.SelectedItem is null) return;

            lb.UpdateLayout();
            lb.ScrollIntoView(lb.SelectedItem);

            ListBoxItem? lbi = lb.ItemContainerGenerator.ContainerFromIndex(lb.SelectedIndex) as ListBoxItem;
            lbi?.Focus();
        }

        //! ====================================================
        //! [+] HISTORY OBJECT SELECTED METHOD
        //! ====================================================
        private void HistoryObjectSelectedMethod(Object o)
        {
            if (o is not VerseData) return;

            VerseData verse = (VerseData)o;

            BookSearchTextBox.Clear();
            ChapterSearchTextBox.Clear();
            VerseSearchTextBox.Clear();
            WideVerseSearchTextBox.Clear();

            _viewModel.SelectedBook = _viewModel.Books.ToList().Find(x => x.Name == verse.FromBook);
            ChapterListBox.SelectedIndex = verse.FromChapter - 1;
            VerseListBox.SelectedIndex = verse.ID - 1;

            fromHistory = true;
        }

        //? =============================[LOADED & UNLOADED]==============================

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!fromHistory)
                SearchFocusMethod();
            else
                FocusListBoxItem(VerseListBox);

            fromHistory = false;

            MainWindow.SearchFocusEvent += SearchFocusMethod;
            MainWindow.SpecificSearchFocusEvent += SpecificSearchFocusMethod;
            MainWindow.CloseDisplayEvent += CloseDisplayMethod;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            MainWindow.SearchFocusEvent -= SearchFocusMethod;
            MainWindow.SpecificSearchFocusEvent -= SpecificSearchFocusMethod;
            MainWindow.CloseDisplayEvent -= CloseDisplayMethod;
        }
    }
}
