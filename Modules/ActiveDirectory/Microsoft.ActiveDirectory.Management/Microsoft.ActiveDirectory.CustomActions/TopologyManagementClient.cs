using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DebuggerStepThrough]
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	internal class TopologyManagementClient : ClientBase<TopologyManagement>, TopologyManagement
	{
		public TopologyManagementClient()
		{
		}

		public TopologyManagementClient(string endpointConfigurationName) : base(endpointConfigurationName)
		{
		}

		public TopologyManagementClient(string endpointConfigurationName, string remoteAddress) : base(endpointConfigurationName, remoteAddress)
		{
		}

		public TopologyManagementClient(string endpointConfigurationName, EndpointAddress remoteAddress) : base(endpointConfigurationName, remoteAddress)
		{
		}

		public TopologyManagementClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
		{
		}

		public void ChangeOptionalFeature(string Server, string DistinguishedName, bool Enable, Guid FeatureId)
		{
			ChangeOptionalFeatureRequest changeOptionalFeatureRequest = new ChangeOptionalFeatureRequest();
			changeOptionalFeatureRequest.Server = Server;
			changeOptionalFeatureRequest.DistinguishedName = DistinguishedName;
			changeOptionalFeatureRequest.Enable = Enable;
			changeOptionalFeatureRequest.FeatureId = FeatureId;
			this.Channel.ChangeOptionalFeature(changeOptionalFeatureRequest);
		}

		public ActiveDirectoryDomain GetADDomain(string Server)
		{
			GetADDomainRequest getADDomainRequest = new GetADDomainRequest();
			getADDomainRequest.Server = Server;
			GetADDomainResponse aDDomain = this.Channel.GetADDomain(getADDomainRequest);
			return aDDomain.Domain;
		}

		public ActiveDirectoryDomainController[] GetADDomainController(string Server, string[] NtdsSettingsDN)
		{
			GetADDomainControllerRequest getADDomainControllerRequest = new GetADDomainControllerRequest();
			getADDomainControllerRequest.Server = Server;
			getADDomainControllerRequest.NtdsSettingsDN = NtdsSettingsDN;
			GetADDomainControllerResponse aDDomainController = this.Channel.GetADDomainController(getADDomainControllerRequest);
			return aDDomainController.DomainControllers;
		}

		public ActiveDirectoryForest GetADForest(string Server)
		{
			GetADForestRequest getADForestRequest = new GetADForestRequest();
			getADForestRequest.Server = Server;
			GetADForestResponse aDForest = this.Channel.GetADForest(getADForestRequest);
			return aDForest.Forest;
		}

		public int GetVersion(out int VersionMinor, out string VersionString)
		{
			GetVersionRequest getVersionRequest = new GetVersionRequest();
			GetVersionResponse version = this.Channel.GetVersion(getVersionRequest);
			VersionMinor = version.VersionMinor;
			VersionString = version.VersionString;
			return version.VersionMajor;
		}

		public bool MoveADOperationMasterRole(string Server, ActiveDirectoryOperationMasterRole OperationMasterRole, bool Seize)
		{
			MoveADOperationMasterRoleRequest moveADOperationMasterRoleRequest = new MoveADOperationMasterRoleRequest();
			moveADOperationMasterRoleRequest.Server = Server;
			moveADOperationMasterRoleRequest.OperationMasterRole = OperationMasterRole;
			moveADOperationMasterRoleRequest.Seize = Seize;
			MoveADOperationMasterRoleResponse moveADOperationMasterRoleResponse = this.Channel.MoveADOperationMasterRole(moveADOperationMasterRoleRequest);
			return moveADOperationMasterRoleResponse.WasSeized;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		ChangeOptionalFeatureResponse Microsoft.ActiveDirectory.CustomActions.TopologyManagement.ChangeOptionalFeature(ChangeOptionalFeatureRequest request)
		{
			return base.Channel.ChangeOptionalFeature(request);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		GetADDomainResponse Microsoft.ActiveDirectory.CustomActions.TopologyManagement.GetADDomain(GetADDomainRequest request)
		{
			return base.Channel.GetADDomain(request);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		GetADDomainControllerResponse Microsoft.ActiveDirectory.CustomActions.TopologyManagement.GetADDomainController(GetADDomainControllerRequest request)
		{
			return base.Channel.GetADDomainController(request);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		GetADForestResponse Microsoft.ActiveDirectory.CustomActions.TopologyManagement.GetADForest(GetADForestRequest request)
		{
			return base.Channel.GetADForest(request);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		GetVersionResponse Microsoft.ActiveDirectory.CustomActions.TopologyManagement.GetVersion(GetVersionRequest request)
		{
			return base.Channel.GetVersion(request);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		MoveADOperationMasterRoleResponse Microsoft.ActiveDirectory.CustomActions.TopologyManagement.MoveADOperationMasterRole(MoveADOperationMasterRoleRequest request)
		{
			return base.Channel.MoveADOperationMasterRole(request);
		}
	}
}