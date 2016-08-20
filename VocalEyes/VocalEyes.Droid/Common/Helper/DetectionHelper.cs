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

namespace VocalEyes.Droid.Common.Helper
{
    public class DetectionHelper
    {
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

        private void DrawCenterPoint(CaptureActivity activity, Rect area, Rect eyeOnlyRectangle)
        {
            if (CenterPoint != null)
            {
                Imgproc.Circle(activity.MRgba.Submat(area), new Point(CenterPoint.X + (eyeOnlyRectangle.X - area.X),
                    CenterPoint.Y + (eyeOnlyRectangle.Y - area.Y)), 10, new Scalar(255, 0, 0), 2);
            }
        }

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

                Imgproc.Rectangle(_activity.MRgba, eyeOnlyRectangle.Tl(), eyeOnlyRectangle.Br(),
                    new Scalar(255, 255, 255), 2);

                Point avg;
                if (isLefteye)
                {
                    avg = _leftEye.Insert(mmG.MinLoc).GetShape();
                    Calibrate(avg);
                    DrawCenterPoint(_activity, area, eyeOnlyRectangle);
                }
                else
                    avg = _righteye.Insert(mmG.MinLoc).GetShape();
                 
                Imgproc.Circle(vyrez, avg, 10, new Scalar(255, 255, 255), 2);
                Imgproc.Circle(vyrez, avg, 4, new Scalar(255, 255, 255), 2);
                Imgproc.Circle(vyrez, avg, 3, new Scalar(255, 255, 255), 2);

                if (_activity.Facing == CameraFacing.Back)
                {
                    avg.X = _activity.MRgba.Width() - avg.X;
                    avg.Y = _activity.MRgba.Height() - avg.Y;
                }

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
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double GetDistance(Point a, Point b)
        {
            var distanceX = a.X - b.X;
            if (distanceX < 0)
                distanceX = distanceX * -1;

            var distanceY = a.Y - b.Y;
            if (distanceY < 0)
                distanceY = distanceY * -1;

            return Math.Sqrt(Math.Pow(distanceX, 2) + Math.Pow(distanceY, 2));
        }

        /// <summary>
        /// Compare the pupil position and the center position 
        /// and determine the direction of the pupil
        /// </summary>
        /// <param name="point">pupil position</param>
        /// <returns>Direction of pupil</returns>
        public Direction GetDirection(Point point)
        {
            var marginX = 10;
            var marginY = 10;

            var direction = Direction.Center;

            var centerpoint = CenterPoint;
            var diffX = point.X - centerpoint.X;
            var diffY = point.Y - centerpoint.Y;

            var distance = GetDistance(point, CenterPoint);

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

            if (_activity.Facing == CameraFacing.Front)
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
    }
}