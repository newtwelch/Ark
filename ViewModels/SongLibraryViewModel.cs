using Ark.Models.SongLibrary;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace Ark.ViewModels
{
    public class SongLibraryViewModel : ViewModelBase
    {

        public SongLibraryDatabase _database;

        //! SongData
        public ICollectionView SongsViewSource;
        public ObservableCollection<SongData> Songs { get; set; }
        public ObservableCollection<SongData> SongLanguages { get; set; }

        //! The selected Song;
        private SongData _selectedSong;
        public SongData SelectedSong
        {
            get { return _selectedSong; }
            set
            {
                _selectedSong = value;
                OnPropertyChanged();
            }
        }
        private string titlefilter;
        public string TitleFilter
        {
            get
            {
                return titlefilter;
            }
            set
            {
                if (value != titlefilter)
                {
                    titlefilter = value;
                    SongsViewSource.Refresh();
                    OnPropertyChanged();
                }
            }
        }

        //! List of Lyrics
        public ObservableCollection<LyricData> Lyrics { get; set; }

        public SongLibraryViewModel()
        {
            _database = new SongLibraryDatabase();
            Songs = new ObservableCollection<SongData>(_database.GetSongs());

            SongsViewSource = CollectionViewSource.GetDefaultView(Songs);
            SongsViewSource.Filter = o => string.IsNullOrEmpty(TitleFilter) ? true : (o as SongData).Title.Contains(TitleFilter, System.StringComparison.OrdinalIgnoreCase);

            if (Songs.Count > 0)
            {
                //! Select First Song
                SelectedSong = Songs[0];
                Lyrics = new ObservableCollection<LyricData>();
                SongLanguages = new ObservableCollection<SongData>();
                SelectedSong.Lyrics = Lyrics.ToList();
            }
        }

        public void AddSong(SongData song)
        {
            _database.AddSong(song);
        }
    }
}
