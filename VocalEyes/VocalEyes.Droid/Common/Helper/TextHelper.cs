using Android.Widget;
using OpenCV.Core;
using OpenCV.ImgProc;
using VocalEyes.Droid.Activities;

namespace VocalEyes.Droid.Common.Helper
{
    public static class TextHelper
    {
        public static void PutOutlinedText(this CaptureActivity activity, Mat mat, string text, double x, double y)
        {
            PutOutlinedText(activity, mat, text, x, y, 1.2, new Scalar(255, 255, 255, 255));
        }

        public static void PutOutlinedText(this CaptureActivity activity, Mat mat, string text, double x, double y, double fontsize, Scalar color)
        {
            var thickness = 1;

            PutText( mat, text, x - thickness, y, new Scalar(0, 0, 0, 255), fontsize);
            PutText( mat, text, x + thickness, y, new Scalar(0, 0, 0, 255), fontsize);
            PutText( mat, text, x, y - thickness, new Scalar(0, 0, 0, 255), fontsize);
            PutText( mat, text, x, y + thickness, new Scalar(0, 0, 0, 255), fontsize);

            PutText( mat, text, x - (thickness + 1), y, new Scalar(0, 0, 0, 255), fontsize);
            PutText( mat, text, x + thickness + 1, y, new Scalar(0, 0, 0, 255), fontsize);
            PutText( mat, text, x, y - (thickness + 1), new Scalar(0, 0, 0, 255), fontsize);
            PutText( mat, text, x, y + thickness + 1, new Scalar(0, 0, 0, 255), fontsize);

            PutText( mat, text, x, y, color, fontsize);
        }

        private static void PutText(Mat mat, string text, double x, double y, Scalar color, double fontsize)
        {
            Imgproc.PutText(mat, text, new Point(x, y),
                Core.FontHersheyPlain, fontsize, color, 2);
        }

        public static void PutText(this CaptureActivity activity, TextView textView, string text)
        {
            PutText(activity, new[] { textView }, text);
        }

        public static void PutText(this CaptureActivity activity, TextView[] textViews, string text)
        {
            activity.RunOnUiThread(() =>
            {
                foreach (var t in textViews)
                    t.Text = text;
            });
        }
    }
}