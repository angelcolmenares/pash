using System;

namespace Microsoft.ActiveDirectory.Management
{
	internal interface IADSession
	{
		ADSessionHandle Create(ADSessionInfo info);

		bool Delete(ADSessionHandle handle);

		object GetOption(ADSessionHandle handle, int option);

		bool SetOption(ADSessionHandle handle, int option, object value);
	}
}