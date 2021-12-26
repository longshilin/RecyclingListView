using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

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
        yield return 0;
        scrollRect.StopMovement();
        _needFixDrag = false;
        foreach (var childItem in childItems)
        {
            childItem.NotifyCurrentAssignment(this, childItem.CurrentRow);
        }

        var rowScrollPosition = GetRowScrollPosition(_index + 1, posType);
        float time = 0;
        while (time < 1f)
        {
            time += Time.deltaTime * 5f;
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition, rowScrollPosition, time);
            yield return 0;
        }
    }

    public void UpdateCenterItem(int index, float offset)
    {
        if (_offset > offset)
        {
            _index = index;
            _offset = offset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _needFixDrag = true;
        _offset = float.MaxValue;
    }
}