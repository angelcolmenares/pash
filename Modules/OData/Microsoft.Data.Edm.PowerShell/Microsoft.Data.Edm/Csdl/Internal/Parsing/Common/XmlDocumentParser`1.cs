using System;
using System.Xml;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Common
{
	internal abstract class XmlDocumentParser<TResult> : XmlDocumentParser
	{
		internal XmlElementValue<TResult> Result
		{
			get
			{
				if (base.Result == null)
				{
					return null;
				}
				else
				{
					return (XmlElementValue<TResult>)base.Result;
				}
			}
		}

		internal XmlDocumentParser(XmlReader underlyingReader, string documentPath) : base(underlyingReader, documentPath)
		{
		}

		protected abstract bool TryGetDocumentElementParser(Version artifactVersion, XmlElementInfo rootElement, out XmlElementParser<TResult> parser);

		protected sealed override bool TryGetRootElementParser(Version artifactVersion, XmlElementInfo rootElement, out XmlElementParser parser)
		{
			XmlElementParser<TResult> xmlElementParser = null;
			if (!this.TryGetDocumentElementParser(artifactVersion, rootElement, out xmlElementParser))
			{
				parser = null;
				return false;
			}
			else
			{
				parser = xmlElementParser;
				return true;
			}
		}
	}
}