using Ark.Models.SongLibrary;
using Ark.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ark.Views
{
    public partial class SongLibrary : UserControl
    {
        private SongLibraryViewModel _viewModel;

        private SongData save_Song;

        bool isAddingASong;

        //! ====================================================
        //! [+] SONG LIBRARY: initialize stuff here
        //! ====================================================
        public SongLibrary()
        {
            _viewModel = new SongLibraryViewModel();
            DataContext = _viewModel;
            InitializeComponent();

            //_viewModel.SongAdded += EditModeSetUp;
            SongLibraryViewModel.SongAdded += AddingASong;
        }

        //? =============================[EVENTS]==============================

        //! ====================================================
        //! [+] BUTTON CLICKS: this has 2 buttons connected to it
        //!         with 2 states, therefore 4 cases.
        //! ====================================================
        private void EditButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button? bt = e.Source as Button;
            switch (bt.Name)
            {
                case "EditButton":
                    switch (EditButton.Content)
                    {
                        case "E": //! ========= EDIT SONG =========
                            //!? ====================================================
                            //!? SAVE: save up a copy of the songs properties [ this is incase the user wants to discard changes ]
                            //!? ====================================================
                            save_Song = new SongData()
                            {
                                Title = _viewModel.SelectedSong.Title,
                                Author = _viewModel.SelectedSong.Author,
                                Language = _viewModel.SelectedSong.Language,
                                RawLyrics = _viewModel.SelectedSong.RawLyrics,
                                Sequence = _viewModel.SelectedSong.Sequence

                            };
                            break;
                        case "s": //! ========= SAVE CHANGES =========
                            //!? ====================================================
                            //!? SAVE CHANGES: set the SelectedSong properties and save into the database
                            //!? ====================================================
                            _viewModel.SelectedSong.RawLyrics = EditRawLyricsTextBox.Text;
                            _viewModel.SelectedSong.Sequence = EditSequenceTextBox.Text;
                            _viewModel.UpdateSong(_viewModel.SelectedSong);
                            isAddingASong = false;
                            break;
                    }

                    //!? ====================================================
                    //!? EDIT MODE SETUP [ if the edit button is clicked, always setup editmode boolean ]
                    //!? ====================================================
                    EditModeSetUp();
                    SongListBox.ScrollIntoView(SongListBox.SelectedItem);

                    break;
                case "TagDiscardButton":
                    switch (TagDiscardButton.Content)
                    {
                        case "T": //! ========= TAGS =========
                            break;
                        case "D": //! ========= DISCARD CHANGES =========
                                  //!? ====================================================
                                  //!? DISCARD CHANGES: revert back the properties and do nothing
                                  //!? ====================================================
                            _viewModel.SelectedSong = null;

                            if (isAddingASong) // if we are discarding a new song, delete it
                                _viewModel.DeleteSong(this);
                            else               // if we are discarding an edit, save previous data
                                _viewModel.SelectedSong = save_Song;

                            //!? EDIT MODE SETUP [ exits out of Edit Mode ]
                            EditModeSetUp();
                            SongListBox.ScrollIntoView(SongListBox.SelectedItem);
                            isAddingASong = false;

                            break;
                    }
                    break;
            }
        }

        //! ====================================================
        //! [+] RAW LYRIC TEXTBOX CHANGED: parses the lyric live and
        //!             gives preview of what the edited lyric looks like
        //! ====================================================
        private void EditRawLyricsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_viewModel.IsEditMode)
            {
                _viewModel.Lyrics.Clear();
                // Parse Lyrics using text from RawLyricTextBox and SequenceTextBox
                foreach (LyricData lyric in _viewModel.ParseLyrics(EditRawLyricsTextBox.Text, EditSequenceTextBox.Text))
                {
                    _viewModel.Lyrics.Add(lyric);
                }
            }
        }

        //! ====================================================
        //! [+] SONG LYRIC LISTBOX LOST FOCUS: update rawlyrics when lyric listbox loses focus
        //! ====================================================
        private void SongLyricListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // don't wanna comment much here,
            // it just updates whatever changes you've made in the lyric listbox
            // converting it to RawLyrics
            List<LyricData> lyrics = new List<LyricData>();
            foreach (LyricData lyric in SongLyricListBox.Items)
            {
                lyrics.Add(lyric);
            }
            EditRawLyricsTextBox.Text = ToRawLyrics(lyrics);
        }


        //? =============================[METHODS & HELPERS]==============================

        //! ====================================================
        //! [+] ADDING A SONG: event for adding a song
        //! ====================================================
        private void AddingASong()
        {
            EditModeSetUp();
            isAddingASong = true;
        }

        //! ====================================================
        //! [+] EDIT MODE SETUP: flip viewModel Editmode and proceed to change properties accordingly
        //! ====================================================
        private void EditModeSetUp()
        {
            //!? ====================================================
            //!? EDIT MODE BOOLEAN: switch up the edit mode boolean
            //!? ====================================================
            _viewModel.IsEditMode = _viewModel.IsEditMode ? false : true;
            bool isEditMode = _viewModel.IsEditMode;

            //!? ====================================================
            //!? EDIT BUTTON: change contents accordingle
            //!? ====================================================
            EditButton.Content = isEditMode ? "s" : "E";
            EditButton.ToolTip = isEditMode ? "Save changes made to this song" : "Edit the selected song";

            //!? ====================================================
            //!? TAG DISCARD BUTTON: change contents accordingly
            //!? ====================================================
            TagDiscardButton.Content = isEditMode ? "D" : "T";
            TagDiscardButton.BorderBrush = isEditMode ? (Brush)Application.Current.Resources["RedBrush"] : (Brush)Application.Current.Resources["ThumbBrush"];
            TagDiscardButton.ToolTip = isEditMode ? "Discard changes made to this song" : "Edit Tags of this song";
        }

        //! ====================================================
        //! [+] TO RAW LYRICS: convertion from listbox items to a string
        //! ====================================================
        public String ToRawLyrics(List<LyricData> lyrics)
        {
            string _rawLyrics = "";

            foreach (LyricData lyric in lyrics)
            {
                //!? ====================[ Logic ]==================
                //!? if lyric is a stanza, just add to the string
                //!? if it's a Chorus, check if there is already a chorus.
                //!? if chorus exists don't add it, if it does, then add to the RawLyric
                //!? if it's a bridge just add as well
                //!? ====================[ Logic ]==================

                if (lyric.Type == LyricType.Stanza)
                    _rawLyrics += lyric.Line.Equals("1") ? $"{ lyric.Text}\r\n" : $"\r\n{ lyric.Text}\r\n";

                if (lyric.Type == LyricType.Chorus)
                {
                    if (!_rawLyrics.Contains("CHORUS", StringComparison.OrdinalIgnoreCase))
                        _rawLyrics += $"\r\nCHORUS\r\n{lyric.Text} \r\n";
                }

                if (lyric.Type == LyricType.Bridge)
                    _rawLyrics += $"\r\nBRIDGE\r\n{lyric.Text} \r\n";
            }
            // Trim the end for unwanted white lines;
            return _rawLyrics.TrimEnd();
        }

        //! ====================================================
        //! [+] SEARCH FOCUS METHOD: hotkey stuff
        //! ====================================================
        public void SearchFocusMethod()
        {
            SongSearchTextBox.Text = "";
            SongSearchTextBox.Focus();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.SearchFocusEvent += SearchFocusMethod;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            MainWindow.SearchFocusEvent -= SearchFocusMethod;
        }
    }
}
