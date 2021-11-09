using Ark.Models.BibleLibrary;
using Ark.Models.Helpers;
using Ark.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Ark.ViewModels
{
    public class BibleLibraryViewModel : ViewModelBase
    {
        //! Database 
        private BibleLibraryDatabase _database;

        //! List of Books
        public List<BookData> TagalogBooks { get; set; }
        public List<BookData> EnglishBooks { get; set; }

        //! RangedObservableCollection of Bible Data
        public RangeObservableCollection<BookData> Books { get; set; }
        public RangeObservableCollection<ChapterData> Chapters { get; set; }
        public RangeObservableCollection<VerseData> Verses { get; set; }
        public RangeObservableCollection<VerseData> AllVerses { get; set; }

        //! Verse Portions
        public ObservableDictionary<int, string> VersePortions { get; set; }
        public string VerseHighlight
        {
            get => _verseHighlight;
            set
            {
                _verseHighlight = value;
                OnPropertyChanged();
                DisplayWindow.Instance.DisplayTextBox.HighlightPhrase = value;
            }
        }
        private string _verseHighlight;

        //! Selected Books and Chapters
        private BookData _selectedBook;
        public BookData SelectedBook
        {
            get => _selectedBook;
            set
            {
                _selectedBook = value;
                OnPropertyChanged();

                if (value is null) return;

                //!? Save Selected Chapter ID so it always has a chapter selected
                int saveChapter = SelectedChapter != null ? SelectedChapter.ID : 1;
                Chapters?.Clear();
                Chapters?.AddRange(value.Chapters);
                SelectedChapter = Chapters?.ToList().Find(x => x.ID == saveChapter);
            }
        }

        private ChapterData _selectedChapter;
        public ChapterData SelectedChapter
        {
            get => _selectedChapter;
            set
            {
                _selectedChapter = value;
                OnPropertyChanged();

                if (value is null) return;

                Verses?.Clear();
                Verses?.AddRange(_selectedChapter.Verses);
            }
        }

        public VerseData SelectedVerse
        {
            get => _selectedVerse;
            set
            {
                _selectedVerse = value;
                OnPropertyChanged();
                VersePortions?.Clear();

                if (value is null) return;

                //!? Clear the TextSearch
                if (!String.IsNullOrWhiteSpace(_searchVerseText))
                    SearchVerseText = "";

                //!? Cut the verse into Portions
                string[] verseBits = Regex.Split(value.Text, @"(?<=[\.,;:!\?])\s+");
                int Key = 1;

                //!? Clear the Portions AGAIN
                VersePortions?.Clear();
                verseBits.ToList().ForEach(x => VersePortions.Add(Key++, x));

                //!? Events
                OnSelectedVerseChanged?.Invoke(_selectedVerse);
                HistoryViewModel.EventInvoking(value);
                DisplayWindow.Instance.Show();
            }
        }
        private VerseData _selectedVerse;

        public VerseData SelectedWideVerse
        {
            get => _selectedWideVerse;
            set
            {
                _selectedWideVerse = value;
                OnPropertyChanged();

                //!? Clear the Portions
                VersePortions?.Clear();

                if (value is null) return;

                SelectedBook = Books.ToList().Find(x => x.Name == value.FromBook);
                SelectedChapter = Chapters.ToList().Find(x => x.ID == value.FromChapter);
                SelectedVerse = Verses.ToList().Find(x => x.ID == value.ID);

                //!? Clear the TextSearch
                if (!String.IsNullOrWhiteSpace(_searchVerseText))
                    SearchVerseText = "";
            }
        }
        private VerseData _selectedWideVerse;

        //!? Event for Verse Change
        public static event Action<VerseData> OnSelectedVerseChanged;

        //! Bible Search & CollectionViews
        public ICollectionView BooksView;
        public ICollectionView AllVerseView;
        public ICollectionView VerseView;
        public string SearchBookText { get; set; }
        public string SearchVerseText
        {
            get => _searchVerseText;
            set
            {
                _searchVerseText = value;

                VersePortions?.Clear();
                VerseHighlight = value;

                if (value.StartsWith("."))
                    AllVerseView.Refresh();
                else
                    VerseView.Refresh();

                OnPropertyChanged();

            }
        }
        private string _searchVerseText;

        //! Language
        private string _language;
        public string Language
        {
            get => _language;
            set
            {

                _language = value;
                OnPropertyChanged();

                //!? Save selection
                int SaveBook = SelectedBook.ID;
                int SaveChapter = SelectedChapter.ID;
                int SaveVerse = SelectedVerse is null ? 0 : SelectedVerse.ID;

                //!? Update Books
                Books?.Clear();
                Books?.AddRange(value.Contains("English") ? EnglishBooks : TagalogBooks);
                SelectedBook = Books.ToList().Find(x => x.ID == SaveBook);

                //!? Update AllVerses First, to get the "From" Data ( Check the GetAllVerses for more Info )
                AllVerses.Clear();
                AllVerses.AddRange(GetAllVerses());

                //!? Update Chapters
                Chapters.Clear();
                Chapters.AddRange(SelectedBook.Chapters);
                SelectedChapter = Chapters.ToList().Find(x => x.ID == SaveChapter);

                //!? Update Verses
                Verses.Clear();
                Verses.AddRange(SelectedChapter.Verses);
                SelectedVerse = Verses.ToList().Find(x => x.ID == SaveVerse);
            }
        }

        //? =============================[METHODS & MAIN]==============================

        //! ====================================================
        //! [+] BIBLE LIBRARY VIEW MODEL: main method for initializing stuff
        //! ====================================================
        public BibleLibraryViewModel()
        {
            _database = new BibleLibraryDatabase();

            //!? BOOKS
            EnglishBooks = new List<BookData>(_database.GetBooks("English"));
            TagalogBooks = new List<BookData>(_database.GetBooks("Tagalog"));
            Books = new RangeObservableCollection<BookData>();
            Books.AddRange(EnglishBooks);
            SelectedBook = Books[0];

            //!? CHAPTERS
            Chapters = new RangeObservableCollection<ChapterData>();
            Chapters.AddRange(SelectedBook.Chapters);
            SelectedChapter = Chapters[0];

            //!? VERSES
            Verses = new RangeObservableCollection<VerseData>();
            Verses.AddRange(SelectedChapter.Verses);
            AllVerses = new RangeObservableCollection<VerseData>();
            AllVerses.AddRange(GetAllVerses());

            VersePortions = new ObservableDictionary<int, string>();

            //!? ====================================================
            //!? SEARCH FILTER: collection view filtering
            //!? ====================================================
            BooksView = CollectionViewSource.GetDefaultView(Books);
            BooksView.Filter = SearchFilterView;

            AllVerseView = CollectionViewSource.GetDefaultView(AllVerses);
            VerseView = CollectionViewSource.GetDefaultView(Verses);
            AllVerseView.Filter = SearchFilterView;
            VerseView.Filter = SearchFilterView;
        }

        //! ====================================================
        //! [+] VIEW FILTER: main method for initializing stuff
        //! ====================================================
        public bool SearchFilterView(object o)
        {
            if (o is BookData)
            {
                if (string.IsNullOrEmpty(SearchBookText))
                    return true;
                else
                    return ((BookData)o).Name.Contains(SearchBookText, StringComparison.OrdinalIgnoreCase);
            }
            else if (o is VerseData)
            {

                if (string.IsNullOrEmpty(SearchVerseText))
                    return true;
                else if (SearchVerseText.StartsWith("."))
                    return ((VerseData)o).Text.
                        Contains(SearchVerseText.Replace(".", ""), StringComparison.OrdinalIgnoreCase);
                else
                    return ((VerseData)o).Text.Contains(SearchVerseText, StringComparison.OrdinalIgnoreCase);

            }
            else
                return true;
        }

        //! ====================================================
        //! [+] GET ALL VERSES: 
        //! ====================================================
        public List<VerseData> GetAllVerses()
        {
            List<VerseData> list = new List<VerseData>();

            //!? For Every Book in Books
            Books.ToList().ForEach(b =>
            {
                string fromBook = b.Name;
                //!? For Every Chapter in said Book
                b.Chapters.ForEach(c =>
                {
                    int fromChapter = c.ID;
                    //!? For Every Verse in said Chapter 
                    //!? Amazingly, this helps us with setting FromBook and FromChapter
                    c.Verses.ForEach(v =>
                    {
                        v.FromBook = fromBook;
                        v.FromChapter = fromChapter;
                        list.Add(v);
                    });
                });
            });

            return list;
        }
    }
}
