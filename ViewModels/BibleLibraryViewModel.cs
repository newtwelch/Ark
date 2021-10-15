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

        //! Verse Bits
        public ObservableDictionary<int, string> VersePortions { get; set; }
        public string VerseHighlight
        {
            get { return _VerseHighlight; }
            set
            {
                _VerseHighlight = value;
                OnPropertyChanged();
                DisplayWindow.Instance.DisplayTextBox.HighlightPhrase = value;
            }
        }
        private string _VerseHighlight;

        //! Selected Books and Chapters
        public BookData SelectedBook
        {
            get { return _selectedBook; }
            set
            {
                _selectedBook = value;
                OnPropertyChanged();

                if (Chapters == null || SelectedBook == null)
                    return;

                int save = SelectedChapter != null ? SelectedChapter.ID : 1;

                Chapters.Clear();
                Chapters.AddRange(_selectedBook.Chapters);

                SelectedChapter = Chapters.ToList().Find(x => x.ID == save);
            }
        }
        private BookData _selectedBook;
        public ChapterData SelectedChapter
        {
            get { return _selectedChapter; }
            set
            {
                _selectedChapter = value;
                OnPropertyChanged();

                if (Verses == null || SelectedChapter == null)
                    return;

                Verses.Clear();
                Verses.AddRange(_selectedChapter.Verses);
            }
        }
        private ChapterData _selectedChapter;
        public VerseData SelectedVerse
        {
            get { return _selectedVerse; }
            set
            {
                _selectedVerse = value;
                //!? Clear the Portions
                VersePortions.Clear();

                OnPropertyChanged();

                if (!Initialized || value == null) return;

                //!? CONTINUE ON
                //!? Clear the TextSearch
                if (!String.IsNullOrWhiteSpace(_searchVerseText))
                    SearchVerseText = "";

                //!? Cut the verse into Portions
                string[] verseBits = Regex.Split(value.Text, @"(?<=[\.,;:!\?])\s+");
                int Key = 1;
                //!? Clear the Portions AGAIN
                VersePortions.Clear();
                foreach (string verseBit in verseBits)
                {
                    VersePortions.Add(Key++, verseBit);
                }


                //!? Show the VERSE
                OnSelectedVerseChanged?.Invoke(_selectedVerse);
                DisplayWindow.Instance.Show();
            }
        }
        private VerseData _selectedVerse;

        public VerseData SelectedWideVerse
        {
            get { return _selectedWideVerse; }
            set
            {
                _selectedWideVerse = value;
                OnPropertyChanged();

                //!? Clear the Portions
                VersePortions.Clear();

                if (!Initialized || value == null) return;

                SelectedBook = (BookData)Books.ToList().Find(x => x.Name == value.FromBook);
                SelectedChapter = (ChapterData)Chapters.ToList().Find(x => x.ID == value.FromChapter);
                SelectedVerse = (VerseData)Verses.ToList().Find(x => x.ID == value.ID);

                //!? CONTINUE ON
                //!? Clear the TextSearch
                if (!String.IsNullOrWhiteSpace(_searchVerseText))
                    SearchVerseText = "";
            }
        }
        private VerseData _selectedWideVerse;

        //!? Event for Verse Change
        public static event Action<VerseData> OnSelectedVerseChanged;

        //! Bible Searching
        public ICollectionView BooksView;          // CollectionView for the songs
        public string SearchBookText;                // Gets the Book from Book Search

        //! Collection Verse Views
        public ICollectionView AllVerseView;          // CollectionView for the songs
        public ICollectionView VerseView;          // CollectionView for the songs
        public string SearchVerseText
        {
            get { return _searchVerseText; }
            set
            {
                _searchVerseText = value;

                VersePortions.Clear();
                VerseHighlight = value;

                if (value.StartsWith("."))
                    AllVerseView.Refresh();
                else
                    VerseView.Refresh();

                OnPropertyChanged();


            }
        }
        private string _searchVerseText;

        bool Initialized = false;

        //! Language
        public string Language
        {
            get { return _language; }
            set
            {

                _language = value;
                OnPropertyChanged();

                //!? Property Change Stuff
                if (!Initialized)
                    return;

                //!? Save selection
                int SaveBook = SelectedBook.ID;
                int SaveChapter = SelectedChapter.ID;
                int SaveVerse = SelectedVerse == null ? 0 : SelectedVerse.ID;

                //!? Update Books
                Books.Clear();
                Books.AddRange(value.Contains("English") ? EnglishBooks : TagalogBooks);
                SelectedBook = SaveBook.Equals(0) ? Books[0] : Books.ToList().Find(x => x.ID == SaveBook);

                //!? Update AllVerses First, to get the "From" Data ( Check the GetAllVerses for more Info )
                AllVerses.Clear();
                AllVerses.AddRange(GetAllVerses());

                //!? Update Chapters
                Chapters.Clear();
                Chapters.AddRange(SelectedBook.Chapters);
                SelectedChapter = SaveChapter.Equals(0) ? Chapters[0] : Chapters.ToList().Find(x => x.ID == SaveChapter);

                //!? Update Verses
                Verses.Clear();
                Verses.AddRange(SelectedChapter.Verses);
                SelectedVerse = SaveVerse.Equals(0) ? null : Verses.ToList().Find(x => x.ID == SaveVerse);


            }
        }
        private string _language;

        //? =============================[METHODS & MAIN]=====================s=========

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
            VersePortions.Add(1, "tsest");

            Initialized = true;

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
            //!? Book Filter
            if (o is BookData)
            {
                if (string.IsNullOrEmpty(SearchBookText))
                    return true;
                else
                {
                    return (o as BookData).Name.Contains(SearchBookText, StringComparison.OrdinalIgnoreCase);
                }
            }
            else if (o is VerseData)
            {

                if (string.IsNullOrEmpty(SearchVerseText))
                    return true;
                else if (SearchVerseText.StartsWith("."))
                    return (o as VerseData).Text.
                        Contains(SearchVerseText.Replace(".", ""), StringComparison.OrdinalIgnoreCase);
                else
                    return (o as VerseData).Text.Contains(SearchVerseText, StringComparison.OrdinalIgnoreCase);

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
            foreach (BookData book in Books)
            {
                string fromBook = book.Name;

                //!? For Every Chapter in said Book
                foreach (ChapterData chapter in book.Chapters)
                {
                    int fromChapter = chapter.ID;

                    //!? For Every Verse in said Chapter 
                    //!? Amazingly, this helps us with setting FromBook and FromChapter
                    foreach (VerseData verse in chapter.Verses)
                    {
                        verse.FromBook = fromBook;
                        verse.FromChapter = fromChapter;
                        //!? Add VERSE
                        list.Add(verse);
                    }
                }
            }

            return list;
        }
    }
}
