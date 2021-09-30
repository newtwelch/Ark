using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Ark.Models.SongLibrary
{
    public class SongLibraryDatabase
    {

        //! ====================================================
        //! [+] SQLCONNECTION: makes connection to the SQL Database
        //! ====================================================
        public static SQLiteConnection SLD_Connection()
        {

            string relativePath = @"Databases\SongDatabase.db";
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
        public List<SongData> GetSongs()
        {
            List<SongData> list = new List<SongData>();
            using (SQLiteConnection connection = SLD_Connection())
            {
                string readString = "select * from SongLibrary";
                using (SQLiteCommand command = new SQLiteCommand(readString, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new SongData
                            {
                                ID = reader.GetInt32(0),
                                Number = reader.GetInt32(1),
                                Language = reader.GetString(2),
                                Title = reader.GetString(3),
                                Author = reader.GetString(4),
                                RawLyrics = reader.GetString(5),
                                Sequence = reader.GetString(6),
                                Tags = reader.GetString(7)
                            });
                        }
                    }
                }
            }
            return list;
        }

        //! ====================================================
        //! [+] ADD SONG: writes a new song on the database
        //! ====================================================
        public void AddSong(SongData song)
        {
            SQLiteConnection connection = SLD_Connection();
            string readString = "INSERT INTO SongLibrary (Number,Language,Title,Author,RawLyrics,Sequence) VALUES (@songNumber, @Language, @Title, @Author, @Lyrics, @Sequence);";
            SQLiteCommand command = new SQLiteCommand(readString, connection);
            command.CommandType = CommandType.Text;
            command.Parameters.AddWithValue("@songId", song.ID);
            command.Parameters.AddWithValue("@songNumber", song.Number);
            command.Parameters.AddWithValue("@Language", song.Language);
            command.Parameters.AddWithValue("@Title", song.Title);
            command.Parameters.AddWithValue("@Author", song.Author);
            command.Parameters.AddWithValue("@Lyrics", song.RawLyrics);
            command.Parameters.AddWithValue("@Sequence", song.Sequence);
            command.ExecuteNonQuery();
            connection.Close();
        }

        //! ====================================================
        //! [+] ADD SONG: writes a new song on the database
        //! ====================================================
        public void UpdateSong(SongData song)
        {
            SQLiteConnection connection = SLD_Connection();
            string readString = "UPDATE SongLibrary SET Language = @Language, Title = @Title, Author = @Author, Lyrics = @Lyrics, Sequence = @Sequence WHERE ID = @songId";
            SQLiteCommand command = new SQLiteCommand(readString, connection);
            command.CommandType = CommandType.Text;
            command.Parameters.AddWithValue("@songId", song.ID);
            command.Parameters.AddWithValue("@Language", song.Language);
            command.Parameters.AddWithValue("@Title", song.Title);
            command.Parameters.AddWithValue("@Author", song.Author);
            command.Parameters.AddWithValue("@Lyrics", song.RawLyrics);
            command.Parameters.AddWithValue("@Sequence", song.Sequence);
            command.Prepare();
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}
