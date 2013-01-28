using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADDirectoryServerFactory<T> : ADFactory<T>
	where T : ADDirectoryServer, new()
	{
		private const string _debugCategory = "ADDirectoryServerFactory";

		private static IADOPathNode _directoryServerStructuralFilter;

		private readonly static string[] _directoryServerDefaultAttributes;

		private ADDirectoryServerFactory<T>.ADDirectoryServerPseudoFactory _addsPseudoFactory;

		private static AttributeConverterEntry[] ADMappingTable;

		private static AttributeConverterEntry[] ADAMMappingTable;

		internal static IDictionary<ADServerType, MappingTable<AttributeConverterEntry>> AttributeTable
		{
			get
			{
				return ADFactoryBase<T>.AttributeTable;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADDirectoryServerFactory<T>._directoryServerStructuralFilter;
			}
		}

		static ADDirectoryServerFactory()
		{
			IADOPathNode[] aDOPathNodeArray = new IADOPathNode[2];
			aDOPathNodeArray[0] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "nTDSDSA");
			aDOPathNodeArray[1] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "server");
			ADDirectoryServerFactory<T>._directoryServerStructuralFilter = ADOPathUtil.CreateOrClause(aDOPathNodeArray);
			string[] strArrays = new string[6];
			strArrays[0] = "name";
			strArrays[1] = "distinguishedName";
			strArrays[2] = "objectClass";
			strArrays[3] = "objectGUID";
			strArrays[4] = "dNSHostName";
			strArrays[5] = "serverReferenceBL";
			ADDirectoryServerFactory<T>._directoryServerDefaultAttributes = strArrays;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[14];
			attributeConverterEntry[0] = new AttributeConverterEntry("Name", "Name", TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[1] = new AttributeConverterEntry("Site", "Site", TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[2] = new AttributeConverterEntry("IPv4Address", "HostName", TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(ADComputerFactory<ADComputer>.ToExtendedIPv4), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[3] = new AttributeConverterEntry("IPv6Address", "HostName", TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(ADComputerFactory<ADComputer>.ToExtendedIPv6), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[4] = new AttributeConverterEntry("Partitions", "Partitions", TypeConstants.String, false, TypeAdapterAccess.Read, false, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[5] = new AttributeConverterEntry("DefaultPartition", "DefaultPartition", TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[6] = new AttributeConverterEntry("HostName", "HostName", TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[7] = new AttributeConverterEntry("LdapPort", "LdapPort", TypeConstants.Int, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[8] = new AttributeConverterEntry("SslPort", "SslPort", TypeConstants.Int, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[9] = new AttributeConverterEntry("OperationMasterRoles", "OperationMasterRole", TypeConstants.ADOperationMasterRole, false, TypeAdapterAccess.Read, false, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[10] = new AttributeConverterEntry("NTDSSettingsObjectDN", "NTDSSettingsObjectDN", TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[11] = new AttributeConverterEntry("ServerObjectDN", "ServerObjectDN", TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[12] = new AttributeConverterEntry("ServerObjectGuid", "ServerObjectGuid", TypeConstants.Guid, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[13] = new AttributeConverterEntry("InvocationId", "InvocationId", TypeConstants.Guid, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			ADDirectoryServerFactory<T>.ADMappingTable = attributeConverterEntry;
			ADDirectoryServerFactory<T>.ADAMMappingTable = ADDirectoryServerFactory<T>.ADMappingTable;
			ADFactoryBase<T>.RegisterMappingTable(ADDirectoryServerFactory<T>.ADAMMappingTable, ADServerType.ADLDS);
			ADFactoryBase<T>.RegisterMappingTable(ADDirectoryServerFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADDirectoryServerFactory()
		{
			this._addsPseudoFactory = new ADDirectoryServerFactory<T>.ADDirectoryServerPseudoFactory();
		}

		internal override string GenerateObjectName(ADParameterSet parameters)
		{
			throw new NotSupportedException();
		}

		internal override ADObject GetDirectoryObjectFromIdentity(T identityObj, string searchRoot, bool showDeleted)
		{
			ADObject aDObject = null;
			ADObject aDObject1 = null;
			ADObject aDObject2 = null;
			ADObject aDSessionInfo;
			if (base.CmdletSessionInfo != null)
			{
				//base.CmdletSessionInfo.ADRootDSE.ConfigurationNamingContext;
				string nTDSSettingsDN = this.ResolveIdentityToNTDSSettingsDN(identityObj, ADDirectoryServerFactory<T>._directoryServerDefaultAttributes, false, out aDObject, out aDObject1, out aDObject2);
				if (nTDSSettingsDN != null)
				{
					if (aDObject1 == null)
					{
						string str = nTDSSettingsDN.Substring("CN=NTDS Settings,".Length);
						ADObjectSearcher aDObjectSearcher = SearchUtility.BuildSearcher(base.CmdletSessionInfo.ADSessionInfo, str, ADSearchScope.Base);
						using (aDObjectSearcher)
						{
							aDObjectSearcher.Filter = ADOPathUtil.CreateFilterClause(ADOperator.Like, "objectClass", "*");
							aDObjectSearcher.Properties.AddRange(ADDirectoryServerFactory<T>._directoryServerDefaultAttributes);
							aDSessionInfo = aDObjectSearcher.FindOne();
							if (aDSessionInfo == null)
							{
								DebugLogger.LogInfo("ADDirectoryServerFactory", string.Format("GetDirectoryObjectFromIdentity: Identity not found.", new object[0]));
								object[] objArray = new object[2];
								objArray[0] = identityObj.ToString();
								objArray[1] = str;
								throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.IdentityNotFound, objArray));
							}
						}
					}
					else
					{
						aDSessionInfo = aDObject1;
					}
					aDSessionInfo.TrackChanges = true;
					aDSessionInfo.SessionInfo = base.CmdletSessionInfo.ADSessionInfo;
					return aDSessionInfo;
				}
				else
				{
					DebugLogger.LogInfo("ADDirectoryServerFactory", string.Format("GetDirectoryObjectFromIdentity: NTDS Settings DN for the given identity not found", new object[0]));
					object[] identity = new object[1];
					identity[0] = identityObj.Identity;
					throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.DirectoryServerNotFound, identity));
				}
			}
			else
			{
				throw new ArgumentNullException(StringResources.SessionRequired);
			}
		}

		internal override T GetExtendedObjectFromIdentity(T identityObj, string identityQueryPath, ICollection<string> propertiesToFetch, bool showDeleted)
		{
			ADObject aDObject = null;
			ADObject aDObject1 = null;
			ADObject aDObject2 = null;
			T t;
			string nTDSSettingsDN = this.ResolveIdentityToNTDSSettingsDN(identityObj, null, false, out aDObject, out aDObject1, out aDObject2);
			if (nTDSSettingsDN != null)
			{
				using (ADTopologyManagement aDTopologyManagement = new ADTopologyManagement(base.CmdletSessionInfo.ADSessionInfo))
				{
					string[] strArrays = new string[1];
					strArrays[0] = nTDSSettingsDN;
					ADEntity[] domainController = aDTopologyManagement.GetDomainController(strArrays);
					if (domainController == null || (int)domainController.Length == 0)
					{
						DebugLogger.LogInfo("ADDirectoryServerFactory", string.Format("GetExtendedObjectFromIdentity: No objects returned from custom action", new object[0]));
						throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.ObjectNotFound, new object[0]));
					}
					else
					{
						AttributeSetRequest attributeSetRequest = this.ConstructAttributeSetRequest(propertiesToFetch);
						T aDSessionInfo = this.Construct(domainController[0], attributeSetRequest);
						aDSessionInfo.SessionInfo = base.CmdletSessionInfo.ADSessionInfo;
						t = aDSessionInfo;
					}
				}
				return t;
			}
			else
			{
				DebugLogger.LogInfo("ADDirectoryServerFactory", string.Format("GetExtendedObjectFromIdentity: Could not find the DirectoryServer's  NTDS Settings DN", new object[0]));
				object[] identity = new object[1];
				identity[0] = identityObj.Identity;
				throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.DirectoryServerNotFound, identity));
			}
		}

		internal override IADOPathNode IdentitySearchConverter(object identity)
		{
			if (identity != null)
			{
				ADDirectoryServer aDDirectoryServer = identity as ADDirectoryServer;
				if (aDDirectoryServer == null)
				{
					ADObject aDObject = new ADObject();
					aDObject.Identity = identity;
					this._addsPseudoFactory.SetCmdletSessionInfo(base.CmdletSessionInfo);
					return this._addsPseudoFactory.BuildIdentityFilter(aDObject);
				}
				else
				{
					List<IADOPathNode> aDOPathNodes = new List<IADOPathNode>();
					if (aDDirectoryServer["ServerObjectDN"] != null && aDDirectoryServer["ServerObjectDN"].Value != null)
					{
						aDOPathNodes.Add(ADOPathUtil.CreateFilterClause(ADOperator.Eq, "distinguishedName", Utils.EscapeDNForFilter(aDDirectoryServer["ServerObjectDN"].Value as string)));
					}
					if (aDDirectoryServer["NTDSSettingsObjectDN"] != null && aDDirectoryServer["NTDSSettingsObjectDN"].Value != null)
					{
						aDOPathNodes.Add(ADOPathUtil.CreateFilterClause(ADOperator.Eq, "distinguishedName", Utils.EscapeDNForFilter(aDDirectoryServer["NTDSSettingsObjectDN"].Value as string)));
					}
					if (aDDirectoryServer["Name"] != null && aDDirectoryServer["Name"].Value != null)
					{
						aDOPathNodes.Add(ADOPathUtil.CreateFilterClause(ADOperator.Eq, "name", aDDirectoryServer["Name"].Value));
					}
					if (aDOPathNodes.Count != 1)
					{
						return ADOPathUtil.CreateOrClause(aDOPathNodes.ToArray());
					}
					else
					{
						return aDOPathNodes[0];
					}
				}
			}
			else
			{
				throw new ArgumentNullException("identity");
			}
		}

		protected internal string ResolveIdentityToNTDSSettingsDN(T identityObj, ICollection<string> propertiesToFetch, bool checkForDCs, out ADObject computerObj, out ADObject serverObj, out ADObject ntdsDSAObj)
		{
			string str = null;
			string str1;
			computerObj = null;
			serverObj = null;
			ntdsDSAObj = null;
			HashSet<string> strs = new HashSet<string>(ADDirectoryServerFactory<T>._directoryServerDefaultAttributes, StringComparer.OrdinalIgnoreCase);
			if (propertiesToFetch != null)
			{
				strs.UnionWith(propertiesToFetch);
			}
			string[] strArrays = new string[strs.Count];
			strs.CopyTo(strArrays);
			string configurationNamingContext = base.CmdletSessionInfo.ADRootDSE.ConfigurationNamingContext;
			ADSearchScope aDSearchScope = ADSearchScope.Subtree;
			IADOPathNode aDOPathNode = this.BuildIdentityFilter(identityObj);
			IADOPathNode[] structuralObjectFilter = new IADOPathNode[2];
			structuralObjectFilter[0] = aDOPathNode;
			structuralObjectFilter[1] = this.StructuralObjectFilter;
			aDOPathNode = ADOPathUtil.CreateAndClause(structuralObjectFilter);
			ADObjectSearcher nullable = SearchUtility.BuildSearcher(base.CmdletSessionInfo.ADSessionInfo, configurationNamingContext, aDSearchScope);
			using (nullable)
			{
				nullable.Filter = aDOPathNode;
				nullable.Properties.AddRange(strArrays);
				if (checkForDCs)
				{
					nullable.SearchOption = new SearchOption?(SearchOption.PhantomRoot);
					nullable.SearchRoot = string.Empty;
				}
				List<ADObject> aDObjects = new List<ADObject>();
				foreach (ADObject aDObject in nullable.FindAll())
				{
					aDObjects.Add(aDObject);
				}
				if (aDObjects.Count != 0)
				{
					DirectoryServerUtil.CheckIfObjectsRefersToSingleDirectoryServer(base.CmdletSessionInfo.ADSessionInfo, aDObjects, checkForDCs, out str, out computerObj, out serverObj, out ntdsDSAObj);
					if (str == null)
					{
						str1 = null;
					}
					else
					{
						str1 = string.Concat("CN=NTDS Settings,", str);
					}
				}
				else
				{
					DebugLogger.LogInfo("ADDirectoryServerFactory", string.Format("Could  not find identity using the following filter: {0}", aDOPathNode.GetLdapFilterString()));
					str1 = null;
				}
			}
			return str1;
		}

		private class ADDirectoryServerPseudoFactory : ADObjectFactory<ADObject>
		{
			private readonly static string[] _identityLdapAttributes;

			internal override string[] IdentityLdapAttributes
			{
				get
				{
					return ADDirectoryServerFactory<T>.ADDirectoryServerPseudoFactory._identityLdapAttributes;
				}
			}

			static ADDirectoryServerPseudoFactory()
			{
				string[] strArrays = new string[3];
				strArrays[0] = "distinguishedName";
				strArrays[1] = "objectGUID";
				strArrays[2] = "name";
				ADDirectoryServerFactory<T>.ADDirectoryServerPseudoFactory._identityLdapAttributes = strArrays;
			}

			public ADDirectoryServerPseudoFactory()
			{
			}
		}
	}
}