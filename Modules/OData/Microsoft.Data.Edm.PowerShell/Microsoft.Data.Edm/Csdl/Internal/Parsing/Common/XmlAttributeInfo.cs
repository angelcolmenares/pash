using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Common
{
	internal class XmlAttributeInfo
	{
		internal readonly static XmlAttributeInfo Missing;

		private readonly string name;

		private readonly string attributeValue;

		private readonly CsdlLocation location;

		internal bool IsMissing
		{
			get
			{
				return object.ReferenceEquals(XmlAttributeInfo.Missing, this);
			}
		}

		internal bool IsUsed
		{
			get;
			set;
		}

		internal CsdlLocation Location
		{
			get
			{
				return this.location;
			}
		}

		internal string Name
		{
			get
			{
				return this.name;
			}
		}

		internal string Value
		{
			get
			{
				return this.attributeValue;
			}
		}

		static XmlAttributeInfo()
		{
			XmlAttributeInfo.Missing = new XmlAttributeInfo();
		}

		internal XmlAttributeInfo(string attrName, string attrValue, CsdlLocation attrLocation)
		{
			this.name = attrName;
			this.attributeValue = attrValue;
			this.location = attrLocation;
		}

		private XmlAttributeInfo()
		{
		}
	}
}