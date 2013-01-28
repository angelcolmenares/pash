using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class SubscriptionOperation : IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=4)]
		public SubscriptionOperationCaller OperationCaller
		{
			get;
			set;
		}

		[DataMember(Order=8, EmitDefaultValue=false)]
		public string OperationCompletedTime
		{
			get;
			set;
		}

		[DataMember(Order=0)]
		public string OperationId
		{
			get;
			set;
		}

		[DataMember(Order=2)]
		public string OperationName
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		public string OperationObjectId
		{
			get;
			set;
		}

		[DataMember(Order=3)]
		public OperationParameterList OperationParameters
		{
			get;
			set;
		}

		[DataMember(Order=7, EmitDefaultValue=false)]
		public string OperationStartedTime
		{
			get;
			set;
		}

		[DataMember(Order=5)]
		public Operation OperationStatus
		{
			get;
			set;
		}

		public SubscriptionOperation()
		{
		}
	}
}