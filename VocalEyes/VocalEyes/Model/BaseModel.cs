using System.ComponentModel;
using System.Runtime.CompilerServices;
using VocalEyes.Common.Interface;
using SQLite.Net.Attributes;

namespace VocalEyes.Model
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
