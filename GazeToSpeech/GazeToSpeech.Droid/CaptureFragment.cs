//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using Android.App;
//using Android.Content;
//using Android.OS;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using GazeToSpeech.Droid.Common;
//using GazeToSpeech.Droid.Detection;
//using Java.IO;
//using OpenCV.Android;
//using OpenCV.Core;
//using OpenCV.ImgProc;
//using OpenCV.ObjDetect;

//namespace GazeToSpeech.Droid
//{
//    public class CaptureFragment : Fragment, CameraBridgeViewBase.ICvCameraViewListener2
//    {
//        #region properties

//        #region public

//        public Point PosLeft;
//        public Point PosRight;

//        public static readonly int JavaDetector = 0;
//        public static readonly int NativeDetector = 1;

//        public Mat MRgba;
//        public Mat MGray;
//        public File MCascadeFile { get; set; }
//        public File MCascadeFileEye { get; set; }
//        public CascadeClassifier MJavaDetector { get; set; }
//        public CascadeClassifier MJavaDetectorEye { get; set; }
//        public DetectionBasedTracker MNativeDetector { get; set; }
//        public DetectionBasedTracker MNativeDetectorEye { get; set; }

//        public TextView TextView1;
//        public TextView TextView2;
//        public TextView Textview3;

//        #endregion

//        #region private

//        public readonly CaptureFragment Instance;

//        private bool _fpsDetermined;

//        private int _framecount;
//        private DateTime? _start;

//        private int _framesPerSecond;

//        private readonly int _mDetectorType = JavaDetector;

//        private float _mRelativeFaceSize = 0.2f;
//        private int _mAbsoluteFaceSize = 0;

//        private CameraBridgeViewBase _mOpenCvCameraView;

//        private Callback _mLoaderCallback;



//        #endregion

//        #endregion

//        #region implementation

//        public void OnCameraViewStarted(int width, int height)
//        {
//            MGray = new Mat();
//            MRgba = new Mat();
//        }

//        public void OnCameraViewStopped()
//        {
//            MGray.Release();
//            MRgba.Release();
//        }

//        public Mat OnCameraFrame(CameraBridgeViewBase.ICvCameraViewFrame inputFrame)
//        {
//            _framecount++;
//            PosLeft = PosRight = null;

//            var faces = new MatOfRect();

//            DetermineFps();

//            MRgba = inputFrame.Rgba();
//            MGray = inputFrame.Gray();

//            if (_mAbsoluteFaceSize == 0)
//            {
//                var height = MGray.Rows();
//                if (Math.Round(height * _mRelativeFaceSize) > 0)
//                    _mAbsoluteFaceSize = Java.Lang.Math.Round(height * _mRelativeFaceSize);

//                MNativeDetector.SetMinFaceSize(_mAbsoluteFaceSize);
//            }

//            if (_mDetectorType == JavaDetector)
//            {
//                if (MJavaDetector != null)
//                    MJavaDetector.DetectMultiScale(MGray, faces, 1.1, 2, 2,
//                            new Size(_mAbsoluteFaceSize, _mAbsoluteFaceSize), new Size());
//            }
//            else if (_mDetectorType == NativeDetector && MNativeDetector != null)
//                MNativeDetector.Detect(MGray, faces);

//            var face = DetectionHelper.GetNearestFace(faces.ToArray());

//            if (face != null)
//            {
//                Imgproc.Rectangle(MRgba, face.Tl(), face.Br(), new Scalar(255, 255, 255), 3);

//                var eyeareaRight = new Rect(face.X + face.Width / 16, (int)(face.Y + (face.Height / 4.5)),
//                    (face.Width - 2 * face.Width / 16) / 2, (int)(face.Height / 3.0));
//                var eyeareaLeft = new Rect(face.X + face.Width / 16 + (face.Width - 2 * face.Width / 16) / 2,
//                    (int)(face.Y + (face.Height / 4.5)), (face.Width - 2 * face.Width / 16) / 2, (int)(face.Height / 3.0));

