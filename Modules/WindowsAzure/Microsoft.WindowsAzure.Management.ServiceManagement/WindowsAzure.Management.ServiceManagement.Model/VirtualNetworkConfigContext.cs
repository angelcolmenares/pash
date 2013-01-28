using Microsoft.WindowsAzure.Management.Model;
using System;
using System.IO;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class VirtualNetworkConfigContext : ManagementOperationContext
	{
		public string XMLConfiguration
		{
			get;
			set;
		}

		public VirtualNetworkConfigContext()
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
						streamWriter.Write(this.XMLConfiguration);
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