using Microsoft.ActiveDirectory.CustomActions;

namespace Microsoft.ActiveDirectory.Management
{
	internal interface IADTopologyManagement
	{
		ChangeOptionalFeatureResponse ChangeOptionalFeature(ADSessionHandle handle, ChangeOptionalFeatureRequest request);

		GetADDomainResponse GetADDomain(ADSessionHandle handle, GetADDomainRequest request);

		GetADDomainControllerResponse GetADDomainController(ADSessionHandle handle, GetADDomainControllerRequest request);

		GetADForestResponse GetADForest(ADSessionHandle handle, GetADForestRequest request);

		MoveADOperationMasterRoleResponse MoveADOperationMasterRole(ADSessionHandle handle, MoveADOperationMasterRoleRequest request);
	}
}