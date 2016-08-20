using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
            Instance = this;
            Calibrating = true;
            CreateGridSubSet();
            _captureMethod = CaptureMethod.Subset;
            _detectionHelper = new DetectionHelper();
            TextToSpeechHelper = new TextToSpeechHelper(this);
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

        public readonly CaptureActivity Instance;

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

        private readonly DetectionHelper _detectionHelper;

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

        private ProgressDialog _progress;

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
            Task.Run(async() =>
            {
                try
                {
                    while (!Running)
                    {
                        await Task.Delay(1000);
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
                                var builder = new AlertDialog.Builder(this);
                                builder.SetMessage(SpeechHelper.CalibrationInit);
                                builder.SetPositiveButton("OK", (a, s) =>
                                {
                                    _calibrationStart = DateTime.Now;
                                    _readyToCapture = true;
                                });
                                builder.Show();
                            }
                            catch(WindowManagerBadTokenException){}
                        });
                    });
                }
                catch (WindowManagerBadTokenException)
                {
                }
            });
        }

        private DateTime _calibrationStart;

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

        private EyeArea _leftEyeArea;
        private EyeArea _rightEyeArea;

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
                _leftEyeArea = new EyeArea(2);

            if(_rightEyeArea == null)
                _rightEyeArea = new EyeArea(2);

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

                _detectionHelper.DetectRightEye(this, MJavaDetectorEye, eyeareaRight, face, 24);
                _detectionHelper.DetectLeftEye(this, MJavaDetectorEye, eyeareaLeft, face, 24);
                var positionToDraw = PosLeft;

                if (positionToDraw == null)
                    return MRgba;

                if(_detectionHelper.CenterPoint != null)
                    PopulateGrid(GetDirection(positionToDraw));

                if (ShouldAct())
                    RunOnUiThread(() => HandleEyePosition(positionToDraw));
            }
            return MRgba;
        }
        #endregion

        #region custom methods

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
                Imgproc.Rectangle(MRgba, rect.Tl(), rect.Br(), new Scalar(255, 0, 0), 2);
            }
        }

        /// <summary>
        /// Determine the absolute distance between 2 points
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private double GetDistance(Point a, Point b)
        {
            var distanceX = a.X - b.X;
            if (distanceX < 0)
                distanceX = distanceX*-1;

            var distanceY = a.Y - b.Y;
            if (distanceY < 0)
                distanceY = distanceY*-1;

            return Math.Sqrt(Math.Pow(distanceX, 2) + Math.Pow(distanceY, 2));
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
        /// Compare the pupil position and the center position 
        /// and determine the direction of the pupil
        /// </summary>
        /// <param name="point">pupil position</param>
        /// <returns>Direction of pupil</returns>
        private Direction GetDirection(Point point)
        {
            var marginX = 10;
            var marginY = 10;
            _handling = true;

            var direction = Direction.Center;

            var centerpoint = _detectionHelper.CenterPoint;
            var diffX = point.X - centerpoint.X;
            var diffY = point.Y - centerpoint.Y;

            var distance = GetDistance(point, _detectionHelper.CenterPoint);

            var diffXInPixels = diffX < 0 ? diffX * -1 : diffX;
            var diffYInPixels = diffY < 0 ? diffY * -1 : diffY;

            //top-center or bottom-center
            if (diffXInPixels <= marginX)
            {
                if (diffY < 0 && diffYInPixels > marginY)
                    direction = Direction.Top;
                if (diffY > 0 && diffYInPixels > marginY)
                    direction = Direction.Bottom;
            }

            if (Facing == CameraFacing.Front)
            {
                //right, bottom-right or top-right
                if (diffX < 0 && diffXInPixels > marginX)
                {
                    if (diffY > 0 && diffYInPixels > marginY)
                        direction = Direction.BottomRight;
                    else if (diffY < 0 && diffYInPixels > marginY)
                        direction = Direction.TopRight;
                    else
                        direction = Direction.Right;
                }

                //left, bottom-left or top-left
                if (diffX > 0 && diffXInPixels > marginX)
                {
                    if (diffY > 0 && diffYInPixels > marginY)
                        direction = Direction.BottomLeft;
                    else if (diffY < 0 && diffYInPixels > marginY)
                        direction = Direction.TopLeft;
                    else
                        direction = Direction.Left;
                }
            }
            else
            {
                //right, bottom-right or top-right
                if (diffX > 0 && diffXInPixels > marginX)
                {
                    if (diffY > 0 && diffYInPixels > marginY)
                        direction = Direction.BottomRight;
                    else if (diffY < 0 && diffYInPixels > marginY)
                        direction = Direction.TopRight;
                    else
                        direction = Direction.Right;
                }

                //left, bottom-left or top-left
                if (diffX < 0 && diffXInPixels > marginX)
                {
                    if (diffY > 0 && diffYInPixels > marginY)
                        direction = Direction.BottomLeft;
                    else if (diffY < 0 && diffYInPixels > marginY)
                        direction = Direction.TopLeft;
                    else
                        direction = Direction.Left;
                }
            }
            return direction;
        }

        /// <summary>
        /// Handle calculated position of the pupil compared to the surface of the entire eye
        /// </summary>
        /// <param name="position"></param>
        public void HandleEyePosition(Point position)
        {
            try
            {
                if (TextToSpeechHelper.IsSpeaking)
                    return;

                if (position == null || _handling || TextToSpeechHelper.IsSpeaking)
                    return;

                var direction = GetDirection(position);

                TextToSpeechHelper.Speak(direction.ToString());

                if (_captureMethod == CaptureMethod.Subset)
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
            }
            finally
            {
                _handling = false;
            }
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