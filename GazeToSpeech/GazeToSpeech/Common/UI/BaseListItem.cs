using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace  GazeToSpeech.Common.UI
{
	public abstract class BaseListItem : IBaseListItem, INotifyPropertyChanged
	{
        public int CellId { get; set; }

        public static Color QColor { get; set; }

		private Color _textColor { get; set; }
		public Color TextColor
		{
			get { return _textColor; }
			set
			{
				_textColor = value;
				OnPropertyChanged();
			}
		}

		private Color _backgroundColor { get; set; }
		public Color BackgroundColor
		{
			get { return _backgroundColor; }
			set
			{
				_backgroundColor = value;
				OnPropertyChanged();
			}
		}

        private string _image;
        public string Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged();
            }
        }

        private string _text;
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }

        public object Id { get; set; }
		public bool Enabled { get; set; }
	    private bool _selected;

	    public bool Selected
	    {
	        get{return _selected;}
	        set
	        {
	            if(value)
                    Select();
                else
                    Unselect();
                OnPropertyChanged();
	        }
	    }
		public bool IsMultiSelect { get; set; }

		protected BaseListItem()
	    {
            TextColor = QColor;
	    }
       
		public virtual void Select()
		{
            _selected = true;
			TextColor = Color.White;
            BackgroundColor = QColor;
		}

		public virtual void Unselect()
		{
            _selected = false;
            TextColor = QColor;
			BackgroundColor = Color.Transparent;
		}

		object IBaseListItem.Id
		{
			get { return Id; }
			set { Id = value; }
		}

	    public event PropertyChangedEventHandler PropertyChanged;

	    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	    {
	        var handler = PropertyChanged;
	        if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
	    }
	}
}
