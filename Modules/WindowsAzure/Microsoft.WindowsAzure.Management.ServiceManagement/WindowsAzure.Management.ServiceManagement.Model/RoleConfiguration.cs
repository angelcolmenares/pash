using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class RoleConfiguration
	{
		private readonly XNamespace ns;

		public Dictionary<string, CertificateConfiguration> Certificates
		{
			get;
			protected set;
		}

		public int InstanceCount
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public Dictionary<string, string> Settings
		{
			get;
			protected set;
		}

		public RoleConfiguration(XElement data)
		{
			this.ns = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration";
			this.Name = data.Attribute("name").Value;
			this.InstanceCount = int.Parse(data.Element(this.ns + "Instances").Attribute("count").Value, CultureInfo.InvariantCulture);
			this.Settings = new Dictionary<string, string>();
			if (data.Element(this.ns + "ConfigurationSettings") != null)
			{
				foreach (XElement xElement in data.Element(this.ns + "ConfigurationSettings").Descendants())
				{
					this.Settings.Add(xElement.Attribute("name").Value, xElement.Attribute("value").Value);
				}
			}
			this.Certificates = new Dictionary<string, CertificateConfiguration>();
			if (data.Element(this.ns + "Certificates") != null)
			{
				foreach (XElement xElement1 in data.Element(this.ns + "Certificates").Descendants())
				{
					CertificateConfiguration certificateConfiguration = new CertificateConfiguration();
					certificateConfiguration.Thumbprint = xElement1.Attribute("thumbprint").Value;
					certificateConfiguration.ThumbprintAlgorithm = xElement1.Attribute("thumbprintAlgorithm").Value;
					CertificateConfiguration certificateConfiguration1 = certificateConfiguration;
					this.Certificates.Add(xElement1.Attribute("name").Value, certificateConfiguration1);
				}
			}
		}

		internal XElement Serialize()
		{
			XElement xElement = new XElement(this.ns + "Role");
			xElement.SetAttributeValue("name", this.Name);
			XElement xElement1 = new XElement(this.ns + "Instances");
			xElement1.SetAttributeValue("count", this.InstanceCount);
			xElement.Add(xElement1);
			XElement xElement2 = new XElement(this.ns + "ConfigurationSettings");
			xElement.Add(xElement2);
			foreach (KeyValuePair<string, string> setting in this.Settings)
			{
				XElement xElement3 = new XElement(this.ns + "Setting");
				xElement3.SetAttributeValue("name", setting.Key);
				xElement3.SetAttributeValue("value", setting.Value);
				xElement2.Add(xElement3);
			}
			XElement xElement4 = new XElement(this.ns + "Certificates");
			xElement.Add(xElement4);
			foreach (KeyValuePair<string, CertificateConfiguration> certificate in this.Certificates)
			{
				XElement xElement5 = new XElement(this.ns + "Certificate");
				xElement5.SetAttributeValue("name", certificate.Key);
				xElement5.SetAttributeValue("thumbprint", certificate.Value.Thumbprint);
				xElement5.SetAttributeValue("thumbprintAlgorithm", certificate.Value.ThumbprintAlgorithm);
				xElement4.Add(xElement5);
			}
			return xElement;
		}
	}
}