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
        #region properties

        private const int PupilRadius = 10;

        public bool Handling;

        private readonly CaptureActivity _activity;

        public const int JavaDetector = 0;

        public Pupil CenterPoint { get; private set; }
        public Pupil LeftPoint { get; private set; }
        public Pupil RightPoint { get; private set; }
        public AreaCoordinate TopPoint { get; private set; }
        public AreaCoordinate BottomPoint { get; private set; }

        private Direction _calibrationDirection = Direction.Center;

        private Pupil _pupil;

        private Eye _eye;

        #endregion

        public DetectionHelper(CaptureActivity activity)
        {
            _activity = activity;
            SetPoints();
        }

        /// <summary>
        /// If multiple faces are detected, focus only on the nearest
        /// </summary>
        /// <param name="faces"></param>
        /// <returns></returns>
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

        /// <summary>
        /// (Re)instantiate variables for storing pupil positions
        /// </summary>
        public void SetPoints()
        {
            var wait = _activity.FramesPerSecond*2;
            LeftPoint = new Pupil(wait);
            RightPoint = new Pupil(wait);
            TopPoint = new AreaCoordinate(wait);
            BottomPoint = new AreaCoordinate(wait);
            CenterPoint = new Pupil(wait);
        }

        /// <summary>
        /// Reset and recalibrate
        /// </summary>
        /// <returns></returns>
        public async Task ResetCalibration()
        {
            App.User.Calibrated = false;
            App.User.Save();

            _calibrationDirection = Direction.Center;
            SetPoints();
            
            _activity.Calibrating = true;
            await _activity.Start();
        }

        /// <summary>
        /// Calibrate extreme pupil positions
        /// </summary>
        /// <param name="point"></param>
        /// <param name="eyeOnlyAreaY"></param>
        private void Calibrate(Point point, Point eyeOnlyAreaY)
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
                    TopPoint.Insert(eyeOnlyAreaY);
                    break;
                case Direction.Bottom:
                    BottomPoint.Insert(eyeOnlyAreaY);
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
              
                _activity.Calibrating = false;
                App.User.Calibrated = true;
                App.User.Save();
                _activity.TextToSpeechHelper.Speak(SpeechHelper.CalibrationComplete);
            }
        }

        /// <summary>
        /// Detect pupil in eyearea bitmap
        /// </summary>
        /// <param name="clasificator"></param>
        /// <param name="area"></param>
        /// <param name="face"></param>
        /// <param name="size"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Mat DetectEye(CascadeClassifier clasificator, Rect area, Rect face, int size, out Direction direction)
        {
            if (_eye == null)
                _eye = new Eye(Constants.AreaSkip);

            if (_pupil == null)
                _pupil = new Pupil(Constants.PupilSkip);

            var template = new Mat();
            var eyes = new MatOfRect();
            var mRoi = _activity.MGray.Submat(area);
            var areaMat = _activity.MRgba.Submat(area);

            //Imgproc.Threshold(mRoi.Clone(), mRoi, 100, 255, Imgproc.ThreshBinary);

            // Built in equalizeHist function.
            Imgproc.EqualizeHist(mRoi.Clone(), mRoi); // Causes a lot of noise in darker images.

            // Built in Contrast Limited Adaptive Histogram Equalization.
            //CLAHE mClahe = Imgproc.CreateCLAHE(2, new Size(8, 8));
            //mClahe.Apply(mRoi.Clone(), mRoi); // Seems not to work in (fairly) dark images.
            //mClahe.CollectGarbage();
            
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
                
                var eyeOnlyPoint = new Point(eyeOnlyRectangle.X, eyeOnlyRectangle.Y);
                var avg = _pupil.Insert(cp).GetShape();

                Calibrate(avg, eyeOnlyPoint);

                HandlePosition(avg, eyeOnlyPoint, out direction);

                Imgproc.Circle(areaMat, avg, PupilRadius, new Scalar(255, 255, 255), 2);
                Imgproc.Circle(areaMat, avg, 4, new Scalar(255, 255, 255), 2);
                Imgproc.Circle(areaMat, avg, 3, new Scalar(255, 255, 255), 2);

                return template;
            }
            direction = Direction.Center;
            return template;
        }

        /// <summary>
        /// Determine position of pupil
        /// </summary>
        /// <param name="position"></param>
        /// <param name="eyeArea"></param>
        /// <param name="direction"></param>
        private void HandlePosition(Point position, Point eyeArea, out Direction direction)
        {
            direction = Direction.Center;

            try
            {
                if (_activity.ShouldAct() && !Handling)
                {
                    Handling = true;
                    _activity.TextToSpeechHelper.PlayBeep();
                    direction = GetDirection(position, eyeArea);
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
                Direction = direction, 
                Distance = TrigonometryHelper.GetDistance(distanceX, distanceY)
            };
        }

        /// <summary>
        /// Determine the absolute horizontal distance between 2 points
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static DirectionDistance GetDistanceX(Direction direction, Point a, Point b)
        {
            return new DirectionDistance
            {
                Direction = direction,
                Distance = a.X - b.X < 0 ? a.X - b.X * -1 : a.X - b.X
            };
        }

        /// <summary>
        /// Determine the absolute vertical distance between 2 points
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static DirectionDistance GetDistanceY(Direction direction, Point a, Point b)
        {
            return new DirectionDistance
            {
                Direction = direction,
                Distance = a.Y - b.Y < 0 ? a.Y - b.Y * -1 : a.Y - b.Y
            };
        }

        /// <summary>
        /// Compare the pupil position and the center position 
        /// and determine the direction of the pupil
        /// </summary>
        /// <param name="point">pupil position</param>
        /// <param name="eyeOnlyArea"></param>
        /// <param name="eyeArea"></param>
        /// <returns>Direction of pupil</returns>
        public Direction GetDirection(Point point, Point eyeArea)
        {
            if (_activity.Calibrating)
                return Direction.Center;

            #region determine top, bottom, center, left and right points for comparison

            var cp = new Point((CenterPoint.GetShape().X + CenterPoint.LastMeasured.X) / 2, 
                (CenterPoint.GetShape().Y + CenterPoint.LastMeasured.Y) / 2);

            var lp = new Point((LeftPoint.GetShape().X + LeftPoint.LastMeasured.X) / 2,
              (LeftPoint.GetShape().Y + LeftPoint.LastMeasured.Y) / 2);

            var rp = new Point((RightPoint.GetShape().X + RightPoint.LastMeasured.X) / 2,
              (RightPoint.GetShape().Y + RightPoint.LastMeasured.Y) / 2);

            var bp = new Point((BottomPoint.GetShape().X + BottomPoint.LastMeasured.X) / 2,
             (BottomPoint.GetShape().Y + BottomPoint.LastMeasured.Y) / 2);

            var tp = new Point((TopPoint.GetShape().X + TopPoint.LastMeasured.X) / 2,
              (TopPoint.GetShape().Y + TopPoint.LastMeasured.Y) / 2);

            #endregion

            #region detect horizontal direction

            var distancesHorizontal = new List<DirectionDistance>
            {
                GetDistanceX(Direction.Center, point, cp),
                GetDistanceX(Direction.Left, point, lp),
                GetDistanceX(Direction.Right, point, rp)
            };

            var directionHorizontal = distancesHorizontal.OrderBy(d => d.Distance).First().Direction;

            #endregion

            #region detect vertical direction

            var distancesVertical = new List<DirectionDistance>
            {
                GetDistanceY(Direction.Center, point, cp),
                GetDistanceY(Direction.Bottom, eyeArea, bp),
                GetDistanceY(Direction.Top, eyeArea, tp),
            };
            var directionVertical = distancesVertical.OrderBy(d => d.Distance).First().Direction;

            #endregion

            #region combine horizontal and vertical direction to determine gaze direction

            switch (directionHorizontal)
            {
                case Direction.Left:
                    switch (directionVertical)
                    {
                        case Direction.Top:
                            return Direction.TopLeft;
                        case Direction.Bottom:
                            return Direction.BottomLeft;
                    }
                    return Direction.Left;
                case Direction.Right:
                    switch (directionVertical)
                    {
                        case Direction.Top:
                            return Direction.TopRight;
                        case Direction.Bottom:
                            return Direction.TopRight;
                    }
                    return Direction.Right;
                case Direction.Center:
                      switch (directionVertical)
                    {
                        case Direction.Top:
                            return Direction.Top;
                        case Direction.Bottom:
                            return Direction.Bottom;
                    }
                    return Direction.Center;
            }

            #endregion

            return Direction.Center;
        }
    }
}