//                Imgproc.Rectangle(MRgba, eyeareaLeft.Tl(), eyeareaLeft.Br(), new Scalar(255, 0, 0, 255), 2);
//                Imgproc.Rectangle(MRgba, eyeareaRight.Tl(), eyeareaRight.Br(), new Scalar(255, 0, 0, 255), 2);

//                bool pupilFoundRight;
//                bool pupilFoundLeft;
//                this.DetectRightEye(MJavaDetectorEye, eyeareaRight, 24, out pupilFoundRight);
//                this.DetectLeftEye(MJavaDetectorEye, eyeareaLeft, 24, out pupilFoundLeft);
//                var eyesAreClosed = DetectionHelper.EyesAreClosed(pupilFoundLeft, pupilFoundRight);
//                var avgPos = this.GetAvgEyePoint();

//                Activity.RunOnUiThread(() => this.PutText(Textview3, "avg X: " + avgPos.X + " Y: " + avgPos.Y));

//                if (ShouldAct())
//                {

//                }
//            }
//            else
//                Activity.RunOnUiThread(() => this.PutText(new[] { TextView1, TextView2, Textview3 }, string.Empty));

//            return MRgba;
//        }

//        #endregion

//        #region overrides

//        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
//        {
//            var view =  inflater.Inflate(Resource.Layout.face_detect_surface_view, container, false);

//            _mOpenCvCameraView = view.FindViewById<CameraBridgeViewBase>(Resource.Id.fd_activity_surface_view);
//            _mOpenCvCameraView.Visibility = ViewStates.Visible;

//            _mOpenCvCameraView.SetCameraIndex(CameraFacing.Front);
//            _mOpenCvCameraView.SetCvCameraViewListener2(this);

//            TextView1 = view.FindViewById<TextView>(Resource.Id.tv1);
//            TextView2 = view.FindViewById<TextView>(Resource.Id.tv2);
//            Textview3 = view.FindViewById<TextView>(Resource.Id.tv3);

//            _mLoaderCallback = new Callback(this, _mOpenCvCameraView);

//            return view;
//        }

//        public override void OnPause()
//        {
//            base.OnPause();
//            if (_mOpenCvCameraView != null)
//                _mOpenCvCameraView.DisableView();
//        }

//        public override void OnResume()
//        {
//            base.OnResume();
//            if (!OpenCVLoader.InitDebug()) //Internal OpenCV library not found. Using OpenCV Manager for initialization
//                OpenCVLoader.InitAsync(OpenCVLoader.OpencvVersion300, Activity, _mLoaderCallback);
//            else  //OpenCV library found inside package. Using it!
//                _mLoaderCallback.OnManagerConnected(LoaderCallbackInterface.Success);
//        }

//        public override void OnDestroy()
//        {
//            base.OnDestroy();
//            _mOpenCvCameraView.DisableView();
//        }

//        #endregion

//        public CaptureFragment()
//        {
//            Instance = this;
//            var mDetectorName = new string[2];
//            mDetectorName[JavaDetector] = "Java";
//            mDetectorName[NativeDetector] = "Native (tracking)";
//        }

//        private void DetermineFps()
//        {
//            if (_framesPerSecond > 0)
//                return;

//            var now = DateTime.Now;
//            if (!_start.HasValue)
//                _start = now;
//            else if (now >= _start.Value.AddSeconds(1))
//            {
//                _framesPerSecond = _framecount;
//                _fpsDetermined = true;
//            }
//        }

//        private bool ShouldAct()
//        {
//            if (_fpsDetermined)
//            {
//                //if(_framecount % _framesPerSecond == 0)
//                if (_framecount >= _framesPerSecond)
//                {
//                    _framecount = 0;
//                    return true;
//                }
//            }
//            return false;
//        }

      
//    }
//}