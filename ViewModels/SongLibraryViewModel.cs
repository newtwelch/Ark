using Ark.Models.Helpers;
using Ark.Models.SongLibrary;
using Ark.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Ark.ViewModels
{
    public class SongLibraryViewModel : ViewModelBase
    {

        //! Database access
        private SongLibraryDatabase _database;

        //! LISTS:
        public ObservableCollection<SongData> Songs { get; set; }
        public ObservableCollection<SongData> SongLanguages { get; set; }
        public ObservableCollection<LyricData> Lyrics { get; set; }

        //! Song Filtering
        private ICollectionView SongLanguagesView;
        private ICollectionView SongsView;
        public string SongFilter
        {
            get => _songFilter;
            set
            {
                _songFilter = value;
                SongsView.Refresh();
                OnPropertyChanged();
            }
        }
        private string _songFilter;

        //! Selected Song
        public SongData SelectedSong
        {
            get => _selectedSong;
            set
            {
                //!? If the new song is null, stick with current _selectedSong
                value ??= _selectedSong;

                //!? Save and check if Previous Song language matches
                int previous = _selectedSong is null ? 0 : _selectedSong.Number;

                //!? ================ [Property Changed] ================
                _selectedSong = value;
                OnPropertyChanged();

                //!? Set the LanguageView Filter
                if (SongLanguagesView != null && value.Number != previous)
                    SongLanguagesView.Filter = o => ((SongData)o).Number.Equals(SelectedSong.Number);  // Filter SongLanguages

                //!? Parse the Lyrics and Add them
                Lyrics?.Clear();
                ParseLyrics(value.RawLyrics, value.Sequence).ForEach(x => Lyrics?.Add(x));

            }
        }
        private SongData _selectedSong;

        //! Selected Lyric
        public LyricData SelectedLyric
        {
            get => _selectedLyric;
            set
            {
                //!? ================ [Property Changed] ================
                _selectedLyric = value;
                OnPropertyChanged();

                //!? If null don't continue
                if (value is null)
                    return;

                //!? Setup second display, Invoking events
                OnSelectedLyricChanged?.Invoke(_selectedLyric);
                HistoryViewModel.EventInvoking(_selectedSong);
                DisplayWindow.Instance.Show();
            }
        }
        private LyricData _selectedLyric;
        public static event Action<LyricData> OnSelectedLyricChanged;

        //! EDIT MODE & VISIBILITIES
        public bool IsEditMode  // Edit Mode
        {
            get => _isEditMode;
            set
            {
                _isEditMode = value;
                OnPropertyChanged();
                OnPropertyChanged("EditModeVisible");
                OnPropertyChanged("EditModeNotVisible");
                SongsView.Refresh();
            }
        }
        private bool _isEditMode;

        //!? Visibility
        public Visibility EditModeVisible => _isEditMode ? Visibility.Visible : Visibility.Collapsed;
        public Visibility EditModeNotVisible => _isEditMode ? Visibility.Collapsed : Visibility.Visible;

        //! Commands
        public ICommand Add_Song { get; set; }
        public static event Action SongAdded;   // Create an event
        public ICommand Delete_Song { get; set; }

        //? =============================[METHODS & MAIN]==============================

        //! ====================================================
        //! [+] SONG LIBRARY VIEW MODEL: main method for initializing stuff
        //! ====================================================
        public SongLibraryViewModel()
        {
            //!? ====================================================
            //!? VARIABLE INITIALIZE: after init, select first song and apply lyrics
            //!? ====================================================
            _database = new SongLibraryDatabase();
            Songs = new ObservableCollection<SongData>(_database.GetSongs());
            SongLanguages = new ObservableCollection<SongData>(Songs);

            //!? ====================================================
            //!? INIT SELECTED SONG: if there are no songs return
            //!? ====================================================
            SelectedSong = Songs[0];
            Lyrics = new ObservableCollection<LyricData>(ParseLyrics(SelectedSong.RawLyrics, SelectedSong.Sequence));

            //!? ====================================================
            //!? SONG FILTER: collection view filtering
            //!? ====================================================
            SongsView = CollectionViewSource.GetDefaultView(Songs);
            SongsView.Filter = SongFilterView;
            //!? Song Language
            SongLanguagesView = CollectionViewSource.GetDefaultView(SongLanguages);
            SongLanguagesView.Filter = o => ((SongData)o).Number.Equals(SelectedSong.Number);

            //!? ====================================================
            //!? COMMANDS
            //!? ====================================================
            Add_Song = new RelayCommands(o => AddSong(o));
            Delete_Song = new RelayCommands(o => DeleteSong(o));
        }

        //? ==========================[ SONG FUNCTIONS ]=============================

        //! ====================================================
        //! [+] ADD SONG: adds song to database
        //! ====================================================
        public void AddSong(Object sender)
        {
            SongData newSong = new SongData()
            {
                Title = "Title",
                Author = "Author",
                RawLyrics = "Filler Lyric",
                Sequence = "o",
                Language = "DEFAULT",
            };

            //!? Add song to the database & local list then select
            _database.AddSong(newSong);
            Songs.Add(_database.LastSong());
            SelectedSong = Songs[Songs.Count - 1];

            //!? The Code-Behind listens to this
            SongAdded?.Invoke();
        }

        //! ====================================================
        //! [+] DELETE SONG: deletes song to database
        //! ====================================================
        public void DeleteSong(Object sender)
        {
            int storeIndex = Songs.IndexOf(SelectedSong);
            _database.DeleteSong(SelectedSong);
            Songs.Remove(SelectedSong);
            SelectedSong = Songs[storeIndex != 0 ? storeIndex - 1 : storeIndex];
        }

        //! ====================================================
        //! [+] UPDATE SONG: rewrites the song in the database
        //! ====================================================
        public void UpdateSong(SongData song) => _database.UpdateSong(song);

        //! ====================================================
        //! [+] FILTER SONG LIST: search based on title, author or lyrics
        //! ====================================================
        private bool SongFilterView(object song)
        {
            ((SongData)song).SearchedLyric = "";

            //!? ====================================================
            //!? NONE: if text is empty, set every object to true
            //!? ====================================================
            if (String.IsNullOrEmpty(SongFilter))
                return true;
            //!? ====================================================
            //!? LYRIC SEARCH: if text starts with a DOT, match the lyrics
            //!? ====================================================
            else if (SongFilter.StartsWith("."))
            {
                string rawLyric = ((SongData)song).RawLyrics;
                string songFilter = SongFilter.Replace(".", "");
                bool _lyricMatch = rawLyric.Contains(songFilter, StringComparison.OrdinalIgnoreCase);

                //!? Get the first 35 letter of matched raw lyric and set it as Searched Lyric
                rawLyric = _lyricMatch ? rawLyric.Substring(rawLyric.IndexOf(songFilter, StringComparison.OrdinalIgnoreCase))
                                                 .Replace("\n", " ").Replace("\r", " ") : "";
                ((SongData)song).SearchedLyric = rawLyric.Substring(0, Math.Min(rawLyric.Length, 35)) + "...";

                return _lyricMatch;
            }
            //!? ====================================================
            //!? AUTHOR SEARCH: if text starts with a STAR, match author name
            //!? ====================================================
            else if (SongFilter.StartsWith("*"))
                return ((SongData)song).Author.Contains(SongFilter.Replace("*", ""), StringComparison.OrdinalIgnoreCase);
            //!? ====================================================
            //!? TITLE SEARCH: if no identifiers are found, match the title
            //!? ====================================================
            else
                return ((SongData)song).Title.Contains(SongFilter, StringComparison.OrdinalIgnoreCase);
        }

        //! ====================================================
        //! [+] LYRICPARSING: returns a list<> of Parsed LyricData
        //! ====================================================
        public List<LyricData> ParseLyrics(string RawLyrics, string Sequence)
        {
            //!? ====================================================
            //!? VARIABLES: initialize the needed variables for lyric parsing
            //!? ====================================================
            List<LyricData> sequencedLyrics = new List<LyricData>();                                                    // List<> that will be returned; already sequenced
            List<LyricData> tempLyrics = new List<LyricData>();                                                         // A temporary list the stores un-sequenced lyrics
            string[] paragraphs = Array.FindAll(Regex.Split(RawLyrics,                                                  // Parses the "RawLyrics" into paragraphs,
                "(\r?\n){2,}", RegexOptions.Multiline), p => !String.IsNullOrWhiteSpace(p));                            //      then to LyricData() and added to "tempLyrics"
            int stanzaNumber = 1;                                                                                       // Stanza Number for the "Line" property 

            int verseBeforeChorus = 0;                                                                                  // How many Verses before the Chorus;
            int verseBeforeChorusCounter = 0;                                                                           //      this counts the verses BEFORE ADDING the Chorus

            //!? ====================================================
            //!? CONVERT TO LYRICDATA: gets the parsed paragraph and does magic;
            //!? ====================================================
            foreach (string paragraph in paragraphs)
            {
                using var reader = new StringReader(paragraph);
                string? first = reader.ReadLine();

                //!? Determine the type of lyric
                if (first.Contains("CHORUS", StringComparison.OrdinalIgnoreCase))
                    tempLyrics.Add(new LyricData() { Line = "C", Text = Regex.Replace(paragraph, "^(.*\n){1}", ""), Type = LyricType.Chorus });
                else if (first.Contains("BRIDGE", StringComparison.OrdinalIgnoreCase))
                    tempLyrics.Add(new LyricData() { Line = "B", Text = Regex.Replace(paragraph, "^(.*\n){1}", ""), Type = LyricType.Bridge });
                else
                    tempLyrics.Add(new LyricData() { Line = stanzaNumber++.ToString(), Text = paragraph, Type = LyricType.Stanza });
            }

            //!? ====================================================
            //!? DEFAULT SEQUENCE: default sequence, has 2 parts
            //!? ====================================================
            if (Sequence == "o" || Sequence == null || Sequence == "")
            {
                foreach (LyricData lyric in tempLyrics)   // For every LyricData in "tempLyrics"                
                {
                    //!? ====================================================
                    //!? PART I: if there is no chorus in the sequenced lyric YET
                    //!? ====================================================
                    if (!sequencedLyrics.Any(x => x.Type == LyricType.Chorus))
                    {
                        if (lyric.Type == LyricType.Stanza) // If it is still a stanza,
                            verseBeforeChorus++;            // Increment number of verses before Chorus

                        sequencedLyrics.Add(lyric);         // Add it
                    }
                    //!? ====================================================
                    //!? PART II: when the chorus is found proceed here
                    //!? ====================================================
                    else
                    {
                        //!? Deafult to 1 if versebeforechorus is 0
                        if (verseBeforeChorus.Equals(0)) verseBeforeChorus = 1;

                        //!? If Type is Stanza, Insert Chorus every (verseBeforeChorus amount)
                        if (lyric.Type == LyricType.Stanza)
                        {
                            sequencedLyrics.Add(lyric);
                            verseBeforeChorusCounter++;

                            if (verseBeforeChorusCounter == verseBeforeChorus)
                            {
                                LyricData? chorus = sequencedLyrics.Find(x => x.Type == LyricType.Chorus);
                                sequencedLyrics.Add(chorus);
                                verseBeforeChorusCounter = 0;
                            }
                        }
                        else if (lyric.Type == LyricType.Bridge)
                            sequencedLyrics.Add(lyric);
                    }
                }
            }

            //!? ====================================================
            //!? CUSTOM SEQUENCE: if a custom Sequence exists
            //!? ====================================================
            else
            {
                //!? Split the sequencer
                string[] sequencer = Sequence.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in sequencer)                                                                     // For each String in sequence
                {   //!? Find the corresponding LyricData()
                    LyricData? lyric = tempLyrics.Find(x => x.Line.ToUpper() == line.ToUpper().Replace("S", ""));   // with the same "Line" in "tempLyrics"
                    if (line != "" && lyric != null)
                        sequencedLyrics.Add(lyric);                                                                 // Make sure it is not null and add it
                }
            }

            return sequencedLyrics;
        }

        //? ==========================[ FUNCTION END ]===============================

    }
}
