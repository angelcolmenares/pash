using System;
using System.Runtime.Serialization;

namespace System.Activities
{
	public abstract class ActivityWithResult : Activity
	{
		internal ActivityWithResult (Type resultType)
		{
			ResultType = resultType;
		}

		[IgnoreDataMemberAttribute]
		public OutArgument Result { get; set; }
		public Type ResultType { get; private set; }
	}
}
