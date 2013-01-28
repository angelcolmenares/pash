using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class OSImage : IExtensibleDataObject
	{
		[DataMember(EmitDefaultValue=false, Order=0)]
		public string AffinityGroup
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=1)]
		public string Category
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=12)]
		public string Description
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=11)]
		public string Eula
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=2)]
		public string Label
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=3)]
		public string Location
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=4)]
		public int LogicalSizeInGB
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=5)]
		public Uri MediaLink
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=6)]
		public string Name
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=10)]
		public string OS
		{
			get;
			set;
		}

		ExtensionDataObject System.Runtime.Serialization.IExtensibleDataObject.ExtensionData
		{
			get;
			set;
		}

		public OSImage()
		{
		}
	}
}