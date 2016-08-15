using System.Collections.Generic;
using System.Linq;

namespace GazeToSpeech
{
    public class Position
    {
        public double X;
        public double Y;

        public Position(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public class PositionHelper
    {
        public static bool IsPointInArea(Position tap, IEnumerable<Position> vertices)
        {
            var intersectCount = 0;
            var latLngs = vertices.ToArray();
            for (var i = 0; i < latLngs.Length - 1; i++)
            {
                if (RayCastIntersect(tap, latLngs.ElementAt(i), latLngs.ElementAt(i + 1)))
                    intersectCount++;
            }
            return intersectCount % 2 == 1;
        }

        private static bool RayCastIntersect(Position tap, Position vertA, Position vertB)
        {
            var aY = vertA.Y;
            var bY = vertB.Y;
            var aX = vertA.X;
            var bX = vertB.X;
            var pY = tap.Y;
            var pX = tap.X;

            if ((aY > pY && bY > pY) || (aY < pY && bY < pY) || (aX < pX && bX < pX))
                return false;

            var m = (aY - bY) / (aX - bX);
            var bee = -aX * m + aY;
            var x = (pY - bee) / m;

            return x > pX;
        }
    }
}
