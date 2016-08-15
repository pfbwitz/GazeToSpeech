using Android.App;
using Android.Content;
using GazeToSpeech.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(NativeOpenCvEngine))]
namespace GazeToSpeech.Droid
{
    public class NativeOpenCvEngine : IOpenCvEngine
    {
        public void Open(int facing)
        {
            var activity = (Activity)Forms.Context;
            var intent = new Intent(activity, typeof(CaptureActivity));
            intent.PutExtra(typeof(CameraFacing).Name, facing);
            activity.StartActivityForResult(intent, 0);
        }
    }
}