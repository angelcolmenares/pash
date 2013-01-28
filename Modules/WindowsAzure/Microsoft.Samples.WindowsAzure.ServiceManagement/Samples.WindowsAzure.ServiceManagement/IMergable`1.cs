using System;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	public interface IMergable<T> : IMergable
	{
		void Merge(T other);
	}
}