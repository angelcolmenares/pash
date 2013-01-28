using System;
using System.Collections.Generic;
using System.Runtime;
using System.Xml.Linq;

namespace System.Activities.Persistence
{
	public abstract class PersistenceIOParticipant : PersistenceParticipant
	{
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected PersistenceIOParticipant(bool isSaveTransactionRequired, bool isLoadTransactionRequired) : base(isSaveTransactionRequired, isLoadTransactionRequired)
		{
		}

		protected abstract void Abort();

		protected virtual IAsyncResult BeginOnLoad(IDictionary<XName, object> readWriteValues, TimeSpan timeout, AsyncCallback callback, object state)
		{
			return new CompletedAsyncResult(callback, state);
		}

		protected virtual IAsyncResult BeginOnSave(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues, TimeSpan timeout, AsyncCallback callback, object state)
		{
			return new CompletedAsyncResult(callback, state);
		}

		protected virtual void EndOnLoad(IAsyncResult result)
		{
			CompletedAsyncResult.End(result);
		}

		protected virtual void EndOnSave(IAsyncResult result)
		{
			CompletedAsyncResult.End(result);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal override void InternalAbort()
		{
			this.Abort();
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal override IAsyncResult InternalBeginOnLoad(IDictionary<XName, object> readWriteValues, TimeSpan timeout, AsyncCallback callback, object state)
		{
			return this.BeginOnLoad(readWriteValues, timeout, callback, state);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal override IAsyncResult InternalBeginOnSave(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues, TimeSpan timeout, AsyncCallback callback, object state)
		{
			return this.BeginOnSave(readWriteValues, writeOnlyValues, timeout, callback, state);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal override void InternalEndOnLoad(IAsyncResult result)
		{
			this.EndOnLoad(result);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal override void InternalEndOnSave(IAsyncResult result)
		{
			this.EndOnSave(result);
		}
	}
}