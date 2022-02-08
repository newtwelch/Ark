using System;

namespace Ark.Models.SongLibrary
{

    //! ====================================================
    //! [+] LYRIC DATA: contains a line, type and text
    //! ====================================================
    public class LyricData
    {
        public string Line { get; set; }
        public string Text { get; set; }
        public LyricType Type { get; set; }
    }

    //! ====================================================
    //! [+] ENUMS: three types of lyrics
    //! ====================================================
    public enum LyricType { Bridge, Stanza, Chorus }
    
}
