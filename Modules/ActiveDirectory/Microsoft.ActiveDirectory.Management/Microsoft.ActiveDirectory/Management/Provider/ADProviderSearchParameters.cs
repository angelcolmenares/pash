using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Provider
{
	internal class ADProviderSearchParameters : ADProviderCommonParameters
	{
		private string[] _properties;

		private int _sizeLimit;

		private int _pageSize;

		[Parameter]
		public override SwitchParameter GlobalCatalog
		{
			get
			{
				return base.GlobalCatalog;
			}
			set
			{
				base.GlobalCatalog = value;
			}
		}

		[Parameter]
		public int PageSize
		{
			get
			{
				return this._pageSize;
			}
			set
			{
				this._pageSize = value;
			}
		}

		[Alias(new string[] { "Property" })]
		[Parameter]
		[ValidateNotNull]
		public string[] Properties
		{
			get
			{
				return this._properties;
			}
			set
			{
				this._properties = value;
			}
		}

		[Parameter]
		public int SizeLimit
		{
			get
			{
				return this._sizeLimit;
			}
			set
			{
				this._sizeLimit = value;
			}
		}

		public ADProviderSearchParameters()
		{
			this._sizeLimit = ADProviderDefaults.ServerSearchSizeLimit;
			this._pageSize = ADProviderDefaults.ServerSearchPageSize;
		}
	}
}