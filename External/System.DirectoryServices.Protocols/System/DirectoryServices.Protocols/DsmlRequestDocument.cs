using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class DsmlRequestDocument : DsmlDocument, IList, ICollection, IEnumerable
	{
		private DsmlDocumentProcessing docProcessing;

		private DsmlResponseOrder resOrder;

		private DsmlErrorProcessing errProcessing;

		private ArrayList dsmlRequests;

		public int Count
		{
			get
			{
				return this.dsmlRequests.Count;
			}
		}

		public DsmlDocumentProcessing DocumentProcessing
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.docProcessing;
			}
			set
			{
				if (value < DsmlDocumentProcessing.Sequential || value > DsmlDocumentProcessing.Parallel)
				{
					throw new InvalidEnumArgumentException("value", (int)value, typeof(DsmlDocumentProcessing));
				}
				else
				{
					this.docProcessing = value;
					return;
				}
			}
		}

		public DsmlErrorProcessing ErrorProcessing
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.errProcessing;
			}
			set
			{
				if (value < DsmlErrorProcessing.Resume || value > DsmlErrorProcessing.Exit)
				{
					throw new InvalidEnumArgumentException("value", (int)value, typeof(DsmlErrorProcessing));
				}
				else
				{
					this.errProcessing = value;
					return;
				}
			}
		}

		protected bool IsFixedSize
		{
			get
			{
				return false;
			}
		}

		protected bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		protected bool IsSynchronized
		{
			get
			{
				return this.dsmlRequests.IsSynchronized;
			}
		}

		public DirectoryRequest this[int index]
		{
			get
			{
				return (DirectoryRequest)this.dsmlRequests[index];
			}
			set
			{
				if (value != null)
				{
					this.dsmlRequests[index] = value;
					return;
				}
				else
				{
					throw new ArgumentNullException("value");
				}
			}
		}

		public string RequestId
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.dsmlRequestID;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.dsmlRequestID = value;
			}
		}

		public DsmlResponseOrder ResponseOrder
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.resOrder;
			}
			set
			{
				if (value < DsmlResponseOrder.Sequential || value > DsmlResponseOrder.Unordered)
				{
					throw new InvalidEnumArgumentException("value", (int)value, typeof(DsmlResponseOrder));
				}
				else
				{
					this.resOrder = value;
					return;
				}
			}
		}

		protected object SyncRoot
		{
			get
			{
				return this.dsmlRequests.SyncRoot;
			}
		}

		int System.Collections.ICollection.Count
		{
			get
			{
				return this.dsmlRequests.Count;
			}
		}

		bool System.Collections.ICollection.IsSynchronized
		{
			get
			{
				return this.dsmlRequests.IsSynchronized;
			}
		}

		object System.Collections.ICollection.SyncRoot
		{
			get
			{
				return this.dsmlRequests.SyncRoot;
			}
		}

		bool System.Collections.IList.IsFixedSize
		{
			get
			{
				return false;
			}
		}

		bool System.Collections.IList.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		object System.Collections.IList.this[int index]
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this[index];
			}
			set
			{
				if (value != null)
				{
					if (value as DirectoryRequest != null)
					{
						this.dsmlRequests[index] = (DirectoryRequest)value;
						return;
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = "DirectoryRequest";
						throw new ArgumentException(Res.GetString("InvalidValueType", objArray), "value");
					}
				}
				else
				{
					throw new ArgumentNullException("value");
				}
			}
		}

		public DsmlRequestDocument()
		{
			this.errProcessing = DsmlErrorProcessing.Exit;
			Utility.CheckOSVersion();
			this.dsmlRequests = new ArrayList();
		}

		public int Add(DirectoryRequest request)
		{
			if (request != null)
			{
				return this.dsmlRequests.Add(request);
			}
			else
			{
				throw new ArgumentNullException("request");
			}
		}

		public void Clear()
		{
			this.dsmlRequests.Clear();
		}

		public bool Contains(DirectoryRequest value)
		{
			return this.dsmlRequests.Contains(value);
		}

		public void CopyTo(DirectoryRequest[] value, int i)
		{
			this.dsmlRequests.CopyTo(value, i);
		}

		public IEnumerator GetEnumerator()
		{
			return this.dsmlRequests.GetEnumerator();
		}

		public int IndexOf(DirectoryRequest value)
		{
			if (value != null)
			{
				return this.dsmlRequests.IndexOf(value);
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		public void Insert(int index, DirectoryRequest value)
		{
			if (value != null)
			{
				this.dsmlRequests.Insert(index, value);
				return;
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		public void Remove(DirectoryRequest value)
		{
			if (value != null)
			{
				this.dsmlRequests.Remove(value);
				return;
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		public void RemoveAt(int index)
		{
			this.dsmlRequests.RemoveAt(index);
		}

		private void StartBatchRequest(XmlDocument xmldoc)
		{
			string str = "<batchRequest xmlns=\"urn:oasis:names:tc:DSML:2:0:core\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" />";
			xmldoc.LoadXml(str);
			XmlAttribute xmlAttribute = xmldoc.CreateAttribute("processing", null);
			DsmlDocumentProcessing dsmlDocumentProcessing = this.docProcessing;
			switch (dsmlDocumentProcessing)
			{
				case DsmlDocumentProcessing.Sequential:
				{
					xmlAttribute.InnerText = "sequential";
					break;
				}
				case DsmlDocumentProcessing.Parallel:
				{
					xmlAttribute.InnerText = "parallel";
					break;
				}
			}
			xmldoc.DocumentElement.Attributes.Append(xmlAttribute);
			xmlAttribute = xmldoc.CreateAttribute("responseOrder", null);
			DsmlResponseOrder dsmlResponseOrder = this.resOrder;
			switch (dsmlResponseOrder)
			{
				case DsmlResponseOrder.Sequential:
				{
					xmlAttribute.InnerText = "sequential";
					break;
				}
				case DsmlResponseOrder.Unordered:
				{
					xmlAttribute.InnerText = "unordered";
					break;
				}
			}
			xmldoc.DocumentElement.Attributes.Append(xmlAttribute);
			xmlAttribute = xmldoc.CreateAttribute("onError", null);
			DsmlErrorProcessing dsmlErrorProcessing = this.errProcessing;
			switch (dsmlErrorProcessing)
			{
				case DsmlErrorProcessing.Resume:
				{
					xmlAttribute.InnerText = "resume";
					break;
				}
				case DsmlErrorProcessing.Exit:
				{
					xmlAttribute.InnerText = "exit";
					break;
				}
			}
			xmldoc.DocumentElement.Attributes.Append(xmlAttribute);
			if (this.dsmlRequestID != null)
			{
				xmlAttribute = xmldoc.CreateAttribute("requestID", null);
				xmlAttribute.InnerText = this.dsmlRequestID;
				xmldoc.DocumentElement.Attributes.Append(xmlAttribute);
			}
		}

		void System.Collections.ICollection.CopyTo(Array value, int i)
		{
			this.dsmlRequests.CopyTo(value, i);
		}

		int System.Collections.IList.Add(object request)
		{
			if (request != null)
			{
				if (request as DirectoryRequest != null)
				{
					return this.Add((DirectoryRequest)request);
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = "DirectoryRequest";
					throw new ArgumentException(Res.GetString("InvalidValueType", objArray), "request");
				}
			}
			else
			{
				throw new ArgumentNullException("request");
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		void System.Collections.IList.Clear()
		{
			this.Clear();
		}

		bool System.Collections.IList.Contains(object value)
		{
			return this.Contains((DirectoryRequest)value);
		}

		int System.Collections.IList.IndexOf(object value)
		{
			if (value != null)
			{
				return this.IndexOf((DirectoryRequest)value);
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		void System.Collections.IList.Insert(int index, object value)
		{
			if (value != null)
			{
				if (value as DirectoryRequest != null)
				{
					this.Insert(index, (DirectoryRequest)value);
					return;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = "DirectoryRequest";
					throw new ArgumentException(Res.GetString("InvalidValueType", objArray), "value");
				}
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		void System.Collections.IList.Remove(object value)
		{
			if (value != null)
			{
				this.Remove((DirectoryRequest)value);
				return;
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		void System.Collections.IList.RemoveAt(int index)
		{
			this.RemoveAt(index);
		}

		public override XmlDocument ToXml()
		{
			XmlDocument xmlDocument = new XmlDocument();
			this.StartBatchRequest(xmlDocument);
			foreach (DirectoryRequest dsmlRequest in this.dsmlRequests)
			{
				xmlDocument.DocumentElement.AppendChild(dsmlRequest.ToXmlNodeHelper(xmlDocument));
			}
			return xmlDocument;
		}
	}
}