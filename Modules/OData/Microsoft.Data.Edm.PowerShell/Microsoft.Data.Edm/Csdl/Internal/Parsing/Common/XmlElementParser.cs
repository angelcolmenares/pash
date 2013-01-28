using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Common
{
	internal abstract class XmlElementParser
	{
		private readonly Dictionary<string, XmlElementParser> childParsers;

		internal string ElementName
		{
			get;
			private set;
		}

		protected XmlElementParser(string elementName, Dictionary<string, XmlElementParser> children)
		{
			this.ElementName = elementName;
			this.childParsers = children;
		}

		public void AddChildParser(XmlElementParser child)
		{
			this.childParsers[child.ElementName] = child;
		}

		internal static XmlElementParser<TResult> Create<TResult>(string elementName, Func<XmlElementInfo, XmlElementValueCollection, TResult> parserFunc, IEnumerable<XmlElementParser> childParsers, IEnumerable<XmlElementParser> descendantParsers)
		{
			Dictionary<string, XmlElementParser> children = null;
			Func<XmlElementParser, string> keySelector = null;
			if (childParsers != null)
			{
				if (keySelector == null)
				{
					keySelector = p => p.ElementName;
				}
				children = childParsers.ToDictionary<XmlElementParser, string>(keySelector);
			}
			return new XmlElementParser<TResult>(elementName, children, parserFunc);
		}

		internal abstract XmlElementValue Parse(XmlElementInfo element, IList<XmlElementValue> children);

		internal bool TryGetChildElementParser(string elementName, out XmlElementParser elementParser)
		{
			elementParser = null;
			if (this.childParsers == null)
			{
				return false;
			}
			else
			{
				return this.childParsers.TryGetValue(elementName, out elementParser);
			}
		}
	}
}