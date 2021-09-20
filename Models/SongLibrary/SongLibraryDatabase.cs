using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;

namespace Ark.Models.SongLibrary
{
    public class SongLibraryDatabase
    {
        //! ====================================================
        //! [+] SQLCONNECTION: makes connection to the SQL Database
        //! ====================================================
        public static SqlConnection SLD_Connection()
        {
            string sld_String = Properties.Settings.Default.SongLibrary_Connection;
            SqlConnection sldb_Connection = new SqlConnection(sld_String);
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
            using (SqlConnection connection = SLD_Connection())
            {
                string readString = "select * from SongLibrary";
                using (SqlCommand command = new SqlCommand(readString, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
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
            SqlConnection connection = SLD_Connection();
            string readString = "INSERT INTO SongLibrary (Number,Language,Title,Author,RawLyrics,Sequence) VALUES (@songNumber, @Language, @Title, @Author, @Lyrics, @Sequence);";
            SqlCommand command = new SqlCommand(readString, connection);
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
    }
}
