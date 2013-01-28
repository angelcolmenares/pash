using System;
using System.Activities;
using System.Activities.Hosting;
using System.Collections.Generic;

namespace Microsoft.PowerShell.Activities
{
	public class PSWorkflowInstanceExtension : IWorkflowInstanceExtension
	{
		private WorkflowInstanceProxy instance;

		public PSWorkflowInstanceExtension()
		{
		}

		public IAsyncResult BeginResumeBookmark(Bookmark bookmark, object value, AsyncCallback callback, object state)
		{
			return this.instance.BeginResumeBookmark(bookmark, value, callback, state);
		}

		public BookmarkResumptionResult EndResumeBookmark(IAsyncResult asyncResult)
		{
			return this.instance.EndResumeBookmark(asyncResult);
		}

		public IEnumerable<object> GetAdditionalExtensions()
		{
			return null;
		}

		public void SetInstance(WorkflowInstanceProxy instance)
		{
			this.instance = instance;
		}
	}
}