using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Helpers
{
	public static class AzureBlob
	{
		private const string BlobEndpointIdentifier = ".blob.";

		private const string ContainerName = "mydeployments";

		public static void DeletePackageFromBlob(IServiceManagement channel, string storageName, string subscriptionId, Uri packageUri)
		{
			StorageService storageKeys = channel.GetStorageKeys(subscriptionId, storageName);
			string primary = storageKeys.StorageServiceKeys.Primary;
			storageKeys = channel.GetStorageService(subscriptionId, storageName);
			EndpointList endpoints = storageKeys.StorageServiceProperties.Endpoints;
			string str = ((List<string>)endpoints).Find((string p) => p.Contains(".blob."));
			StorageCredentialsAccountAndKey storageCredentialsAccountAndKey = new StorageCredentialsAccountAndKey(storageName, primary);
			CloudBlobClient cloudBlobClient = new CloudBlobClient(str, storageCredentialsAccountAndKey);
			CloudBlob blobReference = cloudBlobClient.GetBlobReference(packageUri.AbsoluteUri);
			blobReference.DeleteIfExists();
		}

		private static void UploadBlobStream(CloudBlob blob, string sourceFile)
		{
			FileStream fileStream = File.OpenRead(sourceFile);
			using (fileStream)
			{
				byte[] numArray = new byte[0x20000];
				BlobStream blobStream = blob.OpenWrite();
				using (blobStream)
				{
					blobStream.BlockSize = (long)0x20000;
					while (true)
					{
						int num = fileStream.Read(numArray, 0, (int)numArray.Length);
						if (num <= 0)
						{
							break;
						}
						blobStream.Write(numArray, 0, num);
					}
				}
			}
		}

		private static Uri UploadFile(string storageName, string storageKey, string blobStorageEndpoint, string filePath)
		{
			StorageCredentialsAccountAndKey storageCredentialsAccountAndKey = new StorageCredentialsAccountAndKey(storageName, storageKey);
			CloudBlobClient cloudBlobClient = new CloudBlobClient(blobStorageEndpoint, storageCredentialsAccountAndKey);
			object[] str = new object[2];
			DateTime utcNow = DateTime.UtcNow;
			str[0] = utcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
			str[1] = Path.GetFileName(filePath);
			string str1 = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", str);
			CloudBlobContainer containerReference = cloudBlobClient.GetContainerReference("mydeployments");
			containerReference.CreateIfNotExist();
			CloudBlob blobReference = containerReference.GetBlobReference(str1);
			AzureBlob.UploadBlobStream(blobReference, filePath);
			object[] baseUri = new object[4];
			baseUri[0] = cloudBlobClient.BaseUri;
			baseUri[1] = "mydeployments";
			baseUri[2] = cloudBlobClient.DefaultDelimiter;
			baseUri[3] = str1;
			return new Uri(string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{3}", baseUri));
		}

		public static Uri UploadPackageToBlob(IServiceManagement channel, string storageName, string subscriptionId, string packagePath)
		{
			StorageService storageKeys = channel.GetStorageKeys(subscriptionId, storageName);
			string primary = storageKeys.StorageServiceKeys.Primary;
			storageKeys = channel.GetStorageService(subscriptionId, storageName);
			EndpointList endpoints = storageKeys.StorageServiceProperties.Endpoints;
			string str = ((List<string>)endpoints).Find((string p) => p.Contains(".blob."));
			return AzureBlob.UploadFile(storageName, primary, str, packagePath);
		}
	}
}