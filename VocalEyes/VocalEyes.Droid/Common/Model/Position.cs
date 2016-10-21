using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenCV.Core;

namespace VocalEyes.Droid.Common.Model
{
    public abstract class Position : IEnumerable<Point>
    {
        protected readonly List<Point> Points = new List<Point>();
       
        protected readonly int Skip;

        protected Position(int skip)
        {
            Skip = skip;
        }

        public Position Insert(Point point)
        {
            if (point != null)
                Points.Add(point);
            return this;
        }

        public Point GetShape()
        {
            Point avg;
            if (Points.Count >= Skip)
            {
                avg = new Point(Points.Average(p => p.X), Points.Average(p => p.Y));
                Points.Clear();
                Points.Add(avg);
            }
            else
                avg = new Point(Points.Average(p => p.X), Points.Average(p => p.Y));

            return avg;
        }

        public IEnumerator<Point> GetEnumerator()
        {
            return Points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}