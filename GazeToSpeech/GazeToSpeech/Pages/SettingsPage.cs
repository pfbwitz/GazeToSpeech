using System.Collections.Generic;
using System.Linq;
using GazeToSpeech.Common;
using GazeToSpeech.Common.Controls;
using GazeToSpeech.Common.Data;
using GazeToSpeech.Common.Enumeration;
using GazeToSpeech.Model;
using Xamarin.Forms;

namespace GazeToSpeech.Pages
{
    public class SettingsPage : CustomPage
    {
        public override void LoadMe()
        {
            Title = TextResources.TtlSettings;

            //var languages = DependencyService.Get<IDeviceHelper>().GetAvailableLanguages();
            var languages = new List<Language>
            {
                new Language{Code="nl", Name = "Nederlands"},
                new Language{Code="en", Name = "English"}
            };
            var languageImage = new Image
            {
                Source = "language.png", WidthRequest = 32, HeightRequest = 32, HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center
            };
            var languagePicker = new Picker{HorizontalOptions = LayoutOptions.FillAndExpand};
            foreach(var l in languages)
                languagePicker.Items.Add(l.Name);

            languagePicker.SelectedIndexChanged += (sender, args) =>
            {
                var language = languages.ElementAt(((Picker)sender).SelectedIndex);
                var code = language.Code;
                App.User.Language = code;
                QueryHelper<User>.InsertOrReplace(App.User);
            };
            //languagePicker.SelectedIndex = languages.IndexOf(languages.Single(l => l.Code == App.User.Language));

            var languageStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { languageImage, languagePicker }
            };

            var facingImage = new Image
            {
                Source = "camera.png",
                WidthRequest = 32,
                HeightRequest = 32,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center
            };
            var facingPicker = new Picker {HorizontalOptions = LayoutOptions.FillAndExpand};
            facingPicker.Items.Add(TextResources.LblFront);
            facingPicker.Items.Add(TextResources.LblBack);
            facingPicker.SelectedIndexChanged += (sender, args) =>
            {
                var index = ((Picker) sender).SelectedIndex;
                if (index == 1)
                    App.User.CameraFacing = CameraFacing.Front.ToString();
                if (index == 0)
                    App.User.CameraFacing = CameraFacing.Back.ToString();
                QueryHelper<User>.InsertOrReplace(App.User);
            };
            //facingPicker.SelectedIndex = App.User.CameraFacing == CameraFacing.Front.ToString() ? 0 : 1;

            var facingStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { facingImage, facingPicker }
            };

            Content = new StackLayout { Children =
            {
                new CustomLabel{Text=TextResources.LblLanguage},
                languageStack, 
                new CustomLabel{Text=TextResources.LblFacing},
                facingStack
            } };
        }
    }
}
