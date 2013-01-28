using System;
using System.DirectoryServices.Protocols;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADInputDNControl : DirectoryControl
	{
		private string _inputDN;

		public string InputDN
		{
			get
			{
				return this._inputDN;
			}
			set
			{
				this._inputDN = value;
			}
		}

		public ADInputDNControl() : base("1.2.840.113556.1.4.2026", null, true, true)
		{
		}

		public ADInputDNControl(string inputDN) : this()
		{
			this._inputDN = inputDN;
		}

		public override byte[] GetValue()
		{
			object[] objArray = new object[1];
			objArray[0] = this._inputDN;
			return BerConverter.Encode("{s}", objArray);
		}
	}
}