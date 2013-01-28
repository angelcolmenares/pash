using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class CaptureRoleOperation : RoleOperation
	{
		public override string OperationType
		{
			get
			{
				return "CaptureRoleOperation";
			}
			set
			{
			}
		}

		[DataMember(EmitDefaultValue=false, Order=0)]
		public string PostCaptureAction
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=1)]
		public ProvisioningConfigurationSet ProvisioningConfiguration
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=2)]
		public string TargetImageLabel
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=3)]
		public string TargetImageName
		{
			get;
			set;
		}

		public CaptureRoleOperation()
		{
		}
	}
}