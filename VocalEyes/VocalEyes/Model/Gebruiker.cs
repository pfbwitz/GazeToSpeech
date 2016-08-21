using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite.Net.Attributes;

namespace VocalEyes.Model
{
    [Table("USER")]
    public class User : BaseModel, INotifyPropertyChanged
    {
        [Column("LANGUAGE")]
        public string Language { get; set; }

        [Column("CAMERAFACING")]
        public int CameraFacing { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
