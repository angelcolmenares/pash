using System.CodeDom.Compiler;
using System.ServiceModel;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	[ServiceContract(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", ConfigurationName="TopologyManagement", SessionMode=SessionMode.Required)]
	internal interface TopologyManagement
	{
		[FaultContract(typeof(ChangeOptionalFeatureFault), Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name="ChangeOptionalFeatureFault")]
		[OperationContract(Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/TopologyManagement/ChangeOptionalFeature", ReplyAction="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/TopologyManagement/ChangeOptionalFeatureResponse")]
		ChangeOptionalFeatureResponse ChangeOptionalFeature(ChangeOptionalFeatureRequest request);

		[FaultContract(typeof(GetADDomainFault), Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name="GetADDomainFault")]
		[OperationContract(Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/TopologyManagement/GetADDomain", ReplyAction="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/TopologyManagement/GetADDomainResponse")]
		GetADDomainResponse GetADDomain(GetADDomainRequest request);

		[FaultContract(typeof(GetADDomainControllerFault), Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name="GetADDomainControllerFault")]
		[OperationContract(Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/TopologyManagement/GetADDomainController", ReplyAction="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/TopologyManagement/GetADDomainControllerResponse")]
		GetADDomainControllerResponse GetADDomainController(GetADDomainControllerRequest request);

		[FaultContract(typeof(GetADForestFault), Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name="GetADForestFault")]
		[OperationContract(Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/TopologyManagement/GetADForest", ReplyAction="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/TopologyManagement/GetADForestResponse")]
		GetADForestResponse GetADForest(GetADForestRequest request);

		[FaultContract(typeof(GetVersionFault), Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name="GetVersionFault")]
		[OperationContract(Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/TopologyManagement/GetVersion", ReplyAction="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/TopologyManagement/GetVersionResponse")]
		GetVersionResponse GetVersion(GetVersionRequest request);

		[FaultContract(typeof(MoveADOperationMasterRoleFault), Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name="MoveADOperationMasterRoleFault")]
		[OperationContract(Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/TopologyManagement/MoveADOperationMasterRole", ReplyAction="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/TopologyManagement/MoveADOperationMasterRoleResponse")]
		MoveADOperationMasterRoleResponse MoveADOperationMasterRole(MoveADOperationMasterRoleRequest request);
	}
}