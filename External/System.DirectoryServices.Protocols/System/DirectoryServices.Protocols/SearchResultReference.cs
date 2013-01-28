using System;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class SearchResultReference
	{
		private XmlNode dsmlNode;

		private XmlNamespaceManager dsmlNS;

		private bool dsmlRequest;

		private Uri[] resultReferences;

		private DirectoryControl[] resultControls;

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

		public Uri[] Reference
		{
			get
			{
				if (this.dsmlRequest && this.resultReferences == null)
				{
					this.resultReferences = this.UriHelper();
				}
				if (this.resultReferences != null)
				{
					Uri[] uri = new Uri[(int)this.resultReferences.Length];
					for (int i = 0; i < (int)this.resultReferences.Length; i++)
					{
						uri[i] = new Uri(this.resultReferences[i].AbsoluteUri);
					}
					return uri;
				}
				else
				{
					return new Uri[0];
				}
			}
		}

		internal SearchResultReference(XmlNode node)
		{
			this.dsmlNode = node;
			this.dsmlNS = NamespaceUtils.GetDsmlNamespaceManager();
			this.dsmlRequest = true;
		}

		internal SearchResultReference(Uri[] uris)
		{
			this.resultReferences = uris;
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

		private Uri[] UriHelper()
		{
			XmlNodeList xmlNodeLists = this.dsmlNode.SelectNodes("dsml:ref", this.dsmlNS);
			if (xmlNodeLists.Count != 0)
			{
				Uri[] uri = new Uri[xmlNodeLists.Count];
				int num = 0;
				foreach (XmlNode xmlNodes in xmlNodeLists)
				{
					uri[num] = new Uri(xmlNodes.InnerText);
					num++;
				}
				return uri;
			}
			else
			{
				return new Uri[0];
			}
		}
	}
}