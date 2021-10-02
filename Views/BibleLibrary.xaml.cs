using Ark.ViewModels;
using System.Windows.Controls;

namespace Ark.Views
{
    public partial class BibleLibrary : UserControl
    {
        private BibleLibraryViewModel _viewModel;
        public BibleLibrary()
        {
            _viewModel = new BibleLibraryViewModel();
            DataContext = _viewModel;
            InitializeComponent();
        }
    }
}
