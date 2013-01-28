using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADSearchResponse : ADResponse
	{
		private IList<ADObject> _entries;

		public IList<ADObject> Entries
		{
			get
			{
				return this._entries;
			}
			set
			{
				this._entries = value;
			}
		}

		public ADSearchResponse(string dn, DirectoryControl[] controls, ResultCode result, string message) : base(dn, controls, result, message, null)
		{
			this._entries = new List<ADObject>();
		}

		public ADSearchResponse(string dn, DirectoryControl[] controls, ResultCode result, string message, Uri[] referral) : base(dn, controls, result, message, referral)
		{
			this._entries = new List<ADObject>();
		}
	}
}