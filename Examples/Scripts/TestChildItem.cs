using UnityEngine.UI;
using Longshilin.RecyclingListView;

public class TestChildItem : RecyclingListViewItem
{
    public Text titleText;
    public Text rowText;

    private TestChildData childData;

    public TestChildData ChildData
    {
        get => childData;
        set
        {
            childData = value;
            titleText.text = childData.Title;
            rowText.text = $"行号：{childData.Row}";
        }
    }
}


/// <summary>
/// ListView Item Data
/// </summary>
public struct TestChildData
{
    public string Title;
    public int Row;

    public TestChildData(string title, int row)
    {
        Title = title;
        Row = row;
    }
}