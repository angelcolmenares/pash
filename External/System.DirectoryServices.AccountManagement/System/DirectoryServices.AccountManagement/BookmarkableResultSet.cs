using System;

namespace System.DirectoryServices.AccountManagement
{
	internal abstract class BookmarkableResultSet : ResultSet
	{
		protected BookmarkableResultSet()
		{
		}

		internal abstract ResultSetBookmark BookmarkAndReset();

		internal abstract void RestoreBookmark(ResultSetBookmark bookmark);
	}
}