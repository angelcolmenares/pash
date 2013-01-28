using Microsoft.Management.Odata.Schema;
using System;
using System.Text;

namespace Microsoft.Management.Odata.GenericInvoke
{
	internal class GIEntityMetadata : EntityMetadata
	{
		public GIEntityMetadata() : base((ManagementSystemType)1)
		{
		}

		public override StringBuilder ToTraceMessage(string entityName, StringBuilder builder)
		{
			builder.AppendLine(string.Concat("\n\tEntityMetadata for ", entityName));
			return builder;
		}
	}
}