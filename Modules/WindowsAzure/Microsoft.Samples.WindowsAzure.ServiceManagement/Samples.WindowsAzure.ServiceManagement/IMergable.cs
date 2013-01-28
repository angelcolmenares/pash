using System;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	public interface IMergable
	{
		void Merge(object other);
	}
}