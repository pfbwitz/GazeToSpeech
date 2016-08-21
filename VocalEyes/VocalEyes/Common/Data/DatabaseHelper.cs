using VocalEyes.Common.Enumeration;
using VocalEyes.Model;
using SQLite.Net;

namespace VocalEyes.Common.Data
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

		    Connection.Insert(new User
		    {
		        CameraFacing = CameraFacing.Back,
		        Language = "en"
		    });
		}

		public void UpdateTables()
		{
		    Connection.CreateTable<User>();
		}
	}
}
