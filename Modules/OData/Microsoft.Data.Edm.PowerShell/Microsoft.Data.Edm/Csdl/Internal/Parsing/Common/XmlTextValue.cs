using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Common
{
	internal class XmlTextValue : XmlElementValue<string>
	{
		internal const string ElementName = "<\"Text\">";

		internal readonly static XmlTextValue Missing;

		internal override bool IsText
		{
			get
			{
				return true;
			}
		}

		internal override string TextValue
		{
			get
			{
				return base.Value;
			}
		}

		static XmlTextValue()
		{
			XmlTextValue.Missing = new XmlTextValue(null, null);
		}

		internal XmlTextValue(CsdlLocation textLocation, string textValue) : base("<\"Text\">", textLocation, textValue)
		{
		}
	}
}