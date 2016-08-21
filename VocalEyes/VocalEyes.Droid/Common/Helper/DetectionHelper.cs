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
        const int PupilRadius = 10;

        private readonly CaptureActivity _activity;

        public const int JavaDetector = 0;

        public Point CenterPoint { get; private set; }
        public Point LeftPoint { get; private set; }
        public Point RightPoint { get; private set; }
        public Point TopPoint { get; private set; }
        public Point BottomPoint { get; private set; }
        public Point TopLeftPoint { get; private set; }
        public Point BottomLeftPoint { get; private set; }
        public Point TopRightPoint { get; private set; }
        public Point BottomRightPoint { get; private set; }

        private readonly List<Point> _centerpoints = new List<Point>();
        private readonly List<Point> _leftpoints = new List<Point>();
        private readonly List<Point> _rightpoints = new List<Point>();
        private readonly List<Point> _toppoints = new List<Point>();
        private readonly List<Point> _bottompoints = new List<Point>();
        private readonly List<Point> _bottomleftpoints = new List<Point>();
        private readonly List<Point> _topleftpoints = new List<Point>();
        private readonly List<Point> _bottomrightpoints = new List<Point>();
        private readonly List<Point> _toprightpoints = new List<Point>();

        private Direction _calibrationDirection = Direction.Center;

        private Pupil _leftPupil;

        private Pupil _rightPupil;

        private Eye _leftEye;

        private Eye _rightEye;

        public DetectionHelper(CaptureActivity activity)
        {
            _activity = activity;
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

        public Mat DetectLeftEye(CascadeClassifier clasificator, Rect area, Rect face, int size, out Point p)
        {
            if(_leftEye == null)
                _leftEye = new Eye(Constants.PupilSkip);

            if (_leftPupil == null)
                _leftPupil = new Pupil(Constants.PupilSkip);
            return DetectEye(clasificator, area, face, size, true, out p);
        }

        public Mat DetectRightEye(CascadeClassifier clasificator, Rect area, Rect face, int size, out Point p)
        {
            if (_rightEye == null)
                _rightEye = new Eye(Constants.PupilSkip);

            if (_rightPupil == null)
                _rightPupil = new Pupil(Constants.PupilSkip);
            return DetectEye(clasificator, area, face, size, false, out p);
        }

        public async Task ResetCalibration()
        {
            App.User.Calibrated = false;
            App.User.Save();

            _activity.Calibrating = true;
            CenterPoint = null;
            LeftPoint = null;
            RightPoint = null;
            TopPoint = null;
            BottomPoint = null;
            TopLeftPoint = null;
            BottomLeftPoint = null;
            TopRightPoint = null;
            BottomRightPoint = null;
            _centerpoints.Clear();
            _leftpoints.Clear();
            _rightpoints.Clear();
            _toppoints.Clear();
            _bottompoints.Clear();
            _topleftpoints.Clear();
            _bottomleftpoints.Clear();
            _toprightpoints.Clear();
            _bottomrightpoints.Clear();
            await _activity.Start();
        }

        private void Calibrate(Point point)
        {
            var calibrationTime = _activity.FramesPerSecond*2;

            if (_activity.Calibrating)
            {
                switch (_calibrationDirection)
                {
                    case Direction.Left:
                        _leftpoints.Add(point);
                        break;
                    case Direction.Right:
                        _rightpoints.Add(point);
                        break;
                    case Direction.Top:
                        _toppoints.Add(point);
                        break;
                    case Direction.Bottom:
                        _bottompoints.Add(point);
                        break;
                    case Direction.TopLeft:
                        _topleftpoints.Add(point);
                        break;
                    case Direction.BottomLeft:
                        _bottomleftpoints.Add(point);
                        break;
                    case Direction.TopRight:
                        _toprightpoints.Add(point);
                        break;
                    case Direction.BottomRight:
                        _bottomrightpoints.Add(point);
                        break;
                    case Direction.Center:
                        _centerpoints.Add(point);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }                    
            }

            if (_activity.Calibrating)
            {
                if (_calibrationDirection == Direction.Center && _centerpoints.Count >= calibrationTime)
                {
                    var cut = _centerpoints.Count/2;
                    while(_centerpoints.Count > cut)
                        _centerpoints.RemoveAt(0);

                    CenterPoint = new Point(_centerpoints.Average(c => c.X), _centerpoints.Average(c => c.Y));
                    App.User.CenterX = CenterPoint.X;
                    App.User.CenterY = CenterPoint.Y;
                    _activity.TextToSpeechHelper.Speak(SpeechHelper.GetDirectionString(Direction.Left));
                    _calibrationDirection = Direction.Left;
                }
                else if (_calibrationDirection == Direction.Left && _leftpoints.Count >= calibrationTime)
                {
                    var cut = _leftpoints.Count / 2;
                    while (_leftpoints.Count > cut)
                        _leftpoints.RemoveAt(0);

                    LeftPoint = new Point(_leftpoints.Average(c => c.X), _leftpoints.Average(c => c.Y));
                    App.User.LeftX = LeftPoint.X;
                    App.User.LeftY = LeftPoint.Y;
                    _activity.TextToSpeechHelper.Speak(SpeechHelper.GetDirectionString(Direction.Right));
                    _calibrationDirection = Direction.Right;
                }
                else if (_calibrationDirection == Direction.Right && _rightpoints.Count >= calibrationTime)
                {
                    var cut = _rightpoints.Count / 2;
                    while (_rightpoints.Count > cut)
                        _rightpoints.RemoveAt(0);

                    RightPoint = new Point(_rightpoints.Average(c => c.X), _rightpoints.Average(c => c.Y));
                    App.User.RightX = RightPoint.X;
                    App.User.RightY = RightPoint.Y;
                    _activity.TextToSpeechHelper.Speak(SpeechHelper.GetDirectionString(Direction.Top));
                    _calibrationDirection = Direction.Top;
                }
                else if (_calibrationDirection == Direction.Top && _toppoints.Count >= calibrationTime)
                {
                    var cut = _toppoints.Count / 2;
                    while (_toppoints.Count > cut)
                        _toppoints.RemoveAt(0);

                    TopPoint = new Point(_toppoints.Average(c => c.X), _toppoints.Average(c => c.Y));
                    App.User.TopX = TopPoint.X;
                    App.User.TopY = TopPoint.Y;
                    _activity.TextToSpeechHelper.Speak(SpeechHelper.GetDirectionString(Direction.Bottom));
                    _calibrationDirection = Direction.Bottom;
                }
                else if (_calibrationDirection == Direction.Bottom && _bottompoints.Count >= calibrationTime)
                {
                    var cut = _bottompoints.Count / 2;
                    while (_bottompoints.Count > cut)
                        _bottompoints.RemoveAt(0);

                    BottomPoint = new Point(_bottompoints.Average(c => c.X), _bottompoints.Average(c => c.Y));
                    App.User.BottomX = BottomPoint.X;
                    App.User.BottomY = BottomPoint.Y;
                    _activity.TextToSpeechHelper.Speak(SpeechHelper.GetDirectionString(Direction.TopLeft));
                    _calibrationDirection = Direction.TopLeft;
                }
                else if (_calibrationDirection == Direction.TopLeft && _topleftpoints.Count >= calibrationTime)
                {
                    var cut = _topleftpoints.Count / 2;
                    while (_topleftpoints.Count > cut)
                        _topleftpoints.RemoveAt(0);

                    TopLeftPoint = new Point(_topleftpoints.Average(c => c.X), _topleftpoints.Average(c => c.Y));
                    App.User.TopLeftX = TopLeftPoint.X;
                    App.User.TopLeftY = TopLeftPoint.Y;
                    _activity.TextToSpeechHelper.Speak(SpeechHelper.GetDirectionString(Direction.BottomLeft));
                    _calibrationDirection = Direction.BottomLeft;
                }
                else if (_calibrationDirection == Direction.BottomLeft && _bottomleftpoints.Count >= calibrationTime)
                {
                    var cut = _bottomleftpoints.Count / 2;
                    while (_bottomleftpoints.Count > cut)
                        _bottomleftpoints.RemoveAt(0);

                    BottomLeftPoint = new Point(_bottomleftpoints.Average(c => c.X), _bottomleftpoints.Average(c => c.Y));
                    App.User.BottomLeftX = BottomLeftPoint.X;
                    App.User.BottomLeftY = BottomLeftPoint.Y;
                    _activity.TextToSpeechHelper.Speak(SpeechHelper.GetDirectionString(Direction.TopRight));
                    _calibrationDirection = Direction.TopRight;
                }
                else if (_calibrationDirection == Direction.TopRight && _toprightpoints.Count >= calibrationTime)
                {
                    var cut = _toprightpoints.Count / 2;
                    while (_toprightpoints.Count > cut)
                        _toprightpoints.RemoveAt(0);

                    TopRightPoint = new Point(_toprightpoints.Average(c => c.X), _toprightpoints.Average(c => c.Y));
                    App.User.TopRightX = TopRightPoint.X;
                    App.User.TopRightY = TopRightPoint.Y;
                    _activity.TextToSpeechHelper.Speak(SpeechHelper.GetDirectionString(Direction.BottomRight));
                    _calibrationDirection = Direction.BottomRight;
                }
                else if (_calibrationDirection == Direction.BottomRight && _bottomrightpoints.Count >= calibrationTime)
                {
                    var cut = _bottomrightpoints.Count / 2;
                    while (_bottomrightpoints.Count > cut)
                        _bottomrightpoints.RemoveAt(0);

                    BottomRightPoint = new Point(_bottomrightpoints.Average(c => c.X), _bottomrightpoints.Average(c => c.Y));
                    App.User.BottomRightX = BottomRightPoint.X;
                    App.User.BottomRightY = BottomRightPoint.Y;

                    _activity.TextToSpeechHelper.Speak(SpeechHelper.CalibrationComplete);
                    _activity.Calibrating = false;

                    App.User.Calibrated = true;
                    App.User.Save();
                }
            }
        }

        private void DrawCalibrationPoints(Mat vyrez)
        {
            if (CenterPoint == null || LeftPoint == null || RightPoint == null || TopPoint == null ||
                BottomPoint == null || TopLeftPoint == null || BottomLeftPoint == null || 
                BottomRightPoint == null || TopRightPoint == null)
                return;

            var color = new Scalar(255, 0, 0);
            const int thickness = 2;

            Imgproc.Circle(vyrez, CenterPoint, PupilRadius, color, thickness);
            Imgproc.Circle(vyrez, TopRightPoint, PupilRadius, color, thickness);
            Imgproc.Circle(vyrez, BottomRightPoint, PupilRadius, color, thickness);
            Imgproc.Circle(vyrez, RightPoint, PupilRadius, color, thickness);
            Imgproc.Circle(vyrez, TopLeftPoint, PupilRadius, color, thickness);
            Imgproc.Circle(vyrez, BottomLeftPoint, PupilRadius, color, thickness);
            Imgproc.Circle(vyrez, LeftPoint, PupilRadius, color, thickness);
            Imgproc.Circle(vyrez, TopPoint, PupilRadius, color, thickness);
            Imgproc.Circle(vyrez, BottomPoint, PupilRadius, color, thickness);
        }

        public Mat DetectEye(CascadeClassifier clasificator, Rect area, Rect face, int size, bool isLefteye, out Point p)
        {
            var template = new Mat();
            var mRoi = _activity.MGray.Submat(area);
            var eyes = new MatOfRect();

            clasificator.DetectMultiScale(mRoi, eyes, 1.15, 2, Objdetect.CascadeFindBiggestObject | Objdetect.CascadeScaleImage, new Size(30, 30), new Size());

            var eyesArray = eyes.ToArray();

            for (var i = 0; i < eyesArray.Length;)
            {
                var eye = eyesArray[i];
                eye.X = area.X + eye.X;
                eye.Y = area.Y + eye.Y;

                Rect eyeOnlyRectangle;

                if(isLefteye)
                    eyeOnlyRectangle = _leftEye.Insert(new Rect((int) eye.Tl().X, (int) (eye.Tl().Y + eye.Height*0.4), 
                        eye.Width, (int) (eye.Height*0.6))).GetShape();
                else
                    eyeOnlyRectangle = _rightEye.Insert(new Rect((int)eye.Tl().X, (int)(eye.Tl().Y + eye.Height * 0.4), 
                        eye.Width, (int)(eye.Height * 0.6))).GetShape();
                
                mRoi = _activity.MGray.Submat(eyeOnlyRectangle);
                var vyrez = _activity.MRgba.Submat(eyeOnlyRectangle);

                var mmG = Core.MinMaxLoc(mRoi);
                var cp = new Point(mmG.MinLoc.X + PupilRadius/2, mmG.MinLoc.Y + PupilRadius/2);

                Imgproc.Rectangle(_activity.MRgba, eyeOnlyRectangle.Tl(), eyeOnlyRectangle.Br(), new Scalar(255, 255, 255), 2);

                Point avg;
                if (isLefteye)
                {
                    avg = _leftPupil.Insert(cp).GetShape();
                    Calibrate(avg);
                    DrawCalibrationPoints(vyrez);
                }
                else
                    avg = _rightPupil.Insert(cp).GetShape();

                Imgproc.Circle(vyrez, avg, PupilRadius, new Scalar(255, 255, 255), 2);
                Imgproc.Circle(vyrez, avg, 4, new Scalar(255, 255, 255), 2);
                Imgproc.Circle(vyrez, avg, 3, new Scalar(255, 255, 255), 2);
                p = avg;
                try
                {
                    var iris = new Point(avg.X + eyeOnlyRectangle.X, avg.Y + eyeOnlyRectangle.Y);
                    if (isLefteye)
                    {
                       
                        _activity.PutOutlinedText("X: " + (int) p.X + " Y: " + (int) p.Y, 
                            (int) (iris.X + 10), (int) (iris.Y + 30));
                    }
                    else
                    {
                        _activity.PutOutlinedText("X: " + (int) p.X + " Y: " + (int) p.Y, 
                            (int) (iris.X + 10), (int) (iris.Y + 50));
                    }
                }
                catch
                {
                }
               
                return template;
            }
            p = null;
            return template;
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
                Direction = direction, Distance = Math.Sqrt(Math.Pow(distanceX, 2) + Math.Pow(distanceY, 2))
            };
        }

        /// <summary>
        /// Compare the pupil position and the center position 
        /// and determine the direction of the pupil
        /// </summary>
        /// <param name="point">pupil position</param>
        /// <returns>Direction of pupil</returns>
        public Direction? GetDirection(Point point)
        {
            if (_activity.Calibrating)
                return null;
            var distances = new List<DirectionDistance>
            {
                GetDistance(Direction.Center, point, CenterPoint), 
                GetDistance(Direction.Top, point, TopPoint), 
                GetDistance(Direction.Left, point, LeftPoint), 
                GetDistance(Direction.Right, point, RightPoint), 
                GetDistance(Direction.Bottom, point, BottomPoint), 
                GetDistance(Direction.TopRight, point, TopRightPoint), 
                GetDistance(Direction.TopLeft, point, TopLeftPoint), 
                GetDistance(Direction.BottomRight, point, BottomRightPoint), 
                GetDistance(Direction.BottomLeft, point, BottomLeftPoint)
            };

            return distances.OrderBy(d => d.Distance).First().Direction;
        }
    }
}