using System.Collections.Generic;
using System.Drawing;
using GazeToSpeech.Common.Enumeration;

namespace GazeToSpeech.Droid.Common.Model
{
    public class Subset
    {
        public Direction Direction { get; set; }

        public SubsetPartition Partition { get; set; }

        public Rectangle Coordinate { get; set; }

        public List<char> Characters { get; set; }

        public double DistanceToPoint { get; set; }
    }
}