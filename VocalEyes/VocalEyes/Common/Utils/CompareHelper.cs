using System;

namespace VocalEyes.Common.Utils
{
    public static class CompareHelper
    {
        private const double Tolerance = 0.01;

        public static bool CompareDouble(double a, double b)
        {
            return Math.Abs(a - b) < Tolerance;
        }

        public static bool CompareInt(int a, int b)
        {
            return a == b;
        }

        public static bool CompareDecimal(decimal a, decimal b)
        {
            return a == b;
        }

        public static bool CompareNullable<T>(T a, T b)
        {
            var type = typeof(T);

            if (Nullable.GetUnderlyingType(typeof(T)) == null)
                throw new InvalidOperationException("Type is not nullable or reference type.");

            return a.Equals(b);
        }
    }
}
