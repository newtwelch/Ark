using Ark.Models.SongLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ark.ViewModels
{
    public class SongLibraryViewModel : ViewModelBase
    {

        //! Database access
        private SongLibraryDatabase _database;                                                                      // Song Database class

        //! Song List
        public ObservableCollection<SongData> Songs { get; set; }                                                   // List of Songs
        public ObservableCollection<SongData> SongLanguages { get; set; }                                           // List of Languages in the Song

        //! List of Lyrics
        public ObservableCollection<LyricData> Lyrics { get; set; }

        //! The selected Song;
        public SongData SelectedSong                                                                                // forgot what this is called
        {
            get { return _selectedSong; }
            set
            {
                _selectedSong = value;
                OnPropertyChanged();
                if (Lyrics != null)
                {
                    Lyrics.Clear();
                    foreach (LyricData lyric in ParseLyrics())
                    {
                        Lyrics.Add(lyric);
                    }
                }
            }
        }
        private SongData _selectedSong;                                                                             // SelectedSong but private,

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

        public List<LyricData> ParseLyrics()
        {
            List<LyricData> sequencedLyrics = new List<LyricData>();
            List<LyricData> tempLyrics = new List<LyricData>();
            int stanzaNumber = 1;
            string[] paragraphs = Array.FindAll(Regex.Split(SelectedSong.RawLyrics, "(\r?\n){2,}", RegexOptions.Multiline), p => !String.IsNullOrWhiteSpace(p));

            foreach (string paragraph in paragraphs)
            {
                if (paragraph.StartsWith("CHORUS", StringComparison.OrdinalIgnoreCase))
                    tempLyrics.Add(new LyricData() { Line = "C", Text = Regex.Replace(paragraph, "^(.*\n){1}", ""), Type = LyricType.Chorus });
                else if (paragraph.StartsWith("BRIDGE", StringComparison.OrdinalIgnoreCase))
                    tempLyrics.Add(new LyricData() { Line = "B", Text = Regex.Replace(paragraph, "^(.*\n){1}", ""), Type = LyricType.Bridge });
                else
                    tempLyrics.Add(new LyricData() { Line = stanzaNumber++.ToString(), Text = paragraph, Type = LyricType.Stanza });
            }

            int verseBeforeChorus = 0;
            int verseBeforeChorusCounter = 0;

            if (SelectedSong.Sequence == "o" || SelectedSong.Sequence == null)
            {
                foreach (LyricData lyric in tempLyrics)
                {
                    if (!sequencedLyrics.Any(x => x.Type == LyricType.Chorus))
                    {
                        if (lyric.Type == LyricType.Stanza)
                        {
                            sequencedLyrics.Add(lyric);
                            verseBeforeChorus++;
                        }
                        if (lyric.Type == LyricType.Chorus)
                            sequencedLyrics.Add(lyric);
                    }
                    else
                    {
                        if (lyric.Type == LyricType.Stanza)
                        {
                            sequencedLyrics.Add(lyric);
                            verseBeforeChorusCounter++;
                            if (verseBeforeChorusCounter == verseBeforeChorus)
                            {
                                LyricData chorus = sequencedLyrics.Find(x => x.Type == LyricType.Chorus);
                                sequencedLyrics.Add(chorus);
                                verseBeforeChorusCounter = 0;
                            }
                        }
                    }
                    if (lyric.Type == LyricType.Bridge)
                        sequencedLyrics.Add(lyric);
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
                SelectedSong = Songs[606];                                                                            // Automatically Select the first song
                Lyrics = new ObservableCollection<LyricData>(ParseLyrics());                                        // Initialize the Lyrics
                SongLanguages = new ObservableCollection<SongData>();                                               // and Song Languages
                SelectedSong.Lyrics = Lyrics.ToList();                                                              // Set the lyrics of selected song to the Lyric List    
            }
        }
    }
}
