using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ark.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {

        // Null suppresion
        public event PropertyChangedEventHandler? PropertyChanged;

        //! ====================================================
        //! [+] ON PROPERTY CHANGED: an event that occurs when a property is changed
        //! ====================================================

        // Null suppresion
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //! ====================================================
        //! [+] SET PROPERTY: in theory it sets property value;
        //!                   I am unsure why this is here or what it actually does
        //! ====================================================
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;
            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}
