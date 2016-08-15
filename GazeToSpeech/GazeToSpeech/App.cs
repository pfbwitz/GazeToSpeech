using System;
using GazeToSpeech.Pages;
using Xamarin.Forms;

namespace GazeToSpeech
{
    public class App : Application
    {
        public static App Instance;

        public static int Width;
        public static int Height;

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
                    return new NavigationPage(new HomePage());
                case TargetPlatform.iOS:
                    return new HomePage();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
