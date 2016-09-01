using System;

namespace VocalEyes.Common.Utils
{
    public static class TrigonometryHelper
    {
        public static double GetAngleCosine(double hypotenuse, double adjacent)
        {
            return Math.Acos(adjacent / hypotenuse) * (180 / Math.PI);
        }

        public static double GetAngleTangent(double adjacent, double opposite)
        {
            return Math.Atan(opposite / adjacent) * (180 / Math.PI);
        }

        public static double GetAngleSine(double hypotenuse, double opposite)
        {
            return Math.Asin(opposite / hypotenuse) * (180 / Math.PI);
        }

        public static double GetHypetonuse(double opposite, double adjacent)
        {
            var aSquared = Math.Pow(opposite, 2);
            var bSquared = Math.Pow(adjacent, 2);
            var sum = aSquared + bSquared;
            var h = Math.Sqrt(sum);
            return h;
        }

        public static double GetDistance(double a, double b)
        {
            var distance = a - b;
            if (distance < 0)
                distance = distance * -1;

            return distance;
        }

        public static int GetDistance(int a, int b)
        {
            var distance = a - b;
            if (distance < 0)
                distance = distance * -1;

            return distance;
        }
    }
}
