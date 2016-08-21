using System;
using VocalEyes.Common.Enumeration;

namespace VocalEyes.Common.Utils
{
    public static class SpeechHelper
    {
        public static string CalibrationComplete
        {
            get { return TextResources.LblCalibrationComplete; }
        }

        public static string CalibrationInit
        {
            get { return TextResources.LblCalibrationInit; }
        }

        public static string InitMessage
        {
            get { return TextResources.LblLoadingSlow; }
        }

        public static string GetDirectionString(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    return TextResources.MsgLookLeft;
                case Direction.Right:
                    return TextResources.MsgLookRight;
                case Direction.Top:
                    return TextResources.MsgLookUp;
                case Direction.Bottom:
                    return TextResources.MsgLookDown;
                case Direction.TopLeft:
                    return TextResources.MsgLookTopLeft;
                case Direction.BottomLeft:
                    return TextResources.MsgLookBottomLeft;
                case Direction.TopRight:
                    return TextResources.MsgLookTopRight;
                case Direction.BottomRight:
                    return TextResources.MsgLookBottomRight;
                case Direction.Center:
                    return TextResources.MsgLookCenter;
                default:
                    throw new ArgumentOutOfRangeException("direction", direction, null);
            }
        }
    }
}
