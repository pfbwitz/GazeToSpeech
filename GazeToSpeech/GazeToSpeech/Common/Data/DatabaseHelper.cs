using GazeToSpeech.Common.Enumeration;
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

		    var user = new User();
		    user.CameraFacing = CameraFacing.Back.ToString();
		    user.Language = "en";

            Connection.Insert(user);
		}

		public void UpdateTables()
		{
		    Connection.CreateTable<User>();
		}
	}
}
