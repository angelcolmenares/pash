using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract]
	public class OperationParameter : IExtensibleDataObject
	{
		private static Type[] KnownTypes;

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=0)]
		public string Name
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		private string Value
		{
			get;
			set;
		}

		static OperationParameter()
		{
			Type[] typeArray = new Type[30];
			typeArray[0] = typeof(CreateAffinityGroupInput);
			typeArray[1] = typeof(UpdateAffinityGroupInput);
			typeArray[2] = typeof(CertificateFile);
			typeArray[3] = typeof(ChangeConfigurationInput);
			typeArray[4] = typeof(CreateDeploymentInput);
			typeArray[5] = typeof(CreateHostedServiceInput);
			typeArray[6] = typeof(CreateStorageServiceInput);
			typeArray[7] = typeof(PrepareImageUploadInput);
			typeArray[8] = typeof(RegenerateKeys);
			typeArray[9] = typeof(SetMachineImagePropertiesInput);
			typeArray[10] = typeof(SetParentImageInput);
			typeArray[11] = typeof(StorageDomain);
			typeArray[12] = typeof(SubscriptionCertificate);
			typeArray[13] = typeof(SwapDeploymentInput);
			typeArray[14] = typeof(UpdateDeploymentStatusInput);
			typeArray[15] = typeof(UpdateHostedServiceInput);
			typeArray[16] = typeof(UpdateStorageServiceInput);
			typeArray[17] = typeof(UpgradeDeploymentInput);
			typeArray[18] = typeof(WalkUpgradeDomainInput);
			typeArray[19] = typeof(CaptureRoleOperation);
			typeArray[20] = typeof(ShutdownRoleOperation);
			typeArray[21] = typeof(StartRoleOperation);
			typeArray[22] = typeof(RestartRoleOperation);
			typeArray[23] = typeof(OSImage);
			typeArray[24] = typeof(PersistentVMRole);
			typeArray[25] = typeof(Deployment);
			typeArray[26] = typeof(DataVirtualHardDisk);
			typeArray[27] = typeof(OSImage);
			typeArray[28] = typeof(Disk);
			typeArray[29] = typeof(ExtendedProperty);
			OperationParameter.KnownTypes = typeArray;
		}

		public OperationParameter()
		{
		}

		public string GetSerializedValue()
		{
			return this.Value;
		}

		public object GetValue()
		{
			object value;
			if (!string.IsNullOrEmpty(this.Value))
			{
				DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(object), OperationParameter.KnownTypes);
				try
				{
					value = dataContractSerializer.ReadObject(XmlReader.Create(new StringReader(this.Value)));
				}
				catch
				{
					value = this.Value;
				}
				return value;
			}
			else
			{
				return null;
			}
		}

		public void SetValue(object value)
		{
			if (value != null)
			{
				Type type = value.GetType();
				if (!type.Equals(typeof(string)))
				{
					DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(object), OperationParameter.KnownTypes);
					StringBuilder stringBuilder = new StringBuilder();
					XmlWriter xmlWriter = XmlWriter.Create(stringBuilder);
					using (xmlWriter)
					{
						dataContractSerializer.WriteObject(xmlWriter, value);
						xmlWriter.Flush();
						this.Value = stringBuilder.ToString();
					}
				}
				else
				{
					this.Value = (string)value;
					return;
				}
			}
		}
	}
}