using System;
using UnityEngine;
using UnityEngine.UI;

namespace Longshilin.RecyclingListView
{
    public enum RecyclingListScrollType
    {
        Horizontal, // 水平
        Vertical, // 垂直
    }

    /// <summary>
    /// 循环复用列表
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ScrollRect))]
    public class RecyclingListView : MonoBehaviour
    {
        [Tooltip("子节点物体")] public RecyclingListViewItem m_ChildObj;
        [Tooltip("行间隔")] public float m_RowPadding = 15f;
        [Tooltip("事先预留的最小列表高度")] public float m_PreAllocHeight = 0;

        protected float StartOffset; // 初始偏移值
        protected int StartOffsetIndex; // 初始偏移Index

        private RecyclingListScrollType _scrollType = RecyclingListScrollType.Horizontal;

        public enum ScrollPosType
        {
            Top,
            Center,
            Bottom,
        }

        public RecyclingListScrollType ScrollType => _scrollType;

        public float VerticalNormalizedPosition
        {
            get => scrollRect.verticalNormalizedPosition;
            set => scrollRect.verticalNormalizedPosition = value;
        }

        public Vector3 position => scrollRect.transform.position;

        /// <summary>
        /// 列表行数
        /// </summary>
        protected int rowCount;

        /// <summary>
        /// 列表行数，赋值时，会执行列表重新计算
        /// </summary>
        public int RowCount
        {
            get => rowCount;
            set
            {
                if (rowCount != value)
                {
                    rowCount = value;
                    // 先禁用滚动变化
                    ignoreScrollChange = true;
                    // 重新启用滚动变化
                    ignoreScrollChange = false;
                    // 重新计算item
                    ReorganiseContent(true);
                    // 更新宽度或高度
                    UpdateContentHeight();
                }
            }
        }

        /// <summary>
        /// item更新回调函数委托
        /// </summary>
        /// <param name="item">子节点对象</param>
        /// <param name="rowIndex">行数</param>
        public delegate void ItemDelegate(RecyclingListViewItem item, int rowIndex);

        /// <summary>
        /// item更新回调函数委托
        /// </summary>
        public ItemDelegate ItemCallback;

        protected ScrollRect scrollRect;

        /// <summary>
        /// 复用的item数组
        /// </summary>
        protected RecyclingListViewItem[] childItems;

        /// <summary>
        /// 循环列表中，第一个item的索引，最开始每个item都有一个原始索引，最顶部的item的原始索引就是childBufferStart
        /// 由于列表是循环复用的，所以往下滑动时，childBufferStart会从0开始到n，然后又从0开始，以此往复
        /// 如果是往上滑动，则是从0到-n，再从0开始，以此往复
        /// </summary>
        protected int childBufferStart = 0;

        /// <summary>
        /// 列表中最顶部的item的真实数据索引，比如有一百条数据，复用10个item，当前最顶部是第60条数据，那么sourceDataRowStart就是59（注意索引从0开始）
        /// </summary>
        protected int sourceDataRowStart;

        protected bool ignoreScrollChange = false;
        protected float previousBuildHeight = 0;
        protected const int rowsAboveBelow = 1;

        protected virtual void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();
            _scrollType = scrollRect.horizontal ? RecyclingListScrollType.Horizontal : RecyclingListScrollType.Vertical;
            m_ChildObj.gameObject.SetActive(false);
        }


        protected virtual void OnEnable()
        {
            scrollRect.onValueChanged.AddListener(OnScrollChanged);
            ignoreScrollChange = false;
        }

        protected virtual void OnDisable()
        {
            scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
        }


        /// <summary>
        /// 供外部调用，强制刷新整个列表，比如数据变化了，刷新一下列表
        /// </summary>
        public virtual void Refresh()
        {
            ReorganiseContent(true);
        }

        /// <summary>
        /// 供外部调用，强制刷新整个列表的局部item
        /// </summary>
        /// <param name="rowStart">开始行</param>
        /// <param name="count">数量</param>
        public virtual void Refresh(int rowStart, int count)
        {
            int sourceDataLimit = sourceDataRowStart + childItems.Length;
            for (int i = 0; i < count; ++i)
            {
                int row = rowStart + i;
                if (row < sourceDataRowStart || row >= sourceDataLimit)
                    continue;

                int bufIdx = WrapChildIndex(childBufferStart + row - sourceDataRowStart);
                if (childItems[bufIdx] != null)
                {
                    UpdateChild(childItems[bufIdx], row);
                }
            }
        }

        /// <summary>
        /// 供外部调用，强制刷新整个列表的某一个item
        /// </summary>
        public virtual void Refresh(RecyclingListViewItem item)
        {
            for (int i = 0; i < childItems.Length; ++i)
            {
                int idx = WrapChildIndex(childBufferStart + i);
                if (childItems[idx] != null && childItems[idx] == item)
                {
                    UpdateChild(childItems[i], sourceDataRowStart + i);
                    break;
                }
            }
        }

        /// <summary>
        /// 清空列表
        /// </summary>
        public virtual void Clear()
        {
            RowCount = 0;
        }


        /// <summary>
        /// 供外部调用，强制滚动列表，使某一行显示在列表中
        /// </summary>
        /// <param name="row">行号</param>
        /// <param name="posType">目标行显示在列表的位置：顶部，中心，底部</param>
        public virtual void ScrollToRow(int row, ScrollPosType posType)
        {
            switch (_scrollType)
            {
                case RecyclingListScrollType.Horizontal:
                {
                    scrollRect.horizontalNormalizedPosition = GetRowScrollPosition(row, posType);
                }
                    break;
                case RecyclingListScrollType.Vertical:
                {
                    scrollRect.verticalNormalizedPosition = GetRowScrollPosition(row, posType);
                }
                    break;
            }
        }

        /// <summary>
        /// 获得归一化的滚动位置，该位置将给定的行在视图中居中
        /// </summary>
        /// <param name="row">行号</param>
        /// <returns></returns>
        public float GetRowScrollPosition(int row, ScrollPosType posType)
        {
            // 视图高
            float vpHeight = ViewportWidthOrHeight();
            float rowHeight = RowWidthOrHeight();
            // 将目标行滚动到列表目标位置时，列表顶部的位置
            float vpTop = 0;
            switch (posType)
            {
                case ScrollPosType.Top:
                {
                    vpTop = row * rowHeight;
                }
                    break;
                case ScrollPosType.Center:
                {
                    // 目标行的中心位置与列表顶部的距离
                    float rowCentre = StartOffset + row * rowHeight + 0.5f * ChildWidthOrHeight();
                    // 视口中心位置
                    float halfVpHeight = vpHeight * 0.5f;

                    vpTop = Mathf.Max(0, rowCentre - halfVpHeight);
                }
                    break;
                case ScrollPosType.Bottom:
                {
                    vpTop = row * rowHeight + ChildWidthOrHeight() - vpHeight;
                }
                    break;
            }


            // 滚动后，列表底部的位置
            float vpBottom = vpTop + vpHeight;
            // 列表内容总高度
            float contentHeight = _scrollType == RecyclingListScrollType.Horizontal ? scrollRect.content.sizeDelta.x : scrollRect.content.sizeDelta.y;
            // 如果滚动后，列表底部的位置已经超过了列表总高度，则调整列表顶部的位置
            if (vpBottom > contentHeight)
                vpTop = Mathf.Max(0, vpTop - (vpBottom - contentHeight));

            // 反插值，计算两个值之间的Lerp参数。也就是value在from和to之间的比例值
            var inverseLerp = Mathf.InverseLerp(contentHeight - vpHeight, 0, vpTop);
            return _scrollType == RecyclingListScrollType.Horizontal ? 1 - inverseLerp : inverseLerp;
        }

        /// <summary>
        /// 根据行号获取复用的item对象
        /// </summary>
        /// <param name="row">行号</param>
        protected RecyclingListViewItem GetRowItem(int row)
        {
            if (childItems != null &&
                row >= sourceDataRowStart && row < sourceDataRowStart + childItems.Length &&
                row < rowCount)
            {
                // 注意这里要根据行号计算复用的item原始索引
                return childItems[WrapChildIndex(childBufferStart + row - sourceDataRowStart)];
            }

            return null;
        }

        protected virtual bool CheckChildItems()
        {
            // 列表视口高度
            float vpHeight = ViewportWidthOrHeight();
            float buildHeight = Mathf.Max(vpHeight, m_PreAllocHeight);
            bool rebuild = childItems == null || buildHeight > previousBuildHeight;
            if (rebuild)
            {
                int childCount = Mathf.RoundToInt(0.5f + buildHeight / RowWidthOrHeight());
                childCount += rowsAboveBelow * 2;

                if (childItems == null)
                    childItems = new RecyclingListViewItem[childCount];
                else if (childCount > childItems.Length)
                    Array.Resize(ref childItems, childCount);

                // 创建item
                for (int i = 0; i < childItems.Length; ++i)
                {
                    if (childItems[i] == null)
                    {
                        var item = Instantiate(m_ChildObj);
                        childItems[i] = item;
                    }

                    childItems[i].RectTransform.SetParent(scrollRect.content, false);
                    childItems[i].gameObject.SetActive(false);
                }

                previousBuildHeight = buildHeight;
            }

            return rebuild;
        }


        /// <summary>
        /// 列表滚动时，会回调此函数
        /// </summary>
        /// <param name="normalisedPos">归一化的位置</param>
        protected virtual void OnScrollChanged(Vector2 normalisedPos)
        {
            if (!ignoreScrollChange)
            {
                ReorganiseContent(false);
            }
        }

        /// <summary>
        /// 重新计算列表内容
        /// </summary>
        /// <param name="clearContents">是否要清空列表重新计算</param>
        protected virtual void ReorganiseContent(bool clearContents)
        {
            if (clearContents)
            {
                scrollRect.StopMovement();
                scrollRect.verticalNormalizedPosition = 1;
            }

            bool childrenChanged = CheckChildItems();
            // 是否要更新整个列表
            bool populateAll = childrenChanged || clearContents;

            float ymin = _scrollType == RecyclingListScrollType.Horizontal ? -scrollRect.content.localPosition.x : scrollRect.content.localPosition.y;

            // 第一个可见item的索引
            int firstVisibleIndex = (int) (ymin / RowWidthOrHeight()) - StartOffsetIndex;

            int newRowStart = firstVisibleIndex - rowsAboveBelow;

            // 滚动变化量
            int diff = newRowStart - sourceDataRowStart;
            if (populateAll || Mathf.Abs(diff) >= childItems.Length)
            {
                sourceDataRowStart = newRowStart;
                childBufferStart = 0;
                int rowIdx = newRowStart;
                foreach (var item in childItems)
                {
                    UpdateChild(item, rowIdx++);
                }
            }
            else if (diff != 0)
            {
                int newBufferStart = (childBufferStart + diff) % childItems.Length;

                if (diff < 0)
                {
                    // 向前滑动
                    for (int i = 1; i <= -diff; ++i)
                    {
                        // 得到复用item的索引
                        int wrapIndex = WrapChildIndex(childBufferStart - i);
                        int rowIdx = sourceDataRowStart - i;
                        UpdateChild(childItems[wrapIndex], rowIdx);
                    }
                }
                else
                {
                    // 向后滑动
                    int prevLastBufIdx = childBufferStart + childItems.Length - 1;
                    int prevLastRowIdx = sourceDataRowStart + childItems.Length - 1;
                    for (int i = 1; i <= diff; ++i)
                    {
                        int wrapIndex = WrapChildIndex(prevLastBufIdx + i);
                        int rowIdx = prevLastRowIdx + i;
                        UpdateChild(childItems[wrapIndex], rowIdx);
                    }
                }

                sourceDataRowStart = newRowStart;

                childBufferStart = newBufferStart;
            }
        }

        private int WrapChildIndex(int idx)
        {
            while (idx < 0)
                idx += childItems.Length;

            return idx % childItems.Length;
        }

        /// <summary>
        /// 获取child的宽度或高度
        /// </summary>
        public float ChildWidthOrHeight()
        {
            var rect = m_ChildObj.RectTransform.rect;
            return _scrollType == RecyclingListScrollType.Horizontal ? rect.width : rect.height;
        }

        /// <summary>
        /// 获取一行的高度，注意要加上RowPadding
        /// </summary>
        protected float RowWidthOrHeight()
        {
            return m_RowPadding + ChildWidthOrHeight();
        }

        /// <summary>
        /// 获取列表视口的宽度或高度
        /// </summary>
        protected float ViewportWidthOrHeight()
        {
            var rect = scrollRect.viewport.rect;
            return _scrollType == RecyclingListScrollType.Horizontal ? rect.width : rect.height;
        }

        protected virtual void UpdateChild(RecyclingListViewItem child, int rowIdx)
        {
            if (rowIdx < 0 || rowIdx >= rowCount)
            {
                child.gameObject.SetActive(false);
            }
            else
            {
                if (ItemCallback == null)
                {
                    Debug.Log("RecyclingListView is missing an ItemCallback, cannot function", this);
                    return;
                }

                // 移动到正确的位置
                var childRect = m_ChildObj.RectTransform.rect;
                Vector2 pivot = m_ChildObj.RectTransform.pivot;
                float offset = RowWidthOrHeight() * rowIdx + StartOffset;

                if (_scrollType == RecyclingListScrollType.Horizontal)
                {
                    float xpos = offset + pivot.x * childRect.width;
                    float ypos = 0 + (1f - pivot.y) * childRect.height;
                    child.RectTransform.anchoredPosition = new Vector2(xpos, ypos);
                    child.NotifyCurrentAssignment(this, rowIdx);
                }
                else
                {
                    float xpos = 0 + pivot.x * childRect.width;
                    float ypos = offset + (1f - pivot.y) * childRect.height;
                    child.RectTransform.anchoredPosition = new Vector2(xpos, -ypos);
                    child.NotifyCurrentAssignment(this, rowIdx);
                }

                // 更新数据
                ItemCallback(child, rowIdx);

                child.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 更新content
        /// </summary>
        protected virtual void UpdateContentHeight()
        {
            if (_scrollType == RecyclingListScrollType.Horizontal)
            {
                var width = m_ChildObj.RectTransform.rect.width * rowCount + (rowCount - 1) * m_RowPadding;
                var sz = scrollRect.content.sizeDelta;
                scrollRect.content.sizeDelta = new Vector2(width + StartOffset * 2, sz.y);
            }
            else
            {
                var height = m_ChildObj.RectTransform.rect.height * rowCount + (rowCount - 1) * m_RowPadding;
                var sz = scrollRect.content.sizeDelta;
                scrollRect.content.sizeDelta = new Vector2(sz.x, height + StartOffset * 2);
            }
        }

        protected virtual void DisableAllChildren()
        {
            if (childItems != null)
            {
                for (int i = 0; i < childItems.Length; ++i)
                {
                    childItems[i].gameObject.SetActive(false);
                }
            }
        }
    }
}