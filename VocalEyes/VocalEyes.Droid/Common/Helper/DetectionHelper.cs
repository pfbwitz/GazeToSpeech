using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly List<Point> _centerpoints = new List<Point>();

        private Pupil _leftEye;

        private Pupil _righteye;

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

        public Mat DetectLeftEye(CascadeClassifier clasificator, Rect area, Rect face, int size)
        {
            if (_leftEye == null)
                _leftEye = new Pupil(Constants.PupilSkip);
            return DetectEye(clasificator, area, face, size, true);
        }

        public Mat DetectRightEye(CascadeClassifier clasificator, Rect area, Rect face, int size)
        {
            if (_righteye == null)
                _righteye = new Pupil(Constants.PupilSkip);
            return DetectEye(clasificator, area, face, size, false);
        }

        private void Calibrate(Point point)
        {
            if (_activity.Calibrating)
                _centerpoints.Add(point);
            if (_activity.Calibrating && _centerpoints.Count >= _activity.FramesPerSecond * 5)
            {
                _activity.TextToSpeechHelper.Speak(SpeechHelper.CalibrationComplete);
                _activity.Calibrating = false;
                CenterPoint = new Point(_centerpoints.Average(c => c.X), _centerpoints.Average(c => c.Y));
            }
        }

        private void DrawCenterPoint(Mat vyrez, CaptureActivity activity, Rect area, Rect eyeOnlyRectangle)
        {
            if (CenterPoint != null)
            {
                var centerX = CenterPoint.X;// + (eyeOnlyRectangle.X - area.X);
                var centerY = CenterPoint.Y;// + (eyeOnlyRectangle.Y - area.Y);
                //var mat = activity.MRgba.Submat(area);
                var mat = vyrez;

                Imgproc.Circle(mat, new Point(centerX, centerY),
                    PupilRadius, new Scalar(255, 0, 0), 2);
#if DEBUG
                const int factor = 1;
                //top-right
                Imgproc.Circle(mat, new Point(centerX + PupilRadius * factor, centerY - PupilRadius * factor),
                   PupilRadius, new Scalar(255, 0, 0), 2);

                //bottom-right
                Imgproc.Circle(mat, new Point(centerX + PupilRadius * factor, centerY + PupilRadius * factor),
                   PupilRadius, new Scalar(255, 0, 0), 2);

                //right
                Imgproc.Circle(mat, new Point(centerX + PupilRadius * factor, centerY),
                   PupilRadius, new Scalar(255, 0, 0), 2);

                //top-left
                Imgproc.Circle(mat, new Point(centerX - PupilRadius * factor, centerY - PupilRadius * factor),
                  PupilRadius, new Scalar(255, 0, 0), 2);

                //bottom-left
                Imgproc.Circle(mat, new Point(centerX - PupilRadius * factor, centerY + PupilRadius * factor),
                  PupilRadius, new Scalar(255, 0, 0), 2);

                //left
                Imgproc.Circle(mat, new Point(centerX - PupilRadius * factor, centerY),
                  PupilRadius, new Scalar(255, 0, 0), 2);

                //top
                Imgproc.Circle(mat, new Point(centerX, centerY + PupilRadius * factor),
                PupilRadius, new Scalar(255, 0, 0), 2);

                //bottom
                Imgproc.Circle(mat, new Point(centerX, centerY - PupilRadius * factor),
                PupilRadius, new Scalar(255, 0, 0), 2);
#endif
            }
        }

        private Point _top;
        private Point _left;
        private Point _right;
        private Point _bottom;
        private Point _bottomLeft;
        private Point _topLeft;
        private Point _bottomRight;
        private Point _topRight;

        public Mat DetectEye(CascadeClassifier clasificator, Rect area, Rect face, int size, 
            bool isLefteye)
        {
            var template = new Mat();
            var mRoi = _activity.MGray.Submat(area);
            var eyes = new MatOfRect();
           
            clasificator.DetectMultiScale(
                mRoi, eyes, 1.15, 2,
                Objdetect.CascadeFindBiggestObject | Objdetect.CascadeScaleImage,
                new Size(30, 30), new Size());

            var eyesArray = eyes.ToArray();

            for (var i = 0; i < eyesArray.Length; )
            {
                var eye = eyesArray[i];
                eye.X = area.X + eye.X;
                eye.Y = area.Y + eye.Y;
               
                var eyeOnlyRectangle = new Rect((int)eye.Tl().X,
                        (int)(eye.Tl().Y + eye.Height * 0.4), eye.Width,
                        (int)(eye.Height * 0.6));

                mRoi = _activity.MGray.Submat(eyeOnlyRectangle);
                var vyrez = _activity.MRgba.Submat(eyeOnlyRectangle);

                var mmG = Core.MinMaxLoc(mRoi);
                var cp = new Point(mmG.MinLoc.X + PupilRadius/2, mmG.MinLoc.Y + PupilRadius/2);

                Imgproc.Rectangle(_activity.MRgba, eyeOnlyRectangle.Tl(), eyeOnlyRectangle.Br(),
                    new Scalar(255, 255, 255), 2);

                Point avg;
                if (isLefteye)
                {
                    avg = _leftEye.Insert(cp).GetShape();
                    Calibrate(avg);
                    DrawCenterPoint(vyrez, _activity, area, eyeOnlyRectangle);
                }
                else
                    avg = _righteye.Insert(cp).GetShape();

                if (CenterPoint != null)
                {
                    var factor = 1;
                    _top = new Point(CenterPoint.X, CenterPoint.Y - PupilRadius * factor);
                    _left = new Point(CenterPoint.X - PupilRadius * factor, CenterPoint.Y);
                    _right = new Point(CenterPoint.X + PupilRadius * factor, CenterPoint.Y);
                    _bottom = new Point(CenterPoint.X, CenterPoint.Y + PupilRadius * factor);

                    _bottomLeft = new Point(CenterPoint.X - PupilRadius * factor, CenterPoint.Y + PupilRadius * factor);
                    _topLeft = new Point(CenterPoint.X - PupilRadius * factor, CenterPoint.Y - PupilRadius * factor);
                    _bottomRight = new Point(CenterPoint.X + PupilRadius * factor, CenterPoint.Y + PupilRadius * factor);
                    _topRight = new Point(CenterPoint.X + PupilRadius * factor, CenterPoint.Y - PupilRadius * factor);
                }

                Imgproc.Circle(vyrez, avg, PupilRadius, new Scalar(255, 255, 255), 2);
                Imgproc.Circle(vyrez, avg, 4, new Scalar(255, 255, 255), 2);
                Imgproc.Circle(vyrez, avg, 3, new Scalar(255, 255, 255), 2);

                try
                {
                    var iris = new Point(avg.X + eyeOnlyRectangle.X, avg.Y + eyeOnlyRectangle.Y);

                    if (isLefteye)
                    {
                        _activity.PosLeft = avg;
                        _activity.PutOutlinedText("X: " + (int)_activity.PosLeft.X + " Y: " + (int)_activity.PosLeft.Y, (int)(iris.X + 10),
                            (int)(iris.Y + 30));
                    }
                    else
                    {
                        _activity.PosRight = avg;
                        _activity.PutOutlinedText("X: " + (int)_activity.PosRight.X + " Y: " + (int)_activity.PosRight.Y, (int)(iris.X + 10),
                            (int)(iris.Y + 50));
                    }
                }
                catch { }
                return template;
            }
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
                distanceX = distanceX * -1;

            var distanceY = a.Y - b.Y;
            if (distanceY < 0)
                distanceY = distanceY * -1;
            return new DirectionDistance
            {
                Direction = direction,
                Distance = Math.Sqrt(Math.Pow(distanceX, 2) + Math.Pow(distanceY, 2))
            };
        }

        /// <summary>
        /// Compare the pupil position and the center position 
        /// and determine the direction of the pupil
        /// </summary>
        /// <param name="point">pupil position</param>
        /// <returns>Direction of pupil</returns>
        public Direction GetDirection(Point point)
        {
            var distances = new List<DirectionDistance>
            {
                GetDistance(Direction.Center, point, CenterPoint),
                GetDistance(Direction.Top, point, _top),
                GetDistance(Direction.Left, point, _left),
                GetDistance(Direction.Right, point, _right),
                GetDistance(Direction.Bottom, point, _bottom),
                GetDistance(Direction.TopRight, point, _topRight),
                GetDistance(Direction.TopLeft, point, _topLeft),
                GetDistance(Direction.BottomRight, point, _bottomRight),
                GetDistance(Direction.BottomLeft, point, _bottomLeft)
            };
            var smallestDistance = distances.Min(d => d.Distance);
            var direction = distances.Single(d => d.Distance == smallestDistance).Direction;

            //reverse horizontal direction if using the front camera
            if (_activity.Facing == CameraFacing.Front)
            {
                if (direction == Direction.BottomLeft)
                    direction = Direction.BottomRight;
                else if (direction == Direction.TopLeft)
                    direction = Direction.TopRight;
                else if (direction == Direction.Left)
                    direction = Direction.Right;

                if (direction == Direction.BottomRight)
                    direction = Direction.BottomLeft;
                else if (direction == Direction.TopRight)
                    direction = Direction.TopLeft;
                else if (direction == Direction.Right)
                    direction = Direction.Left;
            }

            return direction;
        }
    }
}