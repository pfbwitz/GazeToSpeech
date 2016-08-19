using Xamarin.Forms;

namespace VocalEyes.Common.Controls
{
    public class Icon : Image
    {
        public Icon()
        {
            Init();
        }

        public Icon(string imageSource)
        {
            Init();
            Source = imageSource;
        }

        private void Init()
        {
            WidthRequest = 32;
            HeightRequest = 32;
        }
    }
}
