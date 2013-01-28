using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class AttributeSetRequest
	{
		private bool _returnAll;

		private bool _returnDefault;

		private ICollection<string> _directoryAttributes;

		private ICollection<string> _customAttributes;

		private ICollection<string> _extendedAttributes;

		internal ICollection<string> CustomAttributes
		{
			get
			{
				return this._customAttributes;
			}
			set
			{
				this._customAttributes = value;
			}
		}

		internal ICollection<string> DirectoryAttributes
		{
			get
			{
				return this._directoryAttributes;
			}
			set
			{
				this._directoryAttributes = value;
			}
		}

		internal ICollection<string> ExtendedAttributes
		{
			get
			{
				return this._extendedAttributes;
			}
			set
			{
				this._extendedAttributes = value;
			}
		}

		internal bool ReturnAll
		{
			get
			{
				return this._returnAll;
			}
			set
			{
				this._returnAll = value;
			}
		}

		internal bool ReturnDefault
		{
			get
			{
				return this._returnDefault;
			}
			set
			{
				this._returnDefault = value;
			}
		}

		internal AttributeSetRequest()
		{
			this._directoryAttributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			this._customAttributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			this._extendedAttributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		}

		internal AttributeSetRequest(bool returnDefault)
		{
			this._directoryAttributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			this._customAttributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			this._extendedAttributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			this._returnDefault = returnDefault;
		}
	}
}