using System;
using GazeToSpeech.Common.Data;
using GazeToSpeech.Common.Interface;
using GazeToSpeech.Model;
using GazeToSpeech.Pages;
using Xamarin.Forms;

namespace GazeToSpeech
{
    public class App : Application
    {
        public static App Instance;

        public static int Width;
        public static int Height;

        private static User _user;
        public static User User
        {
            get { return _user ?? (_user = QueryHelper<User>.GetOne()); }
            set { _user = value; }
        }

        public App()
        {
            Instance = this;
            Load();
        }

        public static void Reset()
        {
            Instance.Load();
        }

        public void Load()
        {
            MainPage = GetMainPage();
        }

        public static Page GetMainPage()
        {
            switch (Device.OS)
            {
                case TargetPlatform.Android:
                    return new NavigationPage(new MainPage());
                case TargetPlatform.iOS:
                    return new MainPage();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void Setup()
        {
            QueryHelper.DatabaseName = "jb_db.db";

            var helper = DependencyService.Get<ISqliteHelper>();
            if (!helper.DatabaseExists(QueryHelper.DatabaseName))
                helper.MakeDatase(QueryHelper.DatabaseName);
            helper.UpdateTables(QueryHelper.DatabaseName);
        }
    }
}
