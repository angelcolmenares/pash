using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Name="Error", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class ServiceManagementError : IExtensibleDataObject
	{
		[DataMember(Order=1)]
		public string Code
		{
			get;
			set;
		}

		[DataMember(Order=3, EmitDefaultValue=false)]
		public ConfigurationWarningsList ConfigurationWarnings
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=2)]
		public string Message
		{
			get;
			set;
		}

		public ServiceManagementError()
		{
		}
	}
}