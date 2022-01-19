﻿using System.Collections.Generic;
using SuperScrollView;
using System.Linq;
using UnityEngine;

namespace Example_Grid
{
    [DisallowMultipleComponent]
    public sealed class Example : MonoBehaviour
    {
        [SerializeField] private LoopListView2 m_view = null;
        [SerializeField] private GameObject m_original = null;
        [SerializeField] private int m_countPerRow = 0;

        private ListItemData[] m_list;
        private int isShowIndex = -1;

        private void Start()
        {
            m_list = Enumerable
                    .Range(0, 200)
                    .Select(c => (c + 1).ToString("000"))
                    .Select(c => new ListItemData(c))
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
                    data.Callback = delegate
                    {
                        Debug.Log($"callback - {index}");
                        itemObj.CachedRectTransform.Find("Tip").gameObject.SetActive(data.IsShow);
                        itemObj.CachedRectTransform.sizeDelta = new Vector2(0, data.IsShow ? 500 : 286);
                        m_view.RefreshItemByItemIndex(index);
                    };
                    child.SetDisp(data, itemObj);
                    /*child.Callback = arg0 =>
                    {
                        if (isShowIndex == index)
                        {
                            child.m_data.IsShow = false;
                            m_view.RefreshItemByItemIndex(index);
                        }
                        else if (arg0.m_Index == index)
                        {
                            m_view.RefreshItemByItemIndex(index);
                            isShowIndex = arg0.m_Index;
                        }
                    };*/
                    
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