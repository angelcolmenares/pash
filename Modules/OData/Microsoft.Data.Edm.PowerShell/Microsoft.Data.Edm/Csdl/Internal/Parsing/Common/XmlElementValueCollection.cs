using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Common
{
	internal class XmlElementValueCollection : IEnumerable<XmlElementValue>, IEnumerable
	{
		private readonly static XmlElementValueCollection empty;

		private readonly IList<XmlElementValue> values;

		private ILookup<string, XmlElementValue> nameLookup;

		internal XmlTextValue FirstText
		{
			get
			{
				XmlTextValue xmlTextValue = this.values.OfText().FirstOrDefault<XmlTextValue>();
				XmlTextValue missing = xmlTextValue;
				if (xmlTextValue == null)
				{
					missing = XmlTextValue.Missing;
				}
				return missing;
			}
		}

		internal XmlElementValue this[string elementName]
		{
			get
			{
				XmlElementValue xmlElementValue = this.EnsureLookup()[elementName].FirstOrDefault<XmlElementValue>();
				XmlElementValue instance = xmlElementValue;
				if (xmlElementValue == null)
				{
					instance = XmlElementValueCollection.MissingXmlElementValue.Instance;
				}
				return instance;
			}
		}

		static XmlElementValueCollection()
		{
			XmlElementValue[] xmlElementValueArray = new XmlElementValue[0];
			XmlElementValue[] xmlElementValueArray1 = new XmlElementValue[0];
			XmlElementValueCollection.empty = new XmlElementValueCollection(xmlElementValueArray, xmlElementValueArray1.ToLookup<XmlElementValue, string>((XmlElementValue value) => value.Name));
		}

		private XmlElementValueCollection(IList<XmlElementValue> list, ILookup<string, XmlElementValue> nameMap)
		{
			this.values = list;
			this.nameLookup = nameMap;
		}

		private ILookup<string, XmlElementValue> EnsureLookup()
		{
			ILookup<string, XmlElementValue> strs = this.nameLookup;
			ILookup<string, XmlElementValue> strs1 = strs;
			if (strs == null)
			{
				XmlElementValueCollection xmlElementValueCollections = this;
				IList<XmlElementValue> xmlElementValues = this.values;
				ILookup<string, XmlElementValue> lookup = xmlElementValues.ToLookup<XmlElementValue, string>((XmlElementValue value) => value.Name);
				ILookup<string, XmlElementValue> strs2 = lookup;
				xmlElementValueCollections.nameLookup = lookup;
				strs1 = strs2;
			}
			return strs1;
		}

		internal IEnumerable<XmlElementValue> FindByName(string elementName)
		{
			return this.EnsureLookup()[elementName];
		}

		internal IEnumerable<XmlElementValue<TResult>> FindByName<TResult>(string elementName)
		where TResult : class
		{
			return this.FindByName(elementName).OfResultType<TResult>();
		}

		internal static XmlElementValueCollection FromList(IList<XmlElementValue> values)
		{
			if (values == null || values.Count == 0)
			{
				return XmlElementValueCollection.empty;
			}
			else
			{
				return new XmlElementValueCollection(values, null);
			}
		}

		public IEnumerator<XmlElementValue> GetEnumerator()
		{
			return this.values.GetEnumerator();
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.values.GetEnumerator();
		}

		internal sealed class MissingXmlElementValue : XmlElementValue
		{
			internal readonly static XmlElementValueCollection.MissingXmlElementValue Instance;

			internal override bool IsUsed
			{
				get
				{
					return false;
				}
			}

			internal override object UntypedValue
			{
				get
				{
					return null;
				}
			}

			static MissingXmlElementValue()
			{
				XmlElementValueCollection.MissingXmlElementValue.Instance = new XmlElementValueCollection.MissingXmlElementValue();
			}

			private MissingXmlElementValue() : base(null, null)
			{
			}
		}
	}
}