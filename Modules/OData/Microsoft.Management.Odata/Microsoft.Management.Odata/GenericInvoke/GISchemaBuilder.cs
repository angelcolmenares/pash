using Microsoft.Management.Odata.Core;
using Microsoft.Management.Odata.Schema;
using System;
using System.Data.Services.Providers;

namespace Microsoft.Management.Odata.GenericInvoke
{
	internal class GISchemaBuilder : ISchemaBuilder
	{
		public GISchemaBuilder()
		{
		}

		public void Build(Microsoft.Management.Odata.Schema.Schema logicalSchema, Microsoft.Management.Odata.Schema.Schema userSchema, UserContext userContext, string membershipId)
		{
			ResourceSet resourceSet = null;
			if (logicalSchema.ResourceSets.TryGetValue("CommandInvocations", out resourceSet))
			{
				string str = "PowerShell.CommandInvocation";
				userSchema.AddEntity(str, true, logicalSchema);
				userSchema.EntityMetadataDictionary.Add(str, logicalSchema.EntityMetadataDictionary[str]);
				userSchema.PopulateAllRelevantResourceTypes(logicalSchema);
			}
		}
	}
}