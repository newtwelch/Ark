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
        SongData temp_Song;
        public SongLibrary()
        {
            _viewModel = new SongLibraryViewModel();
            DataContext = _viewModel;
            InitializeComponent();
            temp_Song = new SongData();

        }

        private void EditButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            if (e.Source is Button bt)
            {
                if (bt.Name == "TagDiscardButton")
                {
                    if (TagDiscardButton.Content == "D")
                    {
                        temp_Song = new SongData();
                        _viewModel.IsEditMode = false;

                        EditButton.Content = "E";
                    }
                }
                else
                {
                    //!? ====================================================
                    //!? UPDATE FIRST: change button icon, and set the viewmodel IsEditMode boolean to true; 
                    //!? ====================================================
                    if (EditButton.Content.Equals("s"))
                    {
                        temp_Song.ID = _viewModel.SelectedSong.ID;
                        temp_Song.Number = _viewModel.SelectedSong.Number;
                        temp_Song.Language = LanguageTextBox.Text;
                        temp_Song.Title = TitleTextBox.Text;
                        temp_Song.Author = AuthorTextBox.Text;
                        temp_Song.RawLyrics = EditRawLyricsTextBox.Text;
                        temp_Song.Sequence = EditSequenceTextBox.Text;

                        _viewModel.SelectedSong = temp_Song;
                        _viewModel.UpdateSong(_viewModel.SelectedSong);
                    }

                    EditButton.Content = EditButton.Content.Equals("E") ? "s" : "E";
                    _viewModel.IsEditMode = EditButton.Content.Equals("E") ? false : true;

                    //!? ====================================================
                    //!? TAG & DISCARD BUTTON: update the button to work appropriately
                    //!? ====================================================
                    // Update the icon
                    TagDiscardButton.Content = EditButton.Content.Equals("E") ? "T" : "D";
                    // Change the BorderBrush ( which is the onhover color )
                    TagDiscardButton.BorderBrush = EditButton.Content.Equals("E") ? (Brush)Application.Current.Resources["ThumbBrush"] : (Brush)Application.Current.Resources["RedBrush"];
                    // Change the tooltip appropriately
                    TagDiscardButton.ToolTip = EditButton.Content.Equals("E") ? "Edit Tags of this song" : "Discard changes made";
                }
            }

        }

        private void RawEdit_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            temp_Song.Sequence = EditSequenceTextBox.Text;
            temp_Song.Lyrics = new List<LyricData>(_viewModel.ParseLyrics(EditRawLyricsTextBox.Text, temp_Song.Sequence));

            _viewModel.Lyrics.Clear();
            foreach (LyricData lyric in temp_Song.Lyrics)
            {
                _viewModel.Lyrics.Add(lyric);
            }

            e.Handled = true;
        }

        public String ToRawLyrics(List<LyricData> lyrics)
        {
            temp_Song.RawLyrics = string.Empty;

            foreach (LyricData lyric in lyrics)
            {
                if (lyric.Type == LyricType.Stanza)
                {
                    temp_Song.RawLyrics += lyric.Line.Equals("1") ? $"{ lyric.Text}\r\n" : $"\r\n{ lyric.Text}\r\n";
                    temp_Song.Sequence += $"{lyric.Line},";

                }
                if (lyric.Type == LyricType.Chorus)
                {
                    if (!temp_Song.RawLyrics.Contains("CHORUS", StringComparison.OrdinalIgnoreCase))
                    {
                        temp_Song.RawLyrics += $"\r\nCHORUS\r\n{lyric.Text} \r\n";
                        temp_Song.Sequence += "C,";
                    }
                }
                if (lyric.Type == LyricType.Bridge)
                {
                    temp_Song.RawLyrics += $"\r\nBRIDGE\r\n{lyric.Text} \r\n";
                    temp_Song.Sequence += "B,";
                }
            }
            return temp_Song.RawLyrics.TrimEnd();
        }

        private void SongLyricListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (e.Source is ListBox lb)
            {
                if (temp_Song.Lyrics == null)
                    temp_Song.Lyrics = new List<LyricData>();

                List<LyricData> lyrics = new List<LyricData>();

                temp_Song.Lyrics.Clear();
                foreach (LyricData item in lb.Items)
                {
                    lyrics.Add(item);
                    temp_Song.Lyrics.Add(item);
                }

                EditRawLyricsTextBox.Text = ToRawLyrics(lyrics);
            }
        }
    }
}
