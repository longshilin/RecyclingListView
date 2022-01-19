using System.Collections.Generic;
using SuperScrollView;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Example_Grid
{
    [DisallowMultipleComponent]
    public sealed class Example : MonoBehaviour
    {
        [SerializeField] private LoopListView2 m_view = null;
        [SerializeField] private GameObject m_original = null;
        [SerializeField] private int m_countPerRow = 0;

        private ListItemData[] m_list;
        private int isShowIndex = -1; // 选择的item所在行号
        private int isShowItemIndex = -1; // 选择的item需要

        private void Start()
        {
            m_list = Enumerable
                    .Range(-1, 2000)
                    .Select(c => (c + 1).ToString("000"))
                    .Select(c => new ListItemData(c, int.Parse(c)))
                    .ToArray()
                ;

            int count = m_list.Length / m_countPerRow;
            if (0 < m_list.Length % m_countPerRow)
            {
                count++;
            }

            m_view.InitListView(count, OnUpdate);
        }

        private LoopListViewItem2 OnUpdate(LoopListView2 view, int index)
        {
            if (index < 0) return null;


            var itemObj = view.NewListViewItem(m_original.name);
            var children = itemObj.GetComponentsInChildren<ListItemUI>(true);
            var tip = itemObj.CachedRectTransform.Find("Tip");

            for (int i = 0; i < m_countPerRow; i++)
            {
                int itemIndex = index * m_countPerRow + i;
                var child = children[i];
                var childObj = child.gameObject;

                if (m_list.Length <= itemIndex)
                {
                    childObj.SetActive(false);
                }

                var data = m_list.ElementAtOrDefault(itemIndex);

                if (data != null)
                {
                    childObj.SetActive(true);

                    data.Callback = selectIndex =>
                    {
                        if (selectIndex == isShowItemIndex)
                        {
                            itemObj.IsShow = !itemObj.IsShow;
                            // Debug.Log("点击的是同一个item");
                            // Debug.Log("需要关闭提示");
                        }
                        else if (selectIndex / 5 == isShowIndex)
                        {
                            // Debug.Log("点击的是同一行item");
                            // Debug.Log("直接显示另一个提示");
                        }
                        else
                        {
                            var preItemObj = m_view.GetShownItemByIndex(isShowIndex);
                            if (preItemObj != null)
                            {
                                preItemObj.IsShow = false;
                                var preTip = preItemObj.CachedRectTransform.Find("Tip");
                                preTip.gameObject.SetActive(preItemObj.IsShow);
                                preItemObj.CachedRectTransform.sizeDelta = new Vector2(0, preItemObj.IsShow ? 500 : 286);
                            }

                            // Debug.Log("点击的是不同行的item");
                            // Debug.Log("关闭之前这个，然后开启新的");
                            itemObj.IsShow = true;
                        }

                        if (itemObj.IsShow)
                        {
                            isShowIndex = selectIndex / 5;
                            isShowItemIndex = selectIndex;
                        }
                        else
                        {
                            isShowIndex = -1;
                            isShowItemIndex = -1;
                        }

                        tip.gameObject.SetActive(itemObj.IsShow);
                        tip.GetComponentInChildren<Text>().text = data.Name;
                        itemObj.CachedRectTransform.sizeDelta = new Vector2(0, itemObj.IsShow ? 500 : 286);
                        m_view.RefreshItemByItemIndex(index);
                        // 当标签的顶部和底部都可以完整出现是，不需要执行强制跳转
                        // 其他情况就是从顶部向下或底部向上滑动一定距离至可视区域
                        // m_view.MovePanelToItemIndex(index, 0);
                    };

                    data.SetIndex(index);
                    child.SetDisp(data, itemObj);
                    if (data.Index == isShowIndex)
                    {
                        if (data.ItemIndex == isShowItemIndex)
                        {
                            // 当前item已经是展示状态
                            itemObj.IsShow = true;
                            tip.gameObject.SetActive(itemObj.IsShow);
                            tip.GetComponentInChildren<Text>().text = data.Name;
                            itemObj.CachedRectTransform.sizeDelta = new Vector2(0, itemObj.IsShow ? 500 : 286);
                        }
                    }
                    else
                    {
                        // 当前item已经是展示状态
                        tip.gameObject.SetActive(false);
                        itemObj.CachedRectTransform.sizeDelta = new Vector2(0, 286);
                    }
                }
                else
                {
                    childObj.SetActive(false);
                }
            }

            return itemObj;
        }
    }
}