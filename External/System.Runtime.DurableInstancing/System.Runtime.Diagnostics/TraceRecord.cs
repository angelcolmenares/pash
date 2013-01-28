using System;
using System.Runtime;
using System.Xml;

namespace System.Runtime.Diagnostics
{
	[Serializable]
	internal class TraceRecord
	{
		protected const string EventIdBase = "http://schemas.microsoft.com/2006/08/ServiceModel/";

		protected const string NamespaceSuffix = "TraceRecord";

		internal virtual string EventId
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.BuildEventId("Empty");
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public TraceRecord()
		{
		}

		protected string BuildEventId(string eventId)
		{
			return string.Concat("http://schemas.microsoft.com/2006/08/ServiceModel/", eventId, "TraceRecord");
		}

		internal virtual void WriteTo(XmlWriter writer)
		{
		}

		protected string XmlEncode(string text)
		{
			return DiagnosticTraceBase.XmlEncode(text);
		}
	}
}