using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VocalEyes.Common.Enumeration;
using VocalEyes.Common.Utils;
using OpenCV.Core;
using OpenCV.ImgProc;
using OpenCV.ObjDetect;
using VocalEyes.Common;
using VocalEyes.Droid.Activities;
using VocalEyes.Droid.Common.Model;
using VocalEyes.Model;

namespace VocalEyes.Droid.Common.Helper
{
    public class DetectionHelper
    {
        private const int PupilRadius = 10;

        public bool Handling;

        private readonly CaptureActivity _activity;

        public const int JavaDetector = 0;

        public Pupil CenterPoint { get; private set; }
        public Pupil LeftPoint { get; private set; }
        public Pupil RightPoint { get; private set; }
        public Pupil TopPoint { get; private set; }
        public Pupil BottomPoint { get; private set; }
        public Pupil TopLeftPoint { get; private set; }
        public Pupil BottomLeftPoint { get; private set; }
        public Pupil TopRightPoint { get; private set; }
        public Pupil BottomRightPoint { get; private set; }

        private Direction _calibrationDirection = Direction.Center;

        private Pupil _pupil;

        private Eye _eye;

        public DetectionHelper(CaptureActivity activity)
        {
            _activity = activity;
            SetPoints();
        }

        public Rect GetNearestFace(IEnumerable<Rect> faces)
        {
            Rect face = null;
            foreach (var f in faces.ToArray())
            {
                if (face == null || f.Area() > face.Area())
                    face = f;
            }
            return face;
        }

        public void SetPoints()
        {
            LeftPoint = new Pupil(_activity.FramesPerSecond * 2);
            RightPoint = new Pupil(_activity.FramesPerSecond * 2);
            TopPoint = new Pupil(_activity.FramesPerSecond * 2);
            BottomPoint = new Pupil(_activity.FramesPerSecond * 2);
            TopLeftPoint = new Pupil(_activity.FramesPerSecond * 2);
            BottomLeftPoint = new Pupil(_activity.FramesPerSecond * 2);
            TopRightPoint = new Pupil(_activity.FramesPerSecond * 2);
            BottomRightPoint = new Pupil(_activity.FramesPerSecond * 2);
            CenterPoint = new Pupil(_activity.FramesPerSecond * 2);
        }

        public async Task ResetCalibration()
        {
            App.User.Calibrated = false;
            App.User.Save();

            _calibrationDirection = Direction.Center;
            SetPoints();
            
            _activity.Calibrating = true;
            await _activity.Start();
        }

