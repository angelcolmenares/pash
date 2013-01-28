using Microsoft.Management.Odata;
using System;
using System.Text;

namespace Microsoft.Management.Odata.Schema
{
	internal abstract class EntityMetadata
	{
		public ManagementSystemType MgmtSystem
		{
			get;
			private set;
		}

		protected EntityMetadata(ManagementSystemType mgmtSystem)
		{
			this.MgmtSystem = mgmtSystem;
		}

		public abstract StringBuilder ToTraceMessage(string entityName, StringBuilder builder);
	}
}