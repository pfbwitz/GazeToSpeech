using GazeToSpeech.Common.Interface;
using Xamarin.Forms;

namespace GazeToSpeech.Pages
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
