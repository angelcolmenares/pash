using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Name="RollbackUpdateOrUpgrade", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class RollbackUpdateOrUpgradeInput : IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=2, EmitDefaultValue=false)]
		public bool? Force
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		public string Mode
		{
			get;
			set;
		}

		public RollbackUpdateOrUpgradeInput()
		{
		}
	}
}