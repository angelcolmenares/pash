using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Core;
using Microsoft.Management.Odata.Schema;
using Microsoft.Management.Odata.Tracing;
using System;
using System.Collections.Generic;
using System.Data.Services.Providers;
using System.Globalization;

namespace Microsoft.Management.Odata.PS
{
	internal class PSCommandManager : ICommandManager
	{
		private ExclusiveItemStore<PSRunspace, UserContext> runspaceStore;

		public PSCommandManager(ExclusiveItemStore<PSRunspace, UserContext> runspaceStore)
		{
			this.runspaceStore = runspaceStore;
		}

		public ICommand GetCommand(CommandType commandType, UserContext userContext, ResourceType entityType, EntityMetadata entityMetadata, string membershipId)
		{
			Envelope<PSRunspace, UserContext> envelope = this.runspaceStore.Borrow(userContext, membershipId);
			PSEntityMetadata pSEntityMetadatum = entityMetadata as PSEntityMetadata;
			object[] assemblyQualifiedName = new object[3];
			assemblyQualifiedName[0] = "entityMetadata";
			assemblyQualifiedName[1] = entityMetadata.GetType().AssemblyQualifiedName;
			assemblyQualifiedName[2] = typeof(PSEntityMetadata).AssemblyQualifiedName;
			ExceptionHelpers.ThrowArgumentExceptionIf("entityMetadata", pSEntityMetadatum == null, Resources.InvalidArgClrType, assemblyQualifiedName);
			object[] name = new object[2];
			name[0] = entityType.Name;
			name[1] = commandType.ToString();
			ExceptionHelpers.ThrowArgumentExceptionIf("entityMetadata", !pSEntityMetadatum.Cmdlets.ContainsKey(commandType), Resources.EntityDoesNotHaveCommand, name);
			return new PSCommand(envelope, entityType, pSEntityMetadatum.Cmdlets[commandType], commandType);
		}

		public IReferenceSetCommand GetReferenceSetCommand(CommandType commandType, UserContext userContext, ResourceProperty property, EntityMetadata entityMetadata, string membershipId, ResourceType resourceType)
		{
			Envelope<PSRunspace, UserContext> envelope = this.runspaceStore.Borrow(userContext, membershipId);
			PSReferenceSetCmdletInfo item = null;
			try
			{
				item = ((PSEntityMetadata)entityMetadata).CmdletsForReferenceSets[property.Name].Cmdlets[commandType];
			}
			catch (KeyNotFoundException keyNotFoundException)
			{
				Tracer tracer = new Tracer();
				tracer.DebugMessage(string.Concat("GetCommand cannot find commandType: ", commandType.ToString()));
				object[] fullName = new object[3];
				fullName[0] = property.ResourceType.FullName;
				fullName[1] = property.Name;
				fullName[2] = commandType.ToString();
				string str = string.Format(CultureInfo.CurrentCulture, Resources.PropertyDoesNotHaveCommand, fullName);
				throw new InvalidOperationException(str);
			}
			return new PSReferenceCommand(envelope, resourceType, item);
		}

		internal static bool IsReferenceCmdlet(CommandType type)
		{
			if (type == CommandType.AddReference || type == CommandType.RemoveReference)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}