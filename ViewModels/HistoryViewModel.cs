using Ark.Models.BibleLibrary;
using Ark.Models.Helpers;
using Ark.Models.SongLibrary;
using System;
using System.Linq;

namespace Ark.ViewModels
{
    public class HistoryViewModel : ViewModelBase
    {

        //! Events
        public static event Action<Object> AddObjectToHistory;
        public static event Action<Object> HistoryObjectSelected;

        //! History Object List
        public ObservableDictionary<Tuple<int, Object>, string> HistoryObjects { get; set; }

        public Tuple<int, Object> SelectedObject
        {
            get { return _selectedObject; }
            set
            {
                _selectedObject = value;

                OnPropertyChanged();

                if (value != null)
                    HistoryObjectSelected.Invoke(_selectedObject.Item2);
            }
        }
        private Tuple<int, Object> _selectedObject;

        //! Variables
        int uniqueKey = 0;

        //! ====================================================
        //! [+] HISTORY VIEW MODEL
        //! ====================================================
        public HistoryViewModel()
        {
            HistoryObjects = new ObservableDictionary<Tuple<int, Object>, string>();
            AddObjectToHistory += AddObjectToHistoryMethod;
        }

        //! ====================================================
        //! [+] HISTORY VIEW MODEL
        //! ====================================================
        public static void EventInvoking(Object o) => AddObjectToHistory?.Invoke(o);


        //! ====================================================
        //! [+] HISTORY VIEW MODEL
        //! ====================================================
        private void AddObjectToHistoryMethod(Object o)
        {
            //!? Check for instance of object in the list
            bool matchingDataValue = HistoryObjects.Values.Any(x => x == $"† - {(o as VerseData)?.FromBook} {(o as VerseData)?.FromChapter}:{(o as VerseData)?.ID}") ||
                                     HistoryObjects.Values.Any(x => x == $"♪ - { (o as SongData)?.Title }");

            //!? If Object already exists, move the existing one up
            if (matchingDataValue)
            {
                string? stringMatch = o is SongData ? HistoryObjects.Values.ToList().Find(x => x == $"♪ - { (o as SongData)?.Title }") :
                                                      HistoryObjects.Values.ToList().Find(x => x == $"† - {(o as VerseData)?.FromBook} {(o as VerseData)?.FromChapter}:{(o as VerseData)?.ID}");
                int index = HistoryObjects.Values.ToList().IndexOf(stringMatch);
                HistoryObjects.RemoveAt(index);
            }

            //!? If object is a song data
            if (o is SongData)
            {
                SongData song = (SongData)o;

                HistoryObjects.Add(new Tuple<int, Object>(uniqueKey++, song), $"♪ - { song.Title }");
            }

            //!? If object is a verse data
            else if (o is VerseData)
            {
                VerseData verse = (VerseData)o;

                HistoryObjects.Add(new Tuple<int, Object>(uniqueKey++, verse),
                            $"† - {verse.FromBook} {verse.FromChapter}:{verse.ID}");
            }

            //!? Move Object up top - Always
            if (o != null)
                HistoryObjects.Move(HistoryObjects.Count - 1, 0);
        }
    }
}
