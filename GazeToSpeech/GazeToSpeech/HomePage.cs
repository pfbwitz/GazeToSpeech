using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace GazeToSpeech
{
    public class HomePage : MasterDetailPage
    {
        public HomePage()
        {
            Padding = new Thickness(0, Device.OnPlatform(20, 0, 0), 0, 0);
            var list = new ListView
            {
                ItemsSource = new List<string>
                {
                    "Empty",
                   "Capture"
                },
            };
            list.ItemSelected += (sender, args) =>
            {
                var t = args.SelectedItem.ToString();
                if(t == "Empty")
                    SetPage(GetEmpty());
                else if (t == "Capture")
                    SetPage(new CapturePage());
            };

            Master = new ContentPage
            {
                Padding = new Thickness(0, Device.OnPlatform(20, 0, 0), 0, 0),
                Title = "Menu",
                Content = list,
            };
            Detail = GetEmpty();
        }

        public ContentPage GetEmpty()
        {
            return new ContentPage{Content = new Label(){ Text="EMPTY"}};
        }

        private void SetPage(ContentPage page)
        {
            Detail = page;
            IsPresented = false;
        }
    }
}
