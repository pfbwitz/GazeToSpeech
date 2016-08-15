using System;
using System.Collections.Generic;
using System.Linq;
using OpenCV.Core;
using OpenCV.ImgProc;
using OpenCV.ObjDetect;

namespace GazeToSpeech.Droid.Common
{
    public static class DetectionHelper
    {
        private static int _blinkCount;

        public static Point GetAvgEyePoint(this DetectActivity activity)
        {
            var avgXpos = 0;
            var avgYpos = 0;

            if (activity.PosRight == null && activity.PosLeft == null)
                activity.RunOnUiThread(() => activity.PutText(new[] { activity.TextView1, activity.TextView2, activity.Textview3 }, string.Empty));
            else
            {
                if (activity.PosLeft != null)
                {
                    avgXpos = (int)(avgXpos + activity.PosLeft.X);
                    avgYpos = (int)(avgYpos + activity.PosLeft.Y);
                }
                if (activity.PosRight != null)
                {
                    avgXpos = (int)(avgXpos + activity.PosRight.X);
                    avgYpos = (int)(avgYpos + activity.PosRight.Y);
                }
                if (activity.PosRight != null && activity.PosLeft != null)
                {
                    avgXpos = avgXpos / 2;
                    avgYpos = avgYpos / 2;
                }

            }
            return new Point(avgXpos, avgYpos);
        }

        public static bool EyesAreClosed(bool pupilFoundLeft, bool pupilFoundRight)
        {
            var retVal = false;
            if (!pupilFoundLeft && !pupilFoundRight)
            {
                _blinkCount++;
                if (_blinkCount <= 20)
                    return false;
                _blinkCount = 0;
                retVal = true;
            }
            else
                _blinkCount = 0;
            return retVal;
        }

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

        public static Mat DetectEye(this DetectActivity activity, CascadeClassifier clasificator, Rect area, int size, bool isLefteye, out bool pupilFound)
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

                Imgproc.Circle(vyrez, mmG.MinLoc, 10, new Scalar(255, 255, 255), 2);
                Imgproc.Circle(vyrez, mmG.MinLoc, 4, new Scalar(255, 255, 255), 2);
                Imgproc.Circle(vyrez, mmG.MinLoc, 3, new Scalar(255, 255, 255), 2);

                iris.X = mmG.MinLoc.X + eyeOnlyRectangle.X;
                iris.Y = mmG.MaxLoc.Y + eyeOnlyRectangle.Y;

                //rect
                Imgproc.Line(activity.MRgba, new Point(iris.X, area.Y),
                new Point(iris.X, area.Y + area.Height), new Scalar(255, 0, 0));
                Imgproc.Line(activity.MRgba, new Point(area.X, iris.Y),
                   new Point(area.X + area.Width, iris.Y), new Scalar(255, 0, 0));

                //eye
                Imgproc.Line(activity.MRgba, new Point(iris.X, eyeOnlyRectangle.Y),
                    new Point(iris.X, eyeOnlyRectangle.Y + eyeOnlyRectangle.Height), new Scalar(255, 255, 255));
                Imgproc.Line(activity.MRgba, new Point(eyeOnlyRectangle.X, iris.Y),
                   new Point(eyeOnlyRectangle.X + eyeOnlyRectangle.Width, iris.Y), new Scalar(255, 255, 255));

                var x = ((iris.X - eyeOnlyRectangle.X) / eyeOnlyRectangle.Width) * 100;
                var y = ((iris.Y - eyeOnlyRectangle.Y) / eyeOnlyRectangle.Height) * 100;

                try
                {
                    if (isLefteye)
                    {

                        activity.PosLeft = new Point(x, y);
                        activity.PutOutlinedText("X: " + (int)activity.PosLeft.X + " Y: " + (int)activity.PosLeft.Y, (int)(iris.X + 10),
                            (int)(iris.Y + 30));
                        activity.RunOnUiThread(() =>
                        {
                            try
                            {
                                if (activity.PosLeft != null)
                                    activity.TextView1.Text = "Linkeroog X: " + (int)activity.PosLeft.X + " Y: " + (int)activity.PosLeft.Y;
                            }
                            catch (NullReferenceException) { }
                        });
                    }
                    else
                    {
                        activity.PosRight = new Point(x, y);
                        activity.PutOutlinedText("X: " + (int)activity.PosRight.X + " Y: " + (int)activity.PosRight.Y, (int)(iris.X + 10),
                           (int)(iris.Y + 50));
                        if (activity.PosRight != null)
                            activity.RunOnUiThread(() =>
                            {
                                try
                                {
                                    activity.TextView2.Text = "Rechteroog X: " + (int)activity.PosRight.X + " Y: " + (int)activity.PosRight.Y;
                                }
                                catch (NullReferenceException) { }
                            });
                    }
                }
                catch { }

                var eyeTemplate = new Rect((int)iris.X - size / 2, (int)iris.Y - size / 2, size, size);
                template = activity.MGray.Submat(eyeTemplate).Clone();

                return template;
            }
            return template;
        }

        public static Mat DetectLeftEye(this DetectActivity activity, CascadeClassifier clasificator, Rect area, int size, out bool pupilFound)
        {
            return activity.DetectEye(clasificator, area, size, true, out pupilFound);
        }

        public static Mat DetectRightEye(this DetectActivity activity, CascadeClassifier clasificator, Rect area, int size, out bool pupilFound)
        {
            return activity.DetectEye(clasificator, area, size, false, out pupilFound);
        }

        //private static int _eyeClosed;
        //[Untested("Untested")]
        //public static bool DetectBlink(this DetectActivity activity, Rect area, Mat mTemplate)
        //{
        //    Mat mROI = activity.MGray.Submat(area);
        //    var resultCols = mROI.Cols() - mTemplate.Cols() + 1;
        //    var resultRows = mROI.Rows() - mTemplate.Rows() + 1;
        //    // Check for bad template size
        //    if (mTemplate.Cols() == 0 || mTemplate.Rows() == 0)
        //        return false;

        //    Mat mResult = new Mat(resultCols, resultRows, CvType.Cv8u);

        //    Imgproc.MatchTemplate(mROI, mTemplate, mResult,
        //                  Imgproc.TmSqdiffNormed);

        //    var mmres = Core.MinMaxLoc(mResult);

        //    if (mmres.MinVal < 0)
        //        _eyeClosed = _eyeClosed + 1;
        //    if (_eyeClosed == 10)
        //    {
        //        _eyeClosed = 0;
        //        return true;
        //    }
        //    return false;
        //}
    }
}