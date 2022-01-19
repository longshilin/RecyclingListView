using SuperScrollView;
using UnityEngine;
using UnityEngine.UI;

namespace Example_Grid
{
    [DisallowMultipleComponent]
    public sealed class ListItemUI : MonoBehaviour
    {
        [SerializeField] private Button m_buttonUI = null;
        [SerializeField] private Text m_textUI = null;
        [SerializeField] private Transform m_Tip = null;

        public ListItemData m_data;
        private LoopListViewItem2 m_Parent;

        private void Awake()
        {
            m_buttonUI.onClick.AddListener(() =>
            {
                m_data.IsShow = !m_data.IsShow;
                m_data.Callback?.Invoke(m_data);
            });
        }

       
        public void SetDisp(ListItemData data, LoopListViewItem2 loopListViewItem2)
        {
            m_Parent = loopListViewItem2;
            m_data = data;

            m_textUI.text = data.Name;
        }
    }
}