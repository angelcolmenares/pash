using System;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADSessionHandle
	{
		private object _handle;

		public object Handle
		{
			get
			{
				return this._handle;
			}
		}

		public ADSessionHandle(object handle)
		{
			this._handle = handle;
		}
	}
}