using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.WSE
{
	internal class ADPullRequest : AdwsRequestMsg
	{
		private TimeSpan? _timeout;

		private uint? _maxElements;

		private string _enumerationContext;

		private IList<DirectoryControl> _controls;

		public override string Action
		{
			get
			{
				return "http://schemas.xmlsoap.org/ws/2004/09/enumeration/Pull";
			}
		}

		public virtual IList<DirectoryControl> Controls
		{
			get
			{
				if (this._controls == null)
				{
					this._controls = new List<DirectoryControl>();
				}
				return this._controls;
			}
		}

		public virtual uint? MaxElements
		{
			get
			{
				return this._maxElements;
			}
			set
			{
				bool hasValue;
				uint? nullable = value;
				if (nullable.GetValueOrDefault() != 0)
				{
					hasValue = false;
				}
				else
				{
					hasValue = nullable.HasValue;
				}
				if (!hasValue)
				{
					this._maxElements = value;
					return;
				}
				else
				{
					throw new ArgumentOutOfRangeException("value");
				}
			}
		}

		public ADPullRequest(string instance, string enumerationContext) : base(instance)
		{
			this._maxElements = null;
			if (!string.IsNullOrEmpty(enumerationContext))
			{
				this._enumerationContext = enumerationContext;
				return;
			}
			else
			{
				throw new ArgumentNullException("enumerationContext");
			}
		}

		public ADPullRequest(string instance, string enumerationContext, IList<DirectoryControl> controls) : this(instance, enumerationContext)
		{
			this._controls = controls;
		}

		public ADPullRequest(string instance, string enumerationContext, TimeSpan maxTime) : this(instance, enumerationContext)
		{
			this._timeout = new TimeSpan?(maxTime);
			this._enumerationContext = enumerationContext;
		}

		public ADPullRequest(string instance, string enumerationContext, TimeSpan maxTime, IList<DirectoryControl> controls) : this(instance, enumerationContext, maxTime)
		{
			this._controls = controls;
		}

		protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
		{
			writer.WriteStartElement("Pull", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
			if (this._enumerationContext != null)
			{
				writer.WriteElementString("EnumerationContext", "http://schemas.xmlsoap.org/ws/2004/09/enumeration", this._enumerationContext);
			}
			if (this._timeout.HasValue)
			{
				writer.WriteStartElement("MaxTime", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
				writer.WriteValue(this._timeout.Value);
				writer.WriteEndElement();
			}
			if (this._maxElements.HasValue)
			{
				writer.WriteStartElement("MaxElements", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
				writer.WriteValue(this._maxElements.Value);
				writer.WriteEndElement();
			}
			if (this._controls != null)
			{
				DirectoryControlSerializer.Serialize(writer, this._controls);
			}
			writer.WriteEndElement();
		}

		protected override void OnWriteStartBody(XmlDictionaryWriter writer)
		{
			base.OnWriteStartBody(writer);
			writer.WriteXmlnsAttribute("wsen", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
		}

		public virtual void SetTimeout(TimeSpan timeout)
		{
			this._timeout = new TimeSpan?(timeout);
		}
	}
}