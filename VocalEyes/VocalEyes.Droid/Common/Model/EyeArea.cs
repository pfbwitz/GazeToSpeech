using System.Collections.Generic;
using System.Linq;
using OpenCV.Core;

namespace VocalEyes.Droid.Common.Model
{
    public class EyeArea
    {
        private readonly List<int> _rectanglesX = new List<int>();
        private readonly List<int> _rectanglesY = new List<int>();
        private readonly List<int> _rectanglesWidth = new List<int>();
        private readonly List<int> _rectanglesHeight = new List<int>();

        private readonly int _skip;

        public EyeArea(int skip)
        {
            _skip = skip;
        }

        public Rect GetShape()
        {
            Rect avg = null;
            if (_rectanglesX.Count >= _skip)
            {
                avg = new Rect((int)_rectanglesX.Average(), (int)_rectanglesY.Average(), (int)_rectanglesWidth.Average(),
                    (int)_rectanglesHeight.Average());

                _rectanglesX.Clear();
                _rectanglesY.Clear();
                _rectanglesWidth.Clear();
                _rectanglesHeight.Clear();

                _rectanglesX.Add(avg.X);
                _rectanglesY.Add(avg.Y);
                _rectanglesWidth.Add(avg.Width);
                _rectanglesHeight.Add(avg.Height);

            }
            else if (_rectanglesX.Any())
                avg = new Rect((int)_rectanglesX.Average(), (int)_rectanglesY.Average(), (int)_rectanglesWidth.Average(),
                    (int)_rectanglesHeight.Average());
           
            return avg;
        }

        public EyeArea Insert(Rect rectangle)
        {
            _rectanglesX.Add(rectangle.X);
            _rectanglesY.Add(rectangle.Y);
            _rectanglesWidth.Add(rectangle.Width);
            _rectanglesHeight.Add(rectangle.Height);
            return this;
        }
    }
}