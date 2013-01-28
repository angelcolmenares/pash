using System;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	internal class DSMLFilterWriter
	{
		public DSMLFilterWriter()
		{
		}

		protected void WriteAttrib(string attrName, ADAttribute attrib, XmlWriter mXmlWriter, string strNamespace)
		{
			if (strNamespace == null)
			{
				mXmlWriter.WriteStartElement(attrName);
			}
			else
			{
				mXmlWriter.WriteStartElement(attrName, strNamespace);
			}
			mXmlWriter.WriteAttributeString("name", attrib.Name);
			foreach (ADValue value in attrib.Values)
			{
				this.WriteValue("value", value, mXmlWriter, strNamespace);
			}
			mXmlWriter.WriteEndElement();
		}

		public void WriteFilter(ADFilter filter, bool filterTags, XmlWriter mXmlWriter, string strNamespace)
		{
			if (filterTags)
			{
				if (strNamespace == null)
				{
					mXmlWriter.WriteStartElement("filter");
				}
				else
				{
					mXmlWriter.WriteStartElement("filter", strNamespace);
				}
			}
			ADFilter.FilterType type = filter.Type;
			if (type == ADFilter.FilterType.And)
			{
				if (strNamespace == null)
				{
					mXmlWriter.WriteStartElement("and");
				}
				else
				{
					mXmlWriter.WriteStartElement("and", strNamespace);
				}
				foreach (object and in filter.Filter.And)
				{
					this.WriteFilter((ADFilter)and, false, mXmlWriter, strNamespace);
				}
				mXmlWriter.WriteEndElement();
			}
			else if (type == ADFilter.FilterType.Or)
			{
				if (strNamespace == null)
				{
					mXmlWriter.WriteStartElement("or");
				}
				else
				{
					mXmlWriter.WriteStartElement("or", strNamespace);
				}
				foreach (object or in filter.Filter.Or)
				{
					this.WriteFilter((ADFilter)or, false, mXmlWriter, strNamespace);
				}
				mXmlWriter.WriteEndElement();
			}
			else if (type == ADFilter.FilterType.Not)
			{
				if (strNamespace == null)
				{
					mXmlWriter.WriteStartElement("not");
				}
				else
				{
					mXmlWriter.WriteStartElement("not", strNamespace);
				}
				this.WriteFilter(filter.Filter.Not, false, mXmlWriter, strNamespace);
				mXmlWriter.WriteEndElement();
			}
			else if (type == ADFilter.FilterType.EqualityMatch)
			{
				this.WriteAttrib("equalityMatch", filter.Filter.EqualityMatch, mXmlWriter, strNamespace);
			}
			else if (type == ADFilter.FilterType.Substrings)
			{
				ADSubstringFilter substrings = filter.Filter.Substrings;
				if (strNamespace == null)
				{
					mXmlWriter.WriteStartElement("substrings");
				}
				else
				{
					mXmlWriter.WriteStartElement("substrings", strNamespace);
				}
				mXmlWriter.WriteAttributeString("name", substrings.Name);
				if (substrings.Initial != null)
				{
					this.WriteValue("initial", substrings.Initial, mXmlWriter, strNamespace);
				}
				if (substrings.Any != null)
				{
					foreach (object any in substrings.Any)
					{
						this.WriteValue("any", (ADValue)any, mXmlWriter, strNamespace);
					}
				}
				if (substrings.Final != null)
				{
					this.WriteValue("final", substrings.Final, mXmlWriter, strNamespace);
				}
				mXmlWriter.WriteEndElement();
			}
			else if (type == ADFilter.FilterType.GreaterOrEqual)
			{
				this.WriteAttrib("greaterOrEqual", filter.Filter.GreaterOrEqual, mXmlWriter, strNamespace);
			}
			else if (type == ADFilter.FilterType.LessOrEqual)
			{
				this.WriteAttrib("lessOrEqual", filter.Filter.LessOrEqual, mXmlWriter, strNamespace);
			}
			else if (type == ADFilter.FilterType.Present)
			{
				if (strNamespace == null)
				{
					mXmlWriter.WriteStartElement("present");
				}
				else
				{
					mXmlWriter.WriteStartElement("present", strNamespace);
				}
				mXmlWriter.WriteAttributeString("name", filter.Filter.Present);
				mXmlWriter.WriteEndElement();
			}
			else if (type == ADFilter.FilterType.ApproxMatch)
			{
				this.WriteAttrib("approxMatch", filter.Filter.ApproxMatch, mXmlWriter, strNamespace);
			}
			else if (type == ADFilter.FilterType.ExtensibleMatch)
			{
				ADExtenMatchFilter extensibleMatch = filter.Filter.ExtensibleMatch;
				if (strNamespace == null)
				{
					mXmlWriter.WriteStartElement("extensibleMatch");
				}
				else
				{
					mXmlWriter.WriteStartElement("extensibleMatch", strNamespace);
				}
				if (extensibleMatch.Name != null && extensibleMatch.Name.Length != 0)
				{
					mXmlWriter.WriteAttributeString("name", extensibleMatch.Name);
				}
				if (extensibleMatch.MatchingRule != null && extensibleMatch.MatchingRule.Length != 0)
				{
					mXmlWriter.WriteAttributeString("matchingRule", extensibleMatch.MatchingRule);
				}
				mXmlWriter.WriteAttributeString("dnAttributes", XmlConvert.ToString(extensibleMatch.DNAttributes));
				this.WriteValue("value", extensibleMatch.Value, mXmlWriter, strNamespace);
				mXmlWriter.WriteEndElement();
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = (object)filter.Type;
				throw new ArgumentException(Res.GetString("InvalidFilterType", objArray));
			}
			if (filterTags)
			{
				mXmlWriter.WriteEndElement();
			}
		}

		protected void WriteValue(string valueElt, ADValue value, XmlWriter mXmlWriter, string strNamespace)
		{
			if (strNamespace == null)
			{
				mXmlWriter.WriteStartElement(valueElt);
			}
			else
			{
				mXmlWriter.WriteStartElement(valueElt, strNamespace);
			}
			if (!value.IsBinary || value.BinaryVal == null)
			{
				mXmlWriter.WriteString(value.StringVal);
			}
			else
			{
				mXmlWriter.WriteAttributeString("xsi", "type", "http://www.w3.org/2001/XMLSchema-instance", "xsd:base64Binary");
				mXmlWriter.WriteBase64(value.BinaryVal, 0, (int)value.BinaryVal.Length);
			}
			mXmlWriter.WriteEndElement();
		}
	}
}