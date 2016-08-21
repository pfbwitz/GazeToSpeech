using SQLite.Net.Attributes;

namespace VocalEyes.Model
{
    public class BaseModel
    {
        [Column("ID"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }
    }
}
