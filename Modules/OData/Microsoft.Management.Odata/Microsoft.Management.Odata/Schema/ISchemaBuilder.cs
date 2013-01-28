using Microsoft.Management.Odata.Core;
using System;

namespace Microsoft.Management.Odata.Schema
{
	internal interface ISchemaBuilder
	{
		void Build(Schema logicalSchema, Schema userSchema, UserContext userContext, string membershipId);
	}
}