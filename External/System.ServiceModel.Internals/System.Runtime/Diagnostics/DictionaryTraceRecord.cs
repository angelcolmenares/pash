using System;
using System.Collections;
using System.Runtime;
using System.Xml;

namespace System.Runtime.Diagnostics
{
	internal class DictionaryTraceRecord : TraceRecord
	{
		private IDictionary dictionary;

		internal override string EventId
		{
			get
			{
				return "http://schemas.microsoft.com/2006/08/ServiceModel/DictionaryTraceRecord";
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal DictionaryTraceRecord(IDictionary dictionary)
		{
			this.dictionary = dictionary;
		}

		internal override void WriteTo(XmlWriter xml)
		{
			string empty;
			if (this.dictionary != null)
			{
				foreach (object key in this.dictionary.Keys)
				{
					object item = this.dictionary[key];
					XmlWriter xmlWriter = xml;
					string str = key.ToString();
					if (item == null)
					{
						empty = string.Empty;
					}
					else
					{
						empty = item.ToString();
					}
					xmlWriter.WriteElementString(str, empty);
				}
			}
		}
	}
}