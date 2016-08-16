using SQLite.Net.Attributes;

namespace GazeToSpeech.Model
{
    [Table("USER")]
    public class User : BaseModel
    {
        [Column("ID"), PrimaryKey, AutoIncrement]
        public new int Id { get; set; }
    }
}
