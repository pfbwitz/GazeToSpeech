using GazeToSpeech.Common.Controls;
using Xamarin.Forms;

namespace GazeToSpeech.Common.UI
{
    internal class MenuViewCell : ViewCell
    {
        public MenuViewCell()
        {
            var label = new CustomLabel
            {
                VerticalOptions = LayoutOptions.Center,
                FontAttributes = FontAttributes.Bold
            };
            label.SetBinding(Label.TextColorProperty, "TextColor");
            label.SetBinding(Label.TextProperty, "Text");

            var icon = new Icon();
            icon.SetBinding(Image.SourceProperty, "Image");

            View = new StackLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                Orientation = StackOrientation.Horizontal,
                Padding = new Thickness(20, 0, 0, 0),
                Spacing = 20,
                Children = { icon, label }
            };
        }
    }
}
