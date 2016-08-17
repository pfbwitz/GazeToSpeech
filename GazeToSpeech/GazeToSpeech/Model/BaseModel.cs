using System.ComponentModel;
using System.Runtime.CompilerServices;
using GazeToSpeech.Common.Interface;
using SQLite.Net.Attributes;

namespace GazeToSpeech.Model
{
    public abstract class BaseModel : IBaseModel, INotifyPropertyChanged
    {
        [Column("ID"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
