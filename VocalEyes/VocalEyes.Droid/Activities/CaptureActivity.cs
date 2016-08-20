using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.IO;
using OpenCV.Android;
using OpenCV.Core;
using OpenCV.ImgProc;
using OpenCV.ObjDetect;
using VocalEyes.Common;
using VocalEyes.Common.Enumeration;
using VocalEyes.Common.Utils;
using VocalEyes.Droid.Common.Helper;
using VocalEyes.Droid.Common.Model;
using VocalEyes.Droid.Engine;
using Point = OpenCV.Core.Point;
using Size = OpenCV.Core.Size;

namespace VocalEyes.Droid.Activities
{
    /// <summary>
    /// App name:       Vocal Eyes
    /// Description:    Mobile application for communication of ALS patients
    ///                 or other disabled individuals, based on the Becker Vocal 
    ///                 Eyes communication by Gary Becker (father of Jason Becker)
    /// Author:         Peter Brachwitz
    /// Last Update:    August 18 2016
    /// </summary>
    [Activity(Label = "Pupil tracking", ConfigurationChanges = ConfigChanges.Orientation,
        Icon = "@drawable/icon",
        ScreenOrientation = ScreenOrientation.Landscape)]
    public class CaptureActivity : Activity, CameraBridgeViewBase.ICvCameraViewListener2
    {
        public CaptureActivity()
        {
            Calibrating = true;
            CreateGridSubSet();
            _captureMethod = CaptureMethod.Subset;
            _detectionHelper = new DetectionHelper(this);
            TextToSpeechHelper = new TextToSpeechHelper();
            var mDetectorName = new string[2];
            mDetectorName[JavaDetector] = "Java";
            mDetectorName[NativeDetector] = "Native (tracking)";
        }

        #region properties

            #region public

        public bool Running;

        public Direction Direction;

        public int FramesPerSecond;

        public bool Calibrating;

        public int Facing;

        public bool Speaking;
        public Rect AvgLeftEye;
        public Rect AvgRightEye;
        public List<Rect> RightRectCaptures = new List<Rect>();
        public List<Rect> LeftRectCaptures = new List<Rect>();

        public TextToSpeechHelper TextToSpeechHelper;

        public List<string> CharacterBuffer = new List<string>();

        public Point PosLeft;
        public Point PosRight;

        public static readonly int JavaDetector = 0;
        public static readonly int NativeDetector = 1;

        public Mat MRgba;
        public Mat MGray;
        public File MCascadeFile { get; set; }
        public File MCascadeFileEye { get; set; }
        public CascadeClassifier MJavaDetector { get; set; }
        public CascadeClassifier MJavaDetectorEye { get; set; }
        public DetectionBasedTracker MNativeDetector { get; set; }
        public DetectionBasedTracker MNativeDetectorEye { get; set; }

        public TextView TextViewTimer;
        public Stopwatch Stopwatch = new Stopwatch();

            #endregion

            #region private

        private EyeArea _leftEyeArea;
        private EyeArea _rightEyeArea;

        private ProgressDialog _progress;

        private readonly DetectionHelper _detectionHelper;

        private DateTime _calibrationStart;

        private bool _readyToCapture;

        private CaptureMethod _captureMethod;
        private Subset _currentSubset;
        public List<Subset> SubSets;

        private bool _handling;

        private bool _fpsDetermined;

        private int _framecount;
        private DateTime? _start;

        private readonly int _mDetectorType = JavaDetector;

        private float _mRelativeFaceSize = 0.2f;
        private int _mAbsoluteFaceSize = 0;

        private CameraBridgeViewBase _mOpenCvCameraView;

        private Callback _mLoaderCallback;

            #endregion

        #endregion

        #region overrides

        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            Window.AddFlags(WindowManagerFlags.Fullscreen);

            SetContentView(Resource.Layout.face_detect_surface_view);

            _mOpenCvCameraView = FindViewById<CameraBridgeViewBase>(Resource.Id.fd_activity_surface_view);
            _mOpenCvCameraView.Visibility = ViewStates.Visible;

            Stopwatch.Start();
            TextViewTimer = FindViewById<TextView>(Resource.Id.tv1);

            Facing = Intent.GetIntExtra(typeof(CameraFacing).Name, CameraFacing.Front);
            _mOpenCvCameraView.SetCameraIndex(Facing);
            _mOpenCvCameraView.SetCvCameraViewListener2(this);

            _mLoaderCallback = new Callback(this, _mOpenCvCameraView);
            _progress = new ProgressDialog(this) {Indeterminate = true};
            _progress.SetCancelable(true);
            _progress.SetProgressStyle(ProgressDialogStyle.Spinner);
            _progress.SetMessage(SpeechHelper.InitMessage);
            _progress.Show();
            _progress.CancelEvent += (sender, args) => Finish();

