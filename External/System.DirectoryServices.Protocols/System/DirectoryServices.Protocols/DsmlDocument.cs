using System;
using System.Runtime;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public abstract class DsmlDocument
	{
		internal string dsmlRequestID;

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected DsmlDocument()
		{
		}

		public abstract XmlDocument ToXml();
	}
}