using System;

namespace Shell32
{
	public class OSXFolder : Folder2, Folder
	{
		public OSXFolder ()
		{

		}

		#region Folder2 implementation

		public void _VtblGap1_4 ()
		{

		}

		public virtual FolderItems Items ()
		{
			return new OSXFolderItems();
		}

		#endregion
	}
}

