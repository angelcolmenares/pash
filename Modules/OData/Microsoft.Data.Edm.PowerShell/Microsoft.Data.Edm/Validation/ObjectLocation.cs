using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Validation
{
	internal class ObjectLocation : EdmLocation
	{
		public object Object
		{
			get;
			private set;
		}

		internal ObjectLocation(object obj)
		{
			this.Object = obj;
		}

		public override string ToString()
		{
			return string.Concat("(", this.Object.ToString(), ")");
		}
	}
}