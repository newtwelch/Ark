using System.Collections.Generic;

namespace Ark.Models.BibleLibrary
{
    public class BookData
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<ChapterData> Chapters;
    }
    public class ChapterData
    {
        public int ID { get; set; }
        public List<VerseData> Verses { get; set; }
    }
    public class VerseData
    {
        public int ID { get; set; }
        public string Text { get; set; }
    }
}
