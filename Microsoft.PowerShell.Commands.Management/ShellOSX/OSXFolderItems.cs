using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Shell32
{
	public class OSXFolderItems : FolderItems3, FolderItems2, FolderItems
	{
		private List<OSXShellFolderItem> _list;

		public OSXFolderItems ()
		{
			_list = new List<OSXShellFolderItem>();
			var files = new List<string>();
			files.AddRange (Directory.GetDirectories ("/System/Library/PreferencePanes", "*.prefPane"));
			files.AddRange (Directory.GetDirectories ("/Library/PreferencePanes", "*.prefPane"));

			_list.AddRange (files.Select (x => new OSXShellFolderItem(x)));
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

