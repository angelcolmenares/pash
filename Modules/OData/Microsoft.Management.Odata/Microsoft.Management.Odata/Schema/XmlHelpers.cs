using Microsoft.Management.Odata.MofParser.ParseTree;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Data.Entity.Core;

namespace Microsoft.Management.Odata.Schema
{
	internal static class XmlHelpers
	{
		public static Qualifier GetQualifier(this IQualifierTarget target, string qualifier)
		{
			return target.Qualifiers.FirstOrDefault<Qualifier>((Qualifier q) => string.Equals(q.Name, qualifier, StringComparison.OrdinalIgnoreCase));
		}

		public static XElement GetUniqueElement(this XContainer xmlContainer, XNamespace ns, string name)
		{
			XElement xElement = xmlContainer.TryGetUniqueElement(ns, name);
			if (xElement != null)
			{
				return xElement;
			}
			else
			{
				object[] objArray = new object[4];
				objArray[0] = "mandatory element ";
				objArray[1] = ns;
				objArray[2] = name;
				objArray[3] = " is not present.";
				throw new MetadataException(string.Concat(objArray));
			}
		}

		public static void InsertResourceMapping(this XDocument primaryMapping, XDocument secondaryMapping)
		{
			XElement uniqueElement = primaryMapping.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "Resources");
			XElement xElement = secondaryMapping.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "Resources");
			foreach (XElement xElement1 in xElement.Elements())
			{
				uniqueElement.AddFirst(xElement1);
			}
			uniqueElement = primaryMapping.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "ClassImplementations");
			xElement = secondaryMapping.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "ClassImplementations");
			foreach (XElement xElement2 in xElement.Elements())
			{
				uniqueElement.AddFirst(xElement2);
			}
		}

		public static bool IsReferenceType(this PropertyDeclaration property)
		{
			if (property.DataType.Type == DataTypeType.ObjectReference || property.DataType.Type == DataTypeType.ObjectReferenceArray)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static XElement TryGetUniqueElement(this XContainer xmlContainer, XNamespace ns, string name)
		{
			XElement xElement;
			XName xName = ns + name;
			IEnumerator<XElement> enumerator = xmlContainer.Descendants(xName).GetEnumerator();
			using (enumerator)
			{
				if (enumerator.MoveNext())
				{
					XElement current = enumerator.Current;
					xElement = current;
				}
				else
				{
					return null;
				}
			}
			return xElement;
		}
	}
}