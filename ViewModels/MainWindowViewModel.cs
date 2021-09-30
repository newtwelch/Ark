using Ark.Views;
using System.Windows.Controls;

namespace Ark.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {

        //! Display Window
        public DisplayWindow DisplayWindow;

        //! User Controls
        public UserControl SongLibrary, BibleLibrary, History;

        //! ====================================================
        //! [+] MAIN WINDOW VIEW MODEL
        //! ====================================================
        public MainWindowViewModel()
        {
            //!? ====================================================
            //!? INITIALIZE: start stuff here
            //!? ====================================================
            DisplayWindow = new DisplayWindow();                                            // Initialize Display Window
            SongLibrary = new SongLibrary();                                                // Initialize Song Library
            BibleLibrary = new BibleLibrary();                                              // Initialize Bible Library
            History = new History();                                              // Initialize Bible Library
        }
    }
}
