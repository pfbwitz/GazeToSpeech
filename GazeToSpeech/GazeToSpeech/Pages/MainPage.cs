using System.Collections.Generic;
using System.Linq;
using GazeToSpeech.Common;
using GazeToSpeech.Common.UI;
using Xamarin.Forms;

namespace GazeToSpeech.Pages
{
    public class MainPage : MasterDetailPage
    {
        public MainPage()
        {
            Padding = new Thickness(0, Device.OnPlatform(20, 0, 0), 0, 0);

            var list = new ListView
            {
                ItemTemplate = new DataTemplate(typeof(MenuViewCell)),
                ItemsSource = new List<MenuListItem>
                {
                    new MenuListItem(typeof(HomePage)) {Text = TextResources.TtlHome, Image = "home.png"},
                    new MenuListItem(typeof(CapturePage)) {Text = TextResources.TtlCapture, Image = "capture.png"},
                    new MenuListItem(typeof(SettingsPage)) {Text = TextResources.TtlSettings, Image = "settings.png"}
                }
            };
            list.ItemSelected += (sender, args) =>
            {
                var item = (MenuListItem) args.SelectedItem;

                foreach (var i in list.ItemsSource.Cast<MenuListItem>())
                    i.Unselect();

                item.Select();
                SetPage(item.GetPage() as ContentPage);
            };
            list.SelectedItem = list.ItemsSource.Cast<MenuListItem>().First();

            Master = new ContentPage
            {
                Padding = new Thickness(0, Device.OnPlatform(20, 0, 0), 0, 0),
                Title = " ",
                Icon = "hamburger.png",
                Content = list
            };
            Detail = new HomePage();
        }

        private void SetPage(ContentPage page)
        {
            Detail = page;
            IsPresented = false;
        }
    }
}
