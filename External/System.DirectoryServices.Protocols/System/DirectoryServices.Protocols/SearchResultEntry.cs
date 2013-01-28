using System;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class SearchResultEntry
	{
		private XmlNode dsmlNode;

		private XmlNamespaceManager dsmlNS;

		private bool dsmlRequest;

		private string distinguishedName;

		private SearchResultAttributeCollection attributes;

		private DirectoryControl[] resultControls;

		public SearchResultAttributeCollection Attributes
		{
			get
			{
				if (this.dsmlRequest && this.attributes.Count == 0)
				{
					this.attributes = this.AttributesHelper();
				}
				return this.attributes;
			}
		}

		public DirectoryControl[] Controls
		{
			get
			{
				if (this.dsmlRequest && this.resultControls == null)
				{
					this.resultControls = this.ControlsHelper();
				}
				if (this.resultControls != null)
				{
					DirectoryControl[] directoryControl = new DirectoryControl[(int)this.resultControls.Length];
					for (int i = 0; i < (int)this.resultControls.Length; i++)
					{
						directoryControl[i] = new DirectoryControl(this.resultControls[i].Type, this.resultControls[i].GetValue(), this.resultControls[i].IsCritical, this.resultControls[i].ServerSide);
					}
					DirectoryControl.TransformControls(directoryControl);
					return directoryControl;
				}
				else
				{
					return new DirectoryControl[0];
				}
			}
		}

		public string DistinguishedName
		{
			get
			{
				if (this.dsmlRequest && this.distinguishedName == null)
				{
					this.distinguishedName = this.DNHelper("@dsml:dn", "@dn");
				}
				return this.distinguishedName;
			}
		}

		internal SearchResultEntry(XmlNode node)
		{
			this.attributes = new SearchResultAttributeCollection();
			this.dsmlNode = node;
			this.dsmlNS = NamespaceUtils.GetDsmlNamespaceManager();
			this.dsmlRequest = true;
		}

		internal SearchResultEntry(string dn, SearchResultAttributeCollection attrs)
		{
			this.attributes = new SearchResultAttributeCollection();
			this.distinguishedName = dn;
			this.attributes = attrs;
		}

		internal SearchResultEntry(string dn)
		{
			this.attributes = new SearchResultAttributeCollection();
			this.distinguishedName = dn;
		}

		private SearchResultAttributeCollection AttributesHelper()
		{
			SearchResultAttributeCollection searchResultAttributeCollection = new SearchResultAttributeCollection();
			XmlNodeList xmlNodeLists = this.dsmlNode.SelectNodes("dsml:attr", this.dsmlNS);
			if (xmlNodeLists.Count != 0)
			{
				foreach (XmlNode xmlNodes in xmlNodeLists)
				{
					DirectoryAttribute directoryAttribute = new DirectoryAttribute((XmlElement)xmlNodes);
					searchResultAttributeCollection.Add(directoryAttribute.Name, directoryAttribute);
				}
			}
			return searchResultAttributeCollection;
		}

		private DirectoryControl[] ControlsHelper()
		{
			XmlNodeList xmlNodeLists = this.dsmlNode.SelectNodes("dsml:control", this.dsmlNS);
			if (xmlNodeLists.Count != 0)
			{
				DirectoryControl[] directoryControl = new DirectoryControl[xmlNodeLists.Count];
				int num = 0;
				foreach (XmlNode xmlNodes in xmlNodeLists)
				{
					directoryControl[num] = new DirectoryControl((XmlElement)xmlNodes);
					num++;
				}
				return directoryControl;
			}
			else
			{
				return new DirectoryControl[0];
			}
		}

		private string DNHelper(string primaryXPath, string secondaryXPath)
		{
			XmlAttribute xmlAttribute = (XmlAttribute)this.dsmlNode.SelectSingleNode(primaryXPath, this.dsmlNS);
			if (xmlAttribute != null)
			{
				return xmlAttribute.Value;
			}
			else
			{
				xmlAttribute = (XmlAttribute)this.dsmlNode.SelectSingleNode(secondaryXPath, this.dsmlNS);
				if (xmlAttribute != null)
				{
					return xmlAttribute.Value;
				}
				else
				{
					throw new DsmlInvalidDocumentException(Res.GetString("MissingSearchResultEntryDN"));
				}
			}
		}
	}
}