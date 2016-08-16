using System;
using System.Collections.Generic;
using System.Linq;
using OpenCV.Core;
using OpenCV.ImgProc;
using OpenCV.ObjDetect;

namespace GazeToSpeech.Droid.Common.Helper
{
    public static class DetectionHelper
    {
        public const int JavaDetector = 0;
        public const int TmSqdiff = 0;
        public const int TmSqdiffNormed = 1;
        public const int TmCcoeff = 2;
        public const int TmCcoeffNormed = 3;
        public const int TmCcorr = 4;
        public const int TmCcorrNormed = 5;
        public static int Learned = 0;

        public static Point GetAvgEyePoint(this CaptureActivity activity)
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

        private static List<Point> _leftPupils = new List<Point>();
        private static List<Point> _rightPupils = new List<Point>();
        private static int _drawCountLeft = 0;
        private static int _drawCountRight = 0;

        public static Mat DetectEye(this CaptureActivity activity, CascadeClassifier clasificator, Rect area, int size, bool isLefteye, out bool pupilFound)
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
                //Imgproc.Rectangle(activity.MRgba, eye.Tl(), eye.Br(), new Scalar(0, 255, 255), 3);
                var eyeOnlyRectangle = new Rect((int)eye.Tl().X,
                        (int)(eye.Tl().Y + eye.Height * 0.4), eye.Width,
                        (int)(eye.Height * 0.6));

                if (activity.Calibrating)
                {
                    if(isLefteye && activity.LeftRectCaptures.Count < 10)
                        activity.LeftRectCaptures.Add(eyeOnlyRectangle);
                    else if (activity.RightRectCaptures.Count < 10)
                        activity.RightRectCaptures.Add(eyeOnlyRectangle);

                    if (activity.LeftRectCaptures.Count == 10 && activity.RightRectCaptures.Count == 10)
                    {
                        var avgLeftWidth = (int)activity.LeftRectCaptures.Average(c => c.Width);
                        var avgLeftHeight = (int)activity.LeftRectCaptures.Average(c => c.Height);
                        var avgLeftX = (int)activity.LeftRectCaptures.Average(c => c.X);
                        var avgLeftY = (int)activity.LeftRectCaptures.Average(c => c.Y);
                        activity.AvgLeftEye = new Rect(avgLeftX, avgLeftY, avgLeftWidth, avgLeftHeight);

                        var avgRightWidth = (int)activity.RightRectCaptures.Average(c => c.Width);
                        var avgRightHeight = (int)activity.RightRectCaptures.Average(c => c.Height);
                        var avgRightX = (int)activity.RightRectCaptures.Average(c => c.X);
                        var avgRightY = (int)activity.RightRectCaptures.Average(c => c.Y);
                        activity.AvgRightEye = new Rect(avgRightX, avgRightY, avgRightWidth, avgRightHeight);

                        activity.Calibrating = false;
                    }

                    return template;
                }

                mRoi = activity.MGray.Submat(eyeOnlyRectangle);
                var vyrez = activity.MRgba.Submat(eyeOnlyRectangle);

                var mmG = Core.MinMaxLoc(mRoi);

                DrawAvgRectangles(activity, isLefteye, area);

                Imgproc.Rectangle(activity.MRgba, eyeOnlyRectangle.Tl(), eyeOnlyRectangle.Br(),
                    new Scalar(255, 255, 255), 2);

                if (isLefteye)
                    _leftPupils.Add(mmG.MinLoc);
                else
                    _rightPupils.Add(mmG.MinLoc);

                Point avg = mmG.MinLoc;
                var frameSkip = 20;
                if (isLefteye && _leftPupils.Count >= frameSkip)
                {
                    _drawCountLeft++;
                    avg = new Point(_leftPupils.Average(p => p.X), _leftPupils.Average(p => p.Y));

                    if (_drawCountLeft >= frameSkip)
                    {
                        _drawCountLeft = 0;
                        _leftPupils.Clear();
                    }
                }
                else if (_rightPupils.Count >= frameSkip)
                {
                    _drawCountRight++;
                    avg = new Point(_rightPupils.Average(p => p.X), _rightPupils.Average(p => p.Y));

                    if (_drawCountRight >= frameSkip)
                    {
                        _drawCountRight = 0;
                        _rightPupils.Clear();
                    }
                }

                Imgproc.Circle(vyrez, avg, 10, new Scalar(255, 255, 255), 2);
                Imgproc.Circle(vyrez, avg, 4, new Scalar(255, 255, 255), 2);
                Imgproc.Circle(vyrez, avg, 3, new Scalar(255, 255, 255), 2);
               
                iris.X = mmG.MinLoc.X + eyeOnlyRectangle.X;
                iris.Y = mmG.MaxLoc.Y + eyeOnlyRectangle.Y;

                //rect
                //Imgproc.Line(activity.MRgba, new Point(iris.X, area.Y),
                //new Point(iris.X, area.Y + area.Height), new Scalar(255, 0, 0));
                //Imgproc.Line(activity.MRgba, new Point(area.X, iris.Y),
                //   new Point(area.X + area.Width, iris.Y), new Scalar(255, 0, 0));

                //eye
                //Imgproc.Line(activity.MRgba, new Point(iris.X, eyeOnlyRectangle.Y),
                //    new Point(iris.X, eyeOnlyRectangle.Y + eyeOnlyRectangle.Height), new Scalar(255, 255, 255));
                //Imgproc.Line(activity.MRgba, new Point(eyeOnlyRectangle.X, iris.Y),
                //   new Point(eyeOnlyRectangle.X + eyeOnlyRectangle.Width, iris.Y), new Scalar(255, 255, 255));

