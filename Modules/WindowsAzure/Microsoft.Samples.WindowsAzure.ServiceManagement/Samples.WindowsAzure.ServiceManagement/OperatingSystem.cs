using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class OperatingSystem : IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=5, EmitDefaultValue=false)]
		public string Family
		{
			get;
			set;
		}

		[DataMember(Order=6, EmitDefaultValue=false)]
		public string FamilyLabel
		{
			get;
			set;
		}

		[DataMember(Order=4)]
		public bool IsActive
		{
			get;
			set;
		}

		[DataMember(Order=3)]
		public bool IsDefault
		{
			get;
			set;
		}

		[DataMember(Order=2, EmitDefaultValue=false)]
		public string Label
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		public string Version
		{
			get;
			set;
		}

		public OperatingSystem()
		{
		}
	}
}