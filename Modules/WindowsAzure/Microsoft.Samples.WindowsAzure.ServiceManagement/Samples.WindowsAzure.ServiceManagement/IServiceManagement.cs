using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[ServiceContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public interface IServiceManagement
	{
		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/certificates")]
		IAsyncResult BeginAddCertificates(string subscriptionId, string serviceName, CertificateFile input, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionID}/services/hostedservices/{serviceName}/deployments/{deploymentName}/Roles/{roleName}/DataDisks")]
		IAsyncResult BeginAddDataDisk(string subscriptionID, string serviceName, string deploymentName, string roleName, DataVirtualHardDisk dataDisk, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[ServiceKnownType(typeof(PersistentVMRole))]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionID}/services/hostedservices/{serviceName}/deployments/{deploymentName}/Roles")]
		IAsyncResult BeginAddRole(string subscriptionID, string serviceName, string deploymentName, Role role, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}/?comp=config")]
		IAsyncResult BeginChangeConfiguration(string subscriptionId, string serviceName, string deploymentName, ChangeConfigurationInput input, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}/?comp=config")]
		IAsyncResult BeginChangeConfigurationBySlot(string subscriptionId, string serviceName, string deploymentSlot, ChangeConfigurationInput input, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionID}/machineimages/{imageName}?comp=commitmachineimage")]
		IAsyncResult BeginCommitImageUpload(string subscriptionID, string imageName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/affinitygroups")]
		IAsyncResult BeginCreateAffinityGroup(string subscriptionId, CreateAffinityGroupInput input, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deployments")]
		IAsyncResult BeginCreateDeployment(string subscriptionId, string serviceName, Deployment deployment, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionID}/services/disks")]
		IAsyncResult BeginCreateDisk(string subscriptionID, Disk disk, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices")]
		IAsyncResult BeginCreateHostedService(string subscriptionId, CreateHostedServiceInput input, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}")]
		IAsyncResult BeginCreateOrUpdateDeployment(string subscriptionId, string serviceName, string deploymentSlot, CreateDeploymentInput input, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionID}/services/images")]
		IAsyncResult BeginCreateOSImage(string subscriptionID, OSImage image, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/storageservices")]
		IAsyncResult BeginCreateStorageService(string subscriptionId, CreateStorageServiceInput input, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="DELETE", UriTemplate="{subscriptionId}/affinitygroups/{affinityGroupName}")]
		IAsyncResult BeginDeleteAffinityGroup(string subscriptionId, string affinityGroupName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="DELETE", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/certificates/{thumbprintalgorithm}-{thumbprint_in_hex}")]
		IAsyncResult BeginDeleteCertificate(string subscriptionId, string serviceName, string thumbprintalgorithm, string thumbprint_in_hex, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="DELETE", UriTemplate="{subscriptionID}/services/hostedservices/{serviceName}/deployments/{deploymentName}/Roles/{roleName}/DataDisks/{lun}")]
		IAsyncResult BeginDeleteDataDisk(string subscriptionID, string serviceName, string deploymentName, string roleName, string lun, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="DELETE", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}")]
		IAsyncResult BeginDeleteDeployment(string subscriptionId, string serviceName, string deploymentName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="DELETE", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}")]
		IAsyncResult BeginDeleteDeploymentBySlot(string subscriptionId, string serviceName, string deploymentSlot, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="DELETE", UriTemplate="{subscriptionID}/services/disks/{diskName}")]
		IAsyncResult BeginDeleteDisk(string subscriptionID, string diskName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="DELETE", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}")]
		IAsyncResult BeginDeleteHostedService(string subscriptionId, string serviceName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="DELETE", UriTemplate="{subscriptionID}/machineimages/{imageName}")]
		IAsyncResult BeginDeleteImage(string subscriptionID, string imageName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="DELETE", UriTemplate="{subscriptionID}/services/images/{imageName}")]
		IAsyncResult BeginDeleteOSImage(string subscriptionID, string imageName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="DELETE", UriTemplate="{subscriptionID}/services/hostedservices/{serviceName}/deployments/{deploymentName}/Roles/{roleName}")]
		IAsyncResult BeginDeleteRole(string subscriptionID, string serviceName, string deploymentName, string roleName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="DELETE", UriTemplate="{subscriptionId}/services/storageservices/{serviceName}")]
		IAsyncResult BeginDeleteStorageService(string subscriptionId, string serviceName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionID}/services/hostedservices/{serviceName}/deployments/{deploymentName}/roleinstances/{instanceName}/ModelFile?FileType=RDP")]
		IAsyncResult BeginDownloadRDPFile(string subscriptionID, string serviceName, string deploymentName, string instanceName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[ServiceKnownType(typeof(StartRoleOperation))]
		[ServiceKnownType(typeof(RestartRoleOperation))]
		[ServiceKnownType(typeof(CaptureRoleOperation))]
		[ServiceKnownType(typeof(ShutdownRoleOperation))]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionID}/services/hostedservices/{serviceName}/deployments/{deploymentName}/roleInstances/{roleInstanceName}/Operations")]
		IAsyncResult BeginExecuteRoleOperation(string subscriptionID, string serviceName, string deploymentName, string roleInstanceName, RoleOperation roleOperation, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionId}/affinitygroups/{affinityGroupName}")]
		IAsyncResult BeginGetAffinityGroup(string subscriptionId, string affinityGroupName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/certificates/{thumbprintalgorithm}-{thumbprint_in_hex}")]
		IAsyncResult BeginGetCertificate(string subscriptionId, string serviceName, string thumbprintalgorithm, string thumbprint_in_hex, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionID}/services/hostedservices/{serviceName}/deployments/{deploymentName}/Roles/{roleName}/DataDisks/{lun}")]
		IAsyncResult BeginGetDataDisk(string subscriptionID, string serviceName, string deploymentName, string roleName, string lun, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}")]
		IAsyncResult BeginGetDeployment(string subscriptionId, string serviceName, string deploymentName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}")]
		IAsyncResult BeginGetDeploymentBySlot(string subscriptionId, string serviceName, string deploymentSlot, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionID}/services/disks/{diskName}")]
		IAsyncResult BeginGetDisk(string subscriptionID, string diskName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}")]
		IAsyncResult BeginGetHostedService(string subscriptionId, string serviceName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}?embed-detail={embedDetail}")]
		IAsyncResult BeginGetHostedServiceWithDetails(string subscriptionId, string serviceName, bool embedDetail, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionID}/machineimages/{imageName}")]
		IAsyncResult BeginGetImageProperties(string subscriptionID, string imageName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionID}/machineimages/{imageName}?comp=reference&expiry={expiry}&permission={permission}")]
		IAsyncResult BeginGetImageReference(string subscriptionID, string imageName, string expiry, string permission, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionId}/services/networking/media")]
		IAsyncResult BeginGetNetworkConfiguration(string subscriptionId, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionId}/operations/{operationTrackingId}")]
		IAsyncResult BeginGetOperationStatus(string subscriptionId, string operationTrackingId, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionID}/services/images/{imageName}")]
		IAsyncResult BeginGetOSImage(string subscriptionID, string imageName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}/package?containerUri={containerUri}&overwriteExisting={overwriteExisting}")]
		IAsyncResult BeginGetPackage(string subscriptionId, string serviceName, string deploymentName, string containerUri, bool overwriteExisting, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}/package?containerUri={containerUri}&overwriteExisting={overwriteExisting}")]
		IAsyncResult BeginGetPackageBySlot(string subscriptionId, string serviceName, string deploymentSlot, string containerUri, bool overwriteExisting, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[ServiceKnownType(typeof(PersistentVMRole))]
		[WebGet(UriTemplate="{subscriptionID}/services/hostedservices/{serviceName}/deployments/{deploymentName}/Roles/{roleName}")]
		IAsyncResult BeginGetRole(string subscriptionID, string serviceName, string deploymentName, string roleName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionId}/services/storageservices/{serviceName}/keys")]
		IAsyncResult BeginGetStorageKeys(string subscriptionId, string serviceName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionId}/services/storageservices/{serviceName}")]
		IAsyncResult BeginGetStorageService(string subscriptionId, string serviceName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionID}")]
		IAsyncResult BeginGetSubscription(string subscriptionID, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="GET", UriTemplate="{subscriptionId}/services/hostedservices/operations/isavailable/{serviceName}")]
		IAsyncResult BeginIsDNSAvailable(string subscriptionId, string serviceName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionID}/services/storageservices/operations/isavailable/{serviceName}")]
		IAsyncResult BeginIsStorageServiceAvailable(string subscriptionID, string serviceName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionId}/affinitygroups")]
		IAsyncResult BeginListAffinityGroups(string subscriptionId, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/certificates")]
		IAsyncResult BeginListCertificates(string subscriptionId, string serviceName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionID}/services/disks")]
		IAsyncResult BeginListDisks(string subscriptionID, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionId}/services/hostedservices")]
		IAsyncResult BeginListHostedServices(string subscriptionId, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionID}/machineimages")]
		IAsyncResult BeginListImages(string subscriptionID, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionId}/locations")]
		IAsyncResult BeginListLocations(string subscriptionId, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="GET", UriTemplate="{subscriptionId}/operatingsystemfamilies")]
		IAsyncResult BeginListOperatingSystemFamilies(string subscriptionId, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="GET", UriTemplate="{subscriptionId}/operatingsystems")]
		IAsyncResult BeginListOperatingSystems(string subscriptionId, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionID}/services/images")]
		IAsyncResult BeginListOSImages(string subscriptionID, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionId}/services/storageservices")]
		IAsyncResult BeginListStorageServices(string subscriptionId, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionID}/operations?starttime={starttime}&endtime={endtime}&objectidfilter={objectidfilter}&operationresultfilter={operationresultfilter}&continuationtoken={continuationtoken}")]
		IAsyncResult BeginListSubscriptionOperations(string subscriptionID, string startTime, string endTime, string objectIdFilter, string operationResultFilter, string continuationToken, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionId}/services/networking/virtualnetwork")]
		IAsyncResult BeginListVirtualNetworkSites(string subscriptionId, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="PUT", UriTemplate="{subscriptionID}/machineimages/{imageName}")]
		IAsyncResult BeginPrepareImageUpload(string subscriptionId, string imageName, PrepareImageUploadInput input, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true, Action="*", ReplyAction="*")]
		[WebInvoke(Method="*", UriTemplate="*")]
		IAsyncResult BeginProcessMessage(Message request, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}/roleinstances/{roleinstancename}?comp=reboot")]
		IAsyncResult BeginRebootDeploymentRoleInstance(string subscriptionId, string serviceName, string deploymentName, string roleInstanceName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}/roleinstances/{roleinstancename}?comp=reboot")]
		IAsyncResult BeginRebootDeploymentRoleInstanceBySlot(string subscriptionId, string serviceName, string deploymentSlot, string roleInstanceName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/storageservices/{serviceName}/keys?action=regenerate")]
		IAsyncResult BeginRegenerateStorageServiceKeys(string subscriptionId, string serviceName, RegenerateKeys regenerateKeys, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}/roleinstances/{roleinstancename}?comp=reimage")]
		IAsyncResult BeginReimageDeploymentRoleInstance(string subscriptionId, string serviceName, string deploymentName, string roleInstanceName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}/roleinstances/{roleinstancename}?comp=reimage")]
		IAsyncResult BeginReimageDeploymentRoleInstanceBySlot(string subscriptionId, string serviceName, string deploymentSlot, string roleInstanceName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}/?comp=resume")]
		IAsyncResult BeginResumeDeploymentUpdateOrUpgrade(string subscriptionId, string serviceName, string deploymentName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}/?comp=resume")]
		IAsyncResult BeginResumeDeploymentUpdateOrUpgradeBySlot(string subscriptionId, string serviceName, string deploymentSlot, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}/?comp=rollback")]
		IAsyncResult BeginRollbackDeploymentUpdateOrUpgrade(string subscriptionId, string serviceName, string deploymentName, RollbackUpdateOrUpgradeInput input, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}/?comp=rollback")]
		IAsyncResult BeginRollbackDeploymentUpdateOrUpgradeBySlot(string subscriptionId, string serviceName, string deploymentSlot, RollbackUpdateOrUpgradeInput input, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionID}/machineimages/{imagename}?comp=properties")]
		IAsyncResult BeginSetImageProperties(string subscriptionID, string imageName, SetMachineImagePropertiesInput imageProperties, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="PUT", UriTemplate="{subscriptionId}/services/networking/media")]
		IAsyncResult BeginSetNetworkConfiguration(string subscriptionId, Stream networkConfiguration, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionID}/machineimages/{imagename}?comp=setparent")]
		IAsyncResult BeginSetParentImage(string subscriptionID, string imageName, SetParentImageInput parentImageInput, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/networking/virtualnetworkgatewayconfiguration?virtualnetworkId={virtualnetworkId}")]
		IAsyncResult BeginSetVirtualNetworkGatewayConfiguration(string subscriptionId, string virtualnetworkId, VirtualNetworkGatewayConfiguration gatewayConfiguration, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}/?comp=suspend")]
		IAsyncResult BeginSuspendDeploymentUpdateOrUpgrade(string subscriptionId, string serviceName, string deploymentName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}/?comp=suspend")]
		IAsyncResult BeginSuspendDeploymentUpdateOrUpgradeBySlot(string subscriptionId, string serviceName, string deploymentSlot, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}")]
		IAsyncResult BeginSwapDeployment(string subscriptionId, string serviceName, SwapDeploymentInput input, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="PUT", UriTemplate="{subscriptionId}/affinitygroups/{affinityGroupName}")]
		IAsyncResult BeginUpdateAffinityGroup(string subscriptionId, string affinityGroupName, UpdateAffinityGroupInput input, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="PUT", UriTemplate="{subscriptionID}/services/hostedservices/{serviceName}/deployments/{deploymentName}/Roles/{roleName}/DataDisks/{lun}")]
		IAsyncResult BeginUpdateDataDisk(string subscriptionID, string serviceName, string deploymentName, string roleName, string lun, DataVirtualHardDisk dataDisk, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}/?comp=status")]
		IAsyncResult BeginUpdateDeploymentStatus(string subscriptionId, string serviceName, string deploymentName, UpdateDeploymentStatusInput input, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}/?comp=status")]
		IAsyncResult BeginUpdateDeploymentStatusBySlot(string subscriptionId, string serviceName, string deploymentSlot, UpdateDeploymentStatusInput input, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="PUT", UriTemplate="{subscriptionID}/services/disks/{diskName}")]
		IAsyncResult BeginUpdateDisk(string subscriptionID, string diskName, Disk disk, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="PUT", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}")]
		IAsyncResult BeginUpdateHostedService(string subscriptionId, string serviceName, UpdateHostedServiceInput input, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="PUT", UriTemplate="{subscriptionID}/services/images/{imageName}")]
		IAsyncResult BeginUpdateOSImage(string subscriptionID, string imageName, OSImage image, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[ServiceKnownType(typeof(PersistentVMRole))]
		[WebInvoke(Method="PUT", UriTemplate="{subscriptionID}/services/hostedservices/{serviceName}/deployments/{deploymentName}/Roles/{roleName}")]
		IAsyncResult BeginUpdateRole(string subscriptionID, string serviceName, string deploymentName, string roleName, Role role, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="PUT", UriTemplate="{subscriptionId}/services/storageservices/{serviceName}")]
		IAsyncResult BeginUpdateStorageService(string subscriptionId, string serviceName, UpdateStorageServiceInput input, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}/?comp=upgrade")]
		IAsyncResult BeginUpgradeDeployment(string subscriptionId, string serviceName, string deploymentName, UpgradeDeploymentInput input, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}/?comp=upgrade")]
		IAsyncResult BeginUpgradeDeploymentBySlot(string subscriptionId, string serviceName, string deploymentSlot, UpgradeDeploymentInput input, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}/?comp=walkupgradedomain")]
		IAsyncResult BeginWalkUpgradeDomain(string subscriptionId, string serviceName, string deploymentName, WalkUpgradeDomainInput input, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(Method="POST", UriTemplate="{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}/?comp=walkupgradedomain")]
		IAsyncResult BeginWalkUpgradeDomainBySlot(string subscriptionId, string serviceName, string deploymentSlot, WalkUpgradeDomainInput input, AsyncCallback callback, object state);

		void EndAddCertificates(IAsyncResult asyncResult);

		void EndAddDataDisk(IAsyncResult asyncResult);

		void EndAddRole(IAsyncResult asyncResult);

		void EndChangeConfiguration(IAsyncResult asyncResult);

		void EndChangeConfigurationBySlot(IAsyncResult asyncResult);

		void EndCommitImageUpload(IAsyncResult asyncResult);

		void EndCreateAffinityGroup(IAsyncResult asyncResult);

		void EndCreateDeployment(IAsyncResult asyncResult);

		Disk EndCreateDisk(IAsyncResult asyncResult);

		void EndCreateHostedService(IAsyncResult asyncResult);

		void EndCreateOrUpdateDeployment(IAsyncResult asyncResult);

		OSImage EndCreateOSImage(IAsyncResult asyncResult);

		void EndCreateStorageService(IAsyncResult asyncResult);

		void EndDeleteAffinityGroup(IAsyncResult asyncResult);

		void EndDeleteCertificate(IAsyncResult asyncResult);

		void EndDeleteDataDisk(IAsyncResult asyncResult);

		void EndDeleteDeployment(IAsyncResult asyncResult);

		void EndDeleteDeploymentBySlot(IAsyncResult asyncResult);

		void EndDeleteDisk(IAsyncResult asyncResult);

		void EndDeleteHostedService(IAsyncResult asyncResult);

		void EndDeleteImage(IAsyncResult asyncResult);

		void EndDeleteOSImage(IAsyncResult asyncResult);

		void EndDeleteRole(IAsyncResult asyncResult);

		void EndDeleteStorageService(IAsyncResult asyncResult);

		Stream EndDownloadRDPFile(IAsyncResult asyncResult);

		void EndExecuteRoleOperation(IAsyncResult asyncResult);

		AffinityGroup EndGetAffinityGroup(IAsyncResult asyncResult);

		Certificate EndGetCertificate(IAsyncResult asyncResult);

		DataVirtualHardDisk EndGetDataDisk(IAsyncResult asyncResult);

		Deployment EndGetDeployment(IAsyncResult asyncResult);

		Deployment EndGetDeploymentBySlot(IAsyncResult asyncResult);

		Disk EndGetDisk(IAsyncResult asyncResult);

		HostedService EndGetHostedService(IAsyncResult asyncResult);

		HostedService EndGetHostedServiceWithDetails(IAsyncResult asyncResult);

		MachineImage EndGetImageProperties(IAsyncResult asyncResult);

		MachineImageReference EndGetImageReference(IAsyncResult asyncResult);

		Stream EndGetNetworkConfiguration(IAsyncResult asyncResult);

		Operation EndGetOperationStatus(IAsyncResult asyncResult);

		OSImage EndGetOSImage(IAsyncResult asyncResult);

		void EndGetPackage(IAsyncResult asyncResult);

		void EndGetPackageBySlot(IAsyncResult asyncResult);

		Role EndGetRole(IAsyncResult asyncResult);

		StorageService EndGetStorageKeys(IAsyncResult asyncResult);

		StorageService EndGetStorageService(IAsyncResult asyncResult);

		Subscription EndGetSubscription(IAsyncResult asyncResult);

		AvailabilityResponse EndIsDNSAvailable(IAsyncResult asyncResult);

		AvailabilityResponse EndIsStorageServiceAvailable(IAsyncResult asyncResult);

		AffinityGroupList EndListAffinityGroups(IAsyncResult asyncResult);

		CertificateList EndListCertificates(IAsyncResult asyncResult);

		DiskList EndListDisks(IAsyncResult asyncResult);

		HostedServiceList EndListHostedServices(IAsyncResult asyncResult);

		MachineImageList EndListImages(IAsyncResult asyncResult);

		LocationList EndListLocations(IAsyncResult asyncResult);

		OperatingSystemFamilyList EndListOperatingSystemFamilies(IAsyncResult asyncResult);

		OperatingSystemList EndListOperatingSystems(IAsyncResult asyncResult);

		OSImageList EndListOSImages(IAsyncResult asyncResult);

		StorageServiceList EndListStorageServices(IAsyncResult asyncResult);

		SubscriptionOperationCollection EndListSubscriptionOperations(IAsyncResult asyncResult);

		VirtualNetworkSiteList EndListVirtualNetworkSites(IAsyncResult asyncResult);

		void EndPrepareImageUpload(IAsyncResult asyncResult);

		Message EndProcessMessage(IAsyncResult result);

		void EndRebootDeploymentRoleInstance(IAsyncResult asyncResult);

		void EndRebootDeploymentRoleInstanceBySlot(IAsyncResult asyncResult);

		StorageService EndRegenerateStorageServiceKeys(IAsyncResult asyncResult);

		void EndReimageDeploymentRoleInstance(IAsyncResult asyncResult);

		void EndReimageDeploymentRoleInstanceBySlot(IAsyncResult asyncResult);

		void EndResumeDeploymentUpdateOrUpgrade(IAsyncResult asyncResult);

		void EndResumeDeploymentUpdateOrUpgradeBySlot(IAsyncResult asyncResult);

		void EndRollbackDeploymentUpdateOrUpgrade(IAsyncResult asyncResult);

		void EndRollbackDeploymentUpdateOrUpgradeBySlot(IAsyncResult asyncResult);

		void EndSetImageProperties(IAsyncResult asyncResult);

		void EndSetNetworkConfiguration(IAsyncResult asyncResult);

		void EndSetParentImage(IAsyncResult asyncResult);

		void EndSetVirtualNetworkGatewayConfiguration(IAsyncResult asyncResult);

		void EndSuspendDeploymentUpdateOrUpgrade(IAsyncResult asyncResult);

		void EndSuspendDeploymentUpdateOrUpgradeBySlot(IAsyncResult asyncResult);

		void EndSwapDeployment(IAsyncResult asyncResult);

		void EndUpdateAffinityGroup(IAsyncResult asyncResult);

		void EndUpdateDataDisk(IAsyncResult asyncResult);

		void EndUpdateDeploymentStatus(IAsyncResult asyncResult);

		void EndUpdateDeploymentStatusBySlot(IAsyncResult asyncResult);

		Disk EndUpdateDisk(IAsyncResult asyncResult);

		void EndUpdateHostedService(IAsyncResult asyncResult);

		OSImage EndUpdateOSImage(IAsyncResult asyncResult);

		void EndUpdateRole(IAsyncResult asyncResult);

		void EndUpdateStorageService(IAsyncResult asyncResult);

		void EndUpgradeDeployment(IAsyncResult asyncResult);

		void EndUpgradeDeploymentBySlot(IAsyncResult asyncResult);

		void EndWalkUpgradeDomain(IAsyncResult asyncResult);

		void EndWalkUpgradeDomainBySlot(IAsyncResult asyncResult);
	}
}