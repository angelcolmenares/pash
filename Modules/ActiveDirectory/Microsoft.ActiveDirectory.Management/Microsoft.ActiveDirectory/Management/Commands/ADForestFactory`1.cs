using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADForestFactory<T> : ADFactory<T>
	where T : ADForest, new()
	{
		private const string _debugCategory = "ADForestFactory";

		private const string _partitionsDNPrefix = "CN=Partitions,";

		private static IADOPathNode _forestStructuralFilter;

		private static string[] _forestIdentityLdapAttributes;

		private static AttributeConverterEntry[] ADMappingTable;

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
				return "CN";
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return "crossRefContainer";
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADForestFactory<T>._forestStructuralFilter;
			}
		}

		static ADForestFactory()
		{
			ADForestFactory<T>._forestStructuralFilter = null;
			ADForestFactory<T>._forestIdentityLdapAttributes = null;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[13];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADForestFactory<T>.ADForestPropertyMap.UPNSuffixes.PropertyName, ADForestFactory<T>.ADForestPropertyMap.UPNSuffixes.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryMultivalueObject), new ToSearchFilterDelegate(SearchConverters.ToSearchMultivalueObject));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADForestFactory<T>.ADForestPropertyMap.SPNSuffixes.PropertyName, ADForestFactory<T>.ADForestPropertyMap.SPNSuffixes.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryMultivalueObject), new ToSearchFilterDelegate(SearchConverters.ToSearchMultivalueObject));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADForestFactory<T>.ADForestPropertyMap.PartitionsContainer.PropertyName, ADForestFactory<T>.ADForestPropertyMap.PartitionsContainer.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObject));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADForestFactory<T>.ADForestPropertyMap.ForestMode.PropertyName, ADForestFactory<T>.ADForestPropertyMap.ForestMode.ADAttribute, TypeConstants.ADForestMode, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObjectWithCast<int>), new ToSearchFilterDelegate(SearchConverters.ToSearchEnum<ADForestMode>));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADForestFactory<T>.ADForestPropertyMap.ApplicationPartitions.PropertyName, ADForestFactory<T>.ADForestPropertyMap.ApplicationPartitions.ADAttribute, TypeConstants.ADPartition, false, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryMultivalueObject), new ToSearchFilterDelegate(SearchConverters.ToSearchMultivalueObject));
			attributeConverterEntry[5] = new AttributeConverterEntry(ADForestFactory<T>.ADForestPropertyMap.CrossForestReferences.PropertyName, ADForestFactory<T>.ADForestPropertyMap.CrossForestReferences.ADAttribute, TypeConstants.ADPartition, false, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryMultivalueObject), new ToSearchFilterDelegate(SearchConverters.ToSearchMultivalueObject));
			attributeConverterEntry[6] = new AttributeConverterEntry(ADForestFactory<T>.ADForestPropertyMap.Domains.PropertyName, ADForestFactory<T>.ADForestPropertyMap.Domains.ADAttribute, TypeConstants.ADDomain, false, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryMultivalueObject), new ToSearchFilterDelegate(SearchConverters.ToSearchMultivalueObject));
			attributeConverterEntry[7] = new AttributeConverterEntry(ADForestFactory<T>.ADForestPropertyMap.GlobalCatalogs.PropertyName, ADForestFactory<T>.ADForestPropertyMap.GlobalCatalogs.ADAttribute, TypeConstants.ADDomainController, false, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryMultivalueObject), new ToSearchFilterDelegate(SearchConverters.ToSearchMultivalueObject));
			attributeConverterEntry[8] = new AttributeConverterEntry(ADForestFactory<T>.ADForestPropertyMap.Sites.PropertyName, ADForestFactory<T>.ADForestPropertyMap.Sites.ADAttribute, TypeConstants.ADReplicationSite, false, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryMultivalueObject), new ToSearchFilterDelegate(SearchConverters.ToSearchMultivalueObject));
			attributeConverterEntry[9] = new AttributeConverterEntry(ADForestFactory<T>.ADForestPropertyMap.DomainNamingMaster.PropertyName, ADForestFactory<T>.ADForestPropertyMap.DomainNamingMaster.ADAttribute, TypeConstants.ADDomainController, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObject));
			attributeConverterEntry[10] = new AttributeConverterEntry(ADForestFactory<T>.ADForestPropertyMap.RootDomain.PropertyName, ADForestFactory<T>.ADForestPropertyMap.RootDomain.ADAttribute, TypeConstants.ADDomain, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObject));
			attributeConverterEntry[11] = new AttributeConverterEntry(ADForestFactory<T>.ADForestPropertyMap.Name.PropertyName, ADForestFactory<T>.ADForestPropertyMap.Name.ADAttribute, TypeConstants.ADDomain, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObject));
			attributeConverterEntry[12] = new AttributeConverterEntry(ADForestFactory<T>.ADForestPropertyMap.SchemaMaster.PropertyName, ADForestFactory<T>.ADForestPropertyMap.SchemaMaster.ADAttribute, TypeConstants.ADDomainController, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObject));
			ADForestFactory<T>.ADMappingTable = attributeConverterEntry;
			ADForestFactory<T>._forestStructuralFilter = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "crossRefContainer");
			string[] strArrays = new string[3];
			strArrays[0] = "distinguishedName";
			strArrays[1] = "objectSid";
			strArrays[2] = "objectGUID";
			ADForestFactory<T>._forestIdentityLdapAttributes = strArrays;
			ADFactoryBase<T>.RegisterMappingTable(ADForestFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADForestFactory()
		{
		}

		internal override T Construct(ADEntity directoryObj, AttributeSetRequest requestedAttributes)
		{
			requestedAttributes = base.ConstructAttributeSetRequest(null);
			return base.Construct(directoryObj, requestedAttributes);
		}

		internal override AttributeSetRequest ConstructAttributeSetRequest(ICollection<string> requestedExtendedAttr)
		{
			return new AttributeSetRequest(false);
		}

		internal override ADObject GetDirectoryObjectFromIdentity(T identityObj, string searchRoot, bool showDeleted)
		{
			ADObject aDSessionInfo;
			this.ValidateIdentity(identityObj);
			string str = string.Concat("CN=Partitions,", base.CmdletSessionInfo.ADRootDSE.ConfigurationNamingContext);
			ADObjectSearcher structuralObjectFilter = SearchUtility.BuildSearcher(base.CmdletSessionInfo.ADSessionInfo, str, ADSearchScope.Base);
			using (structuralObjectFilter)
			{
				AttributeSetRequest attributeSetRequest = this.ConstructAttributeSetRequest(null);
				structuralObjectFilter.Properties.AddRange(attributeSetRequest.DirectoryAttributes);
				structuralObjectFilter.Filter = this.StructuralObjectFilter;
				DebugLogger.LogInfo("ADForestFactory", string.Format("GetDirectoryObjectFromIdentity: Searching for identity using filter: {0} searchbase: {1}", structuralObjectFilter.Filter.GetLdapFilterString(), structuralObjectFilter.SearchRoot));
				aDSessionInfo = structuralObjectFilter.FindOne();
				if (aDSessionInfo == null)
				{
					DebugLogger.LogInfo("ADForestFactory", string.Format("GetDirectoryObjectFromIdentity: Identity not found.", new object[0]));
					object[] objArray = new object[2];
					objArray[0] = identityObj.ToString();
					objArray[1] = structuralObjectFilter.SearchRoot;
					throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.IdentityNotFound, objArray));
				}
			}
			aDSessionInfo.TrackChanges = true;
			aDSessionInfo.SessionInfo = base.CmdletSessionInfo.ADSessionInfo;
			return aDSessionInfo;
		}

		private T GetExtendedForestObjectUsingCurrentSession(ICollection<string> propertiesToFetch, bool showDeleted)
		{
			ADEntity forest;
			using (ADTopologyManagement aDTopologyManagement = new ADTopologyManagement(base.CmdletSessionInfo.ADSessionInfo))
			{
				forest = aDTopologyManagement.GetForest();
			}
			T aDSessionInfo = this.Construct(forest, null);
			aDSessionInfo.SetValue("PartitionsContainer", string.Concat("CN=Partitions,", base.CmdletSessionInfo.ADRootDSE.ConfigurationNamingContext));
			aDSessionInfo.SessionInfo = base.CmdletSessionInfo.ADSessionInfo;
			aDSessionInfo.IsSearchResult = true;
			return aDSessionInfo;
		}

		internal override T GetExtendedObjectFromDN(string distinguishedName, ICollection<string> propertiesToFetch, bool showDeleted)
		{
			string str = ADPathModule.MakePath(base.CmdletSessionInfo.ADRootDSE.ConfigurationNamingContext, "CN=Partitions", ADPathFormat.X500);
			if (ADPathModule.ComparePath(str, distinguishedName, ADPathFormat.X500))
			{
				return this.GetExtendedForestObjectUsingCurrentSession(propertiesToFetch, showDeleted);
			}
			else
			{
				object[] objArray = new object[2];
				objArray[0] = str;
				objArray[1] = distinguishedName;
				throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.ConnectedToWrongForest, objArray));
			}
		}

		internal override T GetExtendedObjectFromIdentity(T identityObj, string identityQueryPath, ICollection<string> propertiesToFetch, bool showDeleted)
		{
			this.ValidateIdentity(identityObj);
			return this.GetExtendedForestObjectUsingCurrentSession(propertiesToFetch, showDeleted);
		}

		internal override IADOPathNode IdentitySearchConverter(object identity)
		{
			throw new NotImplementedException();
		}

		private void ValidateIdentity(T identityObj)
		{
			string str;
			if (identityObj != null)
			{
				if (base.CmdletSessionInfo != null)
				{
					if (!identityObj.IsSearchResult)
					{
						if (identityObj.Identity as ADDomain == null || !((ADDomain)identityObj.Identity).IsSearchResult)
						{
							str = identityObj.Identity.ToString();
						}
						else
						{
							str = ((ADDomain)identityObj.Identity).DNSRoot;
						}
					}
					else
					{
						str = identityObj.Name;
					}
					ADRootDSE aDRootDSE = base.CmdletSessionInfo.ADRootDSE;
					string str1 = string.Concat("CN=Partitions,", aDRootDSE.ConfigurationNamingContext);
					ADObjectSearcher aDObjectSearcher = SearchUtility.BuildSearcher(base.CmdletSessionInfo.ADSessionInfo, str1, ADSearchScope.OneLevel);
					using (aDObjectSearcher)
					{
						IADOPathNode[] aDOPathNodeArray = new IADOPathNode[3];
						aDOPathNodeArray[0] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "crossRef");
						IADOPathNode[] aDOPathNodeArray1 = new IADOPathNode[2];
						aDOPathNodeArray1[0] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "dnsRoot", str);
						aDOPathNodeArray1[1] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "nETBIOSName", str);
						aDOPathNodeArray[1] = ADOPathUtil.CreateOrClause(aDOPathNodeArray1);
						aDOPathNodeArray[2] = ADOPathUtil.CreateFilterClause(ADOperator.Band, "systemFlags", 3);
						aDObjectSearcher.Filter = ADOPathUtil.CreateAndClause(aDOPathNodeArray);
						ADObject aDObject = aDObjectSearcher.FindOne();
						if (aDObject == null)
						{
							object[] objArray = new object[1];
							objArray[0] = str;
							throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.CouldNotFindForestIdentity, objArray));
						}
					}
					return;
				}
				else
				{
					throw new ArgumentNullException(StringResources.SessionRequired);
				}
			}
			else
			{
				object[] type = new object[2];
				type[0] = "ValidateIdentity";
				type[1] = identityObj.GetType();
				throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.MethodNotSupportedForObjectType, type));
			}
		}

		internal static class ADForestPropertyMap
		{
			internal readonly static PropertyMapEntry Name;

			internal readonly static PropertyMapEntry PartitionsContainer;

			internal readonly static PropertyMapEntry UPNSuffixes;

			internal readonly static PropertyMapEntry SPNSuffixes;

			internal readonly static PropertyMapEntry ForestMode;

			internal readonly static PropertyMapEntry ApplicationPartitions;

			internal readonly static PropertyMapEntry CrossForestReferences;

			internal readonly static PropertyMapEntry Domains;

			internal readonly static PropertyMapEntry GlobalCatalogs;

			internal readonly static PropertyMapEntry Sites;

			internal readonly static PropertyMapEntry DomainNamingMaster;

			internal readonly static PropertyMapEntry RootDomain;

			internal readonly static PropertyMapEntry SchemaMaster;

			static ADForestPropertyMap()
			{
				ADForestFactory<T>.ADForestPropertyMap.Name = new PropertyMapEntry("Name", "name", null);
				ADForestFactory<T>.ADForestPropertyMap.PartitionsContainer = new PropertyMapEntry("PartitionsContainer", "distinguishedName", null);
				ADForestFactory<T>.ADForestPropertyMap.UPNSuffixes = new PropertyMapEntry("UPNSuffixes", "uPNSuffixes", null);
				ADForestFactory<T>.ADForestPropertyMap.SPNSuffixes = new PropertyMapEntry("SPNSuffixes", "msDS-SPNSuffixes", null);
				ADForestFactory<T>.ADForestPropertyMap.ForestMode = new PropertyMapEntry("ForestMode", "msDS-Behavior-Version", null);
				ADForestFactory<T>.ADForestPropertyMap.ApplicationPartitions = new PropertyMapEntry("ApplicationPartitions", "ApplicationPartitions", "ApplicationPartitions");
				ADForestFactory<T>.ADForestPropertyMap.CrossForestReferences = new PropertyMapEntry("CrossForestReferences", "CrossForestReferences", "CrossForestReferences");
				ADForestFactory<T>.ADForestPropertyMap.Domains = new PropertyMapEntry("Domains", "Domains", "Domains");
				ADForestFactory<T>.ADForestPropertyMap.GlobalCatalogs = new PropertyMapEntry("GlobalCatalogs", "GlobalCatalogs", "GlobalCatalogs");
				ADForestFactory<T>.ADForestPropertyMap.Sites = new PropertyMapEntry("Sites", "Sites", "Sites");
				ADForestFactory<T>.ADForestPropertyMap.DomainNamingMaster = new PropertyMapEntry("DomainNamingMaster", "DomainNamingMaster", "DomainNamingMaster");
				ADForestFactory<T>.ADForestPropertyMap.RootDomain = new PropertyMapEntry("RootDomain", "RootDomain", "RootDomain");
				ADForestFactory<T>.ADForestPropertyMap.SchemaMaster = new PropertyMapEntry("SchemaMaster", "SchemaMaster", "SchemaMaster");
			}
		}
	}
}