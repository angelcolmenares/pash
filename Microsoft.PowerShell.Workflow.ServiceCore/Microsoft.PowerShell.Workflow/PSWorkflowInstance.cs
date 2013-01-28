using System;
using System.Activities;
using System.Activities.Hosting;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Microsoft.PowerShell.Workflow
{
	public abstract class PSWorkflowInstance : IDisposable
	{
		private object _syncLock;

		protected bool Disposed
		{
			get;
			set;
		}

		public virtual Exception Error
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		internal bool ForceDisableStartOrEndPersistence
		{
			get;
			set;
		}

		internal virtual Guid Id
		{
			get
			{
				return this.InstanceId.Guid;
			}
		}

		public virtual PSWorkflowId InstanceId
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public virtual PSWorkflowInstanceStore InstanceStore
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		internal bool JobStateRetrieved
		{
			get;
			set;
		}

		protected Action<Exception, object> OnAborted
		{
			get;
			private set;
		}

		internal Action<Exception, object> OnAbortedDelegate
		{
			set
			{
				this.OnAborted = value;
			}
		}

		protected Action<object> OnCompleted
		{
			get;
			private set;
		}

		internal Action<object> OnCompletedDelegate
		{
			set
			{
				this.OnCompleted = value;
			}
		}

		protected Action<Exception, object> OnFaulted
		{
			get;
			private set;
		}

		internal Action<Exception, object> OnFaultedDelegate
		{
			set
			{
				this.OnFaulted = value;
			}
		}

		protected Action<ReadOnlyCollection<BookmarkInfo>, object> OnIdle
		{
			get;
			private set;
		}

		internal Action<ReadOnlyCollection<BookmarkInfo>, object> OnIdleDelegate
		{
			set
			{
				this.OnIdle = value;
			}
		}

		protected Func<ReadOnlyCollection<BookmarkInfo>, bool, object, PSPersistableIdleAction> OnPersistableIdleAction
		{
			get;
			private set;
		}

		internal Func<ReadOnlyCollection<BookmarkInfo>, bool, object, PSPersistableIdleAction> OnPersistableIdleActionDelegate
		{
			set
			{
				this.OnPersistableIdleAction = value;
			}
		}

		protected Action<object> OnStopped
		{
			get;
			private set;
		}

		internal Action<object> OnStoppedDelegate
		{
			set
			{
				this.OnStopped = value;
			}
		}

		protected Action<object> OnSuspended
		{
			get;
			private set;
		}

		internal Action<object> OnSuspenedDelegate
		{
			set
			{
				this.OnSuspended = value;
			}
		}

		protected Action<object> OnUnloaded
		{
			get;
			private set;
		}

		internal Action<object> OnUnloadedDelegate
		{
			set
			{
				this.OnUnloaded = value;
			}
		}

		public virtual PSWorkflowContext PSWorkflowContext
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public virtual PSWorkflowDefinition PSWorkflowDefinition
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public virtual PSWorkflowJob PSWorkflowJob
		{
			get
			{
				throw new NotImplementedException();
			}
			protected internal set
			{
				throw new NotImplementedException();
			}
		}

		internal PSWorkflowRuntime Runtime
		{
			get;
			set;
		}

		public virtual JobState State
		{
			get;
			set;
		}

		public virtual PowerShellStreams<PSObject, PSObject> Streams
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		protected object SyncLock
		{
			get
			{
				return this._syncLock;
			}
		}

		public virtual PSWorkflowTimer Timer
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		protected PSWorkflowInstance()
		{
			this._syncLock = new object();
		}

		internal void AbortInstance(string reason)
		{
			this.DoAbortInstance(reason);
		}

		internal virtual void CheckForTerminalAction()
		{
			throw new NotImplementedException();
		}

		internal void CreateInstance()
		{
			this.DoCreateInstance();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing || this.Disposed)
			{
				return;
			}
			else
			{
				lock (this.SyncLock)
				{
					if (!this.Disposed)
					{
						this.Disposed = true;
						this.OnCompleted = null;
						this.OnFaulted = null;
						this.OnStopped = null;
						this.OnAborted = null;
						this.OnSuspended = null;
						this.OnIdle = null;
						this.OnPersistableIdleAction = null;
						this.OnUnloaded = null;
					}
				}
				return;
			}
		}

		public void Dispose()
		{
			if (!this.Disposed)
			{
				this.Dispose(true);
				GC.SuppressFinalize(this);
				return;
			}
			else
			{
				return;
			}
		}

		public virtual void DisposeStreams()
		{
		}

		protected virtual void DoAbortInstance(string reason)
		{
			throw new NotImplementedException();
		}

		protected virtual void DoCreateInstance()
		{
			throw new NotImplementedException();
		}

		protected virtual void DoExecuteInstance()
		{
			throw new NotImplementedException();
		}

		protected virtual PSPersistableIdleAction DoGetPersistableIdleAction(ReadOnlyCollection<BookmarkInfo> bookmarks, bool externalSuspendRequest)
		{
			throw new NotImplementedException();
		}

		internal virtual void DoLoadInstanceForReactivation()
		{
			throw new NotImplementedException();
		}

		protected virtual void DoPersistInstance()
		{
			throw new NotImplementedException();
		}

		protected virtual void DoRemoveInstance()
		{
			throw new NotImplementedException();
		}

		protected virtual void DoResumeBookmark(Bookmark bookmark, object state)
		{
			throw new NotImplementedException();
		}

		protected virtual void DoResumeInstance(string label)
		{
			throw new NotImplementedException();
		}

		protected virtual void DoStopInstance()
		{
			throw new NotImplementedException();
		}

		protected virtual void DoSuspendInstance(bool notStarted)
		{
			throw new NotImplementedException();
		}

		protected virtual void DoTerminateInstance(string reason)
		{
			throw new NotImplementedException();
		}

		internal void ExecuteInstance()
		{
			this.DoExecuteInstance();
		}

		internal PSPersistableIdleAction GetPersistableIdleAction(ReadOnlyCollection<BookmarkInfo> bookmarks, bool externalSuspendRequest)
		{
			return this.DoGetPersistableIdleAction(bookmarks, externalSuspendRequest);
		}

		internal virtual void PerformTaskAtTerminalState()
		{
			throw new NotImplementedException();
		}

		internal void PersistInstance()
		{
			this.DoPersistInstance();
		}

		internal void RemoveInstance()
		{
			this.DoRemoveInstance();
		}

		internal void ResumeBookmark(Bookmark bookmark, object state)
		{
			this.DoResumeBookmark(bookmark, state);
		}

		internal void ResumeInstance(string label)
		{
			this.DoResumeInstance(label);
		}

		internal virtual bool SaveStreamsIfNecessary()
		{
			return false;
		}

		internal void StopInstance()
		{
			this.DoStopInstance();
		}

		internal void SuspendInstance(bool notStarted)
		{
			this.DoSuspendInstance(notStarted);
		}

		internal void TerminateInstance(string reason)
		{
			this.DoTerminateInstance(reason);
		}

		internal virtual void ValidateIfLabelExists(string label)
		{
			throw new NotImplementedException();
		}
	}
}