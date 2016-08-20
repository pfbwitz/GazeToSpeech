using System.Collections.Generic;
using System.Linq;
using VocalEyes.Common.Enumeration;

namespace VocalEyes.Droid.Common.Model
{
    public class Subset
    {
        public Direction Direction { get; set; }

        public SubsetPartition Partition { get; set; }

        public List<string> Characters
        {
            get
            {
                return Partition.ToString().ToCharArray().Select(c => c.ToString().ToUpper()).ToList();
            }
        }

        public double DistanceToPoint { get; set; }

        public string GetCharacter(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    return Characters[0];
                case Direction.Right:
                    return Characters[2];
                case Direction.Top:
                    return Characters[1];
                case Direction.Bottom:
                    return Characters[3];
                case Direction.BottomLeft:
                    if (Characters.Count > 4)
                        return Characters[4];
                    break;
                case Direction.BottomRight:
                    if (Characters.Count > 4)
                        return Characters[5];
                    break;
            }
            return string.Empty;
        }
    }
}