using System;
using System.Runtime.Serialization;

namespace System.Activities
{
	[DataContract]
	public enum ActivityInstanceState
	{
		Executing,
		Closed,
		Canceled,
		Faulted
	}
}
