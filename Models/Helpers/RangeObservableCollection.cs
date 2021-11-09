using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Ark.Models.Helpers
{
    //! ====================================================
    //! RANGE OBSERVABLE COLLECTION: faster observablecollection
    //! http://tliangnet.blogspot.com/2013/04/observablecollection-performance-issue.html
    //! ====================================================
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        public void AddRange(IEnumerable<T> list)
        {
            if (list is null) return;

            foreach (T item in list)
                Items.Add(item);

            SendNotifications();
        }

        public void RemoveRange(IEnumerable<T> list)
        {
            if (list is null) return;

            foreach (T item in list)
                Items.Remove(item);

            SendNotifications();
        }

        private void SendNotifications()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        }
    }


}
