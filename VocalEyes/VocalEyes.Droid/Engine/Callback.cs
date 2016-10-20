using System.Threading.Tasks;
using Android.Content;
using Android.Views;
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
        private readonly CaptureActivity _activity;
        private readonly CameraBridgeViewBase _mOpenCvCameraView;

        public bool Cancelling;

        public Callback(CaptureActivity activity, CameraBridgeViewBase view)
            : base(activity)
        {
            _activity = activity;
            _mOpenCvCameraView = view;
        }

        public override void OnManagerConnected(int status)
        {
            _activity.RunOnUiThread(
                           () => _activity.Load1.Text = "Initializing face detection library STATUS: LOADING");

            _activity.RunOnUiThread(
                         () => _activity.Load2.Text = "Initializing eye detection library STATUS: LOADING");

            //_activity.RunOnUiThread(
            //             () => _activity.Load3.Text = "Initializing nose detection library STATUS: LOADING");

            if (status == LoaderCallbackInterface.Success)
            {
                JavaSystem.LoadLibrary("detection_based_tracker");
              
                Task.Run(() =>
                {
                    if (_activity.IsFinishing)
                        return;

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
                                    if (Cancelling)
                                        break;
                                     os.Write(byteRead);
                                }
                            }
                        }

                        _activity.RunOnUiThread(
                            () => _activity.Load1.Text = "Initializing face detection library STATUS: DONE");


                        if (_activity.IsFinishing)
                            return;

                        using (var istr = _activity.Resources.OpenRawResource(Resource.Raw.haarcascade_lefteye_2splits))
                        {
                            using (var os = new FileOutputStream(_activity.MCascadeFileEye))
                            {
                                int byteRead;
                                while ((byteRead = istr.ReadByte()) != -1)
                                {
                                    if (Cancelling)
                                        break;
                                    os.Write(byteRead);
                                }
                            }
                        }

                        if (_activity.IsFinishing)
                            return;


                        if (_activity.IsFinishing)
                            return;


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

                            _activity.MNativeDetectorEye = new DetectionBasedTracker(
                                _activity.MCascadeFile.AbsolutePath, 0);

                        cascadeDir.Delete();

                        _activity.RunOnUiThread(
                           () => _activity.Load2.Text = "Initializing eye detection library STATUS: DONE");
                    }
                    catch (WindowManagerBadTokenException)
                    {
                    }
                    catch (IOException ex)
                    {
                        _activity.RunOnUiThread(() => _activity.HandleException(ex));
                    }
                    catch (Exception ex)
                    {
                        _activity.RunOnUiThread(() => _activity.HandleException(ex));
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