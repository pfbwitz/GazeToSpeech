using VocalEyes.Common;
using Xamarin.Forms;

namespace VocalEyes.Pages
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
