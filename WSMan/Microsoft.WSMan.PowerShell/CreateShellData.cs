using System;
using System.Xml.Serialization;

namespace Microsoft.WSMan.PowerShell
{
	//[XmlType("CreateShell", Namespace = PowerShellNamespaces.Namespace)]
	public class CreateShellData
	{
		public CreateShellData ()
		{

		}

		public TimeSpan IdleTimeOut
		{
			get;set;
		}

		public TimeSpan MaxIdleTimeOut
		{
			get;set;
		}

		public string BufferMode
		{
			get;set;
		}
	}
}

