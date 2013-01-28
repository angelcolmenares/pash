using System;

namespace Microsoft.Management.Odata.Schema
{
	internal class PropertyCustomState
	{
		public object DefaultValue
		{
			get;
			set;
		}

		public bool IsUpdatable
		{
			get;
			set;
		}

		public string PsProperty
		{
			get;
			set;
		}

		public PropertyCustomState()
		{
			this.IsUpdatable = false;
			this.DefaultValue = null;
			this.PsProperty = null;
		}
	}
}