using Xamarin.Forms;

namespace GazeToSpeech.Common.Controls
{
    public class CustomButton : Button
    {
        public CustomButton()
        {
            BorderRadius = 0;
            BackgroundColor = Constants.BlueColor;
            TextColor = Color.White;
            FontAttributes = FontAttributes.Bold;
        }

        public CustomButton(string text)
            : this()
        {
            Text = text;
        }
    }
}
