using System;
using System.Collections.Generic;
using System.Linq;
using GazeToSpeech.Common.Interface;
using GazeToSpeech.Model;
using SQLite.Net;
using Xamarin.Forms;

namespace GazeToSpeech.Common.Data
{
    public static class QueryHelper
    {
        public static string DatabaseName { get; set; }
    }

	public static class QueryHelper<T> where T : BaseModel
	{
	    private static SQLiteConnection Connection
	    {
	        get { return DependencyService.Get<ISqliteHelper>().GetConnection(QueryHelper.DatabaseName); }
	    }

		private static void CheckDatabaseName()
		{
            if (string.IsNullOrEmpty(QueryHelper.DatabaseName))
				throw new Exception("Databasename empty!");
		}

		public static List<T> GetAll()
		{
		    CheckDatabaseName();
            using (var connection = Connection)
				return connection.Table<T>().ToList();
		}

        public static void Insert(T entity)
        {
            CheckDatabaseName();
            using (var connection = Connection)
                connection.Insert(entity);
        }

		public static void InsertOrReplace(T entity)
		{
            CheckDatabaseName();
            using (var connection = Connection)
				connection.InsertOrReplace(entity);
		}

	    public static void Update(T entity)
	    {
	        CheckDatabaseName();
	        using (var connection = Connection)
	            connection.Update(entity);
	    }

		public static void Delete(T entity)
		{
            CheckDatabaseName();
            using (var connection = Connection)
				connection.Delete<T>(entity.Id);
		}

        public static void Delete(int id)
        {
            CheckDatabaseName();
            using (var connection = Connection)
                connection.Delete<T>(id);
        }

		public static T GetOneById(int id)
		{
            CheckDatabaseName();
            using (var connection = Connection)
				return connection.Table<T>().Single(e => e.Id == id);
		}

		public static T GetOne()
		{
            CheckDatabaseName();
            using (var connection = Connection)
				return connection.Table<T>().Single();
		}
	}
}
