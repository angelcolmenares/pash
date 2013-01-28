using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Common
{
	internal class XmlElementParser<TResult> : XmlElementParser
	{
		private readonly Func<XmlElementInfo, XmlElementValueCollection, TResult> parserFunc;

		internal XmlElementParser(string elementName, Dictionary<string, XmlElementParser> children, Func<XmlElementInfo, XmlElementValueCollection, TResult> parser) : base(elementName, children)
		{
			this.parserFunc = parser;
		}

		internal override XmlElementValue Parse(XmlElementInfo element, IList<XmlElementValue> children)
		{
			TResult tResult = this.parserFunc(element, XmlElementValueCollection.FromList(children));
			return new XmlElementValue<TResult>(element.Name, element.Location, tResult);
		}
	}
}