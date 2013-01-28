using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Common
{
	internal abstract class XmlElementValue
	{
		internal virtual bool IsText
		{
			get
			{
				return false;
			}
		}

		internal abstract bool IsUsed
		{
			get;
		}

		internal CsdlLocation Location
		{
			get;
			private set;
		}

		internal string Name
		{
			get;
			private set;
		}

		internal virtual string TextValue
		{
			get
			{
				return this.ValueAs<string>();
			}
		}

		internal abstract object UntypedValue
		{
			get;
		}

		internal XmlElementValue(string elementName, CsdlLocation elementLocation)
		{
			this.Name = elementName;
			this.Location = elementLocation;
		}

		internal virtual TValue ValueAs<TValue>()
		where TValue : class
		{
			return (TValue)(this.UntypedValue as TValue);
		}
	}
}