            Task.Run((Func<Task>) Start);
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (_mOpenCvCameraView != null)
                _mOpenCvCameraView.DisableView();
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (!OpenCVLoader.InitDebug()) //Internal OpenCV library not found. Using OpenCV Manager for initialization
                OpenCVLoader.InitAsync(OpenCVLoader.OpencvVersion300, this, _mLoaderCallback);
            else  //OpenCV library found inside package. Using it!
                _mLoaderCallback.OnManagerConnected(LoaderCallbackInterface.Success);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _mOpenCvCameraView.DisableView();
        }

        #endregion

        #region ICvCameraViewListener2 implementation

        public void OnCameraViewStarted(int width, int height)
        {
            MGray = new Mat();
            MRgba = new Mat();
        }

        public void OnCameraViewStopped()
        {
            MGray.Release();
            MRgba.Release();
        }

        public Mat OnCameraFrame(CameraBridgeViewBase.ICvCameraViewFrame inputFrame)
        {
            MRgba = inputFrame.Rgba();
            MGray = inputFrame.Gray();

            if (!Running)
                return MRgba;

            _framecount++;
            PosLeft = PosRight = null;

            var faces = new MatOfRect();

            DetermineFps();

            if (!_fpsDetermined || !_readyToCapture)
                return MRgba;

            if(_leftEyeArea == null)
                _leftEyeArea = new EyeArea(Constants.AreaSkip);

            if(_rightEyeArea == null)
                _rightEyeArea = new EyeArea(Constants.AreaSkip);

            if (Calibrating)
            {
                var passed = (DateTime.Now - _calibrationStart).Seconds.ToString();
                this.PutOutlinedText(passed, 10, 10);
            }

            if (_mAbsoluteFaceSize == 0)
            {
                var height = MGray.Rows();
                if (Math.Round(height * _mRelativeFaceSize) > 0)
                    _mAbsoluteFaceSize = Java.Lang.Math.Round(height * _mRelativeFaceSize);

                MNativeDetector.SetMinFaceSize(_mAbsoluteFaceSize);
            }

            if (_mDetectorType == JavaDetector)
            {
                if (MJavaDetector != null)
                    MJavaDetector.DetectMultiScale(MGray, faces, 1.1, 2, 2,
                            new Size(_mAbsoluteFaceSize, _mAbsoluteFaceSize), new Size());
            }
            else if (_mDetectorType == NativeDetector && MNativeDetector != null)
                MNativeDetector.Detect(MGray, faces);

            var face = _detectionHelper.GetNearestFace(faces.ToArray());

            if (face != null)
            {
                var eyeareaLeft = _leftEyeArea.Insert(new Rect(face.X + face.Width / 16 + (face.Width - 2 * face.Width / 16) / 2,
                    (int)(face.Y + (face.Height / 4.5)), (face.Width - 2 * face.Width / 16) / 2, (int)(face.Height / 3.0))).GetShape();
                var eyeareaRight = _rightEyeArea.Insert(new Rect(face.X + face.Width / 16, (int)(face.Y + (face.Height / 4.5)),
                    (face.Width - 2 * face.Width / 16) / 2, (int)(face.Height / 3.0))).GetShape();

                Imgproc.Rectangle(MRgba, eyeareaLeft.Tl(), eyeareaLeft.Br(), new Scalar(255, 0, 0, 255), 2);
                Imgproc.Rectangle(MRgba, eyeareaRight.Tl(), eyeareaRight.Br(), new Scalar(255, 0, 0, 255), 2);

                _detectionHelper.DetectRightEye( MJavaDetectorEye, eyeareaRight, face, 24);
                _detectionHelper.DetectLeftEye( MJavaDetectorEye, eyeareaLeft, face, 24);
                var position = PosLeft;

                if (position == null)
                    return MRgba;

                if(_detectionHelper.CenterPoint != null)
                    PopulateGrid(_detectionHelper.GetDirection(position));

                if (ShouldAct())
                    RunOnUiThread(() => HandleEyePosition(position));
            }
            return MRgba;
        }
        #endregion

        #region custom methods

