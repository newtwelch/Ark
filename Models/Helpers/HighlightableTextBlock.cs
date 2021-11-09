using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Ark.Models.Helpers
{
    public class HighlightableTextBlock : TextBlock
    {
        #region Properties

        public new string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static readonly DependencyPropertyKey MatchCountPropertyKey
            = DependencyProperty.RegisterReadOnly("MatchCount", typeof(int), typeof(HighlightableTextBlock),
                new PropertyMetadata(0));

        public static readonly DependencyProperty MatchCountProperty
            = MatchCountPropertyKey.DependencyProperty;

        public static readonly DependencyProperty HighlightTextProperty =
            DependencyProperty.Register("HighlightText", typeof(string), typeof(HighlightableTextBlock),
                new PropertyMetadata(string.Empty, UpdateHighlighting));

        public new static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string),
            typeof(HighlightableTextBlock), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(UpdateHighlighting)));

        public string HighlightPhrase
        {
            get { return (string)GetValue(HighlightPhraseProperty); }
            set { SetValue(HighlightPhraseProperty, value); }
        }

        public static readonly DependencyProperty HighlightPhraseProperty =
            DependencyProperty.Register("HighlightPhrase", typeof(string),
            typeof(HighlightableTextBlock), new FrameworkPropertyMetadata(String.Empty, FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(UpdateHighlighting)));

        public Brush HighlightBrush
        {
            get { return (Brush)GetValue(HighlightBrushProperty); }
            set { SetValue(HighlightBrushProperty, value); }
        }

        public static readonly DependencyProperty HighlightBrushProperty =
            DependencyProperty.Register("HighlightBrush", typeof(Brush),
            typeof(HighlightableTextBlock), new FrameworkPropertyMetadata(Brushes.Orange, FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(UpdateHighlighting)));

        public bool IsCaseSensitive
        {
            get { return (bool)GetValue(IsCaseSensitiveProperty); }
            set { SetValue(IsCaseSensitiveProperty, value); }
        }

        public bool IgnoreDot
        {
            get { return (bool)GetValue(IsCaseSensitiveProperty); }
            set { SetValue(IsCaseSensitiveProperty, value); }
        }

        public int MatchCount
        {
            get => (int)GetValue(MatchCountProperty);
            protected set => SetValue(MatchCountPropertyKey, value);
        }

        public static readonly DependencyProperty IsCaseSensitiveProperty =
            DependencyProperty.Register("IsCaseSensitive", typeof(bool),
            typeof(HighlightableTextBlock), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(UpdateHighlighting)));

        private static void UpdateHighlighting(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ApplyHighlight(d as HighlightableTextBlock);
        }

        #endregion

        #region Members

        private static void ApplyHighlight(HighlightableTextBlock tb)
        {
            string highlightPhrase = tb.HighlightPhrase;
            string text = tb.Text;

            if (tb.IgnoreDot)
            {
                highlightPhrase = highlightPhrase.Replace(".", "");
            }

            if (tb is null)
                return;

            tb.Inlines.Clear();
            tb.SetValue(MatchCountPropertyKey, 0);
            if (tb is null || string.IsNullOrWhiteSpace(text)) return;
            if (string.IsNullOrWhiteSpace(highlightPhrase))
            {
                var completeRun = new Run(text);
                tb.Inlines.Add(completeRun);
                return;
            }

            var find = 0;
            var searchTextLength = highlightPhrase.Length;

            while (true)
            {
                var oldFind = find;
                find = text.IndexOf(highlightPhrase, find, StringComparison.InvariantCultureIgnoreCase); // returns the word index
                if (find == -1)
                {
                    tb.Inlines.Add(
                        oldFind > 0
                            ? new Run(text.Substring(oldFind, text.Length - oldFind))
                            : new Run(text));
                    break;
                }
                if (oldFind == find)
                {
                    tb.Inlines.Add(tb.GetRunForText(text.Substring(oldFind, searchTextLength), true));
                    tb.SetValue(MatchCountPropertyKey, tb.MatchCount + 1);
                    find = find + searchTextLength;
                    continue;
                }

                tb.Inlines.Add(new Run(text.Substring(oldFind, find - oldFind)));
            }
        }

        private Run GetRunForText(string text, bool isHighlighted)
        {
            var textRun = new Run(text)
            {
                Foreground = isHighlighted ? HighlightBrush : Foreground
            };
            return textRun;
        }
        #endregion
    }
}
