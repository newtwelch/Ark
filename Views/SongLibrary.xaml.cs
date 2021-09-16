using Ark.ViewModels;
using System.Windows.Controls;

namespace Ark.Views
{
    public partial class SongLibrary : UserControl
    {
        private SongLibraryViewModel _viewModel;
        public SongLibrary()
        {
            _viewModel = new SongLibraryViewModel();
            DataContext = _viewModel;
            InitializeComponent();

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.Songs.Clear();
            foreach (var song in _viewModel._database.GetSongsByTitle(Search.Text))
            {
                _viewModel.Songs.Add(song);
            }
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        }
    }
}