        private void Calibrate(Point point, int eyeOnlyAreaY)
        {
            var calibrationTime = _activity.FramesPerSecond*2;
            if (!_activity.Calibrating)
                return;

            switch (_calibrationDirection)
            {
                case Direction.Left:
                    LeftPoint.Insert(point);
                    break;
                case Direction.Right:
                    RightPoint.Insert(point);
                    break;
                case Direction.Top:
                    TopPoint.Insert(point);
                    break;
                case Direction.Bottom:
                    BottomPoint.Insert(point);
                    break;
                case Direction.TopLeft:
                    TopLeftPoint.Insert(point);
                    break;
                case Direction.BottomLeft:
                    BottomLeftPoint.Insert(point);
                    break;
                case Direction.TopRight:
                    TopRightPoint.Insert(point);
                    break;
                case Direction.BottomRight:
                    BottomRightPoint.Insert(point);
                    break;
                case Direction.Center:
                    CenterPoint.Insert(point);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_calibrationDirection == Direction.Center && CenterPoint.Count >= calibrationTime)
            {
                CenterPoint.PreCut();
                App.User.SetPoint(_calibrationDirection, CenterPoint.GetShape().X, CenterPoint.GetShape().Y);
                _calibrationDirection = Direction.Left;
                _activity.TextToSpeechHelper.Speak(SpeechHelper.GetDirectionString(_calibrationDirection));
            }
            else if (_calibrationDirection == Direction.Left && LeftPoint.Count >= calibrationTime)
            {
                LeftPoint.PreCut();
                App.User.SetPoint(_calibrationDirection, LeftPoint.GetShape().X, LeftPoint.GetShape().Y);
                _calibrationDirection = Direction.Right;
                _activity.TextToSpeechHelper.Speak(SpeechHelper.GetDirectionString(_calibrationDirection));
            }
            else if (_calibrationDirection == Direction.Right && RightPoint.Count >= calibrationTime)
            {
                RightPoint.PreCut();
                App.User.SetPoint(_calibrationDirection, RightPoint.GetShape().X, RightPoint.GetShape().Y);
               
                _calibrationDirection = Direction.Top;
                _activity.TextToSpeechHelper.Speak(SpeechHelper.GetDirectionString(_calibrationDirection));
            }
            else if (_calibrationDirection == Direction.Top && TopPoint.Count >= calibrationTime)
            {
                TopPoint.PreCut();
                App.User.SetPoint(_calibrationDirection, TopPoint.GetShape().X, TopPoint.GetShape().Y);
                _calibrationDirection = Direction.Bottom;
                _activity.TextToSpeechHelper.Speak(SpeechHelper.GetDirectionString(_calibrationDirection));
            }
            else if (_calibrationDirection == Direction.Bottom && BottomPoint.Count >= calibrationTime)
            {
                BottomPoint.PreCut();
                App.User.SetPoint(_calibrationDirection, BottomPoint.GetShape().X, BottomPoint.GetShape().Y);
                _calibrationDirection = Direction.TopLeft;
                _activity.TextToSpeechHelper.Speak(SpeechHelper.GetDirectionString(_calibrationDirection));
            }
            else if (_calibrationDirection == Direction.TopLeft && TopLeftPoint.Count >= calibrationTime)
            {
                TopLeftPoint.PreCut();
                App.User.SetPoint(_calibrationDirection, TopLeftPoint.GetShape().X, TopLeftPoint.GetShape().Y);
                _calibrationDirection = Direction.BottomLeft;
                _activity.TextToSpeechHelper.Speak(SpeechHelper.GetDirectionString(_calibrationDirection));
            }
            else if (_calibrationDirection == Direction.BottomLeft && BottomLeftPoint.Count >= calibrationTime)
            {
                BottomLeftPoint.PreCut();
                App.User.SetPoint(_calibrationDirection, BottomLeftPoint.GetShape().X, BottomLeftPoint.GetShape().Y);
                _calibrationDirection = Direction.TopRight;
                _activity.TextToSpeechHelper.Speak(SpeechHelper.GetDirectionString(_calibrationDirection));
            }
            else if (_calibrationDirection == Direction.TopRight && TopRightPoint.Count >= calibrationTime)
            {
                TopRightPoint.PreCut();
                App.User.SetPoint(_calibrationDirection, TopRightPoint.GetShape().X, TopRightPoint.GetShape().Y);
                _calibrationDirection = Direction.BottomRight;
                _activity.TextToSpeechHelper.Speak(SpeechHelper.GetDirectionString(_calibrationDirection));
            }
            else if (_calibrationDirection == Direction.BottomRight && BottomRightPoint.Count >= calibrationTime)
            {
                BottomRightPoint.PreCut();
                App.User.SetPoint(_calibrationDirection, BottomRightPoint.GetShape().X, BottomRightPoint.GetShape().Y);

                _activity.Calibrating = false;
                App.User.Calibrated = true;
                App.User.Save();
                 _activity.TextToSpeechHelper.Speak(SpeechHelper.CalibrationComplete);
            }
               
        }

