using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DebuggerStepThrough]
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	[MessageContract(WrapperName="ChangeOptionalFeatureRequest", WrapperNamespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", IsWrapped=true)]
	internal class ChangeOptionalFeatureRequest
	{
		[MessageHeader(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions")]
		public string Server;

		[MessageBodyMember(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", Order=0)]
		public string DistinguishedName;

		[MessageBodyMember(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", Order=1)]
		public bool Enable;

		[MessageBodyMember(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", Order=2)]
		public Guid FeatureId;

		public ChangeOptionalFeatureRequest()
		{
		}

		public ChangeOptionalFeatureRequest(string Server, string DistinguishedName, bool Enable, Guid FeatureId)
		{
			this.Server = Server;
			this.DistinguishedName = DistinguishedName;
			this.Enable = Enable;
			this.FeatureId = FeatureId;
		}
	}
}