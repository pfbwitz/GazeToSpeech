using SQLite.Net.Attributes;

namespace VocalEyes.Model
{
    [Table("USER")]
    public class User : BaseModel
    {
        [Column("LANGUAGE")]
        public string Language { get; set; }

        [Column("CAMERAFACING")]
        public int CameraFacing { get; set; }
    }
}
