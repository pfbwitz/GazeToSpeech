using SQLite.Net.Attributes;

namespace GazeToSpeech.Model
{
    [Table("USER")]
    public class User : BaseModel
    {
        [Column("LANGUAGE")]
        public string Language { get; set; }

        [Column("CAMERAFACING")]
        public string CameraFacing { get; set; }
    }
}
