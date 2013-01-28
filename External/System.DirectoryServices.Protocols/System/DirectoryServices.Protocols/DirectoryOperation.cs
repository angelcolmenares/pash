using System;
using System.Runtime;

namespace System.DirectoryServices.Protocols
{
	public abstract class DirectoryOperation
	{
		internal string directoryRequestID;

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected DirectoryOperation()
		{
		}
	}
}