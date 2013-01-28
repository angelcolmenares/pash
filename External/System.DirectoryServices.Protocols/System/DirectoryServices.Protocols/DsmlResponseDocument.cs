using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class DsmlResponseDocument : DsmlDocument, ICollection, IEnumerable
	{
		private ArrayList dsmlResponse;

		private XmlDocument dsmlDocument;

		private XmlElement dsmlBatchResponse;

		private XmlNamespaceManager dsmlNS;

		public int Count
		{
			get
			{
				return this.dsmlResponse.Count;
			}
		}

		public bool IsErrorResponse
		{
			get
			{
				bool flag;
				IEnumerator enumerator = this.dsmlResponse.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						DirectoryResponse current = (DirectoryResponse)enumerator.Current;
						if (current as DsmlErrorResponse == null)
						{
							continue;
						}
						flag = true;
						return flag;
					}
					return false;
				}
				finally
				{
					IDisposable disposable = enumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
				return flag;
			}
		}

		public bool IsOperationError
		{
			get
			{
				bool flag;
				IEnumerator enumerator = this.dsmlResponse.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						DirectoryResponse current = (DirectoryResponse)enumerator.Current;
						if (current as DsmlErrorResponse != null)
						{
							continue;
						}
						ResultCode resultCode = current.ResultCode;
						if (resultCode == ResultCode.Success || ResultCode.CompareTrue == resultCode || ResultCode.CompareFalse == resultCode || ResultCode.Referral == resultCode || ResultCode.ReferralV2 == resultCode)
						{
							continue;
						}
						flag = true;
						return flag;
					}
					return false;
				}
				finally
				{
					IDisposable disposable = enumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
				return flag;
			}
		}

		protected bool IsSynchronized
		{
			get
			{
				return this.dsmlResponse.IsSynchronized;
			}
		}

		public DirectoryResponse this[int index]
		{
			get
			{
				return (DirectoryResponse)this.dsmlResponse[index];
			}
		}

		public string RequestId
		{
			get
			{
				XmlAttribute xmlAttribute = (XmlAttribute)this.dsmlBatchResponse.SelectSingleNode("@dsml:requestID", this.dsmlNS);
				if (xmlAttribute != null)
				{
					return xmlAttribute.Value;
				}
				else
				{
					xmlAttribute = (XmlAttribute)this.dsmlBatchResponse.SelectSingleNode("@requestID", this.dsmlNS);
					if (xmlAttribute != null)
					{
						return xmlAttribute.Value;
					}
					else
					{
						return null;
					}
				}
			}
		}

		internal string ResponseString
		{
			get
			{
				if (this.dsmlDocument == null)
				{
					return null;
				}
				else
				{
					return this.dsmlDocument.InnerXml;
				}
			}
		}

		protected object SyncRoot
		{
			get
			{
				return this.dsmlResponse.SyncRoot;
			}
		}

		int System.Collections.ICollection.Count
		{
			get
			{
				return this.dsmlResponse.Count;
			}
		}

		bool System.Collections.ICollection.IsSynchronized
		{
			get
			{
				return this.dsmlResponse.IsSynchronized;
			}
		}

		object System.Collections.ICollection.SyncRoot
		{
			get
			{
				return this.dsmlResponse.SyncRoot;
			}
		}

		private DsmlResponseDocument()
		{
			this.dsmlResponse = new ArrayList();
		}

		internal DsmlResponseDocument(HttpWebResponse resp, string xpathToResponse) : this()
		{
			Stream responseStream = resp.GetResponseStream();
			StreamReader streamReader = new StreamReader(responseStream);
			try
			{
				this.dsmlDocument = new XmlDocument();
				try
				{
					this.dsmlDocument.Load(streamReader);
				}
				catch (XmlException xmlException)
				{
					throw new DsmlInvalidDocumentException(Res.GetString("NotWellFormedResponse"));
				}
				this.dsmlNS = NamespaceUtils.GetDsmlNamespaceManager();
				this.dsmlBatchResponse = (XmlElement)this.dsmlDocument.SelectSingleNode(xpathToResponse, this.dsmlNS);
				if (this.dsmlBatchResponse != null)
				{
					XmlNodeList childNodes = this.dsmlBatchResponse.ChildNodes;
					foreach (XmlNode childNode in childNodes)
					{
						if (childNode.NodeType != XmlNodeType.Element)
						{
							continue;
						}
						DirectoryResponse directoryResponse = this.ConstructElement((XmlElement)childNode);
						this.dsmlResponse.Add(directoryResponse);
					}
				}
				else
				{
					throw new DsmlInvalidDocumentException(Res.GetString("NotWellFormedResponse"));
				}
			}
			finally
			{
				streamReader.Close();
			}
		}

		internal DsmlResponseDocument(StringBuilder responseString, string xpathToResponse) : this()
		{
			this.dsmlDocument = new XmlDocument();
			try
			{
				this.dsmlDocument.LoadXml(responseString.ToString());
			}
			catch (XmlException xmlException)
			{
				throw new DsmlInvalidDocumentException(Res.GetString("NotWellFormedResponse"));
			}
			this.dsmlNS = NamespaceUtils.GetDsmlNamespaceManager();
			this.dsmlBatchResponse = (XmlElement)this.dsmlDocument.SelectSingleNode(xpathToResponse, this.dsmlNS);
			if (this.dsmlBatchResponse != null)
			{
				XmlNodeList childNodes = this.dsmlBatchResponse.ChildNodes;
				foreach (XmlNode childNode in childNodes)
				{
					if (childNode.NodeType != XmlNodeType.Element)
					{
						continue;
					}
					DirectoryResponse directoryResponse = this.ConstructElement((XmlElement)childNode);
					this.dsmlResponse.Add(directoryResponse);
				}
				return;
			}
			else
			{
				throw new DsmlInvalidDocumentException(Res.GetString("NotWellFormedResponse"));
			}
		}

		private DsmlResponseDocument(string responseString) : this(new StringBuilder(responseString), "se:Envelope/se:Body/dsml:batchResponse")
		{
		}

		private DirectoryResponse ConstructElement(XmlElement node)
		{
			DirectoryResponse dsmlErrorResponse = null;
			string localName = node.LocalName;
			string str = localName;
			if (localName != null)
			{
				if (str == "errorResponse")
				{
					dsmlErrorResponse = new DsmlErrorResponse(node);
				}
				else if (str == "searchResponse")
				{
					dsmlErrorResponse = new SearchResponse(node);
				}
				else if (str == "modifyResponse")
				{
					dsmlErrorResponse = new ModifyResponse(node);
				}
				else if (str == "addResponse")
				{
					dsmlErrorResponse = new AddResponse(node);
				}
				else if (str == "delResponse")
				{
					dsmlErrorResponse = new DeleteResponse(node);
				}
				else if (str == "modDNResponse")
				{
					dsmlErrorResponse = new ModifyDNResponse(node);
				}
				else if (str == "compareResponse")
				{
					dsmlErrorResponse = new CompareResponse(node);
				}
				else if (str == "extendedResponse")
				{
					dsmlErrorResponse = new ExtendedResponse(node);
				}
				else if (str == "authResponse")
				{
					dsmlErrorResponse = new DsmlAuthResponse(node);
				}
				else
				{
					throw new DsmlInvalidDocumentException(Res.GetString("UnknownResponseElement"));
				}
				return dsmlErrorResponse;
			}
			throw new DsmlInvalidDocumentException(Res.GetString("UnknownResponseElement"));
		}

		public void CopyTo(DirectoryResponse[] value, int i)
		{
			this.dsmlResponse.CopyTo(value, i);
		}

		public IEnumerator GetEnumerator()
		{
			return this.dsmlResponse.GetEnumerator();
		}

		void System.Collections.ICollection.CopyTo(Array value, int i)
		{
			this.dsmlResponse.CopyTo(value, i);
		}

		public override XmlDocument ToXml()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(this.dsmlBatchResponse.OuterXml);
			return xmlDocument;
		}
	}
}