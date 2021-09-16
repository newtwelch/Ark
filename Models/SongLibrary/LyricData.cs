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

    //! ====================================================
    //! [+] ENUM CONVERTER: make everything easier for the Enums
    //! ====================================================
    class EnumConverter
    {
        public static string SongEnumToString(LyricType e)
        {
            return e switch
            {
                LyricType.Bridge => "Bridge",
                LyricType.Chorus => "Chorus",
                LyricType.Stanza => "Stanza",
                _ => throw new ArgumentNullException()
            };
        }

        public static LyricType StringToSongEnum(string s)
        {
            return s switch
            {
                "Bridge" => LyricType.Bridge,
                "Chorus" => LyricType.Chorus,
                "Stanza" => LyricType.Stanza,
                "bridge" => LyricType.Bridge,
                "chorus" => LyricType.Chorus,
                "stanza" => LyricType.Stanza,
                _ => throw new ArgumentException()
            };
        }
    }
}
