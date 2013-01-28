using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class ConfigurationWarning : IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		public string WarningCode
		{
			get;
			set;
		}

		[DataMember(Order=2)]
		public string WarningMessage
		{
			get;
			set;
		}

		public ConfigurationWarning()
		{
		}

		public override string ToString()
		{
			return string.Format("WarningCode:{0} WarningMessage:{1}", this.WarningCode, this.WarningMessage);
		}
	}
}