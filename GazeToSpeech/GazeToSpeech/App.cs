using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

using Xamarin.Forms;

namespace GazeToSpeech
{
    public class App : Application
    {
        public static App Instance;

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
            //var cp = new ContentPage();


            //var button = new Button() { Text = "Open Camera" };
            //button.Clicked += (s, a) => cp.Navigation.PushAsync(new CapturePage());
            //cp.Content = button;
            //var nav = new NavigationPage(cp);
            //nav.Popped += (sender, args) =>
            //{
            //    var t = args;
            //};
            MainPage = GetMainPage();
        }

        public static Page GetMainPage()
        {
            switch (Device.OS)
            {
                case TargetPlatform.Android:
                    return new NavigationPage(new HomePage());
                case TargetPlatform.iOS:
                    return new HomePage();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
