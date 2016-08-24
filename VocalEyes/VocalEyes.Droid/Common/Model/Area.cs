using System.Collections.Generic;
using System.Linq;
using OpenCV.Core;

namespace VocalEyes.Droid.Common.Model
{
    public abstract class Area
    {
        protected readonly List<int> RectanglesX = new List<int>();
        protected readonly List<int> RectanglesY = new List<int>();
        protected readonly List<int> RectanglesWidth = new List<int>();
        protected readonly List<int> RectanglesHeight = new List<int>();

        protected readonly int Skip;

        protected Area(int skip)
        {
            Skip = skip;
        }

        public Rect GetShape()
        {
            Rect avg = null;
            if (RectanglesX.Count >= Skip)
            {
                avg = new Rect((int)RectanglesX.Average(), (int)RectanglesY.Average(), (int)RectanglesWidth.Average(),
                    (int)RectanglesHeight.Average());

                RectanglesX.Clear();
                RectanglesY.Clear();
                RectanglesWidth.Clear();
                RectanglesHeight.Clear();

                RectanglesX.Add(avg.X);
                RectanglesY.Add(avg.Y);
                RectanglesWidth.Add(avg.Width);
                RectanglesHeight.Add(avg.Height);

            }
            else if (RectanglesX.Any())
                avg = new Rect((int)RectanglesX.Average(), (int)RectanglesY.Average(), (int)RectanglesWidth.Average(),
                    (int)RectanglesHeight.Average());
           
            return avg;
        }

        public Area Insert(Rect rectangle)
        {
            if (rectangle == null)
                return this;

            RectanglesX.Add(rectangle.X);
            RectanglesY.Add(rectangle.Y);
            RectanglesWidth.Add(rectangle.Width);
            RectanglesHeight.Add(rectangle.Height);
            return this;
        }
    }
}