using Xamarin.Forms;

namespace GazeToSpeech
{
    public class CapturePage : ContentPage
    {
        public CapturePage()
        {
            var button = new Button(){Text="Start Backfacing Camera Capture"};
            button.Clicked += (s, a) => DependencyService.Get<ICaptureHelper>().Open(CameraFacing.Back);

            var button2 = new Button() { Text = "Start Front Facing Camera" };
            button2.Clicked += (s, a) => DependencyService.Get<ICaptureHelper>().Open(CameraFacing.Front);

            Content = new StackLayout() { Children = { button, button2 } };
        }
    }
}
