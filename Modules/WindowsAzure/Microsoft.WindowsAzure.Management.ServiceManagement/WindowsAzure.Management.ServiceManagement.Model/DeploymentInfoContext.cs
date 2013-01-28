using Microsoft.Samples.WindowsAzure.ServiceManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class DeploymentInfoContext : ServiceOperationContext
	{
		private readonly XNamespace ns;

		public string Configuration
		{
			get;
			protected set;
		}

		public int CurrentUpgradeDomain
		{
			get;
			set;
		}

		public string CurrentUpgradeDomainState
		{
			get;
			set;
		}

		public string DeploymentId
		{
			get;
			protected set;
		}

		public string DeploymentName
		{
			get;
			protected set;
		}

		public DnsSettings DnsSettings
		{
			get;
			protected set;
		}

		public string Label
		{
			get;
			protected set;
		}

		public string Name
		{
			get;
			protected set;
		}

		public string OSVersion
		{
			get;
			set;
		}

		public IList<RoleInstance> RoleInstanceList
		{
			get;
			protected set;
		}

		public IDictionary<string, RoleConfiguration> RolesConfiguration
		{
			get;
			protected set;
		}

		public bool? RollbackAllowed
		{
			get;
			protected set;
		}

		public string SdkVersion
		{
			get;
			protected set;
		}

		public string Slot
		{
			get;
			protected set;
		}

		public string Status
		{
			get;
			protected set;
		}

		public string UpgradeType
		{
			get;
			set;
		}

		public Uri Url
		{
			get;
			protected set;
		}

		public string VNetName
		{
			get;
			protected set;
		}

		public DeploymentInfoContext(Deployment deployment)
		{
			XDocument xDocument;
			string empty;
			string str;
			string value;
			this.ns = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration";
			this.Slot = deployment.DeploymentSlot;
			this.Name = deployment.Name;
			this.DeploymentName = deployment.Name;
			this.Url = deployment.Url;
			this.Status = deployment.Status;
			this.DeploymentId = deployment.PrivateID;
			this.VNetName = deployment.VirtualNetworkName;
			this.SdkVersion = deployment.SdkVersion;
			this.DnsSettings = deployment.Dns;
			bool? rollbackAllowed = deployment.RollbackAllowed;
			if (rollbackAllowed.HasValue)
			{
				this.RollbackAllowed = deployment.RollbackAllowed;
			}
			if (deployment.UpgradeStatus != null)
			{
				this.CurrentUpgradeDomain = deployment.UpgradeStatus.CurrentUpgradeDomain;
				this.CurrentUpgradeDomainState = deployment.UpgradeStatus.CurrentUpgradeDomainState;
				this.UpgradeType = deployment.UpgradeStatus.UpgradeType;
			}
			DeploymentInfoContext deploymentInfoContext = this;
			if (string.IsNullOrEmpty(deployment.Configuration))
			{
				empty = string.Empty;
			}
			else
			{
				empty = ServiceManagementHelper.DecodeFromBase64String(deployment.Configuration);
			}
			deploymentInfoContext.Configuration = empty;
			DeploymentInfoContext deploymentInfoContext1 = this;
			if (string.IsNullOrEmpty(deployment.Label))
			{
				str = string.Empty;
			}
			else
			{
				str = ServiceManagementHelper.DecodeFromBase64String(deployment.Label);
			}
			deploymentInfoContext1.Label = str;
			if (deployment.RoleInstanceList != null)
			{
				this.RoleInstanceList = new List<RoleInstance>();
				foreach (RoleInstance roleInstanceList in deployment.RoleInstanceList)
				{
					this.RoleInstanceList.Add(roleInstanceList);
				}
			}
			if (!string.IsNullOrEmpty(deployment.Configuration))
			{
				string configuration = this.Configuration;
				using (StringReader stringReader = new StringReader(configuration))
				{
					XmlReader xmlReader = XmlReader.Create(stringReader);
					xDocument = XDocument.Load(xmlReader);
				}
				DeploymentInfoContext deploymentInfoContext2 = this;
				if (xDocument.Root.Attribute("osVersion") != null)
				{
					value = xDocument.Root.Attribute("osVersion").Value;
				}
				else
				{
					value = string.Empty;
				}
				deploymentInfoContext2.OSVersion = value;
				this.RolesConfiguration = new Dictionary<string, RoleConfiguration>();
				IEnumerable<XElement> xElements = xDocument.Root.Descendants(this.ns + "Role");
				foreach (XElement xElement in xElements)
				{
					this.RolesConfiguration.Add(xElement.Attribute("name").Value, new RoleConfiguration(xElement));
				}
			}
		}

		public XDocument SerializeRolesConfiguration()
		{
			XDocument xDocument = new XDocument();
			XElement xElement = new XElement(this.ns + "ServiceConfiguration");
			xDocument.Add(xElement);
			xElement.SetAttributeValue("serviceName", base.ServiceName);
			xElement.SetAttributeValue("osVersion", this.OSVersion);
			xElement.SetAttributeValue("xmlns", this.ns.ToString());
			foreach (KeyValuePair<string, RoleConfiguration> rolesConfiguration in this.RolesConfiguration)
			{
				xElement.Add(rolesConfiguration.Value.Serialize());
			}
			return xDocument;
		}
	}
}