using System;
using Microsoft.WSMan.Enumeration;
using System.Xml.Serialization;

namespace Microsoft.WSMan.Cim
{
	[XmlRoot(ElementName = "NotificationFilter", Namespace = CimNamespaces.CimNamespace)]
	public class CimEnumerationFilter
	{
		public string Namespace
		{
			get;set;
		}

		public string ResourceUri
		{
			get;set;
		}

		public string Filter
		{
			get;set;
		}
	}
}

