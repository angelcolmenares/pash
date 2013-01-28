using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Common
{
	internal static class XmlElementValueExtensions
	{
		internal static IEnumerable<XmlElementValue<T>> OfResultType<T>(this IEnumerable<XmlElementValue> elements)
		where T : class
		{
			foreach (XmlElementValue xmlElementValue in elements)
			{
				XmlElementValue<T> xmlElementValue1 = xmlElementValue as XmlElementValue<T>;
				if (xmlElementValue1 == null)
				{
					if (xmlElementValue.UntypedValue as T == null)
					{
						continue;
					}
					yield return new XmlElementValue<T>(xmlElementValue.Name, xmlElementValue.Location, xmlElementValue.ValueAs<T>());
				}
				else
				{
					yield return xmlElementValue1;
				}
			}
		}

		internal static IEnumerable<XmlTextValue> OfText(this IEnumerable<XmlElementValue> elements)
		{
			foreach (XmlElementValue xmlElementValue in elements)
			{
				if (!xmlElementValue.IsText)
				{
					continue;
				}
				yield return (XmlTextValue)xmlElementValue;
			}
		}

		internal static IEnumerable<T> ValuesOfType<T>(this IEnumerable<XmlElementValue> elements)
		where T : class
		{
			return (from ev in elements.OfResultType<T>() select ev.Value);
		}
	}
}