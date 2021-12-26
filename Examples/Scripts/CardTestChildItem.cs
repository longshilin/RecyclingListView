using Longshilin.RecyclingListView;
using UnityEngine.UI;

public class CardTestChildItem : CardRecyclingListViewItem
{
    public Text titleText;
    public Text rowText;

    private TestChildData _childData;

    public TestChildData ChildData
    {
        get => _childData;
        set
        {
            _childData = value;
            titleText.text = _childData.Title;
            rowText.text = $"行号：{_childData.Row}";
        }
    }
}