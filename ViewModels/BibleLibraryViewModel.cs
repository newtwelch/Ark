using Ark.Models.BibleLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace Ark.ViewModels
{
    public class BibleLibraryViewModel : ViewModelBase
    {
        //! Database 
        private BibleLibraryDatabase _database;

        //! List of Books
        public ObservableCollection<BookData> Books { get; set; }
        public ObservableCollection<ChapterData> Chapters { get; set; }
        public ObservableCollection<VerseData> Verses { get; set; }
        public ObservableCollection<VerseData> AllVerses { get; set; }

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
                foreach (ChapterData chapter in SelectedBook.Chapters)
                {
                    Chapters.Add(chapter);
                }

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
                foreach (VerseData chapter in SelectedChapter.Verses)
                {
                    Verses.Add(chapter);
                }
            }
        }
        private ChapterData _selectedChapter;
        public VerseData SelectedVerse
        {
            get { return _selectedVerse; }
            set
            {
                _selectedVerse = value;
                OnPropertyChanged();
            }
        }
        private VerseData _selectedVerse;


        //! Bible Searching
        public ICollectionView BooksView;          // CollectionView for the songs
        public string SearchBookText;                // Gets the Book from Book Search

        //? =============================[METHODS & MAIN]==============================

        //! ====================================================
        //! [+] BIBLE LIBRARY VIEW MODEL: main method for initializing stuff
        //! ====================================================
        public BibleLibraryViewModel()
        {
            _database = new BibleLibraryDatabase();
            Books = new ObservableCollection<BookData>(_database.GetBooks("t_kjv"));
            SelectedBook = Books[0];
            Chapters = new ObservableCollection<ChapterData>(SelectedBook.Chapters);
            SelectedChapter = Chapters[0];
            Verses = new ObservableCollection<VerseData>(SelectedChapter.Verses);
            AllVerses = new ObservableCollection<VerseData>(GetAllVerses());

            //!? ====================================================
            //!? SEARCH FILTER: collection view filtering
            //!? ====================================================
            BooksView = CollectionViewSource.GetDefaultView(Books);
            BooksView.Filter = SearchFilterView;
        }

        //! ====================================================
        //! [+] VIEW FILTER: main method for initializing stuff
        //! ====================================================
        public bool SearchFilterView(object o)
        {
            //!? Book Filter
            if (string.IsNullOrEmpty(SearchBookText))
                return true;
            else
                return (o as BookData).Name.Contains(SearchBookText, StringComparison.OrdinalIgnoreCase);
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