        private async Task Start()
        {
            try
            {
                while (!Running)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    RunOnUiThread(() => TextViewTimer.Text = Stopwatch.Elapsed.ToString(@"m\:ss"));
                }

                RunOnUiThread(() =>
                {
                    TextViewTimer.Text = string.Empty;
                    _progress.Dismiss();
                    Stopwatch.Stop();
                    FindViewById<LinearLayout>(Resource.Id.l1).Visibility = ViewStates.Gone;
                });

                await Task.Run(() =>
                {
                    RunOnUiThread(() =>
                    {
                        try
                        {
                            TextToSpeechHelper.Speak(SpeechHelper.CalibrationInit);
                            var builder = new AlertDialog.Builder(this);
                            builder.SetMessage(SpeechHelper.CalibrationInit);
                            builder.SetPositiveButton("OK", (a, s) =>
                            {
                                TextToSpeechHelper.CancelSpeak();
                                _calibrationStart = DateTime.Now;
                                _readyToCapture = true;
                            });
                            if (!IsFinishing)
                                builder.Show();
                        }
                        catch (WindowManagerBadTokenException) { }
                    });
                });
            }
            catch (WindowManagerBadTokenException)
            {
            }
        }

        /// <summary>
        /// Draw the full character-grid on screen
        /// </summary>
        private void PopulateGrid(Direction direction)
        {    
            if (_captureMethod == CaptureMethod.Subset)
            {
                var rect = new Rect(0, 0, MRgba.Width() / 3, MRgba.Height() / 2);
                switch (direction)
                {
                    case Direction.TopLeft:
                        rect.X = 0;
                        break;
                    case Direction.BottomLeft:
                        rect.Y = MRgba.Height()/2;
                        break;
                    case Direction.TopRight:
                        rect.X = (MRgba.Width()/3)*2;
                        break;
                    case Direction.BottomRight:
                        rect.X = (MRgba.Width() / 3) * 2;
                        rect.Y = MRgba.Height() / 2;
                        break;
                    case Direction.Top:
                        rect.X = MRgba.Width() / 3;
                        break;
                    case Direction.Bottom:
                        rect.X = MRgba.Width()/3;
                        rect.Y = MRgba.Height() / 2;
                        break;
                }

                Imgproc.Rectangle(MRgba, rect.Tl(), rect.Br(), new Scalar(255, 0, 0), 3);

               // var overlay = new Mat();
               // MRgba.CopyTo(overlay);

               //Imgproc.Rectangle(overlay, rect.Tl(), rect.Br(), new Scalar(255, 0, 0, 0.1), -1);

               //Core.AddWeighted(overlay, 0.4, MRgba, 1 - 0.4, 0, MRgba);
            }
        }

       

        /// <summary>
        /// Create the grid of character-areas, so that the position of the pupil can be mapped to 
        /// a subset of characters. Subsequently, the next measured position can be mapped to a single 
        /// letter in the subset or the end of a word
        /// </summary>
        private void CreateGridSubSet()
        {
            SubSets = new List<Subset>
            {
                new Subset{ Direction = Direction.TopLeft, Partition = SubsetPartition.Abcd },
                new Subset{ Direction = Direction.Top, Partition = SubsetPartition.Efgh },
                new Subset{ Direction = Direction.TopRight, Partition = SubsetPartition.Ijkl },
                new Subset{ Direction = Direction.BottomLeft, Partition = SubsetPartition.Mnop },
                new Subset{ Direction = Direction.Bottom, Partition = SubsetPartition.Qrst },
                new Subset{ Direction = Direction.BottomRight, Partition = SubsetPartition.Uvwxyz }
            };
        }

       

        /// <summary>
        /// Handle calculated position of the pupil compared to the surface of the entire eye
        /// </summary>
        /// <param name="position"></param>
        public void HandleEyePosition(Point position)
        {
            try
            {
                TextToSpeechHelper.CancelSpeak();

                if (position == null || _handling)
                    return;

                _handling = true;

                var direction = _detectionHelper.GetDirection(position);

                TextToSpeechHelper.Speak(direction.ToString());

                var emptyDirection = direction == Direction.Left || direction == Direction.Right ||
                                     direction == Direction.Center;

                if (_captureMethod == CaptureMethod.Subset && !emptyDirection)
                {
                    _currentSubset = SubSets.SingleOrDefault(s => s.Direction == direction);
                    if (_currentSubset == null)
                        return;

                    TextToSpeechHelper.Speak(direction.ToString());
                    _captureMethod = CaptureMethod.Character;
                }
                else if (_captureMethod == CaptureMethod.Character)
                {
                    var character = _currentSubset.GetCharacter(direction);
                    CharacterBuffer.Add(character);
                }
                else
                {
                    TextToSpeechHelper.Speak(string.Join("", CharacterBuffer));
                    CharacterBuffer.Clear();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                _handling = false;
            }
        }

        public void HandleException(Exception ex)
        {
            Alert(ex.Message);
        }

        private void Alert(string message)
        {
            var builder = new AlertDialog.Builder(this);
            builder.SetTitle("Alert");
            builder.SetMessage(message);
            builder.SetPositiveButton("OK", (s, a) => {});
        }

        /// <summary>
        /// Determines the amount of processed frames per second
        /// </summary>
        private void DetermineFps()
        {
            if (_fpsDetermined)
                return;

            var now = DateTime.Now;
            if (!_start.HasValue)
                _start = now;
            else if (now >= _start.Value.AddSeconds(1))
            {
                FramesPerSecond = _framecount;
                _fpsDetermined = true;
            }
        }

        /// <summary>
        /// Should action be taken with the processed frame
        /// </summary>
        /// <returns>bool</returns>
        private bool ShouldAct()
        {
            if (Calibrating)
                return false;

            if (_fpsDetermined && _framecount >= FramesPerSecond && _readyToCapture)
            {
                _framecount = 0;
                return true;
            }
            return false;
        }

        #endregion
    }
}