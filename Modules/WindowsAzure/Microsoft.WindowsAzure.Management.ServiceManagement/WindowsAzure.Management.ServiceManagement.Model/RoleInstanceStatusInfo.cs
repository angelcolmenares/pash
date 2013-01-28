using System;
using System.Globalization;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class RoleInstanceStatusInfo
	{
		public string InstanceName
		{
			get;
			set;
		}

		public string InstanceStatus
		{
			get;
			set;
		}

		public RoleInstanceStatusInfo()
		{
		}

		public override string ToString()
		{
			object[] instanceName = new object[2];
			instanceName[0] = this.InstanceName;
			instanceName[1] = this.InstanceStatus;
			return string.Format(CultureInfo.InvariantCulture, "Instance Name: {0} - Instance Status: {1}", instanceName);
		}
	}
}