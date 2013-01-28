using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.WSE
{
	internal abstract class ADEnumerateRequest : AdwsRequestMsg
	{
		private TimeSpan? _expirationTimeSpan;

		private DateTime? _expirationDateTime;

		private IList<string> _attributes;

		private SortKey _sortKey;

		public override string Action
		{
			get
			{
				return "http://schemas.xmlsoap.org/ws/2004/09/enumeration/Enumerate";
			}
		}

		public virtual IList<string> Attributes
		{
			get
			{
				if (this._attributes == null)
				{
					this._attributes = new List<string>(0);
				}
				return this._attributes;
			}
		}

		protected DateTime? ExpirationDateTime
		{
			get
			{
				return this._expirationDateTime;
			}
			set
			{
				this._expirationDateTime = value;
			}
		}

		protected TimeSpan? ExpirationTimeSpan
		{
			get
			{
				return this._expirationTimeSpan;
			}
			set
			{
				this._expirationTimeSpan = value;
			}
		}

		protected abstract bool IsFilterPresent
		{
			get;
		}

		public virtual SortKey SortKey
		{
			get
			{
				return this._sortKey;
			}
			set
			{
				this._sortKey = value;
			}
		}

		protected ADEnumerateRequest(string instance) : base(instance)
		{
		}

		protected ADEnumerateRequest(string instance, IList<string> attributes) : this(instance)
		{
			this._attributes = attributes;
		}

		protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
		{
			string str;
			writer.WriteStartElement("Enumerate", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
			XmlUtility.SerializeExpires(writer, this._expirationDateTime, this._expirationTimeSpan);
			if (this.IsFilterPresent)
			{
				this.OnWriteStartFilterElement(writer);
				this.OnWriteFilterElementContents(writer);
				writer.WriteEndElement();
			}
			if (this._attributes != null && this._attributes.Count > 0)
			{
				writer.WriteStartElement("Selection", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
				writer.WriteAttributeString("Dialect", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/XPath-Level-1");
				XmlUtility.SerializeAttributeList(writer, "SelectionProperty", "http://schemas.microsoft.com/2008/1/ActiveDirectory", "ad", "addata", this._attributes);
				writer.WriteEndElement();
			}
			if (this._sortKey != null)
			{
				writer.WriteStartElement("Sorting", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
				writer.WriteAttributeString("Dialect", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/XPath-Level-1");
				writer.WriteStartElement("SortingProperty", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
				if (this._sortKey.ReverseOrder)
				{
					writer.WriteAttributeString("Ascending", "false");
				}
				if (AttributeNs.IsSynthetic(this._sortKey.AttributeName, SyntheticAttributeOperation.Read))
				{
					str = "ad";
				}
				else
				{
					str = "addata";
				}
				string str1 = str;
				writer.WriteValue(XmlUtility.AddPrefix(str1, this._sortKey.AttributeName));
				writer.WriteEndElement();
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		protected abstract void OnWriteFilterElementContents(XmlDictionaryWriter writer);

		protected override void OnWriteStartBody(XmlDictionaryWriter writer)
		{
			base.OnWriteStartBody(writer);
			writer.WriteXmlnsAttribute("wsen", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
			writer.WriteXmlnsAttribute("adlq", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/LdapQuery");
		}

		protected virtual void OnWriteStartFilterElement(XmlDictionaryWriter writer)
		{
			writer.WriteStartElement("Filter", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
		}

		public virtual void SetContextExpiration(DateTime expiration)
		{
			this._expirationDateTime = new DateTime?(expiration);
			this._expirationTimeSpan = null;
		}

		public virtual void SetContextExpiration(TimeSpan expiration)
		{
			this._expirationTimeSpan = new TimeSpan?(expiration);
			this._expirationDateTime = null;
		}
	}
}