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

        private Tuple<int, Object> _selectedObject;
        public Tuple<int, Object> SelectedObject
        {
            get => _selectedObject;
            set
            {
                _selectedObject = value;
                OnPropertyChanged();

                if (value is not null)
                    HistoryObjectSelected?.Invoke(_selectedObject.Item2);
            }
        }

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
            bool matchingDataValue = HistoryObjects.Values.Any(x => x == $"† - {(o as VerseData)?.FromBook} {(o as VerseData)?.FromChapter}:{(o as VerseData)?.ID}")
                                  || HistoryObjects.Values.Any(x => x == $"♪ - { (o as SongData)?.Title }");

            //!? If Object already exists, move the existing one up
            if (matchingDataValue)
            {
                string? stringMatch = o is SongData ? HistoryObjects.Values.ToList().Find(x => x == $"♪ - { (o as SongData)?.Title }") :
                                                      HistoryObjects.Values.ToList().Find(x => x == $"† - {(o as VerseData)?.FromBook} {(o as VerseData)?.FromChapter}:{(o as VerseData)?.ID}");
                int index = HistoryObjects.Values.ToList().IndexOf(stringMatch);
                HistoryObjects.RemoveAt(index);
            }

            if (o is SongData song)
                HistoryObjects.Add(new Tuple<int, Object>(uniqueKey++, song), $"♪ - { song.Title }");
            else if (o is VerseData verse)
                HistoryObjects.Add(new Tuple<int, Object>(uniqueKey++, verse),
                                    $"† - {verse.FromBook} {verse.FromChapter}:{verse.ID}");

            //!? Move Object up top - Always
            if (o is not null)
                HistoryObjects.Move(HistoryObjects.Count - 1, 0);
        }
    }
}
