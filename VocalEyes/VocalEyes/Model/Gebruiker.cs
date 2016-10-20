using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite.Net.Attributes;
using VocalEyes.Common.Data;
using VocalEyes.Common.Enumeration;

namespace VocalEyes.Model
{
    [Table("USER")]
    public class User : BaseModel, INotifyPropertyChanged
    {
        [Column("ID"), PrimaryKey, AutoIncrement]
        public new int Id { get; set; }

        [Column("LANGUAGE")]
        public string Language { get; set; }

        [Column("CAMERAFACING")]
        public int CameraFacing { get; set; }

        [Column("CALIBRATED")]
        public bool Calibrated { get; set; }

        [Column("CENTER_X")]
        public double CenterX { get; set; }

        [Column("CENTER_Y")]
        public double CenterY { get; set; }

        public void Save()
        {
            QueryHelper<User>.InsertOrReplace(this);
        }

        [Column("TOP_X")]
        public double TopX { get; set; }

        [Column("TOP_Y")]
        public double TopY { get; set; }

        [Column("LEFT_X")]
        public double LeftX { get; set; }

        [Column("LEFT_Y")]
        public double LeftY { get; set; }

        [Column("RIGHT_X")]
        public double RightX { get; set; }

        [Column("RIGHT_Y")]
        public double RightY { get; set; }

        [Column("BOTTOM_X")]
        public double BottomX { get; set; }

        [Column("BOTTOM_Y")]
        public double BottomY { get; set; }

        [Column("TOPLEFT_X")]
        public double TopLeftX { get; set; }

        [Column("TOP_LEFT_Y")]
        public double TopLeftY { get; set; }

        [Column("BOTTOM_LEFT_X")]
        public double BottomLeftX { get; set; }

        [Column("BOTTOM_LEFT_Y")]
        public double BottomLeftY { get; set; }

        [Column("TOP_RIGHT_X")]
        public double TopRightX { get; set; }

        [Column("TOP_RIGHT_Y")]
        public double TopRightY { get; set; }

        [Column("BOTTOM_RIGHT_X")]
        public double BottomRightX { get; set; }

        [Column("BOTTOM_RIGHT_Y")]
        public double BottomRightY { get; set; }

        public void SetPoint(Direction direction, double x, double y)
        {
            switch (direction)
            {
                case Direction.Left:
                    LeftX = x;
                    LeftY = y;
                    break;
                case Direction.Right:
                    RightX = x;
                    RightY = y;
                    break;
                case Direction.Top:
                    TopX = x;
                    TopY = y;
                    break;
                case Direction.Bottom:
                    BottomX = x;
                    BottomY = y;
                    break;
                case Direction.TopLeft:
                    TopLeftX = x;
                    TopLeftX = y;
                    break;
                case Direction.BottomLeft:
                    BottomLeftX = x;
                    BottomLeftY = y;
                    break;
                case Direction.TopRight:
                    TopRightX = x;
                    TopRightY = y;
                    break;
                case Direction.BottomRight:
                    BottomRightX = x;
                    BottomRightY = y;
                    break;
                case Direction.Center:
                    CenterX = x;
                    CenterY = y;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("direction", direction, null);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
