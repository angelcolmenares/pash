using System;
using System.Collections.Generic;

namespace Shell32
{
	public class OSXCategoryFolderItems : FolderItems3, FolderItems2, FolderItems
	{
		private List<OSXCategoryFolderItem> _list;

			public OSXCategoryFolderItems ()
		{
			_list = new List<OSXCategoryFolderItem>();
			_list.Add (new OSXCategoryFolderItem("OSX"));
		}
		
		#region IEnumerable implementation
		
		public System.Collections.IEnumerator GetEnumerator ()
		{
			return _list.GetEnumerator ();
		}
		
		#endregion
		
		#region FolderItems3 implementation
		
		public void _VtblGap1_4 ()
		{
			
		}
		
		#endregion

	}
}

