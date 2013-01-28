using System;
using System.Collections.Generic;
using System.Runtime;
using System.Xml.Linq;

namespace System.Activities.Persistence
{
	public abstract class PersistenceParticipant : IPersistencePipelineModule
	{
		private bool isSaveTransactionRequired;

		private bool isLoadTransactionRequired;

		private bool isIOParticipant;

		bool System.Runtime.IPersistencePipelineModule.IsIOParticipant
		{
			get
			{
				return this.isIOParticipant;
			}
		}

		bool System.Runtime.IPersistencePipelineModule.IsLoadTransactionRequired
		{
			get
			{
				return this.isLoadTransactionRequired;
			}
		}

		bool System.Runtime.IPersistencePipelineModule.IsSaveTransactionRequired
		{
			get
			{
				return this.isSaveTransactionRequired;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected PersistenceParticipant()
		{
		}

		internal PersistenceParticipant(bool isSaveTransactionRequired, bool isLoadTransactionRequired)
		{
			this.isIOParticipant = true;
			this.isSaveTransactionRequired = isSaveTransactionRequired;
			this.isLoadTransactionRequired = isLoadTransactionRequired;
		}

		protected virtual void CollectValues(out IDictionary<XName, object> readWriteValues, out IDictionary<XName, object> writeOnlyValues)
		{
			readWriteValues = null;
			writeOnlyValues = null;
		}

		internal virtual void InternalAbort()
		{
		}

		internal virtual IAsyncResult InternalBeginOnLoad(IDictionary<XName, object> readWriteValues, TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new InvalidOperationException("BeginOnLoad should not be called on PersistenceParticipant."); //TODO: Fx.AssertAndThrow("BeginOnLoad should not be called on PersistenceParticipant.");
		}

		internal virtual IAsyncResult InternalBeginOnSave(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues, TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new InvalidOperationException("BeginOnSave should not be called on PersistenceParticipant."); //TODO: Fx.AssertAndThrow("BeginOnSave should not be called on PersistenceParticipant.");
		}

		internal virtual void InternalEndOnLoad(IAsyncResult result)
		{
		}

		internal virtual void InternalEndOnSave(IAsyncResult result)
		{
		}

		protected virtual IDictionary<XName, object> MapValues(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues)
		{
			return null;
		}

		protected virtual void PublishValues(IDictionary<XName, object> readWriteValues)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		void System.Runtime.IPersistencePipelineModule.Abort()
		{
			this.InternalAbort();
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		IAsyncResult System.Runtime.IPersistencePipelineModule.BeginOnLoad(IDictionary<XName, object> readWriteValues, TimeSpan timeout, AsyncCallback callback, object state)
		{
			return this.InternalBeginOnLoad(readWriteValues, timeout, callback, state);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		IAsyncResult System.Runtime.IPersistencePipelineModule.BeginOnSave(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues, TimeSpan timeout, AsyncCallback callback, object state)
		{
			return this.InternalBeginOnSave(readWriteValues, writeOnlyValues, timeout, callback, state);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		void System.Runtime.IPersistencePipelineModule.CollectValues(out IDictionary<XName, object> readWriteValues, out IDictionary<XName, object> writeOnlyValues)
		{
			this.CollectValues(out readWriteValues, out writeOnlyValues);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		void System.Runtime.IPersistencePipelineModule.EndOnLoad(IAsyncResult result)
		{
			this.InternalEndOnLoad(result);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		void System.Runtime.IPersistencePipelineModule.EndOnSave(IAsyncResult result)
		{
			this.InternalEndOnSave(result);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		IDictionary<XName, object> System.Runtime.IPersistencePipelineModule.MapValues(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues)
		{
			return this.MapValues(readWriteValues, writeOnlyValues);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		void System.Runtime.IPersistencePipelineModule.PublishValues(IDictionary<XName, object> readWriteValues)
		{
			this.PublishValues(readWriteValues);
		}
	}
}