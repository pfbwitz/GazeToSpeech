using System;
using System.Collections.Generic;
using System.Linq;
using GazeToSpeech.Common.Enumeration;
using OpenCV.Core;
using OpenCV.ImgProc;
using OpenCV.ObjDetect;

namespace GazeToSpeech.Droid.Common.Helper
{
    public static class DetectionHelper
    {
        public const int JavaDetector = 0;
        public static int Learned = 0;

        public static Point GetAvgEyePoint(this CaptureActivity activity)
        {
            var avgXpos = 0;
            var avgYpos = 0;

            if (activity.PosRight != null || activity.PosLeft != null)
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

        private static readonly List<Point> LeftPupils = new List<Point>();
        private static readonly List<Point> RightPupils = new List<Point>();
        private static int _drawCountLeft;
        private static int _drawCountRight;

        public static Mat DetectLeftEye(this CaptureActivity activity, CascadeClassifier clasificator, Rect area, Rect face, int size, out bool pupilFound)
        {
            return activity.DetectEye(clasificator, area, face, size, true, out pupilFound);
        }

        public static Mat DetectRightEye(this CaptureActivity activity, CascadeClassifier clasificator, Rect area, Rect face, int size, out bool pupilFound)
        {
            return activity.DetectEye(clasificator, area, face, size, false, out pupilFound);
        }

        public static Mat DetectEye(this CaptureActivity activity, CascadeClassifier clasificator, Rect area, Rect face, int size, bool isLefteye, out bool pupilFound)
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

                //// First predict, to update the internal statePre variable
                // var filter = _kalmanFilter ?? (_kalmanFilter = new KalmanFilter());
                // filter.Set_statePre(mRoi);
                //var prediction = filter.Predict();
                //var correct = filter.Correct(prediction);
                //var predictPt = new Point(correct.ToArray<double>()[0], correct.ToArray<double>()[0]);


                var frameSkip = 5;
                if (isLefteye && LeftPupils.Count >= frameSkip)
                {
                    _drawCountLeft++;
                    avg = new Point(LeftPupils.Average(p => p.X), LeftPupils.Average(p => p.Y));

                    if (_drawCountLeft >= frameSkip)
                    {
                        _drawCountLeft = 0;
                        LeftPupils.Clear();
                    }
                }
                else if(isLefteye && LeftPupils.Any())
                    avg = new Point(LeftPupils.Average(p => p.X), LeftPupils.Average(p => p.Y));
                
                if (!isLefteye && RightPupils.Count >= frameSkip)
                {
                    _drawCountRight++;
                    avg = new Point(RightPupils.Average(p => p.X), RightPupils.Average(p => p.Y));

                    if (_drawCountRight >= frameSkip)
                    {
                        _drawCountRight = 0;
                        RightPupils.Clear();
                    }
                }
                else if (!isLefteye && RightPupils.Any())
                    avg = new Point(RightPupils.Average(p => p.X), RightPupils.Average(p => p.Y));
                
                if (avg != null)
                {
                    Imgproc.Circle(vyrez, avg, 10, new Scalar(255, 255, 255), 2);
                    Imgproc.Circle(vyrez, avg, 4, new Scalar(255, 255, 255), 2);
                    Imgproc.Circle(vyrez, avg, 3, new Scalar(255, 255, 255), 2);

                    iris.X = avg.X + eyeOnlyRectangle.X;
                    iris.Y = avg.Y + eyeOnlyRectangle.Y;

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

                    var x = (mmG.MinLoc.X + eyeOnlyRectangle.X - area.X) / area.Width * 100;
                    var y = (mmG.MinLoc.Y + eyeOnlyRectangle.Y - area.Y) / area.Height * 100;

                    if (activity.Facing == CameraFacing.Back)
                    {
                        x = 100 - x;
                        y = 100 - y;
                    }

                    try
                    {
                        if (isLefteye)
                        {
                            activity.PosLeft = new Point(x, y);
                            activity.PutOutlinedText("X: " + (int)activity.PosLeft.X + " Y: " + (int)activity.PosLeft.Y, (int)(iris.X + 10),
                                (int)(iris.Y + 30));
                          
                        }
                        else
                        {
                            activity.PosRight = new Point(x, y);
                            activity.PutOutlinedText("X: " + (int)activity.PosRight.X + " Y: " + (int)activity.PosRight.Y, (int)(iris.X + 10),
                               (int)(iris.Y + 50));
                        }
                    }
                    catch { }

                    var eyeTemplate = new Rect((int)iris.X - size / 2, (int)iris.Y - size / 2, size, size);
                    template = activity.MGray.Submat(eyeTemplate).Clone();
                }
               
                

                return template;
            }
            return template;
        }

        [Obsolete]
        public static Mat GetTemplate(this CaptureActivity activity, CascadeClassifier clasificator, Rect area, int size)
        {
            var template = new Mat();
            var mRoi = activity.MGray.Submat(area);
            var eyes = new MatOfRect();
            var iris = new Point();
            clasificator.DetectMultiScale(
                mRoi, 
                eyes, 
                1.15, 
                2,
                Objdetect.CascadeFindBiggestObject | Objdetect.CascadeScaleImage, 
                new Size(30, 30), new Size()
                );

            var eyesArray = eyes.ToArray();

            foreach (var e in eyesArray)
            {
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

        [Obsolete]
        public static void MatchEye(this CaptureActivity activity, Rect area, Mat mTemplate, int type)
        {
            Point matchLoc;
            var mRoi = activity.MGray.Submat(area);
            var resultCols = mRoi.Cols() - mTemplate.Cols() + 1;
            var resultRows = mRoi.Rows() - mTemplate.Rows() + 1;
            // Check for bad template size
            if (mTemplate.Cols() == 0 || mTemplate.Rows() == 0)
                return;
            
            var mResult = new Mat(resultCols, resultRows, CvType.Cv8u);

            switch (type)
            {
                case MatchingMethod.TmSqdiff:
                    Imgproc.MatchTemplate(mRoi, mTemplate, mResult, Imgproc.TmSqdiff);
                    break;
                case MatchingMethod.TmSqdiffNormed:
                    Imgproc.MatchTemplate(mRoi, mTemplate, mResult, Imgproc.TmSqdiffNormed);
                    break;
                case MatchingMethod.TmCcoeff:
                    Imgproc.MatchTemplate(mRoi, mTemplate, mResult, Imgproc.TmCcoeff);
                    break;
                case MatchingMethod.TmCcoeffNormed:
                    Imgproc.MatchTemplate(mRoi, mTemplate, mResult, Imgproc.TmCcoeffNormed);
                    break;
                case MatchingMethod.TmCcorr:
                    Imgproc.MatchTemplate(mRoi, mTemplate, mResult, Imgproc.TmCcorr);
                    break;
                case MatchingMethod.TmCcorrNormed:
                    Imgproc.MatchTemplate(mRoi, mTemplate, mResult, Imgproc.TmCcorrNormed);
                    break;
            }

            var mmres = Core.MinMaxLoc(mResult);
            // there is difference in matching methods - best match is max/min value
            if (type == MatchingMethod.TmSqdiff || type == MatchingMethod.TmSqdiffNormed)
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

        [Obsolete]
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
    }
}