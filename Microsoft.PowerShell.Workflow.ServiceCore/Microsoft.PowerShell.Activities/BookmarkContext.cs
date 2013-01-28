using System.Activities;

namespace Microsoft.PowerShell.Activities
{
	internal class BookmarkContext
	{
		internal PSWorkflowInstanceExtension BookmarkResumingExtension
		{
			get;
			set;
		}

		internal Bookmark CurrentBookmark
		{
			get;
			set;
		}

		public BookmarkContext()
		{
		}
	}
}