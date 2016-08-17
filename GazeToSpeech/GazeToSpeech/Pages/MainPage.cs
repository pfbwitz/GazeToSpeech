using System.Collections.Generic;
using System.Linq;
using GazeToSpeech.Common;
using GazeToSpeech.Common.Controls;
using GazeToSpeech.Common.Interface;
using GazeToSpeech.Common.UI;
using Xamarin.Forms;

namespace GazeToSpeech.Pages
{
    public class MainPage : MasterDetailPage
    {
        private ListView _list;
        public MainPage()
        {
            Padding = new Thickness(0, Device.OnPlatform(20, 0, 0), 0, 0);
            Master = GetMenu();
            _list.SelectedItem = _list.ItemsSource.Cast<MenuListItem>().First();
        }

        private ContentPage GetMenu()
        {
            _list = new ListView
            {
                ItemTemplate = new DataTemplate(typeof(MenuViewCell)),
                ItemsSource = new List<MenuListItem>
                {
                    new MenuListItem(typeof(HomePage)) {Text = TextResources.TtlHome, Image = "home.png"},
                    new MenuListItem(typeof(CapturePage)) {Text = TextResources.TtlCapture, Image = "capture.png"},
                    new MenuListItem(typeof(SettingsPage)) {Text = TextResources.TtlSettings, Image = "settings.png"}
                }
            };
            _list.ItemSelected += (sender, args) =>
            {
                var item = (MenuListItem)args.SelectedItem;

                foreach (var i in _list.ItemsSource.Cast<MenuListItem>())
                    i.Unselect();

                item.Select();
                SetPage(item.GetPage() as ContentPage);
            };

            var version = new CustomLabel
            {
                Text = "v" + DependencyService.Get<IDeviceHelper>().GetVersion() + " ",
                HorizontalOptions = LayoutOptions.EndAndExpand,
                VerticalOptions = LayoutOptions.EndAndExpand
            };

            return new ContentPage
            {
                Padding = new Thickness(0, Device.OnPlatform(20, 0, 0), 0, 0),
                Title = " ",
                Icon = "hamburger.png",
                Content = new StackLayout
                {
                    Children =
                    {
                        new Image{Source = "banner.jpg", Aspect = Aspect.AspectFill, HorizontalOptions = LayoutOptions.FillAndExpand},
                        _list, version
                    }
                }
            };
        }

        private void SetPage(Page page)
        {
            Detail = page;
            Title = page.Title;
            Master.Title = page.Title;
            IsPresented = false;
        }
    }
}
