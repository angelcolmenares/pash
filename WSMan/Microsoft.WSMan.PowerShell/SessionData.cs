using System;
using System.Xml.Serialization;

namespace Microsoft.WSMan.PowerShell
{
	//[XmlType("Session", Namespace = PowerShellNamespaces.Namespace)]
	public class SessionData
	{
		//[XmlAttribute("Id", Namespace = PowerShellNamespaces.Namespace)]
		public Guid Id
		{
			get;set;
		}
	}
}

