using System;
using System.DirectoryServices.Protocols;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADPageResultRequestControl : PageResultRequestControl
	{
		private object _pageCookie;

		public object Cookie
		{
			get
			{
				if (this._pageCookie == null)
				{
					return base.Cookie;
				}
				else
				{
					return this._pageCookie;
				}
			}
			set
			{
				if (value as byte[] == null)
				{
					this._pageCookie = value;
					return;
				}
				else
				{
					base.Cookie = (byte[])value;
					return;
				}
			}
		}

		public ADPageResultRequestControl()
		{
		}

		public ADPageResultRequestControl(int pageSize) : base(pageSize)
		{
		}

		public ADPageResultRequestControl(object cookie)
		{
			if (cookie as byte[] == null)
			{
				this._pageCookie = cookie;
				return;
			}
			else
			{
				base.Cookie = (byte[])cookie;
				return;
			}
		}
	}
}