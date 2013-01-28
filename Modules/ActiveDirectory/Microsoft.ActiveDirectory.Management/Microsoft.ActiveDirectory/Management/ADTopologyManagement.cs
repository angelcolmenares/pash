using Microsoft.ActiveDirectory.CustomActions;
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADTopologyManagement : IDisposable
	{
		private const string _debugCategory = "ADTopologyManagement";

		private ADSession _adSession;

		private ADSessionHandle _sessionHandle;

		private IADTopologyManagement _topoMgmt;

		private bool _disposed;

		internal ADTopologyManagement() : this(null)
		{
		}

		internal ADTopologyManagement(ADSessionInfo sessionInfo)
		{
			this._adSession = ADSession.ConstructSession(sessionInfo);
		}

		internal void ChangeOptionalFeature(string distinguishedName, bool enable, string featureId)
		{
			this.Init();
			ChangeOptionalFeatureRequest changeOptionalFeatureRequest = new ChangeOptionalFeatureRequest();
			changeOptionalFeatureRequest.DistinguishedName = distinguishedName;
			changeOptionalFeatureRequest.Enable = enable;
			changeOptionalFeatureRequest.FeatureId = new Guid(featureId);
			this._topoMgmt.ChangeOptionalFeature(this._sessionHandle, changeOptionalFeatureRequest);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.Uninit();
			}
			this._disposed = true;
		}

		~ADTopologyManagement()
		{
			try
			{
				this.Dispose(false);
			}
			finally
			{
				//this.Finalize();
			}
		}

		internal ADObject GetDomain()
		{
			this.Init();
			GetADDomainRequest getADDomainRequest = new GetADDomainRequest();
			GetADDomainResponse aDDomain = this._topoMgmt.GetADDomain(this._sessionHandle, getADDomainRequest);
			if (aDDomain.Domain == null)
			{
				return null;
			}
			else
			{
				ActiveDirectoryDomain domain = aDDomain.Domain;
				ADObject aDObject = new ADObject();

				aDObject.Add("objectGUID", domain.ObjectGuid);
				aDObject.Add("name", domain.Name);
				aDObject.Add("distinguishedName", domain.DistinguishedName);
				aDObject.Add("objectClass", domain.ObjectClass);
				aDObject.Add("msDS-AllowedDNSSuffixes", domain.AllowedDNSSuffixes);
				aDObject.Add("objectSid", new SecurityIdentifier(domain.DomainSID, 0));
				aDObject.Add("msDS-Behavior-Version", (ADDomainMode)domain.DomainMode);
				aDObject.Add("managedBy", domain.ManagedBy);
				aDObject.Add("msDS-LogonTimeSyncInterval", domain.LastLogonReplicationInterval);
				aDObject.Add("SubordinateReferences", domain.SubordinateReferences);
				aDObject.Add("DNSRoot", domain.DNSRoot);
				aDObject.Add("LostAndFoundContainer", domain.LostAndFoundContainer);
				aDObject.Add("DeletedObjectsContainer", domain.DeletedObjectsContainer);
				aDObject.Add("QuotasContainer", domain.QuotasContainer);
				aDObject.Add("ReadOnlyReplicaDirectoryServers", domain.ReadOnlyReplicaDirectoryServer);
				aDObject.Add("ReplicaDirectoryServers", domain.ReplicaDirectoryServer);
				aDObject.Add("LinkedGroupPolicyObjects", domain.AppliedGroupPolicies);
				aDObject.Add("ChildDomains", domain.ChildDomains);
				aDObject.Add("ComputersContainer", domain.ComputersContainer);
				aDObject.Add("DomainControllersContainer", domain.DomainControllersContainer);
				aDObject.Add("ForeignSecurityPrincipalsContainer", domain.ForeignSecurityPrincipalsContainer);
				aDObject.Add("Forest", domain.Forest);
				aDObject.Add("InfrastructureMaster", domain.InfrastructureMaster);
				aDObject.Add("NetBIOSName", domain.NetBIOSName);
				aDObject.Add("PDCEmulator", domain.PDCEmulator);
				aDObject.Add("ParentDomain", domain.ParentDomain);
				aDObject.Add("RIDMaster", domain.RIDMaster);
				aDObject.Add("SystemsContainer", domain.SystemsContainer);
				aDObject.Add("UsersContainer", domain.UsersContainer);
				ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection();
				if (domain.ObjectTypes != null) {
					aDPropertyValueCollection.AddRange(domain.ObjectTypes);
				}
				aDObject.ObjectTypes = aDPropertyValueCollection;
				return aDObject;
			}
		}

		internal ADEntity[] GetDomainController(string[] dcNtdsSettingsDN)
		{
			this.Init();
			GetADDomainControllerRequest getADDomainControllerRequest = new GetADDomainControllerRequest();
			getADDomainControllerRequest.NtdsSettingsDN = dcNtdsSettingsDN;
			GetADDomainControllerResponse aDDomainController = this._topoMgmt.GetADDomainController(this._sessionHandle, getADDomainControllerRequest);
			List<ADEntity> aDEntities = new List<ADEntity>();
			if (aDDomainController.DomainControllers == null)
			{
				return new ADEntity[0];
			}
			else
			{
				ActiveDirectoryDomainController[] domainControllers = aDDomainController.DomainControllers;
				for (int i = 0; i < (int)domainControllers.Length; i++)
				{
					ActiveDirectoryDomainController activeDirectoryDomainController = domainControllers[i];
					ADEntity aDEntity = new ADEntity();
					aDEntity.Add("ComputerDN", activeDirectoryDomainController.ComputerDN);
					aDEntity.Add("Domain", activeDirectoryDomainController.Domain);
					aDEntity.Add("Forest", activeDirectoryDomainController.Forest);
					aDEntity.Add("Enabled", activeDirectoryDomainController.Enabled);
					aDEntity.Add("IsGlobalCatalog", activeDirectoryDomainController.IsGlobalCatalog);
					aDEntity.Add("IsReadOnly", activeDirectoryDomainController.IsReadOnly);
					aDEntity.Add("OSHotFix", activeDirectoryDomainController.OSHotFix);
					aDEntity.Add("OSName", activeDirectoryDomainController.OSName);
					aDEntity.Add("OSServicepack", activeDirectoryDomainController.OSServicepack);
					aDEntity.Add("OSVersion", activeDirectoryDomainController.OSVersion);
					aDEntity.Add("DefaultPartition", activeDirectoryDomainController.DefaultPartition);
					aDEntity.Add("HostName", activeDirectoryDomainController.HostName);
					aDEntity.Add("InvocationId", activeDirectoryDomainController.InvocationId);
					aDEntity.Add("LdapPort", activeDirectoryDomainController.LdapPort);
					aDEntity.Add("NTDSSettingsObjectDN", activeDirectoryDomainController.NTDSSettingsObjectDN);
					aDEntity.Add("Name", activeDirectoryDomainController.Name);
					aDEntity.Add("OperationMasterRole", activeDirectoryDomainController.OperationMasterRole);
					aDEntity.Add("Partitions", activeDirectoryDomainController.Partitions);
					aDEntity.Add("ServerObjectDN", activeDirectoryDomainController.ServerObjectDN);
					aDEntity.Add("ServerObjectGuid", activeDirectoryDomainController.ServerObjectGuid);
					aDEntity.Add("Site", activeDirectoryDomainController.Site);
					aDEntity.Add("SslPort", activeDirectoryDomainController.SslPort);
					aDEntities.Add(aDEntity);
				}
				return aDEntities.ToArray();
			}
		}

		internal ADEntity GetForest()
		{
			this.Init();
			GetADForestRequest getADForestRequest = new GetADForestRequest();
			GetADForestResponse aDForest = this._topoMgmt.GetADForest(this._sessionHandle, getADForestRequest);
			if (aDForest.Forest == null)
			{
				return null;
			}
			else
			{
				ActiveDirectoryForest forest = aDForest.Forest;
				ADEntity aDEntity = new ADEntity();
				aDEntity.Add("msDS-SPNSuffixes", forest.SPNSuffixes);
				aDEntity.Add("uPNSuffixes", forest.UPNSuffixes);
				aDEntity.Add("msDS-Behavior-Version", (ADForestMode)forest.ForestMode);
				aDEntity.Add("name", forest.Name);
				aDEntity.Add("RootDomain", forest.RootDomain);
				aDEntity.Add("ApplicationPartitions", forest.ApplicationPartitions);
				aDEntity.Add("CrossForestReferences", forest.CrossForestReferences);
				aDEntity.Add("Domains", forest.Domains);
				aDEntity.Add("GlobalCatalogs", forest.GlobalCatalogs);
				aDEntity.Add("Sites", forest.Sites);
				aDEntity.Add("DomainNamingMaster", forest.DomainNamingMaster);
				aDEntity.Add("SchemaMaster", forest.SchemaMaster);
				return aDEntity;
			}
		}

		private void Init()
		{
			if (!this._disposed)
			{
				if (this._topoMgmt == null)
				{
					this._sessionHandle = this._adSession.GetSessionHandle();
					this._topoMgmt = this._adSession.GetTopologyManagementInterface();
				}
				return;
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		internal void MoveOperationMasterRole(ADOperationMasterRole operationMasterRole, bool seize, out bool wasSeized)
		{
			this.Init();
			MoveADOperationMasterRoleRequest moveADOperationMasterRoleRequest = new MoveADOperationMasterRoleRequest();
			moveADOperationMasterRoleRequest.Seize = seize;
			ADOperationMasterRole aDOperationMasterRole = operationMasterRole;
			switch (aDOperationMasterRole)
			{
				case ADOperationMasterRole.PDCEmulator:
				{
					moveADOperationMasterRoleRequest.OperationMasterRole = ActiveDirectoryOperationMasterRole.PDCEmulator;
					break;
				}
				case ADOperationMasterRole.RIDMaster:
				{
					moveADOperationMasterRoleRequest.OperationMasterRole = ActiveDirectoryOperationMasterRole.RIDMaster;
					break;
				}
				case ADOperationMasterRole.InfrastructureMaster:
				{
					moveADOperationMasterRoleRequest.OperationMasterRole = ActiveDirectoryOperationMasterRole.InfrastructureMaster;
					break;
				}
				case ADOperationMasterRole.SchemaMaster:
				{
					moveADOperationMasterRoleRequest.OperationMasterRole = ActiveDirectoryOperationMasterRole.SchemaMaster;
					break;
				}
				case ADOperationMasterRole.DomainNamingMaster:
				{
					moveADOperationMasterRoleRequest.OperationMasterRole = ActiveDirectoryOperationMasterRole.DomainNamingMaster;
					break;
				}
			}
			MoveADOperationMasterRoleResponse moveADOperationMasterRoleResponse = this._topoMgmt.MoveADOperationMasterRole(this._sessionHandle, moveADOperationMasterRoleRequest);
			wasSeized = moveADOperationMasterRoleResponse.WasSeized;
		}

		private void Uninit()
		{
			if (this._adSession != null)
			{
				this._adSession.Delete();
				this._adSession = null;
			}
		}
	}
}