using System.Collections;
using UnityEngine;
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
        private int _index;

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

            var rowScrollPosition = GetRowScrollPosition(_index, posType);
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
        }

        public override void ScrollToRow(int row, ScrollPosType posType)
        {
            base.ScrollToRow(row + StartOffsetIndex, posType);
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
                _index = index + StartOffsetIndex;
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