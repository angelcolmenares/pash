using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADObjectIdentifier
	{
		private string _value;

		private string _displayName;

		public string DisplayName
		{
			get
			{
				return this._displayName;
			}
		}

		public string Value
		{
			get
			{
				return this._value;
			}
		}

		public ADObjectIdentifier(string value)
		{
			this._value = value;
		}

		public ADObjectIdentifier(string value, string displayName)
		{
			this._value = value;
			this._displayName = displayName;
		}

		public override string ToString()
		{
			if (string.IsNullOrEmpty(this._displayName))
			{
				return this._value;
			}
			else
			{
				return string.Concat(this._value, " (", this._displayName, ")");
			}
		}
	}
}