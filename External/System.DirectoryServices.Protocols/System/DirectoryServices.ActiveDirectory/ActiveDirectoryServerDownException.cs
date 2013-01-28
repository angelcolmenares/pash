using System;
using System.DirectoryServices;
using System.Runtime;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[Serializable]
	public class ActiveDirectoryServerDownException : Exception, ISerializable
	{
		private int errorCode;

		private string name;

		public int ErrorCode
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.errorCode;
			}
		}

		public override string Message
		{
			get
			{
				string message = base.Message;
				if (this.name == null || this.name.Length == 0)
				{
					return message;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = this.name;
					return string.Concat(message, Environment.NewLine, Res.GetString("Name", objArray), Environment.NewLine);
				}
			}
		}

		public string Name
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.name;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ActiveDirectoryServerDownException(string message, Exception inner, int errorCode, string name) : base(message, inner)
		{
			this.errorCode = errorCode;
			this.name = name;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ActiveDirectoryServerDownException(string message, int errorCode, string name) : base(message)
		{
			this.errorCode = errorCode;
			this.name = name;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ActiveDirectoryServerDownException(string message, Exception inner) : base(message, inner)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ActiveDirectoryServerDownException(string message) : base(message)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ActiveDirectoryServerDownException()
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected ActiveDirectoryServerDownException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		[SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			base.GetObjectData(serializationInfo, streamingContext);
		}
	}
}