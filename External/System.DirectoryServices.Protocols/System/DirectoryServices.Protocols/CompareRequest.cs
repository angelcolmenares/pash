using System;
using System.Runtime;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class CompareRequest : DirectoryRequest
	{
		private string dn;

		private DirectoryAttribute attribute;

		public DirectoryAttribute Assertion
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.attribute;
			}
		}

		public string DistinguishedName
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.dn;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.dn = value;
			}
		}

		public CompareRequest()
		{
			this.attribute = new DirectoryAttribute();
		}

		public CompareRequest(string distinguishedName, string attributeName, string value)
		{
			this.attribute = new DirectoryAttribute();
			this.CompareRequestHelper(distinguishedName, attributeName, value);
		}

		public CompareRequest(string distinguishedName, string attributeName, byte[] value)
		{
			this.attribute = new DirectoryAttribute();
			this.CompareRequestHelper(distinguishedName, attributeName, value);
		}

		public CompareRequest(string distinguishedName, string attributeName, Uri value)
		{
			this.attribute = new DirectoryAttribute();
			this.CompareRequestHelper(distinguishedName, attributeName, value);
		}

		public CompareRequest(string distinguishedName, DirectoryAttribute assertion)
		{
			this.attribute = new DirectoryAttribute();
			if (assertion != null)
			{
				if (assertion.Count == 1)
				{
					this.CompareRequestHelper(distinguishedName, assertion.Name, assertion[0]);
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("WrongNumValuesCompare"));
				}
			}
			else
			{
				throw new ArgumentNullException("assertion");
			}
		}

		private void CompareRequestHelper(string distinguishedName, string attributeName, object value)
		{
			if (attributeName != null)
			{
				if (value != null)
				{
					this.dn = distinguishedName;
					this.attribute.Name = attributeName;
					this.attribute.Add(value);
					return;
				}
				else
				{
					throw new ArgumentNullException("value");
				}
			}
			else
			{
				throw new ArgumentNullException("attributeName");
			}
		}

		protected override XmlElement ToXmlNode(XmlDocument doc)
		{
			XmlElement xmlElement = base.CreateRequestElement(doc, "compareRequest", true, this.dn);
			if (this.attribute.Count == 1)
			{
				XmlElement xmlNode = this.attribute.ToXmlNode(doc, "assertion");
				xmlElement.AppendChild(xmlNode);
				return xmlElement;
			}
			else
			{
				throw new ArgumentException(Res.GetString("WrongNumValuesCompare"));
			}
		}
	}
}