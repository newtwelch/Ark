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
        //! ViewModel
        private SongLibraryViewModel _viewModel;

        //! Store Song [ for editing ]
        private SongData save_Song;

        //! Variables
        bool fromHistory;
        bool isAddingASong;
        int storeSongIndex;

        //! ====================================================
        //! [+] SONG LIBRARY: initialize stuff here
        //! ====================================================
        public SongLibrary()
        {
            _viewModel = new SongLibraryViewModel();
            DataContext = _viewModel;
            InitializeComponent();

            //!? EVENTS
            HistoryViewModel.HistoryObjectSelected += HistoryObjectSelectedMethod;
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
                            //!? Save a copy of the song for discarding changes
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
                            //!? Save the changes to the song
                            _viewModel.SelectedSong.RawLyrics = EditRawLyricsTextBox.Text;
                            _viewModel.SelectedSong.Sequence = EditSequenceTextBox.Text;
                            _viewModel.UpdateSong(_viewModel.SelectedSong);
                            isAddingASong = false;
                            break;
                    }

                    //!? Call the Edit Mode Setup
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
            if (!_viewModel.IsEditMode) return;

            //!? Refresh, Parse Lyrics using text from RawLyricTextBox and SequenceTextBox
            _viewModel.Lyrics.Clear();
            _viewModel.ParseLyrics(EditRawLyricsTextBox.Text, EditSequenceTextBox.Text).ForEach(x => _viewModel.Lyrics.Add(x));
        }

        //! ====================================================
        //! [+] SONG LYRIC LISTBOX LOST FOCUS: update rawlyrics when lyric listbox loses focus
        //! ====================================================
        private void SongLyricListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.IsEditMode) return;

            //!? Update the RawLyrics when LostFocus 
            List<LyricData> lyrics = new List<LyricData>();
            SongLyricListBox.Items.Cast<LyricData>().ToList().ForEach(x => lyrics.Add(x));

            EditRawLyricsTextBox.Text = ToRawLyrics(lyrics);
        }

        //! ====================================================
        //! [+] LIST BOX KEY DOWN: hotkeys for listbox item navigation
        //! ====================================================
        private void ListBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (_viewModel.IsEditMode) return;

            ListBox? lb = sender as ListBox;
            switch (lb.Name)
            {
                //!? ====================[ SONG LYRICS ]==================
                case "SongLyricListBox":

                    //!? ====================================================
                    //!? Selected the Stanza Number according to the Number pressed down
                    //!? ====================================================
                    if (e.Key > Key.D0 && e.Key <= Key.D9 || e.Key > Key.NumPad0 && e.Key <= Key.NumPad9)
                    {
                        int lindex = lb.Items.Cast<LyricData>().ToList().FindIndex(x => x.Line == e.Key.ToString().Replace("D", "").Replace("NumPad", ""));
                        lb.SelectedIndex = lindex;
                        e.Handled = true;
                    }
                    //!? If 0 is pressed, then get the chorus
                    else if (e.Key == Key.D0 || e.Key == Key.NumPad0 && _viewModel.Lyrics.Any(x => x.Line == "C"))
                    {
                        while (true)
                        {
                            lb.SelectedIndex++;

                            LyricData selectedLyric = (LyricData)lb.SelectedItem;
                            if (selectedLyric?.Line == "C")
                                break;
                        }

                        e.Handled = true;
                    }

                    if (lb.SelectedItem is null && e.Key == Key.Enter)
                        lb.SelectedIndex = storeSongIndex;

                    FocusListBoxItem(lb);
                    break;

                //!? ====================[ SONG LIST ]==================
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
            TextBox tb = (TextBox)sender;
            if (tb.Name == "SongSearchTextBox" && e.Key == Key.Enter)
            {
                SongListBox.SelectedIndex = 0;
                FocusListBoxItem(SongListBox);
            }
        }

        //? =============================[METHODS & HELPERS]==============================

        //! ====================================================
        //! [+] FOCUS LIST BOX ITEM
        //! ====================================================
        void FocusListBoxItem(ListBox lb)
        {
            if (lb.SelectedItem is null)
                return;

            lb.UpdateLayout();
            lb.ScrollIntoView(lb.SelectedItem);

            ListBoxItem? lbi = lb.ItemContainerGenerator.ContainerFromItem(lb.SelectedItem) as ListBoxItem;
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
            //!? ====================[ EDIT MODE ]==================
            _viewModel.IsEditMode = _viewModel.IsEditMode ? false : true;
            bool isEditMode = _viewModel.IsEditMode;

            //!? ====================[ EDIT BUTTON ]==================
            EditButton.Content = isEditMode ? "s" : "E";
            EditButton.ToolTip = isEditMode ? "Save changes made to this song" : "Edit the selected song";

            //!? ====================[ TAG BUTTON ]==================
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
                //!? Add string according to type
                //!? If it's a Chorus, Add if it isn't there yet

                if (lyric.Type == LyricType.Stanza)
                    //!? If first staza, don't add white line at the start
                    _rawLyrics += lyric.Line.Equals("1") ? $"{ lyric.Text}\r\n" : $"\r\n{ lyric.Text}\r\n";
                else if (lyric.Type == LyricType.Chorus && !_rawLyrics.Contains("CHORUS", StringComparison.OrdinalIgnoreCase))
                    _rawLyrics += $"\r\nCHORUS\r\n{lyric.Text} \r\n";
                else if (lyric.Type == LyricType.Bridge)
                    _rawLyrics += $"\r\nBRIDGE\r\n{lyric.Text} \r\n";
            }
            //!? Trim the end for unwanted white lines;
            return _rawLyrics.TrimEnd();
        }

        //! ====================================================
        //! [+] SEARCH FOCUS METHOD: hotkey stuff
        //! ====================================================
        public void SearchFocusMethod()
        {
            SongSearchTextBox.Clear();
            SongSearchTextBox.Focus();
        }

        //! ====================================================
        //! [+] ADD SONG: adds song to database
        //! ====================================================
        private void HistoryObjectSelectedMethod(object o)
        {
            if (o is not SongData) return;

            _viewModel.SelectedSong = _viewModel.Songs.ToList().Find(x => x.ID == ((SongData)o).ID);

            SongSearchTextBox.Clear();
            FocusListBoxItem(SongListBox);
            fromHistory = true;
        }

        //! ====================================================
        //! [+] CLOSE DISPLAY
        //! ====================================================
        public void CloseDisplayMethod()
        {
            storeSongIndex = SongLyricListBox.SelectedIndex;
            _viewModel.SelectedLyric = null;
            SongLyricListBox.Focus();
        }

        //? =============================[LOADED & UNLOADED]==============================

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!fromHistory)
                SongSearchTextBox.Focus();

            MainWindow.SearchFocusEvent += SearchFocusMethod;
            MainWindow.CloseDisplayEvent += CloseDisplayMethod;

            fromHistory = false;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            MainWindow.SearchFocusEvent -= SearchFocusMethod;
            MainWindow.CloseDisplayEvent -= CloseDisplayMethod;
        }

    }
}
