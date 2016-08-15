using Android.App;
using Android.Content;
using GazeToSpeech.Droid;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(CaptureHelper))]
namespace GazeToSpeech.Droid
{
    public class CaptureHelper : ICaptureHelper
    {
        public void Open()
        {
            var activity = (Activity)Forms.Context;

            var intent = new Intent(activity, typeof(DetectActivity));
            activity.StartActivityForResult(intent, 0);
        }
    }
}