using VocalEyes.Common;
using VocalEyes.Common.Controls;
using VocalEyes.Common.Interface;
using Xamarin.Forms;

namespace VocalEyes.Pages
{
    public class CapturePage : BaseContentPage
    {
        public override void LoadMe()
        {
            Title = TextResources.TtlCapture;
            var button = new CustomButton { Text = TextResources.BtnStartCapture };
            button.Clicked += (s, a) => DependencyService.Get<IOpenCvEngine>().Open(App.User.CameraFacing);

            Content = new StackLayout { 
                Children =
                {
                    new CustomLabel {Text=TextResources.LblCaptureInfo},
                    button
                } 
            };
        }
    }
}
