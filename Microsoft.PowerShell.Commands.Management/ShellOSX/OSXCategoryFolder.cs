using System;

namespace Shell32
{
	public class OSXCategoryFolder : OSXFolder
	{
		public OSXCategoryFolder ()
		{
		}

		public override FolderItems Items ()
		{
			return new OSXCategoryFolderItems();
		}
	}
}

