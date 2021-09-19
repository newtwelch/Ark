using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;

namespace Ark.Models.SongLibrary
{
    public class SongLibraryDatabase
    {
        //TODO ============================================
        //TODO FIX ALL OF THIS MESS!!!
        //TODO ============================================

        public static SqlConnection SLD_Connection()
        {
            string sld_String = Properties.Settings.Default.SongLibrary_Connection;
            SqlConnection sldb_Connection = new SqlConnection(sld_String);
            if (sldb_Connection.State != ConnectionState.Open)
                sldb_Connection.Open();

            return sldb_Connection;
        }

        public SongLibraryDatabase()
        {

        }

        //reads
        public List<SongData> GetSongs()
        {
            List<SongData> list = new List<SongData>();
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.SongLibrary_Connection))
            {
                connection.Open();
                string readString = "select * from SongLibrary ORDER BY Title ASC";
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

        public List<SongData> GetSongsByTitle(string Title)
        {
            List<SongData> list = new List<SongData>();
            using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.SongLibrary_Connection))
            {
                connection.Open();
                string readString = $"SELECT * FROM SongLibrary WHERE lower(Title) LIKE lower('%{Title}%')";
                using (SqlCommand command = new SqlCommand(readString, connection))
                {
                    command.Prepare();
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
                    connection.Close();
                }
            }
            return list;
        }

        //writes

        public void AddSong(SongData song)
        {
            SqlConnection connection = SLD_Connection();
            string readString = "INSERT INTO SongLibrary (Number,Language,Title,Author,RawLyrics,Sequence) VALUES (@songNum, @Language, @Title, @Author, @Lyrics, @Sequence);";
            SqlCommand command = new SqlCommand(readString, connection);
            command.CommandType = CommandType.Text;
            command.Parameters.AddWithValue("@songId", song.ID);
            command.Parameters.AddWithValue("@songNum", song.Number);
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
