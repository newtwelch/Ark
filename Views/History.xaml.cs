using Ark.ViewModels;
using System.Windows.Controls;

namespace Ark.Views
{
    /// <summary>
    /// Interaction logic for History.xaml
    /// </summary>
    public partial class History : UserControl
    {
        private HistoryViewModel _viewModel;

        public History()
        {
            _viewModel = new HistoryViewModel();
            DataContext = _viewModel;

            InitializeComponent();

        }
    }
}
