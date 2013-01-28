using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class PersistentVMDowntimeInfo : IExtensibleDataObject
	{
		[DataMember(Order=2, EmitDefaultValue=false)]
		public DateTime? EndTime
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=1, EmitDefaultValue=false)]
		public DateTime? StartTime
		{
			get;
			set;
		}

		[DataMember(Order=3, EmitDefaultValue=false)]
		public string Status
		{
			get;
			set;
		}

		public PersistentVMDowntimeInfo()
		{
		}
	}
}