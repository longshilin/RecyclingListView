using UnityEngine.Events;

namespace Example_Grid
{
	public sealed class ListItemData
	{
		private readonly string m_name;

		public string Name { get { return m_name; } }
        public bool IsShow;
        public UnityAction<ListItemData> Callback;

		public ListItemData( string name)
		{
			m_name = name;
        }
	}
}