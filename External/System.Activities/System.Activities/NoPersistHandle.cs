using System;
using System.Runtime.Serialization;

namespace System.Activities
{
	[DataContract]
	public class NoPersistHandle : Handle
	{
		public void Enter (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
		public void Exit (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
