using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public abstract class ADFactoryBase<T>
	where T : ADEntity, new()
	{
		private const string _debugCategory = "ADFactoryBase";

		private static Dictionary<ADServerType, MappingTable<AttributeConverterEntry>> _attributeTable;

		private ADServerType _connectedStore;

		private CmdletSessionInfo _cmdletSessionInfo;

		internal static IDictionary<ADServerType, MappingTable<AttributeConverterEntry>> AttributeTable
		{
			get
			{
				return ADFactoryBase<T>._attributeTable;
			}
		}

		internal CmdletSessionInfo CmdletSessionInfo
		{
			get
			{
				return this._cmdletSessionInfo;
			}
			set
			{
				this._cmdletSessionInfo = value;
			}
		}

		internal ADServerType ConnectedStore
		{
			get
			{
				return this._connectedStore;
			}
			set
			{
				this._connectedStore = value;
			}
		}

		static ADFactoryBase()
		{
			ADFactoryBase<T>._attributeTable = new Dictionary<ADServerType, MappingTable<AttributeConverterEntry>>();
		}

		internal ADFactoryBase()
		{
		}

		internal virtual IEnumerable<T> ApplyClientSideFilter(IEnumerable<T> objectList)
		{
			if (this.CmdletSessionInfo.CmdletParameters.Contains("Filter"))
			{
				string str = this.CmdletSessionInfo.CmdletParameters["Filter"].ToString();
				if (str != "objectClass -like \"*\"")
				{
					List<T> ts = new List<T>();
					ADFactoryBase<T> aDFactoryBase = this;
					ConvertSearchFilterDelegate convertSearchFilterDelegate = new ConvertSearchFilterDelegate(aDFactoryBase.BuildSearchFilter);
					VariableExpressionConverter variableExpressionConverter = new VariableExpressionConverter(new EvaluateVariableDelegate(this.CmdletSessionInfo.CmdletBase.GetVariableValue));
					QueryParser queryParser = new QueryParser(str, variableExpressionConverter, convertSearchFilterDelegate);
					str = ADOPathUtil.ChangeNodeToWhereFilterSyntax(queryParser.FilterExpressionTree);
					StringBuilder stringBuilder = new StringBuilder("$args[0] | Where-Object -filterScript { ");
					stringBuilder.Append(str);
					stringBuilder.Append(" } ");
					object[] objArray = new object[1];
					objArray[0] = objectList;
					Collection<PSObject> pSObjects = this.CmdletSessionInfo.CmdletBase.InvokeCommand.InvokeScript(stringBuilder.ToString(), false, PipelineResultTypes.None, null, objArray);
					foreach (PSObject pSObject in pSObjects)
					{
						ts.Add((T)pSObject.BaseObject);
					}
					return ts;
				}
				else
				{
					DebugLogger.LogInfo("ADFactoryBase", string.Format("Filter: Found MatchAnyObject filter: {0}", str));
					return objectList;
				}
			}
			else
			{
				return objectList;
			}
		}

		internal virtual IADOPathNode BuildSearchFilter(IADOPathNode filter)
		{
			if (this.CmdletSessionInfo != null)
			{
				MappingTable<AttributeConverterEntry> item = ADFactoryBase<T>.AttributeTable[this.ConnectedStore];
				BinaryADOPathNode binaryADOPathNode = filter as BinaryADOPathNode;
				if (binaryADOPathNode == null)
				{
					return null;
				}
				else
				{
					string ldapFilterString = binaryADOPathNode.LeftNode.GetLdapFilterString();
					AttributeConverterEntry attributeConverterEntry = null;
					if (!item.TryGetValue(ldapFilterString, out attributeConverterEntry))
					{
						string[] strArrays = new string[1];
						strArrays[0] = ldapFilterString;
						return SearchConverters.ToSearchUsingSchemaInfo(ldapFilterString, strArrays, filter, this.CmdletSessionInfo);
					}
					else
					{
						return attributeConverterEntry.InvokeToSearcherConverter(filter, this.CmdletSessionInfo);
					}
				}
			}
			else
			{
				throw new ArgumentNullException(StringResources.SessionRequired);
			}
		}

		internal static void RegisterMappingTable(AttributeConverterEntry[] attributeMap, ADServerType storeType)
		{
			ADFactoryBase<T>.RegisterMappingTable<AttributeConverterEntry>(ADFactoryBase<T>._attributeTable, attributeMap, storeType);
		}

		private static void RegisterMappingTable<MT>(Dictionary<ADServerType, MappingTable<MT>> table, MT[] map, ADServerType storeType)
		where MT : MappingTableEntry
		{
			if (!table.ContainsKey(storeType))
			{
				table.Add(storeType, new MappingTable<MT>());
			}
			MappingTable<MT> item = table[storeType];
			MT[] mTArray = map;
			for (int i = 0; i < (int)mTArray.Length; i++)
			{
				MT mT = mTArray[i];
				if (!item.ContainsKey(mT.ExtendedAttribute))
				{
					item.Add(mT.ExtendedAttribute, mT);
				}
			}
		}

		internal virtual void SetCmdletSessionInfo(CmdletSessionInfo cmdletSessionInfo)
		{
			if (cmdletSessionInfo != null)
			{
				this._cmdletSessionInfo = cmdletSessionInfo;
				this._connectedStore = this._cmdletSessionInfo.ConnectedADServerType;
				return;
			}
			else
			{
				throw new ArgumentNullException("cmdletSessionInfo");
			}
		}
	}
}