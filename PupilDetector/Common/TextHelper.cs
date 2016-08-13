using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OpenCV.Core;
using OpenCV.ImgProc;

namespace PupilDetector.Common
{
    public static class TextHelper
    {
        public static void PutOutlinedText(this DetectActivity activity, string text, int x, int y)
        {
            var thickness = 1;

            PutText(activity, text, x - thickness, y, new Scalar(0, 0, 0, 255));
            PutText(activity, text, x + thickness, y, new Scalar(0, 0, 0, 255));
            PutText(activity, text, x, y - thickness, new Scalar(0, 0, 0, 255));
            PutText(activity, text, x, y + thickness, new Scalar(0, 0, 0, 255));

            PutText(activity, text, x - (thickness + 1), y, new Scalar(0, 0, 0, 255));
            PutText(activity, text, x + thickness + 1, y, new Scalar(0, 0, 0, 255));
            PutText(activity, text, x, y - (thickness + 1), new Scalar(0, 0, 0, 255));
            PutText(activity, text, x, y + thickness + 1, new Scalar(0, 0, 0, 255));

            PutText(activity, text, x, y, new Scalar(255, 255, 255, 255));
        }

        private static void PutText(DetectActivity activity, string text, int x, int y, Scalar color)
        {
            Imgproc.PutText(activity.MRgba, text, new Point(x, y),
                Core.FontHersheyPlain, 1.3, color);
        }

        public static void PutText(this DetectActivity activity, TextView textView, string text)
        {
            PutText(activity, new[] { textView }, text);
        }

        public static void PutText(this DetectActivity activity, TextView[] textViews, string text)
        {
            activity.RunOnUiThread(() =>
            {
                foreach (var t in textViews)
                    t.Text = text;
            });
        }
    }
}