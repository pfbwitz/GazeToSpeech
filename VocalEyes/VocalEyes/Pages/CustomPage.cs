using VocalEyes.Common.Interface;
using Xamarin.Forms;

namespace VocalEyes.Pages
{
    public abstract class CustomPage : ContentPage, IPage
    {
        protected CustomPage()
        {
            Padding = new Thickness(5);
            LoadMe();
        }

        public virtual void LoadMe()
        {
            throw new System.NotImplementedException();
        }
    }
}
