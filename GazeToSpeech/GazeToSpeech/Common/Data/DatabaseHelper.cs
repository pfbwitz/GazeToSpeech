using GazeToSpeech.Model;
using SQLite.Net;

namespace GazeToSpeech.Common.Data
{
	public class DatabaseHelper
	{
	    public SQLiteConnection Connection;

        public DatabaseHelper(SQLiteConnection connection)
        {
            Connection = connection;
        }

		public void CreateTables()
		{
		    Connection.DropTable<User>();
			UpdateTables();

            Connection.Insert(new User());
		}

		public void UpdateTables()
		{
		    Connection.CreateTable<User>();
		}
	}
}
