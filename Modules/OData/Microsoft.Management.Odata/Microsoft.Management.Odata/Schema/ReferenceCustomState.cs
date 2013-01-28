using System;

namespace Microsoft.Management.Odata.Schema
{
	internal class ReferenceCustomState : PropertyCustomState
	{
		public Schema.AssociationType AssociationType
		{
			get;
			set;
		}

		public bool IsSet
		{
			get;
			private set;
		}

		public ReferenceCustomState(bool isSet)
		{
			this.IsSet = isSet;
		}
	}
}