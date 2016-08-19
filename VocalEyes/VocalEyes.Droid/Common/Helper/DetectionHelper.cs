using System.Collections.Generic;
using System.Linq;
using VocalEyes.Common.Enumeration;
using VocalEyes.Common.Utils;
using OpenCV.Core;
using OpenCV.ImgProc;
using OpenCV.ObjDetect;
using VocalEyes.Droid.Activities;

namespace VocalEyes.Droid.Common.Helper
{
    public static class DetectionHelper
    {
        public const int JavaDetector = 0;
        private static readonly List<Point> LeftPupils = new List<Point>();
        private static readonly List<Point> RightPupils = new List<Point>();
        public static Point CenterPoint { get; private set; }
        private static readonly List<Point> Centerpoints = new List<Point>();

        public static Rect GetNearestFace(IEnumerable<Rect> faces)
        {
            Rect face = null;
            foreach (var f in faces.ToArray())
            {
                if (face == null || f.Area() > face.Area())
                    face = f;
            }
            return face;
        }

        public static Mat DetectLeftEye(this CaptureActivity activity, CascadeClassifier clasificator, Rect area, Rect face, int size, out bool pupilFound)
        {
            return activity.DetectEye(clasificator, area, face, size, true, out pupilFound);
        }

        public static Mat DetectRightEye(this CaptureActivity activity, CascadeClassifier clasificator, Rect area, Rect face, int size, out bool pupilFound)
        {
            return activity.DetectEye(clasificator, area, face, size, false, out pupilFound);
        }

        public static Mat DetectEye(this CaptureActivity activity, CascadeClassifier clasificator, Rect area, Rect face, int size, 
            bool isLefteye, out bool pupilFound)
        {
            var template = new Mat();
            var mRoi = activity.MGray.Submat(area);
            var eyes = new MatOfRect();
            var iris = new Point();

            clasificator.DetectMultiScale(
                mRoi, eyes, 1.15, 2,
                Objdetect.CascadeFindBiggestObject | Objdetect.CascadeScaleImage,
                new Size(30, 30), new Size());

            var eyesArray = eyes.ToArray();

            pupilFound = eyesArray.Any();

            for (var i = 0; i < eyesArray.Length; )
            {
                var eye = eyesArray[i];
                eye.X = area.X + eye.X;
                eye.Y = area.Y + eye.Y;
               
                var eyeOnlyRectangle = new Rect((int)eye.Tl().X,
                        (int)(eye.Tl().Y + eye.Height * 0.4), eye.Width,
                        (int)(eye.Height * 0.6));

                mRoi = activity.MGray.Submat(eyeOnlyRectangle);
                var vyrez = activity.MRgba.Submat(eyeOnlyRectangle);

                var mmG = Core.MinMaxLoc(mRoi);

                Imgproc.Rectangle(activity.MRgba, eyeOnlyRectangle.Tl(), eyeOnlyRectangle.Br(),
                    new Scalar(255, 255, 255), 2);

                if (isLefteye)
                    LeftPupils.Add(mmG.MinLoc);
                else
                    RightPupils.Add(mmG.MinLoc);

                Point avg = null;

                if (isLefteye && LeftPupils.Count >= activity.FramesPerSecond / 4)
                {
                    avg = new Point(LeftPupils.Average(p => p.X), LeftPupils.Average(p => p.Y));
                    LeftPupils.Clear();
                    LeftPupils.Add(avg);
                }
                else if(isLefteye)
                    avg = new Point(LeftPupils.Average(p => p.X), LeftPupils.Average(p => p.Y));

                if (!isLefteye && RightPupils.Count >= activity.FramesPerSecond/4)
                {
                    avg = new Point(RightPupils.Average(p => p.X), RightPupils.Average(p => p.Y));
                        RightPupils.Clear();
                        RightPupils.Add(avg);
                }
                else if (!isLefteye)
                    avg = new Point(RightPupils.Average(p => p.X), RightPupils.Average(p => p.Y));

                if (isLefteye)
                {
                    if (activity.Calibrating)
                        Centerpoints.Add(avg);
                    if (activity.Calibrating && Centerpoints.Count >= activity.FramesPerSecond * 5)
                    {
                        activity.TextToSpeechHelper.Speak(SpeechHelper.CalibrationComplete);
                        activity.Calibrating = false;
                        CenterPoint = new Point(Centerpoints.Average(c => c.X), Centerpoints.Average(c => c.Y));
                    }

                    if (CenterPoint != null)
                    {
                        var cp = new Point(CenterPoint.X + (eyeOnlyRectangle.X - area.X), 
                            CenterPoint.Y + (eyeOnlyRectangle.Y - area.Y));
                        Imgproc.Circle(activity.MRgba.Submat(area), cp, 10, new Scalar(255, 0, 0), 2);
                    }
                }
                 
                Imgproc.Circle(vyrez, avg, 10, new Scalar(255, 255, 255), 2);
                Imgproc.Circle(vyrez, avg, 4, new Scalar(255, 255, 255), 2);
                Imgproc.Circle(vyrez, avg, 3, new Scalar(255, 255, 255), 2);

                if (activity.Facing == CameraFacing.Back)
                {
                    avg.X = activity.MRgba.Width() - avg.X;
                    avg.Y = activity.MRgba.Height() - avg.Y;
                }

                try
                {
                    iris.X = avg.X + eyeOnlyRectangle.X;
                    iris.Y = avg.Y + eyeOnlyRectangle.Y;

                    if (isLefteye)
                    {
                        activity.PosLeft = avg;
                        activity.PutOutlinedText("X: " + (int)activity.PosLeft.X + " Y: " + (int)activity.PosLeft.Y, (int)(iris.X + 10),
                            (int)(iris.Y + 30));
                    }
                    else
                    {
                        activity.PosRight = avg;
                        activity.PutOutlinedText("X: " + (int)activity.PosRight.X + " Y: " + (int)activity.PosRight.Y, (int)(iris.X + 10),
                            (int)(iris.Y + 50));
                    }
                }
                catch { }
                return template;
            }
            return template;
        }
    }
}