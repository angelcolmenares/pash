using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Core;
using Microsoft.Management.Odata.PS;
using Microsoft.Management.Odata.Schema;
using System;
using System.Data.Services.Providers;

namespace Microsoft.Management.Odata.GenericInvoke
{
	internal class GICommandManager : ICommandManager
	{
		private ExclusiveItemStore<PSRunspace, UserContext> runspaceStore;

		public GICommandManager(ExclusiveItemStore<PSRunspace, UserContext> runspaceStore)
		{
			this.runspaceStore = runspaceStore;
		}

		public ICommand GetCommand(CommandType commandType, UserContext userContext, ResourceType entityType, EntityMetadata entityMetadata, string membershipId)
		{
			if (entityType.Name == "CommandInvocation")
			{
				CommandType commandType1 = commandType;
				switch (commandType1)
				{
					case CommandType.Create:
					case CommandType.Read:
					case CommandType.Delete:
					{
						return new GICommand(commandType, this.runspaceStore, entityType, userContext, membershipId);
					}
					case CommandType.Update:
					{
						throw new NotImplementedException();
					}
					default:
					{
						throw new NotImplementedException();
					}
				}
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		public IReferenceSetCommand GetReferenceSetCommand(CommandType commandType, UserContext userContext, ResourceProperty property, EntityMetadata entityMetadata, string membershipId, ResourceType resourceType)
		{
			throw new NotImplementedException();
		}
	}
}