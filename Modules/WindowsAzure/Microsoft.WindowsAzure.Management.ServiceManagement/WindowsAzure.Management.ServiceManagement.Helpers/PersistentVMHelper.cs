using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.IO;
using System.Xml.Serialization;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Helpers
{
	public static class PersistentVMHelper
	{
		public static PersistentVM LoadStateFromFile(string filePath)
		{
			PersistentVM persistentVM;
			if (File.Exists(filePath))
			{
				XmlAttributeOverrides xmlAttributeOverride = new XmlAttributeOverrides();
				XmlAttributes xmlAttribute = new XmlAttributes();
				xmlAttribute.XmlIgnore = true;
				xmlAttributeOverride.Add(typeof(DataVirtualHardDisk), "MediaLink", xmlAttribute);
				xmlAttributeOverride.Add(typeof(DataVirtualHardDisk), "SourceMediaLink", xmlAttribute);
				xmlAttributeOverride.Add(typeof(OSVirtualHardDisk), "MediaLink", xmlAttribute);
				xmlAttributeOverride.Add(typeof(OSVirtualHardDisk), "SourceImageName", xmlAttribute);
				Type[] typeArray = new Type[1];
				typeArray[0] = typeof(NetworkConfigurationSet);
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(PersistentVM), xmlAttributeOverride, typeArray, null, null);
				using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
				{
					persistentVM = xmlSerializer.Deserialize(fileStream) as PersistentVM;
				}
				return persistentVM;
			}
			else
			{
				throw new ArgumentException("The file to load the role does not exist", "filePath");
			}
		}

		public static void SaveStateToFile(PersistentVM role, string filePath)
		{
			if (role != null)
			{
				XmlAttributeOverrides xmlAttributeOverride = new XmlAttributeOverrides();
				XmlAttributes xmlAttribute = new XmlAttributes();
				xmlAttribute.XmlIgnore = true;
				xmlAttributeOverride.Add(typeof(DataVirtualHardDisk), "MediaLink", xmlAttribute);
				xmlAttributeOverride.Add(typeof(DataVirtualHardDisk), "SourceMediaLink", xmlAttribute);
				xmlAttributeOverride.Add(typeof(OSVirtualHardDisk), "MediaLink", xmlAttribute);
				Type[] typeArray = new Type[1];
				typeArray[0] = typeof(NetworkConfigurationSet);
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(PersistentVM), xmlAttributeOverride, typeArray, null, null);
				using (TextWriter streamWriter = new StreamWriter(filePath))
				{
					xmlSerializer.Serialize(streamWriter, role);
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("role", "Role cannot be null");
			}
		}
	}
}