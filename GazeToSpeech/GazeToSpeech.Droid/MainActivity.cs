using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.OS;

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

            Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }

        //public override bool OnOptionsItemSelected(IMenuItem item)
        //{
         
        //    if (item.ItemId == 16908332)
        //    {
        //        App.Reset();
        //        return true;
        //    }

        //    return base.OnOptionsItemSelected(item);
        //}
    }
}

