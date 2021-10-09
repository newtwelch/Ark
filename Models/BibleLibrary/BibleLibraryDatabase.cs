using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Ark.Models.BibleLibrary
{
    public class BibleLibraryDatabase
    {

        //! ====================================================
        //! [+] SQLCONNECTION: makes connection to the SQL Database
        //! ====================================================
        public static SQLiteConnection SLD_Connection()
        {
            string relativePath = @"Databases\BibleDatabase.db";
            string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
            string ConnectionString = string.Format("Data Source={0};Version=3;", absolutePath);
            SQLiteConnection sldb_Connection = new SQLiteConnection(ConnectionString, true);
            if (sldb_Connection.State != ConnectionState.Open)
                sldb_Connection.Open();

            return sldb_Connection;
        }

        //! ====================================================
        //! [+] GET SONGS: returns a list
        //! ====================================================
        public List<BookData> GetBooks(string Language)
        {
            List<BookData> books = new List<BookData>();    

            using (SQLiteConnection connection = SLD_Connection())
            {
                string readString = "select * from key_english";
                using (SQLiteCommand command = new SQLiteCommand(readString, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            books.Add(new BookData
                            {
                                ID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Chapters = new List<ChapterData>()
                            });
                        }
                    }
                }

                string readString1 = "select * from " + Language;
                using (SQLiteCommand command = new SQLiteCommand(readString1, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int chapterID = reader.GetInt32(2);
                            int verseID = reader.GetInt32(3);
                            string verseText = reader.GetString(4);

                            BookData matchedBook = books.Find(x => x.ID == reader.GetInt32(1));

                            if (matchedBook.Chapters.Count < chapterID)
                                matchedBook.Chapters.Add(new ChapterData()
                                {
                                    ID = chapterID,
                                    Verses = new List<VerseData>()
                                });

                            ChapterData matchedChapter = matchedBook.Chapters.Find(x => x.ID == chapterID);

                            matchedChapter.Verses.Add(new VerseData()
                            {
                                ID = verseID,
                                Text = verseText
                            });
                        }
                    }
                }
            }
            return books;
        }
    }
}
