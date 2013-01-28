using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Helpers
{
	public static class Disks
	{
		public static void RemoveVHD(IServiceManagement channel, string subscriptionId, Uri mediaLink)
		{
			StorageService storageKeys;
			char[] chrArray = new char[1];
			chrArray[0] = '.';
			string str = mediaLink.Host.Split(chrArray)[0];
			string components = mediaLink.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)channel))
			{
				storageKeys = channel.GetStorageKeys(subscriptionId, str);
			}
			StorageCredentialsAccountAndKey storageCredentialsAccountAndKey = new StorageCredentialsAccountAndKey(str, storageKeys.StorageServiceKeys.Primary);
			CloudBlobClient cloudBlobClient = new CloudBlobClient(components, storageCredentialsAccountAndKey);
			CloudBlob blobReference = cloudBlobClient.GetBlobReference(mediaLink.AbsoluteUri);
			blobReference.DeleteIfExists();
		}
	}
}