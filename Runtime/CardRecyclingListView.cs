using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Longshilin.RecyclingListView
{
    /// <summary>
    /// 循环复用列表
    /// </summary>
    public class CardRecyclingListView : RecyclingListView, IEndDragHandler
    {
        private bool _needFixDrag;
        private float _offset;
        private int _previousIndex = -1;

        private int _currentIndex; // 居中的item序号
        public int CurrentIndex => _currentIndex;

        private UnityAction _onCurrentItemChanged; // 列表滑动更新时回调方法

        public UnityAction OnCurrentItemChanged
        {
            get => _onCurrentItemChanged;
            set => _onCurrentItemChanged = value;
        }

        private void LateUpdate()
        {
            if (!_needFixDrag) return;

            if (scrollRect.velocity.y < 100f)
            {
                StartCoroutine(ScrollToRowCor(ScrollPosType.Center));
            }
        }

        IEnumerator ScrollToRowCor(ScrollPosType posType)
        {
            scrollRect.StopMovement();
            _needFixDrag = false;
            foreach (var childItem in childItems)
            {
                if (childItem.isActiveAndEnabled)
                    childItem.NotifyCurrentAssignment(this, childItem.CurrentRow);
            }

            if (_previousIndex != _currentIndex) _onCurrentItemChanged?.Invoke();

            var rowScrollPosition = GetRowScrollPosition(_currentIndex + StartOffsetIndex, posType);
            float time = 0;
            while (time < 1f)
            {
                time += Time.deltaTime * 5f;

                switch (ScrollType)
                {
                    case RecyclingListScrollType.Horizontal:
                    {
                        scrollRect.horizontalNormalizedPosition = Mathf.Lerp(scrollRect.horizontalNormalizedPosition, rowScrollPosition, time);
                    }
                        break;
                    case RecyclingListScrollType.Vertical:
                    {
                        scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition, rowScrollPosition, time);
                    }
                        break;
                }

                yield return 0;
            }

            _previousIndex = _currentIndex;
        }

        public override void Clear()
        {
            base.Clear();
            _previousIndex = _currentIndex = -1;
        }

        public override void ScrollToRow(int row, ScrollPosType posType)
        {
            _currentIndex = row + StartOffsetIndex;
            base.ScrollToRow(_currentIndex, posType);
        }

        protected override void UpdateChild(RecyclingListViewItem child, int rowIdx)
        {
            StartOffset = 0.5f * (ViewportWidthOrHeight() - ChildWidthOrHeight());
            StartOffsetIndex = (int) (0.5f * ViewportWidthOrHeight() / RowWidthOrHeight());
            base.UpdateChild(child, rowIdx);
        }

        public void UpdateCenterItem(int index, float offset)
        {
            if (_offset > offset)
            {
                _currentIndex = index;
                _offset = offset;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _needFixDrag = true;
            _offset = float.MaxValue;
        }
    }
}