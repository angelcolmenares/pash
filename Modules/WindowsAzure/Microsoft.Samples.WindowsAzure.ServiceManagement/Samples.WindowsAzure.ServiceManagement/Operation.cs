using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class Operation : IExtensibleDataObject
	{
		[DataMember(Order=4, EmitDefaultValue=false)]
		public ServiceManagementError Error
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=3, EmitDefaultValue=false)]
		public int HttpStatusCode
		{
			get;
			set;
		}

		[DataMember(Name="ID", Order=1)]
		public string OperationTrackingId
		{
			get;
			set;
		}

		[DataMember(Order=2)]
		public string Status
		{
			get;
			set;
		}

		public Operation()
		{
		}
	}
}