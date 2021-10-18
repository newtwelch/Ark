using Ark.Models.SongLibrary;
using Ark.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ark.Views
{
    public partial class SongLibrary : UserControl
    {
        private SongLibraryViewModel _viewModel;

        private SongData save_Song;

        bool isAddingASong;
        int storeSong;

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
                            _viewModel.SelectedSong = _viewModel.Songs[0];
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
            //!? If we are not in edit mode, don't continue
            if (!_viewModel.IsEditMode)
                return;

            //!? ====================================================
            //!? Auto parse lyrics when editing raw lyrics
            //!? ====================================================
            _viewModel.Lyrics.Clear();
            // Parse Lyrics using text from RawLyricTextBox and SequenceTextBox
            foreach (LyricData lyric in _viewModel.ParseLyrics(EditRawLyricsTextBox.Text, EditSequenceTextBox.Text))
            {
                _viewModel.Lyrics.Add(lyric);
            }

        }

        //! ====================================================
        //! [+] SONG LYRIC LISTBOX LOST FOCUS: update rawlyrics when lyric listbox loses focus
        //! ====================================================
        private void SongLyricListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //!? If we are not in edit mode, don't save anything
            if (!_viewModel.IsEditMode)
                return;

            //!? don't wanna comment much here,
            //!? it just updates whatever changes you've made in the lyric listbox
            //!? converting it to RawLyrics
            List<LyricData> lyrics = new List<LyricData>();
            foreach (LyricData lyric in SongLyricListBox.Items)
            {
                lyrics.Add(lyric);
            }
            EditRawLyricsTextBox.Text = ToRawLyrics(lyrics);
        }

        //! ====================================================
        //! [+] LIST BOX KEY DOWN: hotkeys for listbox item navigation
        //! ====================================================
        private void ListBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (_viewModel.IsEditMode)
                return;

            ListBox? lb = sender as ListBox;
            switch (lb.Name)
            {
                //!? ====================================================
                //!? Song Lyrics
                //!? ====================================================
                case "SongLyricListBox":

                    //!? ====================================================
                    //!? Selected the Stanza Number according to the Number pressed down
                    //!? ====================================================
                    if (e.Key > Key.D0 && e.Key <= Key.D9 ||
                        e.Key > Key.NumPad0 && e.Key <= Key.NumPad9)
                    {
                        int lindex = lb.Items.Cast<LyricData>().ToList().FindIndex(x => x.Line == e.Key.ToString().Replace("D", "").Replace("NumPad", ""));
                        lb.SelectedIndex = lindex;
                        lb.ScrollIntoView(lb.SelectedItem);
                        e.Handled = true;
                    }
                    //!? If 0 is pressed, then get the chorus
                    else if (e.Key == Key.D0 || e.Key == Key.NumPad0 && _viewModel.Lyrics.Any(x => x.Line == "C"))
                    {
                        while (true)
                        {
                            lb.SelectedIndex++;

                            LyricData selectedLyric = lb?.SelectedItem as LyricData;
                            if (selectedLyric?.Line == "C")
                                break;
                        }

                        lb?.ScrollIntoView(lb.SelectedItem);
                        e.Handled = true;
                    }

                    if (lb.SelectedItem != null)
                        FocusListBoxItem(lb);
                    else if (e.Key == Key.Enter)
                    {
                        lb.SelectedIndex = storeSong;
                        FocusListBoxItem(lb);
                    }

                    break;

                case "SongListBox":
                    if (e.Key == Key.Enter)
                    {
                        SongLyricListBox.SelectedIndex = 0;
                        FocusListBoxItem(SongLyricListBox);
                    }
                    break;
            }
        }

        //! ====================================================
        //! [+] TEXT BOX KEY DOWN: hotkey for confirmation
        //! ====================================================
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox? tb = sender as TextBox;
            if (tb.Name == "SongSearchTextBox")
            {
                if (e.Key == Key.Enter)
                {
                    SongListBox.SelectedIndex = 0;
                    FocusListBoxItem(SongListBox);
                }
            }
        }

        //? =============================[METHODS & HELPERS]==============================

        //! ====================================================
        //! [+] FOCUS LIST BOX ITEM
        //! ====================================================
        void FocusListBoxItem(ListBox lb)
        {
            ListBoxItem? lbi = lb.ItemContainerGenerator.ContainerFromIndex(lb.SelectedIndex) as ListBoxItem;
            lbi?.Focus();
        }

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

        //! ====================================================
        //! [+] CLOSE DISPLAY
        //! ====================================================
        public void CloseDisplayMethod()
        {
            storeSong = SongLyricListBox.SelectedIndex;
            _viewModel.SelectedLyric = null;
            SongLyricListBox.Focus();
        }

        //? =============================[LOADED & UNLOADED]==============================

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SongSearchTextBox.Focus();
            MainWindow.SearchFocusEvent += SearchFocusMethod;
            MainWindow.CloseDisplayEvent += CloseDisplayMethod;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            MainWindow.SearchFocusEvent -= SearchFocusMethod;
            MainWindow.CloseDisplayEvent -= CloseDisplayMethod;
        }

    }
}
