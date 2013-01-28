using Microsoft.WindowsAzure.Management.Model;
using System;
using System.IO;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class VirtualNetworkSupportedDevicesContext : ManagementOperationContext
	{
		public string DeviceList
		{
			get;
			set;
		}

		public VirtualNetworkSupportedDevicesContext()
		{
		}

		public void ExportToFile(string filePath)
		{
			if (!string.IsNullOrEmpty(filePath))
			{
				if (Directory.Exists(Path.GetDirectoryName(filePath)))
				{
					using (StreamWriter streamWriter = new StreamWriter(filePath))
					{
						streamWriter.Write(this.DeviceList);
					}
					return;
				}
				else
				{
					throw new ArgumentException("The directory specified by the file path does not exist.", "filePath");
				}
			}
			else
			{
				throw new ArgumentNullException("filePath", "A file path should be specified.");
			}
		}
	}
}