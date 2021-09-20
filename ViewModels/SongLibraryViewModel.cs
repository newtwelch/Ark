using Ark.Models.SongLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Ark.ViewModels
{
    public class SongLibraryViewModel : ViewModelBase
    {

        //! Database access
        private SongLibraryDatabase _database;                                                                      // Song Database class

        //! Song List
        public ObservableCollection<SongData> Songs { get; set; }                                                   // List of Songs
        public ObservableCollection<SongData> SongLanguages { get; set; }                                           // List of Languages in the Song

        //! Song Filtering
        private ICollectionView SongsView;                                                                          // CollectionView for the songs
        private string _songFilter;                                                                                 // SongsView but pricate
        public string SongFilter                                                                                    // Gets the text from Song Search
        {
            get { return _songFilter; }
            set
            {
                if (value != _songFilter)
                {
                    _songFilter = value;
                    SongsView.Refresh();
                    OnPropertyChanged();
                }
            }
        }

        //! List of Lyrics
        public ObservableCollection<LyricData> Lyrics { get; set; }

        //! The selected Song;
        public SongData SelectedSong                                                                                // Forgot what this is called
        {
            get { return _selectedSong; }
            set                                                                                                     // Do stuff on selection change here
            {
                _selectedSong = value;
                OnPropertyChanged();
                if (Lyrics != null && SelectedSong != null)                                                                                 // Change Lyrics
                {
                    Lyrics.Clear();
                    foreach (LyricData lyric in ParseLyrics())
                    {
                        Lyrics.Add(lyric);
                    }
                }
            }
        }
        private SongData _selectedSong;                                                                             // SelectedSong but private

        //? =============================[METHODS & MAIN]==============================

        //! ====================================================
        //! [+] SONG LIBRARY VIEW MODEL: main method for initializing stuff
        //! ====================================================
        public SongLibraryViewModel()
        {
            //!? ====================================================
            //!? VARIABLE INITIALIZE: class property here
            //!? ====================================================
            _database = new SongLibraryDatabase();
            Songs = new ObservableCollection<SongData>(_database.GetSongs());

            //!? ====================================================
            //!? SONG FILTER: collection view filtering
            //!? ====================================================
            SongsView = CollectionViewSource.GetDefaultView(Songs);
            //SongsView.Filter = o => String.IsNullOrEmpty(SongFilter) ? true : (o as SongData).Title.Contains(SongFilter, StringComparison.OrdinalIgnoreCase);
            SongsView.Filter = SongFilterView;

            //!? ====================================================
            //!? INITIALIZE: methods and stuff here
            //!? ====================================================
            init();
        }

        //? ==========================[ SONG FUNCTIONS ]=============================

        //! ====================================================
        //! [+] ADD SONG: adds song to database
        //! ====================================================
        public void AddSong(SongData song)
        {
            _database.AddSong(song);
        }

        //! ====================================================
        //! [+] FILTER SONG LIST: search based on title, author or lyrics
        //! ====================================================
        private bool SongFilterView(object song)
        {

            // if "SongFilter" is empty, show all
            if (String.IsNullOrEmpty(SongFilter))
            {
                (song as SongData).SearchedLyric = null;
                return true;
            }
            // if it starts with a DOT (.) then match the RawLyric of songs
            else if (SongFilter.StartsWith("."))
            {
                // get the raw lyric
                string lyric = (song as SongData).RawLyrics;
                // set the match lyric as boolean
                bool _lyricMatch = (song as SongData).RawLyrics.
                    Contains(SongFilter.Replace(".", ""), StringComparison.OrdinalIgnoreCase);

                //if boolean is true / if it is a match
                if (_lyricMatch)
                    lyric = lyric.Substring(lyric.IndexOf(SongFilter.Replace(".", ""), StringComparison.OrdinalIgnoreCase)).Replace("\n", " ").Replace("\r", " ");
                // set the "SearchedLyric" to display a preview of the searched lyric
                (song as SongData).SearchedLyric = lyric.Substring(0, Math.Min(lyric.Length, 35)) + "...";

                //return the boolean lyricmatch
                return _lyricMatch;
            }
            // if it starts with a START (*) then check for author matches
            else if (SongFilter.StartsWith("*"))
                return (song as SongData).Author.
                    Contains(SongFilter.Replace("*", ""), StringComparison.OrdinalIgnoreCase);
            // if it doesn't start with any identifiers then search for title matches
            else
                return (song as SongData).Title.
                    Contains(SongFilter, StringComparison.OrdinalIgnoreCase);

        }

        //! ====================================================
        //! [+] LYRICPARSING: returns a list<> of Parsed LyricData
        //! ====================================================
        public List<LyricData> ParseLyrics()
        {
            //!? ====================================================
            //!? VARIABLES: initialize the needed variables for lyric parsing
            //!? ====================================================
            List<LyricData> sequencedLyrics = new List<LyricData>();                                                    // List<> that will be returned; already sequenced
            List<LyricData> tempLyrics = new List<LyricData>();                                                         // A temporary list the stores un-sequenced lyrics
            string[] paragraphs = Array.FindAll(Regex.Split(SelectedSong.RawLyrics,                                     // Parses the "RawLyrics" into paragraphs,
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

                // Check if the beginning contains "CHORUS" string
                if (first.Contains("CHORUS", StringComparison.OrdinalIgnoreCase))
                    // Convert to a LyricData with Chorus Type
                    tempLyrics.Add(new LyricData() { Line = "C", Text = Regex.Replace(paragraph, "^(.*\n){1}", ""), Type = LyricType.Chorus });
                // Check if the beginning contains "BRIDGE" string
                else if (first.Contains("BRIDGE", StringComparison.OrdinalIgnoreCase))
                    // Convert to a LyricData with Bridge Type
                    tempLyrics.Add(new LyricData() { Line = "B", Text = Regex.Replace(paragraph, "^(.*\n){1}", ""), Type = LyricType.Bridge });
                // If no identifiers then it is a verse
                else
                    // Convert to a LyricData with Verse Type
                    tempLyrics.Add(new LyricData() { Line = stanzaNumber++.ToString(), Text = paragraph, Type = LyricType.Stanza });
            }

            //!? ====================================================
            //!? SEQUENCE THE LYRIC: default sequence, has 2 parts
            //!? ====================================================
            if (SelectedSong.Sequence == "o" || SelectedSong.Sequence == null || SelectedSong.Sequence == "")
            {
                foreach (LyricData lyric in tempLyrics)                                                                             // For every LyricData in "tempLyrics"                
                {
                    //!? ====================================================
                    //!? PART I: if there is no chorus in the sequenced lyric YET
                    //!? ====================================================
                    if (!sequencedLyrics.Any(x => x.Type == LyricType.Chorus))
                    {
                        if (lyric.Type == LyricType.Stanza)                                                                         // If Lyric is a STANZA
                        {
                            sequencedLyrics.Add(lyric);                                                                             // Add to the "sequencedLyric" List<>
                            verseBeforeChorus++;                                                                                    // Increment the "VerseBeforeChorus"
                        }
                        if (lyric.Type == LyricType.Chorus)                                                                         // If Chorus is found
                            sequencedLyrics.Add(lyric);                                                                             // Add it. Proceed to next part
                    }
                    //!? ====================================================
                    //!? PART II: when the chorus is found proceed here
                    //!? ====================================================
                    else
                    {
                        if (verseBeforeChorus == 0)                                                                                 // If there are no verses before chorus
                            verseBeforeChorus = 1;                                                                                  // Set it to 1 as default

                        if (lyric.Type == LyricType.Stanza)                                                                         // If Lyric is a STANZA
                        {
                            sequencedLyrics.Add(lyric);                                                                             // Add to the "sequencedLyric" List<>
                            verseBeforeChorusCounter++;                                                                             // Increment the Verse Counter
                            if (verseBeforeChorusCounter == verseBeforeChorus)                                                      // If Verse Counter = "VerseBeforeChorus"
                            {
                                // Null suppresion
                                LyricData? chorus = sequencedLyrics.Find(x => x.Type == LyricType.Chorus);                          // Find the Chorus and insert it
                                sequencedLyrics.Add(chorus);
                                verseBeforeChorusCounter = 0;
                            }
                        }

                        if (lyric.Type == LyricType.Bridge)                                                                             // If it's a bridge, just add 
                            sequencedLyrics.Add(lyric);
                    }
                }
            }

            //!? ====================================================
            //!? SEQUENCE THE LYRIC: if a custom Sequence exists
            //!? ====================================================
            else
            {
                string[] sequencer = SelectedSong.Sequence.Split(",");                                                              // Split sequence string

                foreach (var line in sequencer)                                                                                     // For each String in sequence
                {
                    // Null supperesion                                                                                             // Find the corresponding LyricData()
                    LyricData? lyric = tempLyrics.Find(x => x.Line.ToUpper() == line.ToUpper().Replace("S", ""));                   // with the same "Line" in "tempLyrics"
                    if (line != "" && lyric != null)
                    {
                        sequencedLyrics.Add(lyric);                                                                                 // Make sure it is not null and add it
                    }
                }
            }

            return sequencedLyrics;
        }

        //? ==========================[ FUNCTION END ]===============================

        private void init()
        {
            if (Songs.Count > 0)
            {
                //! Select First Song
                SelectedSong = Songs[0];                                                                            // Automatically Select the first song
                Lyrics = new ObservableCollection<LyricData>(ParseLyrics());                                        // Initialize the Lyrics
                SongLanguages = new ObservableCollection<SongData>();                                               // Initialize Song Languages
                SelectedSong.Lyrics = Lyrics.ToList();                                                              // Set the lyrics of selected song to the Lyric List    
            }
        }
    }
}
