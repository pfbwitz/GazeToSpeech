using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
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
    /// Last Update:    August 20 2016
    /// </summary>
    [Activity(Label = "Pupil tracking", ConfigurationChanges = ConfigChanges.Orientation,
        Icon = "@drawable/icon",
        ScreenOrientation = ScreenOrientation.Landscape)]
    public class CaptureActivity : Activity, CameraBridgeViewBase.ICvCameraViewListener2
    {
        public CaptureActivity()
        {
            Calibrating = true;

            _eyeArea = new EyeArea(Constants.AreaSkip);
            _face = new FaceArea(Constants.AreaSkip*10);
            _captureMethod = CaptureMethod.Subset;
            _detectionHelper = new DetectionHelper(this);

            TextToSpeechHelper = new TextToSpeechHelper();

            CreateGridSubSet();
        }

        #region properties

            #region public

        public List<Subset> SubSets;

        public bool Running;
        public bool Calibrating;
        
        public Direction Direction;

        public Subset CurrentSubset;

        public TextToSpeechHelper TextToSpeechHelper;

        public List<string> CharacterBuffer = new List<string>();

        public int FramesPerSecond;
        public int Facing;
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
        public TextView Clock;
        public TextView Load1;
        public TextView Load2;
        public TextView Load3;
        public Stopwatch Stopwatch = new Stopwatch();

            #endregion

            #region private

        private readonly FaceArea _face;
        private readonly EyeArea _eyeArea;

        private Button _resetButton;

        private bool _readyToCapture;
        private bool _fpsDetermined;

        private int _framecount;
        private int _mAbsoluteFaceSize;
        private readonly int _mDetectorType = JavaDetector;

        private float _mRelativeFaceSize = 0.2f;

        private DateTime? _start;

        private ProgressDialog _progress;

        private readonly DetectionHelper _detectionHelper;

        private CaptureMethod _captureMethod;

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

            FindViewById<ImageView>(Resource.Id.overlay).Visibility = ViewStates.Gone;

            _resetButton = FindViewById<Button>(Resource.Id.reset);
            _resetButton.Click +=
                async (sender, args) => await _detectionHelper.ResetCalibration();
            _resetButton.Visibility = ViewStates.Gone;
            
            _mOpenCvCameraView = FindViewById<CameraBridgeViewBase>(Resource.Id.fd_activity_surface_view);
            _mOpenCvCameraView.Visibility = ViewStates.Visible;

            Stopwatch.Start();
            TextViewTimer = FindViewById<TextView>(Resource.Id.tv1);
            Clock = FindViewById<TextView>(Resource.Id.tv2);

            Load1 = FindViewById<TextView>(Resource.Id.p1);
            Load2 = FindViewById<TextView>(Resource.Id.p2);
            Load3 = FindViewById<TextView>(Resource.Id.p3);

            Facing = Intent.GetIntExtra(typeof(CameraFacing).Name, CameraFacing.Front);
            _mOpenCvCameraView.SetCameraIndex(Facing);
            _mOpenCvCameraView.SetCvCameraViewListener2(this);

            _mLoaderCallback = new Callback(this, _mOpenCvCameraView);
            _progress = new ProgressDialog(this) {Indeterminate = true};
            _progress.SetCancelable(true);
            _progress.SetProgressStyle(ProgressDialogStyle.Spinner);
            _progress.SetMessage(SpeechHelper.InitMessage);
            _progress.Window.ClearFlags(WindowManagerFlags.DimBehind);
            _progress.Show();
            _progress.CancelEvent += (sender, args) =>
            {
                _mLoaderCallback.Cancelling = true;
                Finish();
            };

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
            RunOnUiThread(() =>
            {
                var battery = GetBatteryLife();
                var batteryString = string.Empty;
                if (battery.HasValue)
                    batteryString = battery.Value + "% - ";

                Clock.Text = batteryString + DateTime.Now.ToString("t") + " ";
            });
            MRgba = inputFrame.Rgba();
            MGray = inputFrame.Gray();

            if (!Running)
                return MRgba;

            _framecount++;

            DetermineFps();

            if (!_fpsDetermined || !_readyToCapture)
                return MRgba;

            if (_mAbsoluteFaceSize == 0)
            {
                var height = MGray.Rows();
                if (Math.Round(height * _mRelativeFaceSize) > 0)
                    _mAbsoluteFaceSize = Java.Lang.Math.Round(height * _mRelativeFaceSize);

                MNativeDetector.SetMinFaceSize(_mAbsoluteFaceSize);
            }

            var faces = new MatOfRect();
            if (_mDetectorType == JavaDetector)
            {
                if (MJavaDetector != null)
                    MJavaDetector.DetectMultiScale(MGray, faces, 1.1, 2, 2,
                            new Size(_mAbsoluteFaceSize, _mAbsoluteFaceSize), new Size());
            }
            else if (_mDetectorType == NativeDetector && MNativeDetector != null)
                MNativeDetector.Detect(MGray, faces);

            var face = _face.Insert(_detectionHelper.GetNearestFace(faces.ToArray())).GetShape();

            if (face == null) 
                return MRgba;

            Imgproc.Rectangle(MRgba, face.Tl(), face.Br(), new Scalar(255, 255, 255));

            var eyearea = _eyeArea.Insert(new Rect(face.X + face.Width / 16 + (face.Width - 2 * face.Width / 16) / 2,
                (int)(face.Y + (face.Height / 4.5)), (face.Width - 2 * face.Width / 16) / 2, (int)(face.Height / 3.0))).GetShape();

            Imgproc.Rectangle(MRgba, eyearea.Tl(), eyearea.Br(), new Scalar(255, 0, 0, 255), 2);

            Point position;
            _detectionHelper.DetectLeftEye(MJavaDetectorEye, eyearea, face, 24, out position);

            if (position == null)
                return MRgba;

            if(!Calibrating)
                PopulateGrid(_detectionHelper.GetDirection(position).Value);
            return MRgba;
        }
        #endregion

        #region custom methods

        public int? GetBatteryLife()
        {
            try
            {
                using (var filter = new IntentFilter(Intent.ActionBatteryChanged))
                {
                    using (var battery = Application.Context.RegisterReceiver(null, filter))
                    {
                        var level = battery.GetIntExtra(BatteryManager.ExtraLevel, -1);
                        var scale = battery.GetIntExtra(BatteryManager.ExtraScale, -1);

                        return (int) Math.Floor(level*100D/scale);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task Start()
        {
            if (IsFinishing || _mLoaderCallback.Cancelling)
                return;
            try
            {
                while (!Running)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    RunOnUiThread(() =>
                    {
                        try
                        {
                            var battery = GetBatteryLife();
                            var batteryString = string.Empty;
                            if (battery.HasValue)
                                batteryString = battery.Value + "% - ";

                            Clock.Text = batteryString + DateTime.Now.ToString("t") + " ";
                            TextViewTimer.Text =Stopwatch.Elapsed.ToString(@"m\:ss");
                        }
                        catch(WindowManagerBadTokenException){}
                    });
                }

                RunOnUiThread(() =>
                {
                    try
                    {
                        TextViewTimer.Text = string.Empty;
                        _progress.Dismiss();
                        Stopwatch.Stop();
                    }
                    catch(WindowManagerBadTokenException){}
                });

                await Task.Run(() =>
                {
                    RunOnUiThread(() =>
                    {
                        try
                        {
                            _resetButton.Visibility = ViewStates.Gone;

                            _resetButton.Visibility = ViewStates.Visible;
                            FindViewById<ImageView>(Resource.Id.overlay).Visibility = ViewStates.Visible;
                            Load1.Text = Load2.Text = Load3.Text = string.Empty;

                            if(!IsFinishing && !_mLoaderCallback.Cancelling)
                                TextToSpeechHelper.Speak(SpeechHelper.CalibrationInit);
                            var builder = new AlertDialog.Builder(this);
                            builder.SetMessage(SpeechHelper.CalibrationInit);
                            builder.SetPositiveButton("OK", (a, s) =>
                            {
                                _resetButton.Visibility = ViewStates.Visible;
                                FindViewById<ImageView>(Resource.Id.overlay).Visibility = ViewStates.Visible;
                                Load1.Text = Load2.Text = Load3.Text = string.Empty;

                                TextToSpeechHelper.Speak(SpeechHelper.GetDirectionString(Direction.Center));

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
        /// Translate the direction of the eye to a highlighted area on the grid
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
                    case Direction.Center:
                        rect = null;
                        break;
                }

                if(rect != null)
                    Imgproc.Rectangle(MRgba, rect.Tl(), rect.Br(), new Scalar(255, 0, 0), 3);
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

        public void HandleException(Exception ex)
        {
            Alert(ex.Message, Finish);
        }

        private void Alert(string message, Action ac)
        {
            var builder = new AlertDialog.Builder(this);
            builder.SetTitle("Alert");
            builder.SetMessage(message);
            builder.SetPositiveButton("OK", (s, a) => ac());
        }

        /// <summary>
        /// Determines the amount of processed frames per second
        /// </summary>
        private void DetermineFps()
        {
            if (_fpsDetermined)
                return;

            var now = DateTime.Now;
            var start = _start ?? (_start = now);
            if (now >= start.Value.AddSeconds(1))
            {
                FramesPerSecond = _framecount;
                _fpsDetermined = true;
            }
        }

        /// <summary>
        /// Should action be taken with the processed frame
        /// </summary>
        /// <returns>bool</returns>
        public bool ShouldAct()
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
};