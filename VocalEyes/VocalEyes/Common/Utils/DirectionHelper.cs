using System;
using VocalEyes.Common.Enumeration;

namespace VocalEyes.Common.Utils
{
    public static class DirectionHelper
    {
        public static Direction Reverse(Direction @in) 
        {
            Direction @out;
            switch (@in)
            {
                case Direction.Left:
                    @out = Direction.Right;
                    break;
                case Direction.Right:
                    @out = Direction.Left;
                    break;
                case Direction.Top:
                    @out = Direction.Top;
                    break;
                case Direction.Bottom:
                    @out = Direction.Bottom;
                    break;
                case Direction.TopLeft:
                    @out = Direction.TopRight;
                    break;
                case Direction.BottomLeft:
                    @out = Direction.BottomRight;
                    break;
                case Direction.TopRight:
                    @out = Direction.TopLeft;
                    break;
                case Direction.BottomRight:
                    @out = Direction.BottomLeft;
                    break;
                case Direction.Center:
                    @out = Direction.Center;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("in", @in, null);
            }
            return @out;
        }
    }
}
