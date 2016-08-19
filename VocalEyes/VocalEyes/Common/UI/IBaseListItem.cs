namespace  VocalEyes.Common.UI
{
    public interface IBaseListItem
    {
        string Text { get; set; }

        object Id { get; set; }

        int CellId { get; set; }

		bool Enabled { get; set; }

        bool Selected { get; set; }

        bool IsMultiSelect { get; set; }

        void Select();

        void Unselect();
    }
}
