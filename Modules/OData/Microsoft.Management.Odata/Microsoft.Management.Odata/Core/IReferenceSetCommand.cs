using System;
using System.Collections.Generic;

namespace Microsoft.Management.Odata.Core
{
	internal interface IReferenceSetCommand : ICommand, IDisposable
	{
		void AddReferredObject(Dictionary<string, object> keys);

		void AddReferringObject(Dictionary<string, object> keys);
	}
}