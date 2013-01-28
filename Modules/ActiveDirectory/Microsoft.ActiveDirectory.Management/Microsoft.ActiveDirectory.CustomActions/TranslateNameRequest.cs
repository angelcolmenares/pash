using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DebuggerStepThrough]
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	[MessageContract(WrapperName="TranslateNameRequest", WrapperNamespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", IsWrapped=true)]
	internal class TranslateNameRequest
	{
		[MessageHeader(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions")]
		public string Server;

		[MessageBodyMember(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", Order=0)]
		public ActiveDirectoryNameFormat FormatDesired;

		[MessageBodyMember(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", Order=1)]
		public ActiveDirectoryNameFormat FormatOffered;

		[MessageBodyMember(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", Order=2)]
		public string[] Names;

		public TranslateNameRequest()
		{
		}

		public TranslateNameRequest(string Server, ActiveDirectoryNameFormat FormatDesired, ActiveDirectoryNameFormat FormatOffered, string[] Names)
		{
			this.Server = Server;
			this.FormatDesired = FormatDesired;
			this.FormatOffered = FormatOffered;
			this.Names = Names;
		}
	}
}