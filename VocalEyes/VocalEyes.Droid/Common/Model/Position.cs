using System.Collections.Generic;
using System.Linq;
using OpenCV.Core;

namespace VocalEyes.Droid.Common.Model
{
    public abstract class Position
    {
        protected readonly List<Point> _pupils = new List<Point>();
       
        protected readonly int _skip;

        protected Position(int skip)
        {
            _skip = skip;
        }

        public Position Insert(Point point)
        {
            _pupils.Add(point);
            return this;
        }

        public Point GetShape()
        {
             Point avg;
             if (_pupils.Count >= _skip)
            {
                avg = new Point(_pupils.Average(p => p.X), _pupils.Average(p => p.Y));
                _pupils.Clear();
                _pupils.Add(avg);
            }
            else
                 avg = new Point(_pupils.Average(p => p.X), _pupils.Average(p => p.Y));

            return avg;
        }
    }
}