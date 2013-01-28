using System;
using System.IO;
using System.ServiceModel.Web;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	public static class ServiceManagementExtensionMethods
	{
		public static void AddCertificates(this IServiceManagement proxy, string subscriptionId, string serviceName, CertificateFile input)
		{
			proxy.EndAddCertificates(proxy.BeginAddCertificates(subscriptionId, serviceName, input, null, null));
		}

		public static void AddDataDisk(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleName, DataVirtualHardDisk dataDisk)
		{
			proxy.EndAddDataDisk(proxy.BeginAddDataDisk(subscriptionId, serviceName, deploymentName, roleName, dataDisk, null, null));
		}

		public static void AddRole(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, PersistentVMRole role)
		{
			proxy.EndAddRole(proxy.BeginAddRole(subscriptionId, serviceName, deploymentName, role, null, null));
		}

		public static void CaptureRole(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleInstanceName, string targetImageName, string targetImageLabel, PostCaptureAction postCaptureAction, ProvisioningConfigurationSet provisioningConfiguration)
		{
			CaptureRoleOperation captureRoleOperation = new CaptureRoleOperation();
			captureRoleOperation.PostCaptureAction = postCaptureAction.ToString();
			captureRoleOperation.ProvisioningConfiguration = provisioningConfiguration;
			captureRoleOperation.TargetImageName = targetImageName;
			captureRoleOperation.TargetImageLabel = targetImageLabel;
			proxy.EndExecuteRoleOperation(proxy.BeginExecuteRoleOperation(subscriptionId, serviceName, deploymentName, roleInstanceName, captureRoleOperation, null, null));
		}

		public static void ChangeConfiguration(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, ChangeConfigurationInput input)
		{
			proxy.EndChangeConfiguration(proxy.BeginChangeConfiguration(subscriptionId, serviceName, deploymentName, input, null, null));
		}

		public static void ChangeConfigurationBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, ChangeConfigurationInput input)
		{
			proxy.EndChangeConfigurationBySlot(proxy.BeginChangeConfigurationBySlot(subscriptionId, serviceName, deploymentSlot, input, null, null));
		}

		public static void CommitImageUpload(this IServiceManagement proxy, string subscriptionId, string imageName)
		{
			proxy.EndCommitImageUpload(proxy.BeginCommitImageUpload(subscriptionId, imageName, null, null));
		}

		public static void CreateAffinityGroup(this IServiceManagement proxy, string subscriptionId, CreateAffinityGroupInput input)
		{
			proxy.EndCreateAffinityGroup(proxy.BeginCreateAffinityGroup(subscriptionId, input, null, null));
		}

		public static void CreateDeployment(this IServiceManagement proxy, string subscriptionId, string serviceName, Deployment deployment)
		{
			proxy.EndCreateDeployment(proxy.BeginCreateDeployment(subscriptionId, serviceName, deployment, null, null));
		}

		public static Disk CreateDisk(this IServiceManagement proxy, string subscriptionID, Disk disk)
		{
			return proxy.EndCreateDisk(proxy.BeginCreateDisk(subscriptionID, disk, null, null));
		}

		public static void CreateHostedService(this IServiceManagement proxy, string subscriptionId, CreateHostedServiceInput input)
		{
			proxy.EndCreateHostedService(proxy.BeginCreateHostedService(subscriptionId, input, null, null));
		}

		public static void CreateOrUpdateDeployment(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, CreateDeploymentInput input)
		{
			proxy.EndCreateOrUpdateDeployment(proxy.BeginCreateOrUpdateDeployment(subscriptionId, serviceName, deploymentSlot, input, null, null));
		}

		public static OSImage CreateOSImage(this IServiceManagement proxy, string subscriptionID, OSImage image)
		{
			return proxy.EndCreateOSImage(proxy.BeginCreateOSImage(subscriptionID, image, null, null));
		}

		public static void CreateStorageService(this IServiceManagement proxy, string subscriptionId, CreateStorageServiceInput input)
		{
			proxy.EndCreateStorageService(proxy.BeginCreateStorageService(subscriptionId, input, null, null));
		}

		public static void DeleteAffinityGroup(this IServiceManagement proxy, string subscriptionId, string affinityGroupName)
		{
			proxy.EndDeleteAffinityGroup(proxy.BeginDeleteAffinityGroup(subscriptionId, affinityGroupName, null, null));
		}

		public static void DeleteCertificate(this IServiceManagement proxy, string subscriptionId, string serviceName, string algorithm, string thumbprint)
		{
			proxy.EndDeleteCertificate(proxy.BeginDeleteCertificate(subscriptionId, serviceName, algorithm, thumbprint, null, null));
		}

		public static void DeleteDataDisk(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleName, int lun)
		{
			proxy.EndDeleteDataDisk(proxy.BeginDeleteDataDisk(subscriptionId, serviceName, deploymentName, roleName, lun.ToString(), null, null));
		}

		public static void DeleteDeployment(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName)
		{
			proxy.EndDeleteDeployment(proxy.BeginDeleteDeployment(subscriptionId, serviceName, deploymentName, null, null));
		}

		public static void DeleteDeploymentBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot)
		{
			proxy.EndDeleteDeploymentBySlot(proxy.BeginDeleteDeploymentBySlot(subscriptionId, serviceName, deploymentSlot, null, null));
		}

		public static void DeleteDisk(this IServiceManagement proxy, string subscriptionID, string diskName)
		{
			proxy.EndDeleteDisk(proxy.BeginDeleteDisk(subscriptionID, diskName, null, null));
		}

		public static void DeleteHostedService(this IServiceManagement proxy, string subscriptionId, string serviceName)
		{
			proxy.EndDeleteHostedService(proxy.BeginDeleteHostedService(subscriptionId, serviceName, null, null));
		}

		public static void DeleteImage(this IServiceManagement proxy, string subscriptionID, string imageName)
		{
			proxy.EndDeleteImage(proxy.BeginDeleteImage(subscriptionID, imageName, null, null));
		}

		public static void DeleteOSImage(this IServiceManagement proxy, string subscriptionID, string imageName)
		{
			proxy.EndDeleteOSImage(proxy.BeginDeleteOSImage(subscriptionID, imageName, null, null));
		}

		public static void DeleteRole(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleName)
		{
			proxy.EndDeleteRole(proxy.BeginDeleteRole(subscriptionId, serviceName, deploymentName, roleName, null, null));
		}

		public static void DeleteStorageService(this IServiceManagement proxy, string subscriptionId, string serviceName)
		{
			proxy.EndDeleteStorageService(proxy.BeginDeleteStorageService(subscriptionId, serviceName, null, null));
		}

		public static Stream DownloadRDPFile(this IServiceManagement proxy, string subscriptionID, string serviceName, string deploymentName, string roleInstanceName)
		{
			return proxy.EndDownloadRDPFile(proxy.BeginDownloadRDPFile(subscriptionID, serviceName, deploymentName, roleInstanceName, null, null));
		}

		public static AffinityGroup GetAffinityGroup(this IServiceManagement proxy, string subscriptionId, string affinityGroupName)
		{
			return proxy.EndGetAffinityGroup(proxy.BeginGetAffinityGroup(subscriptionId, affinityGroupName, null, null));
		}

		public static Certificate GetCertificate(this IServiceManagement proxy, string subscriptionId, string serviceName, string algorithm, string thumbprint)
		{
			return proxy.EndGetCertificate(proxy.BeginGetCertificate(subscriptionId, serviceName, algorithm, thumbprint, null, null));
		}

		public static DataVirtualHardDisk GetDataDisk(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleName, int lun)
		{
			return proxy.EndGetDataDisk(proxy.BeginGetDataDisk(subscriptionId, serviceName, deploymentName, roleName, lun.ToString(), null, null));
		}

		public static Deployment GetDeployment(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName)
		{
			return proxy.EndGetDeployment(proxy.BeginGetDeployment(subscriptionId, serviceName, deploymentName, null, null));
		}

		public static Deployment GetDeploymentBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot)
		{
			return proxy.EndGetDeploymentBySlot(proxy.BeginGetDeploymentBySlot(subscriptionId, serviceName, deploymentSlot, null, null));
		}

		public static Disk GetDisk(this IServiceManagement proxy, string subscriptionID, string diskName)
		{
			return proxy.EndGetDisk(proxy.BeginGetDisk(subscriptionID, diskName, null, null));
		}

		public static HostedService GetHostedService(this IServiceManagement proxy, string subscriptionId, string serviceName)
		{
			return proxy.EndGetHostedService(proxy.BeginGetHostedService(subscriptionId, serviceName, null, null));
		}

		public static HostedService GetHostedServiceWithDetails(this IServiceManagement proxy, string subscriptionId, string serviceName, bool embedDetail)
		{
			return proxy.EndGetHostedServiceWithDetails(proxy.BeginGetHostedServiceWithDetails(subscriptionId, serviceName, embedDetail, null, null));
		}

		public static MachineImage GetImageProperties(this IServiceManagement proxy, string subscriptionID, string imageName)
		{
			return proxy.EndGetImageProperties(proxy.BeginGetImageProperties(subscriptionID, imageName, null, null));
		}

		public static MachineImageReference GetImageReference(this IServiceManagement proxy, string subscriptionId, string imageName, DateTime expiry, ImageSharedAccessSignaturePermission accessModifier)
		{
			return proxy.EndGetImageReference(proxy.BeginGetImageReference(subscriptionId, imageName, expiry.ToString("o"), accessModifier.ToString().ToLower(), null, null));
		}

		public static Stream GetNetworkConfiguration(this IServiceManagement proxy, string subscriptionID)
		{
			return proxy.EndGetNetworkConfiguration(proxy.BeginGetNetworkConfiguration(subscriptionID, null, null));
		}

		public static Operation GetOperationStatus(this IServiceManagement proxy, string subscriptionId, string operationId)
		{
			return proxy.EndGetOperationStatus(proxy.BeginGetOperationStatus(subscriptionId, operationId, null, null));
		}

		public static OSImage GetOSImage(this IServiceManagement proxy, string subscriptionID, string imageName)
		{
			return proxy.EndGetOSImage(proxy.BeginGetOSImage(subscriptionID, imageName, null, null));
		}

		public static void GetPackage(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string containerUri, bool overwriteExisting)
		{
			proxy.EndGetPackage(proxy.BeginGetPackage(subscriptionId, serviceName, deploymentName, containerUri, overwriteExisting, null, null));
		}

		public static void GetPackageBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, string containerUri, bool overwriteExisting)
		{
			proxy.EndGetPackageBySlot(proxy.BeginGetPackageBySlot(subscriptionId, serviceName, deploymentSlot, containerUri, overwriteExisting, null, null));
		}

		public static PersistentVMRole GetRole(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleName)
		{
			return (PersistentVMRole)proxy.EndGetRole(proxy.BeginGetRole(subscriptionId, serviceName, deploymentName, roleName, null, null));
		}

		public static StorageService GetStorageKeys(this IServiceManagement proxy, string subscriptionId, string name)
		{
			return proxy.EndGetStorageKeys(proxy.BeginGetStorageKeys(subscriptionId, name, null, null));
		}

		public static StorageService GetStorageService(this IServiceManagement proxy, string subscriptionId, string name)
		{
			return proxy.EndGetStorageService(proxy.BeginGetStorageService(subscriptionId, name, null, null));
		}

		public static Subscription GetSubscription(this IServiceManagement proxy, string subscriptionID)
		{
			return proxy.EndGetSubscription(proxy.BeginGetSubscription(subscriptionID, null, null));
		}

		public static AvailabilityResponse IsDNSAvailable(this IServiceManagement proxy, string subscriptionID, string dnsname)
		{
			return proxy.EndIsDNSAvailable(proxy.BeginIsDNSAvailable(subscriptionID, dnsname, null, null));
		}

		public static AvailabilityResponse IsStorageServiceAvailable(this IServiceManagement proxy, string subscriptionId, string serviceName)
		{
			return proxy.EndIsStorageServiceAvailable(proxy.BeginIsStorageServiceAvailable(subscriptionId, serviceName, null, null));
		}

		public static AffinityGroupList ListAffinityGroups(this IServiceManagement proxy, string subscriptionId)
		{
			return proxy.EndListAffinityGroups(proxy.BeginListAffinityGroups(subscriptionId, null, null));
		}

		public static CertificateList ListCertificates(this IServiceManagement proxy, string subscriptionId, string serviceName)
		{
			return proxy.EndListCertificates(proxy.BeginListCertificates(subscriptionId, serviceName, null, null));
		}

		public static DiskList ListDisks(this IServiceManagement proxy, string subscriptionID)
		{
			return proxy.EndListDisks(proxy.BeginListDisks(subscriptionID, null, null));
		}

		public static HostedServiceList ListHostedServices(this IServiceManagement proxy, string subscriptionId)
		{
			return proxy.EndListHostedServices(proxy.BeginListHostedServices(subscriptionId, null, null));
		}

		public static HostedServiceList ListHostedServicesWithDetails(this IServiceManagement proxy, string subscriptionId, ref string continuationToken)
		{
			if (continuationToken == null)
			{
				WebOperationContext.Current.OutgoingRequest.Headers["x-ms-continuation-token"] = "All";
			}
			else
			{
				WebOperationContext.Current.OutgoingRequest.Headers["x-ms-continuation-token"] = continuationToken;
			}
			HostedServiceList hostedServiceList = proxy.EndListHostedServices(proxy.BeginListHostedServices(subscriptionId, null, null));
			continuationToken = WebOperationContext.Current.IncomingResponse.Headers["x-ms-continuation-token"];
			return hostedServiceList;
		}

		public static MachineImageList ListImages(this IServiceManagement proxy, string subscriptionID)
		{
			return proxy.EndListImages(proxy.BeginListImages(subscriptionID, null, null));
		}

		public static LocationList ListLocations(this IServiceManagement proxy, string subscriptionId)
		{
			return proxy.EndListLocations(proxy.BeginListLocations(subscriptionId, null, null));
		}

		public static OperatingSystemFamilyList ListOperatingSystemFamilies(this IServiceManagement proxy, string subscriptionId)
		{
			return proxy.EndListOperatingSystemFamilies(proxy.BeginListOperatingSystemFamilies(subscriptionId, null, null));
		}

		public static OperatingSystemList ListOperatingSystems(this IServiceManagement proxy, string subscriptionId)
		{
			return proxy.EndListOperatingSystems(proxy.BeginListOperatingSystems(subscriptionId, null, null));
		}

		public static OSImageList ListOSImages(this IServiceManagement proxy, string subscriptionID)
		{
			return proxy.EndListOSImages(proxy.BeginListOSImages(subscriptionID, null, null));
		}

		public static StorageServiceList ListStorageServices(this IServiceManagement proxy, string subscriptionId)
		{
			return proxy.EndListStorageServices(proxy.BeginListStorageServices(subscriptionId, null, null));
		}

		public static SubscriptionOperationCollection ListSubscriptionOperations(this IServiceManagement proxy, string subscriptionID, string startTime, string endTime, string objectIdFilter, string operationResultFilter, string continuationToken)
		{
			return proxy.EndListSubscriptionOperations(proxy.BeginListSubscriptionOperations(subscriptionID, startTime, endTime, objectIdFilter, operationResultFilter, continuationToken, null, null));
		}

		public static VirtualNetworkSiteList ListVirtualNetworkSites(this IServiceManagement proxy, string subscriptionID)
		{
			return proxy.EndListVirtualNetworkSites(proxy.BeginListVirtualNetworkSites(subscriptionID, null, null));
		}

		public static void PrepareImageUpload(this IServiceManagement proxy, string subscriptionID, string imageName, PrepareImageUploadInput input)
		{
			proxy.EndPrepareImageUpload(proxy.BeginPrepareImageUpload(subscriptionID, imageName, input, null, null));
		}

		public static void RebootDeploymentRoleInstance(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleInstanceName)
		{
			proxy.EndRebootDeploymentRoleInstance(proxy.BeginRebootDeploymentRoleInstance(subscriptionId, serviceName, deploymentName, roleInstanceName, null, null));
		}

		public static void RebootDeploymentRoleInstanceBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, string roleInstanceName)
		{
			proxy.EndRebootDeploymentRoleInstanceBySlot(proxy.BeginRebootDeploymentRoleInstanceBySlot(subscriptionId, serviceName, deploymentSlot, roleInstanceName, null, null));
		}

		public static StorageService RegenerateStorageServiceKeys(this IServiceManagement proxy, string subscriptionId, string name, RegenerateKeys regenerateKeys)
		{
			return proxy.EndRegenerateStorageServiceKeys(proxy.BeginRegenerateStorageServiceKeys(subscriptionId, name, regenerateKeys, null, null));
		}

		public static void ReimageDeploymentRoleInstance(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleInstanceName)
		{
			proxy.EndReimageDeploymentRoleInstance(proxy.BeginReimageDeploymentRoleInstance(subscriptionId, serviceName, deploymentName, roleInstanceName, null, null));
		}

		public static void ReimageDeploymentRoleInstanceBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, string roleInstanceName)
		{
			proxy.EndReimageDeploymentRoleInstanceBySlot(proxy.BeginReimageDeploymentRoleInstanceBySlot(subscriptionId, serviceName, deploymentSlot, roleInstanceName, null, null));
		}

		public static void RestartRole(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleInstanceName)
		{
			proxy.EndExecuteRoleOperation(proxy.BeginExecuteRoleOperation(subscriptionId, serviceName, deploymentName, roleInstanceName, new RestartRoleOperation(), null, null));
		}

		public static void ResumeDeploymentUpdateOrUpgrade(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName)
		{
			proxy.EndResumeDeploymentUpdateOrUpgrade(proxy.BeginResumeDeploymentUpdateOrUpgrade(subscriptionId, serviceName, deploymentName, null, null));
		}

		public static void ResumeDeploymentUpdateOrUpgradeBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot)
		{
			proxy.EndResumeDeploymentUpdateOrUpgradeBySlot(proxy.BeginResumeDeploymentUpdateOrUpgradeBySlot(subscriptionId, serviceName, deploymentSlot, null, null));
		}

		public static void RollbackDeploymentUpdateOrUpgrade(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, RollbackUpdateOrUpgradeInput input)
		{
			proxy.EndRollbackDeploymentUpdateOrUpgrade(proxy.BeginRollbackDeploymentUpdateOrUpgrade(subscriptionId, serviceName, deploymentName, input, null, null));
		}

		public static void RollbackDeploymentUpdateOrUpgradeBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string slotName, RollbackUpdateOrUpgradeInput input)
		{
			proxy.EndRollbackDeploymentUpdateOrUpgradeBySlot(proxy.BeginRollbackDeploymentUpdateOrUpgradeBySlot(subscriptionId, serviceName, slotName, input, null, null));
		}

		public static void SetImageProperties(this IServiceManagement proxy, string subscriptionID, string imageName, SetMachineImagePropertiesInput input)
		{
			proxy.EndSetImageProperties(proxy.BeginSetImageProperties(subscriptionID, imageName, input, null, null));
		}

		public static void SetNetworkConfiguration(this IServiceManagement proxy, string subscriptionID, Stream networkConfiguration)
		{
			proxy.EndSetNetworkConfiguration(proxy.BeginSetNetworkConfiguration(subscriptionID, networkConfiguration, null, null));
		}

		public static void SetParentImage(this IServiceManagement proxy, string subscriptionID, string imageName, SetParentImageInput input)
		{
			proxy.EndSetParentImage(proxy.BeginSetParentImage(subscriptionID, imageName, input, null, null));
		}

		public static void SetVirtualNetworkGatewayConfiguration(this IServiceManagement proxy, string subscriptionID, string virtualnetworkID, VirtualNetworkGatewayConfiguration gatewayConfiguration)
		{
			proxy.EndSetVirtualNetworkGatewayConfiguration(proxy.BeginSetVirtualNetworkGatewayConfiguration(subscriptionID, virtualnetworkID, gatewayConfiguration, null, null));
		}

		public static void ShutdownRole(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleInstanceName)
		{
			proxy.EndExecuteRoleOperation(proxy.BeginExecuteRoleOperation(subscriptionId, serviceName, deploymentName, roleInstanceName, new ShutdownRoleOperation(), null, null));
		}

		public static void StartRole(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleInstanceName)
		{
			proxy.EndExecuteRoleOperation(proxy.BeginExecuteRoleOperation(subscriptionId, serviceName, deploymentName, roleInstanceName, new StartRoleOperation(), null, null));
		}

		public static void SuspendDeploymentUpdateOrUpgrade(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName)
		{
			proxy.EndSuspendDeploymentUpdateOrUpgrade(proxy.BeginSuspendDeploymentUpdateOrUpgrade(subscriptionId, serviceName, deploymentName, null, null));
		}

		public static void SuspendDeploymentUpdateOrUpgradeBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot)
		{
			proxy.EndSuspendDeploymentUpdateOrUpgradeBySlot(proxy.BeginSuspendDeploymentUpdateOrUpgradeBySlot(subscriptionId, serviceName, deploymentSlot, null, null));
		}

		public static void SwapDeployment(this IServiceManagement proxy, string subscriptionId, string serviceName, SwapDeploymentInput input)
		{
			proxy.EndSwapDeployment(proxy.BeginSwapDeployment(subscriptionId, serviceName, input, null, null));
		}

		public static void UpdateAffinityGroup(this IServiceManagement proxy, string subscriptionId, string affinityGroupName, UpdateAffinityGroupInput input)
		{
			proxy.EndUpdateAffinityGroup(proxy.BeginUpdateAffinityGroup(subscriptionId, affinityGroupName, input, null, null));
		}

		public static void UpdateDataDisk(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleName, int lun, DataVirtualHardDisk dataDisk)
		{
			proxy.EndUpdateDataDisk(proxy.BeginUpdateDataDisk(subscriptionId, serviceName, deploymentName, roleName, lun.ToString(), dataDisk, null, null));
		}

		public static void UpdateDeploymentStatus(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, UpdateDeploymentStatusInput input)
		{
			proxy.EndUpdateDeploymentStatus(proxy.BeginUpdateDeploymentStatus(subscriptionId, serviceName, deploymentName, input, null, null));
		}

		public static void UpdateDeploymentStatusBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, UpdateDeploymentStatusInput input)
		{
			proxy.EndUpdateDeploymentStatusBySlot(proxy.BeginUpdateDeploymentStatusBySlot(subscriptionId, serviceName, deploymentSlot, input, null, null));
		}

		public static Disk UpdateDisk(this IServiceManagement proxy, string subscriptionID, string diskName, Disk disk)
		{
			return proxy.EndUpdateDisk(proxy.BeginUpdateDisk(subscriptionID, diskName, disk, null, null));
		}

		public static void UpdateHostedService(this IServiceManagement proxy, string subscriptionId, string serviceName, UpdateHostedServiceInput input)
		{
			proxy.EndUpdateHostedService(proxy.BeginUpdateHostedService(subscriptionId, serviceName, input, null, null));
		}

		public static OSImage UpdateOSImage(this IServiceManagement proxy, string subscriptionID, string imageName, OSImage image)
		{
			return proxy.EndUpdateOSImage(proxy.BeginUpdateOSImage(subscriptionID, imageName, image, null, null));
		}

		public static void UpdateRole(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleName, PersistentVMRole role)
		{
			proxy.EndUpdateRole(proxy.BeginUpdateRole(subscriptionId, serviceName, deploymentName, roleName, role, null, null));
		}

		public static void UpdateStorageService(this IServiceManagement proxy, string subscriptionId, string serviceName, UpdateStorageServiceInput input)
		{
			proxy.EndUpdateStorageService(proxy.BeginUpdateStorageService(subscriptionId, serviceName, input, null, null));
		}

		public static void UpgradeDeployment(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, UpgradeDeploymentInput input)
		{
			proxy.EndUpgradeDeployment(proxy.BeginUpgradeDeployment(subscriptionId, serviceName, deploymentName, input, null, null));
		}

		public static void UpgradeDeploymentBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, UpgradeDeploymentInput input)
		{
			proxy.EndUpgradeDeploymentBySlot(proxy.BeginUpgradeDeploymentBySlot(subscriptionId, serviceName, deploymentSlot, input, null, null));
		}

		public static void WalkUpgradeDomain(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, WalkUpgradeDomainInput input)
		{
			proxy.EndWalkUpgradeDomain(proxy.BeginWalkUpgradeDomain(subscriptionId, serviceName, deploymentName, input, null, null));
		}

		public static void WalkUpgradeDomainBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, WalkUpgradeDomainInput input)
		{
			proxy.EndWalkUpgradeDomainBySlot(proxy.BeginWalkUpgradeDomainBySlot(subscriptionId, serviceName, deploymentSlot, input, null, null));
		}
	}
}