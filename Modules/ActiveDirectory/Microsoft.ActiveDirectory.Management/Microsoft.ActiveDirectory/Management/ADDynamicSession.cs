using System;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADDynamicSession : ADSession
	{
		public ADDynamicSession(ADSessionInfo info) : base(info)
		{
		}

		public object GetOption(int option)
		{
			return null;
		}

		public void SetOption(int option, object value)
		{
		}
	}
}