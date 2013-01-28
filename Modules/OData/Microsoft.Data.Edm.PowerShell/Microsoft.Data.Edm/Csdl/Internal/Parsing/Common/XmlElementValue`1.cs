using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Common
{
	internal class XmlElementValue<TValue> : XmlElementValue
	{
		private readonly TValue @value;

		private bool isUsed;

		internal override bool IsText
		{
			get
			{
				return false;
			}
		}

		internal override bool IsUsed
		{
			get
			{
				return this.isUsed;
			}
		}

		internal override object UntypedValue
		{
			get
			{
				return this.@value;
			}
		}

		internal TValue Value
		{
			get
			{
				this.isUsed = true;
				return this.@value;
			}
		}

		internal XmlElementValue(string name, CsdlLocation location, TValue newValue) : base(name, location)
		{
			this.@value = newValue;
		}

		internal override T ValueAs<T>()
		{
			return (T)((object)this.Value as T);
		}
	}
}