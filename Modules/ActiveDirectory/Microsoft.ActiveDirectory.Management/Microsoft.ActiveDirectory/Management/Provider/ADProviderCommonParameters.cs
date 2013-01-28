using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Provider
{
	internal class ADProviderCommonParameters
	{
		private ADPathFormat _formatType;

		private bool _formatTypeSet;

		private string _server;

		private bool _isGC;

		private bool _isGCSet;

		private ADAuthType _authType;

		private bool _authTypeSet;

		[Parameter]
		public ADAuthType AuthType
		{
			get
			{
				return this._authType;
			}
			set
			{
				this._authType = value;
				this._authTypeSet = true;
			}
		}

		[Parameter]
		public ADPathFormat FormatType
		{
			get
			{
				return this._formatType;
			}
			set
			{
				this._formatType = value;
				this._formatTypeSet = true;
			}
		}

		public virtual SwitchParameter GlobalCatalog
		{
			get
			{
				return this._isGC;
			}
			set
			{
				this._isGC = value;
				this._isGCSet = true;
			}
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		public string Server
		{
			get
			{
				return this._server;
			}
			set
			{
				this._server = value;
			}
		}

		public ADProviderCommonParameters()
		{
			this._formatType = ADProviderDefaults.PathFormat;
			this._server = ADProviderDefaults.Server;
			this._isGC = ADProviderDefaults.IsGC;
			this._authType = ADProviderDefaults.AuthType;
		}

		public bool IsPropertySet(string propertyName)
		{
			bool flag;
			string str = propertyName;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "FormatType")
				{
					flag = this._formatTypeSet;
				}
				else
				{
					if (str1 == "GlobalCatalog")
					{
						flag = this._isGCSet;
					}
					else
					{
						if (str1 != "AuthType")
						{
							throw new ArgumentException(StringResources.ADProviderInvalidPropertyName, "propertyName");
						}
						flag = this._authTypeSet;
					}
				}
				return flag;
			}
			throw new ArgumentException(StringResources.ADProviderInvalidPropertyName, "propertyName");
		}

		public virtual void ValidateParameters()
		{
		}
	}
}