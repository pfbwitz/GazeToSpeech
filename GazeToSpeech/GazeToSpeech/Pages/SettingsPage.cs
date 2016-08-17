using GazeToSpeech.Common;
using Xamarin.Forms;

namespace GazeToSpeech.Pages
{
    public class SettingsPage : CustomPage
    {
        public override void LoadMe()
        {
            Title = TextResources.TtlSettings;
            Content = new Label { Text = "Settings" };
        }
    }
}
