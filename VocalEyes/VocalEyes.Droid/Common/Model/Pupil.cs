using System.Linq;
using OpenCV.Core;

namespace VocalEyes.Droid.Common.Model
{
    public class Pupil : Position
    {
        public Pupil(int skip):base(skip)
        {
        }

        public int Count
        {
            get { return Points.Count; }
        }

        public void RemoveAt(int index)
        {
            Points.RemoveAt(index);
        }

        public Pupil PreCut()
        {
            var cut = Count / 2;
            while (Count > cut)
                RemoveAt(0);

            return this;
        }

        public Point LastMeasured
        {
            get { return Points.Last(); }
        }
    }
}