using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class Certificate : IExtensibleDataObject
	{
		[DataMember(Order=1, EmitDefaultValue=false)]
		public Uri CertificateUrl
		{
			get;
			set;
		}

		[DataMember(Order=4, EmitDefaultValue=false)]
		public string Data
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=2, EmitDefaultValue=false)]
		public string Thumbprint
		{
			get;
			set;
		}

		[DataMember(Order=3, EmitDefaultValue=false)]
		public string ThumbprintAlgorithm
		{
			get;
			set;
		}

		public Certificate()
		{
		}
	}
}