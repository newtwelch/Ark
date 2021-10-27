using Ark.ViewModels;
using System.Windows.Controls;

namespace Ark.Views
{
    /// <summary>
    /// Interaction logic for History.xaml
    /// </summary>
    public partial class History : UserControl
    {
        //! ViewModel
        private HistoryViewModel _viewModel;

        //! ====================================================
        //! [+] HISTORY
        //! ====================================================
        public History()
        {
            _viewModel = new HistoryViewModel();
            DataContext = _viewModel;

            InitializeComponent();

        }

        //! ====================================================
        //! [+] USER CONTROL UNLOADED
        //! ====================================================
        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            HistoryListBox.SelectedItem = null;
        }
    }
}
