using System;
using System.IO;
using VocalEyes.Common.Data;
using VocalEyes.Common.Interface;
using VocalEyes.Droid.Implementation;
using SQLite.Net;
using SQLite.Net.Platform.XamarinAndroid;

[assembly: Xamarin.Forms.Dependency(typeof(SqliteHelper))]
namespace VocalEyes.Droid.Implementation
{
    public class SqliteHelper : ISqliteHelper
    {
        public bool DatabaseExists(string databaseName)
        {
            return File.Exists(GetDatabasePath(databaseName));
        }

        public void MakeDatase(string databaseName)
        {
            if (!File.Exists(GetDatabasePath(databaseName)))
            {
                File.Create(GetDatabasePath(databaseName));
                var userDb = new DatabaseHelper(GetConnection(databaseName));
                userDb.CreateTables();
            }
        }

        public void UpdateTables(string databaseName)
        {
            var userDb = new DatabaseHelper(GetConnection(databaseName));
            userDb.UpdateTables();
        }

        public SQLiteConnection GetConnection(string databaseName)
        {
            return new SQLiteConnection(new SQLitePlatformAndroid(), GetDatabasePath(databaseName));
        }

        public string GetDatabasePath(string databaseName)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), databaseName + ".db");
        }


    }
}