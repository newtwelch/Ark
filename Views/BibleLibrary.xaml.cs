using Ark.Models.Helpers;
using Ark.ViewModels;
using System;
using System.Text.RegularExpressions;
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

                    //!? Reset the other searches
                    WideVerseSearchTextBox.Text = "";
                    verseAssistant.TextChanged();
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
                    ChapterListBox.SelectedIndex = Int32.Parse(ChapterSearchTextBox.Text) - 1;
                ChapterListBox.ScrollIntoView(ChapterListBox.SelectedItem);
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
    }
}
