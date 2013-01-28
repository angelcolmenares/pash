using System;
using System.DirectoryServices.Protocols;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADPageResultResponseControl : DirectoryControl
	{
		private object _pageCookie;

		private int _count;

		public object Cookie
		{
			get
			{
				return this._pageCookie;
			}
			set
			{
				this._pageCookie = value;
			}
		}

		public int TotalCount
		{
			get
			{
				return this._count;
			}
		}

		public ADPageResultResponseControl(int count, object cookie, bool criticality, byte[] controlValue) : base("1.2.840.113556.1.4.319", controlValue, criticality, true)
		{
			this._pageCookie = cookie;
			this._count = count;
		}
	}
}