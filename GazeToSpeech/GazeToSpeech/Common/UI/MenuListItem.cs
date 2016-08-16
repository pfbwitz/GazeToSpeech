using System;

namespace GazeToSpeech.Common.UI
{
    public class MenuListItem : BaseListItem
    {
        private Type _page;

        public MenuListItem(Type page)
        {
            _page = page;
        }

		public override void Unselect()
	    {
			base.Unselect();
			Image = Image.Replace("_selected", "");
	    }

	    public override void Select()
	    {
		    base.Select();
			Image = Image.Contains("_selected") ? Image : Image.Replace(".png", "_selected.png");
	    }

        public object GetPage()
        {
            return Activator.CreateInstance(_page);
        }
    }
}
