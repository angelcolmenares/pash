using System;
using System.DirectoryServices;
using System.Runtime;
using System.Xml;
using System.Security.Permissions;

namespace System.DirectoryServices.Protocols
{
	public abstract class DsmlSoapConnection : DirectoryConnection
	{
		internal XmlNode soapHeaders;

		public abstract string SessionId
		{
			get;
		}

		public XmlNode SoapRequestHeader
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.soapHeaders;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.soapHeaders = value;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected DsmlSoapConnection()
		{
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public abstract void BeginSession();

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public abstract void EndSession();
	}
}