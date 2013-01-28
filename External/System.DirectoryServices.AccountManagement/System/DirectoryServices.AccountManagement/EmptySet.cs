using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class EmptySet : BookmarkableResultSet
	{
		internal override object CurrentAsPrincipal
		{
			get
			{
				return null;
			}
		}

		internal EmptySet()
		{
		}

		internal override ResultSetBookmark BookmarkAndReset()
		{
			return new EmptySetBookmark();
		}

		internal override bool MoveNext()
		{
			return false;
		}

		internal override void Reset()
		{
		}

		internal override void RestoreBookmark(ResultSetBookmark bookmark)
		{
		}
	}
}