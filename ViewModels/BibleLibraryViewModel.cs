using Ark.Models.BibleLibrary;
using System.Collections.ObjectModel;
using System.Linq;

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
        }

    }
}
