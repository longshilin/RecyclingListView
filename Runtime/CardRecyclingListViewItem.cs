using System;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class CardRecyclingListViewItem : RecyclingListViewItem
{
    public RectTransform content;

    [Header("选中对象相对大小比例"), Range(0.5f, 1.5f)]
    public float m_picthScale = 0;

    private float _offset;


    public override void NotifyCurrentAssignment(RecyclingListView v, int row)
    {
        base.NotifyCurrentAssignment(v, row);
        (ParentList as CardRecyclingListView)?.UpdateCenterItem(row, _offset);
    }

    /// <summary>
    /// 居中的item有放大效果
    /// </summary>
    private void Update()
    {
        float sizeOffset = 1;
        if (ParentList.ScrollType == RecyclingListScrollType.Hor)
        {
            _offset = Math.Abs(content.position.x - ParentList.position.x) / content.lossyScale.x;
            sizeOffset = _offset / ((ParentList.ChildWidthOrHeight() + ParentList.RowPadding) * 1.0f);
        }
        else
        {
            _offset = Math.Abs(content.position.y - ParentList.position.y) / content.lossyScale.y;
            sizeOffset = _offset / ((ParentList.ChildWidthOrHeight() + ParentList.RowPadding) * 1.0f);
        }

        sizeOffset = Math.Min(sizeOffset, 1);
        content.localScale = Vector3.one * ((m_picthScale - 1) * (1 - sizeOffset) + 1);
    }
}