        public Mat DetectEye(CascadeClassifier clasificator, Rect area, Rect face, int size, out Direction direction)
        {
            if (_eye == null)
                _eye = new Eye(Constants.PupilSkip);

            if (_pupil == null)
                _pupil = new Pupil(Constants.PupilSkip);

            var template = new Mat();
            var eyes = new MatOfRect();
            var mRoi = _activity.MGray.Submat(area);
            var areaMat = _activity.MRgba.Submat(area);
            clasificator.DetectMultiScale(mRoi, eyes, 1.15, 2, Objdetect.CascadeFindBiggestObject | 
                Objdetect.CascadeScaleImage, new Size(30, 30), new Size());

            foreach (var eye in eyes.ToArray())
            {
                eye.X = area.X + eye.X;
                eye.Y = area.Y + eye.Y;

                var eyeOnlyRectangle = _eye.Insert(new Rect((int)eye.Tl().X, (int)(eye.Tl().Y + eye.Height * 0.4),
                    eye.Width, (int)(eye.Height * 0.6))).GetShape();

                mRoi = _activity.MGray.Submat(eyeOnlyRectangle);

                var mmG = Core.MinMaxLoc(mRoi);
                var cp = new Point(mmG.MinLoc.X + PupilRadius / 2 + (eyeOnlyRectangle.X - area.X),
                    mmG.MinLoc.Y + PupilRadius / 2 + (eyeOnlyRectangle.Y - area.Y));

                Imgproc.Rectangle(_activity.MRgba, eyeOnlyRectangle.Tl(), eyeOnlyRectangle.Br(), new Scalar(255, 255, 255), 2);

                var avg = _pupil.Insert(cp).GetShape();
                Calibrate(avg, eyeOnlyRectangle.Y);

                HandlePosition(areaMat, avg, out direction, area.Y, eyeOnlyRectangle.Y);

                Imgproc.Circle(areaMat, avg, PupilRadius, new Scalar(255, 255, 255), 2);
                Imgproc.Circle(areaMat, avg, 4, new Scalar(255, 255, 255), 2);
                Imgproc.Circle(areaMat, avg, 3, new Scalar(255, 255, 255), 2);

                return template;
            }
            direction = Direction.Center;
            return template;
        }

        private void HandlePosition(Mat areaMat, Point position, out Direction direction, int eyeAreaY, int eyeOnlyAreaY)
        {
            direction = Direction.Center;

            try
            {
                if (_activity.ShouldAct() && !Handling)
                {
                    Handling = true;
                    _activity.TextToSpeechHelper.PlayBeep();
                    direction = GetDirection(position, eyeOnlyAreaY);
                    _activity.TextToSpeechHelper.Speak(direction.ToString());
                }
            }
            catch
            {
            }
            finally
            {
                Handling = false;
            }
        }

        /// <summary>
        /// Determine the absolute distance between 2 points
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static DirectionDistance GetDistance(Direction direction, Point a, Point b)
        {
            var distanceX = a.X - b.X;
            if (distanceX < 0)
                distanceX = distanceX*-1;

            var distanceY = a.Y - b.Y;
            if (distanceY < 0)
                distanceY = distanceY*-1;
            
            return new DirectionDistance
            {
                Direction = direction, Distance = TrigonometryHelper.GetDistance(distanceX, distanceY)
            };
        }

        /// <summary>
        /// Compare the pupil position and the center position 
        /// and determine the direction of the pupil
        /// </summary>
        /// <param name="point">pupil position</param>
        /// <param name="eyeOnlyArea"></param>
        /// <returns>Direction of pupil</returns>
        public Direction GetDirection(Point point, int eyeOnlyAreaY)
        {
            if (_activity.Calibrating)
                return Direction.Center;

            var avgEyeAreaY = _eye.GetShape().Y;
            var diffY = _eye.GetShape().Y - eyeOnlyAreaY;

            var cp = new Point((CenterPoint.GetShape().X + CenterPoint.LastMeasured.X) / 2, 
                (CenterPoint.GetShape().Y + CenterPoint.LastMeasured.Y) / 2);

            var lp = new Point((LeftPoint.GetShape().X + LeftPoint.LastMeasured.X) / 2,
              (LeftPoint.GetShape().Y + LeftPoint.LastMeasured.Y) / 2);

            var rp = new Point((RightPoint.GetShape().X + RightPoint.LastMeasured.X) / 2,
              (RightPoint.GetShape().Y + RightPoint.LastMeasured.Y) / 2);

            var distances = new List<DirectionDistance>
            {
                GetDistance(Direction.Center, point, cp),
                GetDistance(Direction.Left, point, lp),
                GetDistance(Direction.Right, point, rp),
                //GetDistance(Direction.Top, point, TopPoint.GetShape()),
                //GetDistance(Direction.Bottom, point, BottomPoint.GetShape()),
                //GetDistance(Direction.TopLeft, point, TopLeftPoint.GetShape()),
                //GetDistance(Direction.BottomRight, point, BottomRightPoint.GetShape()),
                //GetDistance(Direction.TopRight, point, TopRightPoint.GetShape()),
                //GetDistance(Direction.BottomLeft, point, BottomLeftPoint.GetShape())
            };

            var direction = distances.OrderBy(d => d.Distance).First().Direction;
            //TODO: determine up/down
            return direction;
        }
    }
}