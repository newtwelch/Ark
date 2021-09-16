using System.Collections.Generic;

namespace Ark.Models.SongLibrary
{
    //! ====================================================
    //! [+] SONG DATA: cool data in here
    //! ====================================================
    public class SongData
    {
        public int ID { get; set; }
        public int Number { get; set; }
        public string Language { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Sequence { get; set; }
        public string RawLyrics { get; set; }
        public List<LyricData> Lyrics { get; set; }
    }
}
