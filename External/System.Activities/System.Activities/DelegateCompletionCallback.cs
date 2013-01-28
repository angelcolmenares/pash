using System;
using System.Collections.Generic;

namespace System.Activities
{
	public delegate void DelegateCompletionCallback (NativeActivityContext context, ActivityInstance completedInstance, IDictionary<string, Object> outArguments);
}
