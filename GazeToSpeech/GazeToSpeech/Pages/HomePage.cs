using GazeToSpeech.Common;
using Xamarin.Forms;

namespace GazeToSpeech.Pages
{
    public class HomePage : CustomPage
    {
        public override void LoadMe()
        {
            Title = TextResources.TtlHome;
            Padding = new Thickness(0);
            var webview = new WebView
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Source = "http://jasonbeckerguitar.com/eye_communication.html"
            };
            webview.Navigating += (sender, args) =>
            {
                args.Cancel = true;
            };
            Content = webview;
        }
    }
}
