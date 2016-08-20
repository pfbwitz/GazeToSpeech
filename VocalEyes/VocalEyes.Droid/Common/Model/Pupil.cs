using System;
using System.Collections.Generic;
using System.Linq;
using OpenCV.Core;
using VocalEyes.Droid.Activities;

namespace VocalEyes.Droid.Common.Model
{
    public class Pupil
    {
        private readonly List<Point> _pupils = new List<Point>();
       
        private readonly int _skip;

        public Pupil(int skip)
        {
            _skip = skip;
        }

        public Pupil Insert(Point point)
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