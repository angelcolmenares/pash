using System.Runtime;

namespace System.DirectoryServices.Protocols
{
	public sealed class ReferralCallback
	{
		private QueryForConnectionCallback query;

		private NotifyOfNewConnectionCallback notify;

		private DereferenceConnectionCallback dereference;

		public DereferenceConnectionCallback DereferenceConnection
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.dereference;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.dereference = value;
			}
		}

		public NotifyOfNewConnectionCallback NotifyNewConnection
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.notify;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.notify = value;
			}
		}

		public QueryForConnectionCallback QueryForConnection
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.query;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.query = value;
			}
		}

		public ReferralCallback()
		{
			Utility.CheckOSVersion();
		}
	}
}