using System;
using UnityEngine;

namespace Longshilin.RecyclingListView
{
    /// <summary>
    /// Card ListView Item
    /// </summary>
    public class CardRecyclingListViewItem : RecyclingListViewItem
    {
        [Tooltip("卡片式Item的子层级content，用于child缩放")]
        public RectTransform m_Content;

        [Tooltip("选中对象相对大小比例"), Range(0.5f, 1.5f)]
        public float m_PitchScale = 0;

        private float _offset; // item与中心点的偏移值

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
            if (ParentList.ScrollType == RecyclingListScrollType.Horizontal)
            {
                _offset = Math.Abs(m_Content.position.x - ParentList.position.x) / m_Content.lossyScale.x;
                sizeOffset = _offset / ((ParentList.ChildWidthOrHeight() + ParentList.m_RowPadding) * 1.0f);
            }
            else
            {
                _offset = Math.Abs(m_Content.position.y - ParentList.position.y) / m_Content.lossyScale.y;
                sizeOffset = _offset / ((ParentList.ChildWidthOrHeight() + ParentList.m_RowPadding) * 1.0f);
            }

            sizeOffset = Math.Min(sizeOffset, 1);
            m_Content.localScale = Vector3.one * ((m_PitchScale - 1) * (1 - sizeOffset) + 1);
        }
    }
}