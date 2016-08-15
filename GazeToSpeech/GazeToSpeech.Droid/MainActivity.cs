using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.OS;
using Android.Util;

namespace GazeToSpeech.Droid
{
    [Activity(Label = "GazeToSpeech", Icon = "@drawable/icon", MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            Window.AddFlags(WindowManagerFlags.Fullscreen);

            DisplayMetrics metrics = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetMetrics(metrics);

            App.Height = metrics.HeightPixels;
            App.Width = metrics.WidthPixels;

            Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }
    }
}

