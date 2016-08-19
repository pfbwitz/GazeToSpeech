using Android.App;
using Android.Content;
using VocalEyes.Common.Enumeration;
using VocalEyes.Common.Interface;
using VocalEyes.Droid.Activities;
using VocalEyes.Droid.Implementation;
using Xamarin.Forms;

[assembly: Dependency(typeof(NativeOpenCvEngine))]
namespace VocalEyes.Droid.Implementation
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