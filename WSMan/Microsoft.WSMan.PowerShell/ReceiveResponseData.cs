using System;
using System.Xml.Serialization;

namespace Microsoft.WSMan.PowerShell
{
	/// <summary>
	/// 
	/// </summary>
	//[XmlType("ReceiveResponse", Namespace = PowerShellNamespaces.Namespace)]
	public class ReceiveResponseData
	{
		//[XmlElement("Stream")]
		public StreamData Stream
		{
			get;set;
		}
	}
}

