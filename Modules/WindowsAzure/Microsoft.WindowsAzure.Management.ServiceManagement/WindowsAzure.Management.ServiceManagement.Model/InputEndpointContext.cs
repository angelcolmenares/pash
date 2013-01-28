using System;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class InputEndpointContext
	{
		public string LBSetName
		{
			get;
			set;
		}

		public int LocalPort
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public int? Port
		{
			get;
			set;
		}

		public string ProbePath
		{
			get;
			set;
		}

		public int ProbePort
		{
			get;
			set;
		}

		public string ProbeProtocol
		{
			get;
			set;
		}

		public string Protocol
		{
			get;
			set;
		}

		public string Vip
		{
			get;
			set;
		}

		public InputEndpointContext()
		{
		}
	}
}