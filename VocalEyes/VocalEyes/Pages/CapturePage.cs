using VocalEyes.Common;
using VocalEyes.Common.Controls;
using VocalEyes.Common.Enumeration;
using VocalEyes.Common.Interface;
using Xamarin.Forms;

namespace VocalEyes.Pages
{
    public class CapturePage : CustomPage
    {
        public override void LoadMe()
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
