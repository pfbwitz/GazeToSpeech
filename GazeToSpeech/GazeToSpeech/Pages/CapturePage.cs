using GazeToSpeech.Common;
using GazeToSpeech.Common.Controls;
using GazeToSpeech.Common.Enumeration;
using GazeToSpeech.Common.Interface;
using Xamarin.Forms;

namespace GazeToSpeech.Pages
{
    public class CapturePage : CustomPage
    {
        public CapturePage()
        {
            Title = TextResources.TtlCapture;
            var button = new CustomButton { Text = "Start Backfacing Camera Capture" };
            button.Clicked += (s, a) => DependencyService.Get<IOpenCvEngine>().Open(CameraFacing.Back);

            var button2 = new CustomButton { Text = "Start Front Facing Camera" };
            button2.Clicked += (s, a) => DependencyService.Get<IOpenCvEngine>().Open(CameraFacing.Front);

            Content = new StackLayout { Children = { button, button2 } };
        }
    }
}
