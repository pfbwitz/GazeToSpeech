using Android.App;
using Android.Content;
using GazeToSpeech.Droid;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(CaptureHelper))]
namespace GazeToSpeech.Droid
{
    public class CaptureHelper : ICaptureHelper
    {
        public void Open(int facing)
        {
            var activity = (Activity)Forms.Context;

            var intent = new Intent(activity, typeof(DetectActivity));
            intent.PutExtra(typeof(CameraFacing).Name, facing);
            activity.StartActivityForResult(intent, 0);
        }
    }
}