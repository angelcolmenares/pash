using Microsoft.Samples.WindowsAzure.ServiceManagement;
using System;
using System.IO;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Helpers
{
	public static class Utility
	{
		public static string GetConfiguration(string configurationPath)
		{
			string str = string.Join(string.Empty, File.ReadAllLines(configurationPath));
			return ServiceManagementHelper.EncodeToBase64String(str);
		}
	}
}