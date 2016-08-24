using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite.Net.Attributes;
using VocalEyes.Common.Data;

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
