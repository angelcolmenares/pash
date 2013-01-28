using System;
using System.Collections.Generic;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.WSE
{
	internal class ADEnumerateLdapRequest : ADEnumerateRequest
	{
		private string _filter;

		private string _searchBase;

		private string _searchScope;

		private string Filter
		{
			get
			{
				return this._filter;
			}
			set
			{
				this._filter = value;
			}
		}

		protected override bool IsFilterPresent
		{
			get
			{
				return this._filter != null;
			}
		}

		private string SearchBase
		{
			get
			{
				return this._searchBase;
			}
			set
			{
				this._searchBase = value;
			}
		}

		private string SearchScope
		{
			get
			{
				return this._searchScope;
			}
			set
			{
				this._searchScope = value;
			}
		}

		public ADEnumerateLdapRequest(string instance) : base(instance)
		{
		}

		public ADEnumerateLdapRequest(string instance, string filter, string searchBase, string searchScope) : this(instance)
		{
			this.Filter = filter;
			this.SearchBase = searchBase;
			this.SearchScope = searchScope;
		}

		public ADEnumerateLdapRequest(string instance, string filter, string searchBase, string searchScope, IList<string> attributes) : base(instance, attributes)
		{
			this.Filter = filter;
			this.SearchBase = searchBase;
			this.SearchScope = searchScope;
		}

		protected override void OnWriteFilterElementContents(XmlDictionaryWriter writer)
		{
			writer.WriteStartElement("LdapQuery", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/LdapQuery");
			writer.WriteElementString("Filter", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/LdapQuery", this.Filter);
			writer.WriteElementString("BaseObject", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/LdapQuery", this.SearchBase);
			writer.WriteElementString("Scope", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/LdapQuery", this.SearchScope);
			writer.WriteEndElement();
		}

		protected override void OnWriteStartFilterElement(XmlDictionaryWriter writer)
		{
			base.OnWriteStartFilterElement(writer);
			writer.WriteAttributeString("Dialect", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/LdapQuery");
			writer.WriteXmlnsAttribute("adlq", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/LdapQuery");
		}
	}
}