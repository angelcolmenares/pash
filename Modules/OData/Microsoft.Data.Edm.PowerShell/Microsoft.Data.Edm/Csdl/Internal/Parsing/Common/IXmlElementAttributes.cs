using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Common
{
	internal interface IXmlElementAttributes
	{
		XmlAttributeInfo this[string attributeName]
		{
			get;
		}

		IEnumerable<XmlAttributeInfo> Unused
		{
			get;
		}

	}
}