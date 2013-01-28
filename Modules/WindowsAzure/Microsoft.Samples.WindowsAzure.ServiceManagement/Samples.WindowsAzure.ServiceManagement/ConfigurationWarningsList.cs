using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class ConfigurationWarningsList : List<ConfigurationWarning>
	{
		public ConfigurationWarningsList()
		{
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder(string.Format("ConfigurationWarnings({0}):\n", base.Count));
			foreach (ConfigurationWarning configurationWarning in this)
			{
				stringBuilder.Append(string.Concat(configurationWarning, "\n"));
			}
			return stringBuilder.ToString();
		}
	}
}