using Android.Content;
using Java.IO;
using Java.Lang;
using OpenCV.Android;
using OpenCV.ObjDetect;

namespace GazeToSpeech.Droid.Detection
{
    internal class Callback : BaseLoaderCallback
    {
        private readonly DetectActivity _activity;
        private readonly CameraBridgeViewBase mOpenCvCameraView;
        public Callback(DetectActivity activity, CameraBridgeViewBase view)
            : base(activity)
        {
            _activity = activity;
            mOpenCvCameraView = view;
        }

        public override void OnManagerConnected(int status)
        {
            if (status == LoaderCallbackInterface.Success)
            {
                JavaSystem.LoadLibrary("detection_based_tracker");
                try
                {
                    File cascadeDir;
                    using (var istr = _activity.Resources.OpenRawResource(Resource.Raw.lbpcascade_frontalface))
                    {
                        cascadeDir = _activity.GetDir("cascade", FileCreationMode.Private);
                        _activity.MCascadeFile = new File(cascadeDir, "lbpcascade_frontalface.xml");

                        using (FileOutputStream os = new FileOutputStream(_activity.MCascadeFile))
                        {
                            int byteRead;
                            while ((byteRead = istr.ReadByte()) != -1)
                            {
                                os.Write(byteRead);
                            }
                        }
                    }

                    using (var istr = _activity.Resources.OpenRawResource(Resource.Raw.haarcascade_lefteye_2splits))
                    {
                        cascadeDir = _activity.GetDir("cascade", FileCreationMode.Private);
                        _activity.MCascadeFileEye = new File(cascadeDir, "haarcascade_lefteye_2splits.xml");

                        using (FileOutputStream os = new FileOutputStream(_activity.MCascadeFileEye))
                        {
                            int byteRead;
                            while ((byteRead = istr.ReadByte()) != -1)
                                os.Write(byteRead);
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
                mOpenCvCameraView.EnableView();
            }
            else
                base.OnManagerConnected(status);
        }
    }
}