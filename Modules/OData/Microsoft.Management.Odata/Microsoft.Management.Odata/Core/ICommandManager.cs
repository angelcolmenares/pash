using Microsoft.Management.Odata.Schema;
using System;
using System.Data.Services.Providers;

namespace Microsoft.Management.Odata.Core
{
	internal interface ICommandManager
	{
		ICommand GetCommand(CommandType commandType, UserContext userContext, ResourceType entityType, EntityMetadata entityMetadata, string membershipId);

		IReferenceSetCommand GetReferenceSetCommand(CommandType commandType, UserContext userContext, ResourceProperty property, EntityMetadata entityMetadata, string membershipId, ResourceType resourceType);
	}
}