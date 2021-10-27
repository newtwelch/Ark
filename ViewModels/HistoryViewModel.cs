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

                if (value != null)
                    HistoryObjectSelected.Invoke(value.Item2);

                OnPropertyChanged();
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
        public static void EventInvoking(Object o)
        {
            AddObjectToHistory?.Invoke(o);
        }

        //! ====================================================
        //! [+] HISTORY VIEW MODEL
        //! ====================================================
        private void AddObjectToHistoryMethod(Object o)
        {
            if (HistoryObjects.Keys.Any(x => x.Item2 == o))
            {
                Tuple<int, Object> tupleMatch = HistoryObjects.Keys.ToList().Find(x => x.Item2 == o);
                int index = HistoryObjects.Keys.ToList().IndexOf(tupleMatch);
                HistoryObjects.Move(index, 0);
                return;
            }

            if (o is SongData)
            {
                SongData objectAsSong = (SongData)o;

                HistoryObjects.Add(new Tuple<int, Object>(uniqueKey++, objectAsSong), objectAsSong.Title);
            }

            else if (o is VerseData)
            {
                VerseData objectAsVerse = (VerseData)o;

                HistoryObjects.Add(new Tuple<int, Object>(uniqueKey++, objectAsVerse),
                            $"{objectAsVerse.FromBook} {objectAsVerse.FromChapter}:{objectAsVerse.ID}");
            }

            if (o != null)
                HistoryObjects.Move(HistoryObjects.Count - 1, 0);
        }
    }
}
