using Xamarin.Forms;

namespace VocalEyes.Common.Controls
{
    public class CustomLabel : Label
    {
        public object Tag { get; set; }
        public object ID { get; set; }
        public CustomLabel()
        {
            TextColor = Color.Black;

        }
    }
}
