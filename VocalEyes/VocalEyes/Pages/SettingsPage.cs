using System.Collections.Generic;
using System.Linq;
using VocalEyes.Common;
using VocalEyes.Common.Controls;
using VocalEyes.Common.Data;
using VocalEyes.Common.Enumeration;
using VocalEyes.Model;
using Xamarin.Forms;

namespace VocalEyes.Pages
{
    public class SettingsPage : BaseContentPage
    {
        public override void LoadMe()
        {
            Title = TextResources.TtlSettings;

            var languages = new List<Language>
            {
                new Language{Code="en", Name = TextResources.LblEnglish},
                new Language{Code="nl", Name = TextResources.LblDutch},
               
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
            languagePicker.SelectedIndex = languages.IndexOf(languages.Single(l => l.Code == App.User.Language));

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
            facingPicker.Items.Add(TextResources.LblBack);
            facingPicker.Items.Add(TextResources.LblFront);
            facingPicker.SelectedIndexChanged += (sender, args) =>
            {
                App.User.CameraFacing = ((Picker)sender).SelectedIndex;      
                QueryHelper<User>.InsertOrReplace(App.User);
            };
            facingPicker.SelectedIndex = App.User.CameraFacing;

            var facingStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { facingImage, facingPicker }
            };

            Content = new StackLayout { 
                Children =
                {
                    new CustomLabel{Text=TextResources.LblLanguage},
                    languageStack, 
                    new CustomLabel{Text=TextResources.LblFacing},
                    facingStack
                } 
            };
        }
    }
}
