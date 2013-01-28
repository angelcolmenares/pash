using System;
using System.Xml.Serialization;

namespace Microsoft.WSMan.PowerShell
{
	[XmlType("Stream", Namespace = PowerShellNamespaces.Namespace)]
	public class StreamData
	{
		[XmlAttribute("Name", Namespace = PowerShellNamespaces.Namespace)]
		public string Name
		{
			get;set;
		}

		[XmlText]
		public string Value
		{
			get;set;
		}
	}
}

