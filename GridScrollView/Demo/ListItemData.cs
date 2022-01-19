using UnityEngine.Events;

namespace Example_Grid
{
	public sealed class ListItemData
	{
		private readonly string m_name;

		public string Name { get { return m_name; } }
        public int Index;
        public int ItemIndex;
        public UnityAction<int> Callback;

		public ListItemData( string name, int itemIndex)
		{
			m_name = name;
            ItemIndex = itemIndex;
        }

        public void SetIndex(int index)
        {
            Index = index;
            
        }
            
	}
}