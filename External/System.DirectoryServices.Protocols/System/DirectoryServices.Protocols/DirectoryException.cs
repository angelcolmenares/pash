using System;
using System.Runtime;
using System.Runtime.Serialization;

namespace System.DirectoryServices.Protocols
{
	[Serializable]
	public class DirectoryException : Exception
	{
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected DirectoryException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public DirectoryException(string message, Exception inner) : base(message, inner)
		{
			Utility.CheckOSVersion();
		}

		public DirectoryException(string message) : base(message)
		{
			Utility.CheckOSVersion();
		}

		public DirectoryException()
		{
			Utility.CheckOSVersion();
		}
	}
}