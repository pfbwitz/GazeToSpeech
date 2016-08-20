using System;
using VocalEyes.Common;
using VocalEyes.Common.Controls;
using Xamarin.Forms;

namespace VocalEyes.Pages
{
    public class HomePage : BaseContentPage
    {
        public override void LoadMe()
        {
            Title = TextResources.TtlHome;
            Padding = new Thickness(0);

            var button = new CustomButton(TextResources.BtnMoreInformation);
            button.Clicked += (sender, args) => Device.OpenUri(new Uri("http://jasonbeckerguitar.com/eye_communication.html"));
            Content = new StackLayout
            {
                Children =
                {
                    new CustomLabel{Text = TextResources.LblHomeInfo},
                    button
                }
            };
        }
    }
}
