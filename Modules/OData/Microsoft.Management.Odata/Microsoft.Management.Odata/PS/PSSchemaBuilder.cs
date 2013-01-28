using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Core;
using Microsoft.Management.Odata.Schema;
using System;
using System.Collections.Generic;
using System.Data.Services.Providers;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace Microsoft.Management.Odata.PS
{
	internal class PSSchemaBuilder : ISchemaBuilder
	{
		private ExclusiveItemStore<PSRunspace, UserContext> runspaceStore;

		public PSSchemaBuilder(ExclusiveItemStore<PSRunspace, UserContext> runspaceStore)
		{
			this.runspaceStore = runspaceStore;
		}

		private void AddEntitiesToSchema(Microsoft.Management.Odata.Schema.Schema logicalSchema, Microsoft.Management.Odata.Schema.Schema userSchema, List<PSSchemaBuilder.EntityDataForSchemaBuilding> entityDataCollection, Dictionary<string, CommandInfo> sessionCmdlets)
		{
			foreach (PSSchemaBuilder.EntityDataForSchemaBuilding referenceSetCommand in entityDataCollection)
			{
				if (!referenceSetCommand.IncludeInSchema)
				{
					continue;
				}
				userSchema.AddEntity(referenceSetCommand.EntityName, referenceSetCommand.IncludeEntitySet, logicalSchema);
				PSEntityMetadata pSEntityMetadatum = new PSEntityMetadata();
				PSEntityMetadata item = (PSEntityMetadata)logicalSchema.EntityMetadataDictionary[referenceSetCommand.EntityName];
				foreach (CommandType command in referenceSetCommand.Commands)
				{
					PSCmdletInfo pSCmdletInfo = item.Cmdlets[command];
					pSEntityMetadatum.Cmdlets.Add(command, PSSchemaBuilder.ConstructMetadata(pSCmdletInfo, sessionCmdlets));
				}
				Dictionary<string, PSSchemaBuilder.EntityDataForSchemaBuilding.ReferencePropertyData>.Enumerator enumerator = referenceSetCommand.ReferenceSetCommands.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<string, PSSchemaBuilder.EntityDataForSchemaBuilding.ReferencePropertyData> keyValuePair = enumerator.Current;
						PSReferenceSetCmdletInfo pSReferenceSetCmdletInfo = null;
						PSReferenceSetCmdletInfo item1 = null;
						PSReferenceSetCmdletInfo pSReferenceSetCmdletInfo1 = null;
						if (keyValuePair.Value.IncludeAdd)
						{
							pSReferenceSetCmdletInfo = item.CmdletsForReferenceSets[keyValuePair.Key].Cmdlets[CommandType.AddReference];
						}
						if (keyValuePair.Value.IncludeRemove)
						{
							item1 = item.CmdletsForReferenceSets[keyValuePair.Key].Cmdlets[CommandType.RemoveReference];
						}
						if (keyValuePair.Value.IncludeGet)
						{
							pSReferenceSetCmdletInfo1 = item.CmdletsForReferenceSets[keyValuePair.Key].Cmdlets[CommandType.GetReference];
						}
						PSEntityMetadata.ReferenceSetCmdlets referenceSetCmdlet = new PSEntityMetadata.ReferenceSetCmdlets(item.CmdletsForReferenceSets[keyValuePair.Key].PropertyType, pSReferenceSetCmdletInfo, item1, pSReferenceSetCmdletInfo1, keyValuePair.Value.GetHidden);
						pSEntityMetadatum.CmdletsForReferenceSets[keyValuePair.Key] = referenceSetCmdlet;
					}
				}
				finally
				{
					enumerator.Dispose();
				}
				userSchema.EntityMetadataDictionary.Add(referenceSetCommand.EntityName, pSEntityMetadatum);
			}
		}

		public void Build(Microsoft.Management.Odata.Schema.Schema logicalSchema, Microsoft.Management.Odata.Schema.Schema userSchema, UserContext userContext, string membershipId)
		{
			Envelope<PSRunspace, UserContext> envelope = this.runspaceStore.Borrow(userContext, membershipId);
			using (envelope)
			{
				this.Build(logicalSchema, userSchema, envelope.Item.Runspace);
			}
		}

		internal void Build(Microsoft.Management.Odata.Schema.Schema logicalSchema, Microsoft.Management.Odata.Schema.Schema userSchema, Runspace runspace)
		{
			HashSet<string> strs = null;
			Dictionary<string, CommandInfo> strs1 = null;
			PSSchemaBuilder.GetCommands(runspace, out strs, out strs1);
			List<PSSchemaBuilder.EntityDataForSchemaBuilding> entityDataForSchemaBuildings = this.CreateEntityDataForSchemaBuilding(logicalSchema, strs);
			this.AddEntitiesToSchema(logicalSchema, userSchema, entityDataForSchemaBuildings, strs1);
			userSchema.PopulateAllRelevantResourceTypes(logicalSchema);
		}

		private static PSCmdletInfo ConstructMetadata(PSCmdletInfo cmdletInfo, Dictionary<string, CommandInfo> availableCmdlets)
		{
			CommandInfo commandInfo = null;
			if (availableCmdlets.TryGetValue(cmdletInfo.CmdletName, out commandInfo))
			{
				List<string> strs = new List<string>();
				foreach (KeyValuePair<string, string> fieldParameterMapping in cmdletInfo.FieldParameterMapping)
				{
					if (commandInfo.Parameters.ContainsKey(fieldParameterMapping.Value))
					{
						continue;
					}
					strs.Add(fieldParameterMapping.Key);
				}
				List<string> strs1 = new List<string>();
				foreach (string option in cmdletInfo.Options)
				{
					if (commandInfo.Parameters.ContainsKey(option))
					{
						continue;
					}
					strs1.Add(option);
				}
				if (strs.Count != 0 || strs1.Count != 0)
				{
					PSCmdletInfo pSCmdletInfo = new PSCmdletInfo(cmdletInfo);
					foreach (string str in strs)
					{
						pSCmdletInfo.FieldParameterMapping.Remove(str);
					}
					foreach (string str1 in strs1)
					{
						pSCmdletInfo.Options.Remove(str1);
					}
					return pSCmdletInfo;
				}
				else
				{
					return cmdletInfo;
				}
			}
			else
			{
				return cmdletInfo;
			}
		}

		private List<PSSchemaBuilder.EntityDataForSchemaBuilding> CreateEntityDataForSchemaBuilding(Microsoft.Management.Odata.Schema.Schema logicalSchema, HashSet<string> initialSessionCommands)
		{
			IEnumerable<PSSchemaBuilder.EntityDataForSchemaBuilding> entityDataForSchemaBuildings = null;
			List<PSSchemaBuilder.EntityDataForSchemaBuilding> entityDataForSchemaBuildings1 = new List<PSSchemaBuilder.EntityDataForSchemaBuilding>();
			foreach (string key in logicalSchema.EntityMetadataDictionary.Keys)
			{
				if (logicalSchema.EntityMetadataDictionary[key].MgmtSystem != ManagementSystemType.PowerShell)
				{
					continue;
				}
				PSSchemaBuilder.EntityDataForSchemaBuilding entityDataForSchemaBuilding = new PSSchemaBuilder.EntityDataForSchemaBuilding(key);
				PSEntityMetadata item = (PSEntityMetadata)logicalSchema.EntityMetadataDictionary[key];
				entityDataForSchemaBuilding.Commands = this.FindSupportedCommands(item, initialSessionCommands);
				entityDataForSchemaBuilding.ReferenceSetCommands = this.FindSupportedReferenceCommands(item, initialSessionCommands);
				entityDataForSchemaBuildings1.Add(entityDataForSchemaBuilding);
			}
			foreach (PSSchemaBuilder.EntityDataForSchemaBuilding entityDataForSchemaBuilding1 in entityDataForSchemaBuildings1)
			{
				if (!entityDataForSchemaBuilding1.Commands.Contains(CommandType.Read))
				{
					continue;
				}
				entityDataForSchemaBuilding1.IncludeEntitySet = true;
				entityDataForSchemaBuilding1.IncludeInSchema = true;
				new HashSet<ResourceType>();
				ResourceType resourceType = logicalSchema.ResourceTypes[entityDataForSchemaBuilding1.EntityName];
				HashSet<ResourceType> family = resourceType.GetFamily();
				List<PSSchemaBuilder.EntityDataForSchemaBuilding> entityDataForSchemaBuildings2 = entityDataForSchemaBuildings1;
				HashSet<ResourceType> resourceTypes = family;
				Func<PSSchemaBuilder.EntityDataForSchemaBuilding, string> func = (PSSchemaBuilder.EntityDataForSchemaBuilding entityStat) => entityStat.EntityName;
				Func<ResourceType, string> func1 = (ResourceType res) => res.FullName;
				entityDataForSchemaBuildings = entityDataForSchemaBuildings2.Join<PSSchemaBuilder.EntityDataForSchemaBuilding, ResourceType, string, PSSchemaBuilder.EntityDataForSchemaBuilding>(resourceTypes, func, func1, (PSSchemaBuilder.EntityDataForSchemaBuilding entityStat, ResourceType res) => entityStat);
				IEnumerator<PSSchemaBuilder.EntityDataForSchemaBuilding> enumerator = entityDataForSchemaBuildings.GetEnumerator();
				using (enumerator)
				{
					while (enumerator.MoveNext())
					{
						PSSchemaBuilder.EntityDataForSchemaBuilding entityDataForSchemaBuilding2 = entityDataForSchemaBuilding1;
						entityDataForSchemaBuilding2.IncludeInSchema = true;
					}
				}
			}
			return entityDataForSchemaBuildings1;
		}

		internal List<CommandType> FindSupportedCommands(PSEntityMetadata entityMetadata, HashSet<string> initialSessionCommands)
		{
			List<CommandType> commandTypes = new List<CommandType>();
			foreach (CommandType key in entityMetadata.Cmdlets.Keys)
			{
				Func<string, bool> func = null;
				HashSet<string> strs = initialSessionCommands;
				if (func == null)
				{
					func = (string item) => string.Equals(item, entityMetadata.Cmdlets[key].CmdletName, StringComparison.OrdinalIgnoreCase);
				}
				string str = strs.FirstOrDefault<string>(func);
				if (str == null)
				{
					continue;
				}
				commandTypes.Add(key);
			}
			return commandTypes;
		}

		internal Dictionary<string, PSSchemaBuilder.EntityDataForSchemaBuilding.ReferencePropertyData> FindSupportedReferenceCommands(PSEntityMetadata entityMetadata, HashSet<string> initialSessionCommands)
		{
			string str;
			Dictionary<string, PSSchemaBuilder.EntityDataForSchemaBuilding.ReferencePropertyData> strs = new Dictionary<string, PSSchemaBuilder.EntityDataForSchemaBuilding.ReferencePropertyData>();
			foreach (KeyValuePair<string, PSEntityMetadata.ReferenceSetCmdlets> cmdletsForReferenceSet in entityMetadata.CmdletsForReferenceSets)
			{
				Func<string, bool> func = null;
				Func<string, bool> func1 = null;
				Func<string, bool> func2 = null;
				PSSchemaBuilder.EntityDataForSchemaBuilding.ReferencePropertyData referencePropertyDatum = new PSSchemaBuilder.EntityDataForSchemaBuilding.ReferencePropertyData();
				PSReferenceSetCmdletInfo pSReferenceSetCmdletInfo = null;
				if (cmdletsForReferenceSet.Value.Cmdlets.TryGetValue(CommandType.AddReference, out pSReferenceSetCmdletInfo))
				{
					HashSet<string> strs1 = initialSessionCommands;
					if (func == null)
					{
						func = (string item) => string.Equals(item, pSReferenceSetCmdletInfo.CmdletName, StringComparison.OrdinalIgnoreCase);
					}
					str = strs1.FirstOrDefault<string>(func);
					referencePropertyDatum.IncludeAdd = str != null;
				}
				if (cmdletsForReferenceSet.Value.Cmdlets.TryGetValue(CommandType.RemoveReference, out pSReferenceSetCmdletInfo))
				{
					HashSet<string> strs2 = initialSessionCommands;
					if (func1 == null)
					{
						func1 = (string item) => string.Equals(item, pSReferenceSetCmdletInfo.CmdletName, StringComparison.OrdinalIgnoreCase);
					}
					str = strs2.FirstOrDefault<string>(func1);
					referencePropertyDatum.IncludeRemove = str != null;
				}
				if (cmdletsForReferenceSet.Value.Cmdlets.TryGetValue(CommandType.GetReference, out pSReferenceSetCmdletInfo))
				{
					HashSet<string> strs3 = initialSessionCommands;
					if (func2 == null)
					{
						func2 = (string item) => string.Equals(item, pSReferenceSetCmdletInfo.CmdletName, StringComparison.OrdinalIgnoreCase);
					}
					str = strs3.FirstOrDefault<string>(func2);
					referencePropertyDatum.IncludeGet = true;
					referencePropertyDatum.GetHidden = str == null;
				}
				strs.Add(cmdletsForReferenceSet.Key, referencePropertyDatum);
			}
			return strs;
		}

		internal static void GetCommands(Runspace runspace, out HashSet<string> visibleCommands, out Dictionary<string, CommandInfo> sessionCmdlets)
		{
			visibleCommands = new HashSet<string>();
			sessionCmdlets = new Dictionary<string, CommandInfo>();
			IEnumerable<CommandInfo> commands = runspace.SessionStateProxy.InvokeCommand.GetCommands("*", CommandTypes.Alias | CommandTypes.Function | CommandTypes.Cmdlet | CommandTypes.ExternalScript | CommandTypes.Script, true);
			foreach (CommandInfo command in commands)
			{
				if (command.Visibility != SessionStateEntryVisibility.Public)
				{
					continue;
				}
				visibleCommands.Add(command.Name);
				sessionCmdlets.Add(command.Name, command);
			}
			foreach (string script in runspace.SessionStateProxy.Scripts)
			{
				if (script.Equals("*", StringComparison.CurrentCulture))
				{
					continue;
				}
				visibleCommands.Add(script);
			}
			StringBuilder stringBuilder = new StringBuilder("Visible commands in runsapce");
			visibleCommands.ToList<string>().ForEach((string item) => stringBuilder.AppendLine(item));
			TraceHelper.Current.DebugMessage(stringBuilder.ToString());
		}

		internal class EntityDataForSchemaBuilding
		{
			public List<CommandType> Commands
			{
				get;
				set;
			}

			public string EntityName
			{
				get;
				private set;
			}

			public bool IncludeEntitySet
			{
				get;
				set;
			}

			public bool IncludeInSchema
			{
				get;
				set;
			}

			public Dictionary<string, PSSchemaBuilder.EntityDataForSchemaBuilding.ReferencePropertyData> ReferenceSetCommands
			{
				get;
				set;
			}

			public EntityDataForSchemaBuilding(string entityName)
			{
				this.EntityName = entityName;
				this.Commands = new List<CommandType>();
				this.ReferenceSetCommands = new Dictionary<string, PSSchemaBuilder.EntityDataForSchemaBuilding.ReferencePropertyData>();
			}

			internal struct ReferencePropertyData
			{
				public bool IncludeAdd;

				public bool IncludeRemove;

				public bool IncludeGet;

				public bool GetHidden;

			}
		}
	}
}