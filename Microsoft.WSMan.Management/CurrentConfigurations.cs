using System;
using System.Xml;

namespace Microsoft.WSMan.Management
{
	internal class CurrentConfigurations
	{
		public const string DefaultNameSpacePrefix = "defaultNameSpace";

		private XmlDocument rootDocument;

		private XmlElement documentElement;

		private XmlNamespaceManager nameSpaceManger;

		private IWSManSession serverSession;

		public XmlDocument RootDocument
		{
			get
			{
				return this.rootDocument;
			}
		}

		public IWSManSession ServerSession
		{
			get
			{
				return this.serverSession;
			}
		}

		public CurrentConfigurations(IWSManSession serverSession)
		{
			if (serverSession != null)
			{
				this.rootDocument = new XmlDocument();
				this.serverSession = serverSession;
				return;
			}
			else
			{
				throw new ArgumentNullException("serverSession");
			}
		}

		public string GetOneConfiguration(string pathFromRoot)
		{
			if (pathFromRoot != null)
			{
				XmlNode xmlNodes = this.documentElement.SelectSingleNode(pathFromRoot, this.nameSpaceManger);
				if (xmlNodes == null)
				{
					return null;
				}
				else
				{
					return xmlNodes.Value;
				}
			}
			else
			{
				throw new ArgumentNullException("pathFromRoot");
			}
		}

		public void PutConfiguraitonOnServer(string resourceUri)
		{
			if (!string.IsNullOrEmpty(resourceUri))
			{
				this.serverSession.Put(resourceUri, this.rootDocument.InnerXml, 0);
				return;
			}
			else
			{
				throw new ArgumentNullException("resourceUri");
			}
		}

		public bool RefreshCurrentConfiguration(string responseOfGet)
		{
			if (!string.IsNullOrEmpty(responseOfGet))
			{
				this.rootDocument.LoadXml(responseOfGet);
				this.documentElement = this.rootDocument.DocumentElement;
				this.nameSpaceManger = new XmlNamespaceManager(this.rootDocument.NameTable);
				this.nameSpaceManger.AddNamespace("defaultNameSpace", this.documentElement.NamespaceURI);
				return string.IsNullOrEmpty(this.serverSession.Error);
			}
			else
			{
				throw new ArgumentNullException("responseOfGet");
			}
		}

		private void RemoveAttribute(XmlAttribute attributeToRemove)
		{
			XmlElement ownerElement = attributeToRemove.OwnerElement;
			ownerElement.RemoveAttribute(attributeToRemove.Name);
		}

		public void RemoveOneConfiguration(string pathToNodeFromRoot)
		{
			if (pathToNodeFromRoot != null)
			{
				XmlNode xmlNodes = this.documentElement.SelectSingleNode(pathToNodeFromRoot, this.nameSpaceManger);
				if (xmlNodes == null)
				{
					throw new ArgumentException("Node is not present in the XML, Please give valid XPath", "pathToNodeFromRoot");
				}
				else
				{
					if (xmlNodes as XmlAttribute == null)
					{
						return;
					}
					else
					{
						this.RemoveAttribute(xmlNodes as XmlAttribute);
						return;
					}
				}
			}
			else
			{
				throw new ArgumentNullException("pathToNodeFromRoot");
			}
		}

		public void UpdateOneConfiguration(string pathToNodeFromRoot, string configurationName, string configurationValue)
		{
			if (pathToNodeFromRoot != null)
			{
				if (!string.IsNullOrEmpty(configurationName))
				{
					if (configurationValue != null)
					{
						XmlNode xmlNodes = this.documentElement.SelectSingleNode(pathToNodeFromRoot, this.nameSpaceManger);
						if (xmlNodes != null)
						{
							foreach (XmlAttribute attribute in xmlNodes.Attributes)
							{
								if (!attribute.Name.Equals(configurationName, StringComparison.OrdinalIgnoreCase))
								{
									continue;
								}
								attribute.Value = configurationValue;
								return;
							}
							XmlNode xmlNodes1 = this.rootDocument.CreateNode(XmlNodeType.Attribute, configurationName, string.Empty);
							xmlNodes1.Value = configurationValue;
							xmlNodes.Attributes.SetNamedItem(xmlNodes1);
						}
						return;
					}
					else
					{
						throw new ArgumentNullException("configurationValue");
					}
				}
				else
				{
					throw new ArgumentNullException("configurationName");
				}
			}
			else
			{
				throw new ArgumentNullException("pathToNodeFromRoot");
			}
		}
	}
}