using Microsoft.Data.Edm.Csdl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Common
{
	internal class XmlElementInfo : IXmlElementAttributes
	{
		private readonly Dictionary<string, XmlAttributeInfo> attributes;

		private List<XmlAnnotationInfo> annotations;

		internal IList<XmlAnnotationInfo> Annotations
		{
			get
			{
				List<XmlAnnotationInfo> xmlAnnotationInfos = this.annotations;
				IList<XmlAnnotationInfo> xmlAnnotationInfos1 = xmlAnnotationInfos;
				if (xmlAnnotationInfos == null)
				{
					xmlAnnotationInfos1 = (IList<XmlAnnotationInfo>)(new XmlAnnotationInfo[0]);
				}
				return xmlAnnotationInfos1;
			}
		}

		internal IXmlElementAttributes Attributes
		{
			get
			{
				return this;
			}
		}

		internal CsdlLocation Location
		{
			get;
			private set;
		}

		XmlAttributeInfo IXmlElementAttributes.this[string attributeName]
		{
			get
			{
				XmlAttributeInfo info;
				if ((this.attributes != null) && this.attributes.TryGetValue(attributeName, out info))
				{
					info.IsUsed = true;
					return info;
				}
				return XmlAttributeInfo.Missing;
			}
		}

		IEnumerable<XmlAttributeInfo> Microsoft.Data.Edm.Csdl.Internal.Parsing.Common.IXmlElementAttributes.Unused
		{
			get
			{
				if (this.attributes != null)
				{
					var values = this.attributes.Values;
					foreach (XmlAttributeInfo xmlAttributeInfo in values.Where<XmlAttributeInfo>((XmlAttributeInfo attr) => !attr.IsUsed))
					{
						yield return xmlAttributeInfo;
					}
				}
			}
		}

		internal string Name
		{
			get;
			private set;
		}

		internal XmlElementInfo(string elementName, CsdlLocation elementLocation, IList<XmlAttributeInfo> attributes, List<XmlAnnotationInfo> annotations)
		{
			this.Name = elementName;
			this.Location = elementLocation;
			if (attributes != null && attributes.Count > 0)
			{
				this.attributes = new Dictionary<string, XmlAttributeInfo>();
				foreach (XmlAttributeInfo attribute in attributes)
				{
					this.attributes.Add(attribute.Name, attribute);
				}
			}
			this.annotations = annotations;
		}

		internal void AddAnnotation(XmlAnnotationInfo annotation)
		{
			if (this.annotations == null)
			{
				this.annotations = new List<XmlAnnotationInfo>();
			}
			this.annotations.Add(annotation);
		}
	}
}