using System.Collections;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Provider
{
	internal class ADProviderSetPropertyParameters : ADProviderCommonParameters
	{
		private Hashtable _replacePropertyValue;

		private Hashtable _addPropertyValue;

		private Hashtable _removePropertyValue;

		[Parameter]
		[ValidateNotNull]
		public Hashtable AddPropertyValue
		{
			get
			{
				return this._addPropertyValue;
			}
			set
			{
				this._addPropertyValue = value;
			}
		}

		[Parameter]
		[ValidateNotNull]
		public Hashtable RemovePropertyValue
		{
			get
			{
				return this._removePropertyValue;
			}
			set
			{
				this._removePropertyValue = value;
			}
		}

		[Parameter]
		[ValidateNotNull]
		public Hashtable ReplacePropertyValue
		{
			get
			{
				return this._replacePropertyValue;
			}
			set
			{
				this._replacePropertyValue = value;
			}
		}

		public ADProviderSetPropertyParameters()
		{
		}
	}
}