using System;
using System.DirectoryServices;
using System.Runtime;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[Serializable]
	public class SyncFromAllServersOperationException : ActiveDirectoryOperationException, ISerializable
	{
		private SyncFromAllServersErrorInformation[] errors;

		public SyncFromAllServersErrorInformation[] ErrorInformation
		{
			get
			{
				if (this.errors != null)
				{
					SyncFromAllServersErrorInformation[] syncFromAllServersErrorInformation = new SyncFromAllServersErrorInformation[(int)this.errors.Length];
					for (int i = 0; i < (int)this.errors.Length; i++)
					{
						syncFromAllServersErrorInformation[i] = new SyncFromAllServersErrorInformation(this.errors[i].ErrorCategory, this.errors[i].ErrorCode, this.errors[i].ErrorMessage, this.errors[i].SourceServer, this.errors[i].TargetServer);
					}
					return syncFromAllServersErrorInformation;
				}
				else
				{
					return new SyncFromAllServersErrorInformation[0];
				}
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public SyncFromAllServersOperationException(string message, Exception inner, SyncFromAllServersErrorInformation[] errors) : base(message, inner)
		{
			this.errors = errors;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public SyncFromAllServersOperationException(string message, Exception inner) : base(message, inner)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public SyncFromAllServersOperationException(string message) : base(message)
		{
		}

		public SyncFromAllServersOperationException() : base(Res.GetString("DSSyncAllFailure"))
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected SyncFromAllServersOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		[SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			base.GetObjectData(serializationInfo, streamingContext);
		}
	}
}