                var x = (avg.X / eyeOnlyRectangle.Width) * 100;
                var y = (avg.Y / eyeOnlyRectangle.Height) * 100;

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

        public static Mat GetTemplate(this CaptureActivity activity, CascadeClassifier clasificator, Rect area, int size)
        {
            Mat template = new Mat();
            Mat mRoi = activity.MGray.Submat(area);
            MatOfRect eyes = new MatOfRect();
            Point iris = new Point();
            clasificator.DetectMultiScale(mRoi, eyes, 1.15, 2,
                    Objdetect.CascadeFindBiggestObject
                            | Objdetect.CascadeScaleImage, new Size(30, 30),
                    new Size());

            Rect[] eyesArray = eyes.ToArray();
            for (int i = 0; i < eyesArray.Length; )
            {
                var e = eyesArray[i];
                e.X = area.X + e.X;
                e.Y = area.Y + e.Y;
                var eyeOnlyRectangle = new Rect((int)e.Tl().X,
                        (int)(e.Tl().Y + e.Height * 0.4), (int)e.Width,
                        (int)(e.Height * 0.6));
                mRoi = activity.MGray.Submat(eyeOnlyRectangle);
                var vyrez = activity.MRgba.Submat(eyeOnlyRectangle);


                var mmG = Core.MinMaxLoc(mRoi);

                Imgproc.Circle(vyrez, mmG.MinLoc, 2, new Scalar(255, 255, 255, 255), 2);
                iris.X = mmG.MinLoc.X + eyeOnlyRectangle.X;
                iris.Y = mmG.MinLoc.Y + eyeOnlyRectangle.Y;
                var eyeTemplate = new Rect((int)iris.X - size / 2, (int)iris.Y
                                                                     - size / 2, size, size);
                Imgproc.Rectangle(activity.MRgba, eyeTemplate.Tl(), eyeTemplate.Br(),
                        new Scalar(255, 0, 0, 255), 2);
                template = (activity.MGray.Submat(eyeTemplate)).Clone();
                return template;
            }
            return template;
        } 

        public static void MatchEye(this CaptureActivity activity, Rect area, Mat mTemplate, int type)
        {
            Point matchLoc;
            var mROI = activity.MGray.Submat(area);
            int resultCols = mROI.Cols() - mTemplate.Cols() + 1;
            int resultRows = mROI.Rows() - mTemplate.Rows() + 1;
            // Check for bad template size
            if (mTemplate.Cols() == 0 || mTemplate.Rows() == 0)
            {
                return;
            }
            var mResult = new Mat(resultCols, resultRows, CvType.Cv8u);

            switch (type)
            {
                case TmSqdiff:
                    Imgproc.MatchTemplate(mROI, mTemplate, mResult, Imgproc.TmSqdiff);
                    break;
                case TmSqdiffNormed:
                    Imgproc.MatchTemplate(mROI, mTemplate, mResult, Imgproc.TmSqdiffNormed);
                    break;
                case TmCcoeff:
                    Imgproc.MatchTemplate(mROI, mTemplate, mResult, Imgproc.TmCcoeff);
                    break;
                case TmCcoeffNormed:
                    Imgproc.MatchTemplate(mROI, mTemplate, mResult, Imgproc.TmCcoeffNormed);
                    break;
                case TmCcorr:
                    Imgproc.MatchTemplate(mROI, mTemplate, mResult, Imgproc.TmCcorr);
                    break;
                case TmCcorrNormed:
                    Imgproc.MatchTemplate(mROI, mTemplate, mResult, Imgproc.TmCcorrNormed);
                    break;
            }

            var mmres = Core.MinMaxLoc(mResult);
            // there is difference in matching methods - best match is max/min value
            if (type == TmSqdiff || type == TmSqdiffNormed)
                matchLoc = mmres.MinLoc;
            else
                matchLoc = mmres.MaxLoc;
            

            var matchLocTx = new Point(matchLoc.X + area.X, matchLoc.Y + area.Y);
            var matchLocTy = new Point(matchLoc.X + mTemplate.Cols() + area.X,
                    matchLoc.Y + mTemplate.Rows() + area.Y);

            Imgproc.Rectangle(activity.MRgba, matchLocTx, matchLocTy, new Scalar(255, 255, 0,
                    255));
            var rec = new Rect(matchLocTx, matchLocTy);


        }

        public static void DrawAvgRectangles(this CaptureActivity activity, bool isLefteye, Rect area)
        {
            //var r = isLefteye ? activity.AvgLeftEye : activity.AvgRightEye;

            //Imgproc.Rectangle(
            //  activity.MRgba,
            //  new Point(area.X + ((area.X - r.Width) / 2), area.Y + ((area.Y - r.Height) / 2)),
            //  new Point(area.Br().X - (((area.X - r.Width) / 2)), area.Br().Y - (((area.X - r.Width) / 2))),
            //  new Scalar(255, 0, 255)
            //  ); 

            //Imgproc.Rectangle(
            //    activity.MRgba, 
            //    new Point(area.X + ((area.X - r.Width)/2), area.Y + ((area.Y - r.Height)/2)),
            //    new Point(area.Br().X - (((area.X - r.Width) / 2)), area.Br().Y - (((area.X - r.Width) / 2))), 
            //    new Scalar(255, 0, 255)
            //    ); 
        }

        public static Mat DetectLeftEye(this CaptureActivity activity, CascadeClassifier clasificator, Rect area, int size, out bool pupilFound)
        {
            return activity.DetectEye(clasificator, area, size, true, out pupilFound);
        }

        public static Mat DetectRightEye(this CaptureActivity activity, CascadeClassifier clasificator, Rect area, int size, out bool pupilFound)
        {
            return activity.DetectEye(clasificator, area, size, false, out pupilFound);
        }
    }
}