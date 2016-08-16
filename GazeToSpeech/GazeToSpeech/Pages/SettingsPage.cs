using GazeToSpeech.Common;
using Xamarin.Forms;

namespace GazeToSpeech.Pages
{
    class SettingsPage : CustomPage
    {
        public SettingsPage()
        {
            Title = TextResources.TtlSettings;
            Content = new Label {Text = "Settings"};
        }
    }
}
