using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.OS;
using Android.Util;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace GazeToSpeech.Droid
{
    [Activity(Label = "GazeToSpeech", Icon = "@drawable/icon", Theme = "@style/MyTheme", 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var metrics = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetMetrics(metrics);

            App.Height = metrics.HeightPixels;
            App.Width = metrics.WidthPixels;

            Forms.Init(this, bundle);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                Window.SetStatusBarColor(Color.FromHex("004a87").ToAndroid());
            }

            LoadApplication(new App());
        }
    }
}

