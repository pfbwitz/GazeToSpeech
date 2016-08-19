using System.Threading.Tasks;
using Android.Content;
using Java.IO;
using Java.Lang;
using OpenCV.Android;
using OpenCV.ObjDetect;
using VocalEyes.Droid.Activities;
using File = Java.IO.File;
using IOException = Java.IO.IOException;

namespace VocalEyes.Droid.Engine
{
    internal class Callback : BaseLoaderCallback
    {
        public static bool Cancelled;

        private readonly CaptureActivity _activity;
        private readonly CameraBridgeViewBase _mOpenCvCameraView;

        public Callback(CaptureActivity activity, CameraBridgeViewBase view)
            : base(activity)
        {
            _activity = activity;
            _mOpenCvCameraView = view;
        }

        public override void OnManagerConnected(int status)
        {
            if (status == LoaderCallbackInterface.Success)
            {
                JavaSystem.LoadLibrary("detection_based_tracker");
              
                Task.Run(() =>
                {
                    try
                    {
                        var cascadeDir = _activity.GetDir("cascade", FileCreationMode.Private);
                        _activity.MCascadeFile = new File(cascadeDir, "lbpcascade_frontalface.xml");
                        _activity.MCascadeFileEye = new File(cascadeDir, "haarcascade_lefteye_2splits.xml");

                        using (var istr = _activity.Resources.OpenRawResource(Resource.Raw.lbpcascade_frontalface))
                        {
                            using (var os = new FileOutputStream(_activity.MCascadeFile))
                            {
                                int byteRead;
                                while ((byteRead = istr.ReadByte()) != -1)
                                {
                                    if (Cancelled)
                                        break;
                                    os.Write(byteRead);
                                }
                            }
                        }

                        using (var istr = _activity.Resources.OpenRawResource(Resource.Raw.haarcascade_lefteye_2splits))
                        {
                            using (var os = new FileOutputStream(_activity.MCascadeFileEye))
                            {
                                int byteRead;
                                while ((byteRead = istr.ReadByte()) != -1)
                                {
                                    if (Cancelled)
                                        break;
                                    os.Write(byteRead);
                                }
                            }
                        }

                        _activity.MJavaDetector = new CascadeClassifier(_activity.MCascadeFile.AbsolutePath);

                        if (_activity.MJavaDetector.Empty())
                        {
                            //Failed to load cascade classifier"
                            _activity.MJavaDetector = null;
                        }
                        else
                            //Loaded cascade classifier from " + _activity.MCascadeFile.AbsolutePath

                            _activity.MNativeDetector = new DetectionBasedTracker(_activity.MCascadeFile.AbsolutePath, 0);


                        _activity.MJavaDetectorEye = new CascadeClassifier(_activity.MCascadeFileEye.AbsolutePath);

                        if (_activity.MJavaDetectorEye.Empty())
                        {
                            //Failed to load cascade classifier
                            _activity.MJavaDetectorEye = null;
                        }
                        else
                            //Loaded cascade classifier from " + _activity.MCascadeFileEye.AbsolutePath

                            _activity.MNativeDetectorEye = new DetectionBasedTracker(_activity.MCascadeFile.AbsolutePath, 0);

                        cascadeDir.Delete();

                    }
                    catch (IOException e)
                    {
                        e.PrintStackTrace();
                    }
                    _activity.RunOnUiThread(() =>
                    {
                        _mOpenCvCameraView.EnableView();
                        _activity.Running = true;
                    });
                });
            }
            else
                base.OnManagerConnected(status);
        }
    }
}