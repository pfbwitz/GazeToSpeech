using VocalEyes.Common.Interface;
using Xamarin.Forms;

namespace VocalEyes.Pages
{
    public abstract class BaseContentPage : ContentPage, IPage
    {
        protected BaseContentPage()
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
