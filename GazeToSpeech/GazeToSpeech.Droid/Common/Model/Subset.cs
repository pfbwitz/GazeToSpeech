using System.Collections.Generic;
using System.Drawing;

namespace GazeToSpeech.Droid.Common.Model
{
    public class Subset
    {
        public SubsetPartition Partition { get; set; }
        public Rectangle Coordinate { get; set; }

        public List<char> Characters { get; set; }
    }
}