using System;
using System.Runtime;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[Serializable]
	public class ActiveDirectoryObjectNotFoundException : Exception, ISerializable
	{
		private Type objectType;

		private string name;

		public string Name
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.name;
			}
		}

		public Type Type
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.objectType;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ActiveDirectoryObjectNotFoundException(string message, Type type, string name) : base(message)
		{
			this.objectType = type;
			this.name = name;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ActiveDirectoryObjectNotFoundException(string message, Exception inner) : base(message, inner)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ActiveDirectoryObjectNotFoundException(string message) : base(message)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ActiveDirectoryObjectNotFoundException()
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected ActiveDirectoryObjectNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		[SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			base.GetObjectData(serializationInfo, streamingContext);
		}
	}
}