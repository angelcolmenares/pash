using System.Xml;

namespace System.DirectoryServices.Protocols
{
	internal class NamespaceUtils
	{
		private static XmlNamespaceManager xmlNamespace;

		static NamespaceUtils()
		{
			NamespaceUtils.xmlNamespace = new XmlNamespaceManager(new NameTable());
			NamespaceUtils.xmlNamespace.AddNamespace("se", "http://schemas.xmlsoap.org/soap/envelope/");
			NamespaceUtils.xmlNamespace.AddNamespace("dsml", "urn:oasis:names:tc:DSML:2:0:core");
			NamespaceUtils.xmlNamespace.AddNamespace("ad", "urn:schema-microsoft-com:activedirectory:dsmlv2");
			NamespaceUtils.xmlNamespace.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
			NamespaceUtils.xmlNamespace.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
		}

		private NamespaceUtils()
		{
		}

		public static XmlNamespaceManager GetDsmlNamespaceManager()
		{
			return NamespaceUtils.xmlNamespace;
		}
	}
}