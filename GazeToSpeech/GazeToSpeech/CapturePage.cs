using Xamarin.Forms;

namespace GazeToSpeech
{
    public class CapturePage : ContentPage
    {
        public CapturePage()
        {
            var button = new Button(){Text="Start Capture"};
            button.Clicked += (s, a) => DependencyService.Get<ICaptureHelper>().Open();

            Content = button;
        }
    }
}
