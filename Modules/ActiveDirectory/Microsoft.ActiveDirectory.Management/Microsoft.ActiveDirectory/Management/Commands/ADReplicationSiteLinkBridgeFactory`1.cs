using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADReplicationSiteLinkBridgeFactory<T> : ADObjectFactory<T>
	where T : ADReplicationSiteLinkBridge, new()
	{
		private readonly static string _rDNPrefix;

		private readonly static string[] _identityLdapAttributes;

		private readonly static IdentityResolverDelegate[] _identityResolvers;

		private static AttributeConverterEntry[] ADMappingTable;

		private static AttributeConverterEntry[] ADAMMappingTable;

		internal static IDictionary<ADServerType, MappingTable<AttributeConverterEntry>> AttributeTable
		{
			get
			{
				return ADObjectFactory<T>.AttributeTable;
			}
		}

		internal override IdentityResolverDelegate[] IdentityResolvers
		{
			get
			{
				return ADReplicationSiteLinkBridgeFactory<T>._identityResolvers;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				return ADReplicationSiteLinkBridgeFactory<T>._rDNPrefix;
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return "siteLinkBridge";
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", this.StructuralObjectClass);
			}
		}

		static ADReplicationSiteLinkBridgeFactory()
		{
			ADReplicationSiteLinkBridgeFactory<T>._rDNPrefix = "CN";
			string[] strArrays = new string[1];
			strArrays[0] = "name";
			ADReplicationSiteLinkBridgeFactory<T>._identityLdapAttributes = strArrays;
			IdentityResolverDelegate[] customIdentityResolver = new IdentityResolverDelegate[2];
			customIdentityResolver[0] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.DistinguishedNameIdentityResolver));
			IdentityResolverDelegate[] genericIdentityResolver = new IdentityResolverDelegate[2];
			genericIdentityResolver[0] = IdentityResolverMethods.GetGenericIdentityResolver(ADReplicationSiteLinkBridgeFactory<T>._identityLdapAttributes);
			genericIdentityResolver[1] = new IdentityResolverDelegate(IdentityResolverMethods.GuidSearchFilterIdentityResolver);
			customIdentityResolver[1] = IdentityResolverMethods.GetAggregatedIdentityResolver(ADOperator.Or, genericIdentityResolver);
			ADReplicationSiteLinkBridgeFactory<T>._identityResolvers = customIdentityResolver;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[2];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADReplicationSiteLinkBridgeFactory<T>.ADReplicationSiteLinkBridgePropertyMap.InterSiteTransportProtocol.PropertyName, ADReplicationSiteLinkBridgeFactory<T>.ADReplicationSiteLinkBridgePropertyMap.InterSiteTransportProtocol.ADAttribute, typeof(ADInterSiteTransportProtocolType), true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(ADTopologyUtil.ToExtendedFromDNToISTPEnum), null, new ToSearchFilterDelegate(SearchConverters.ToSearchNotSupported));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADReplicationSiteLinkBridgeFactory<T>.ADReplicationSiteLinkBridgePropertyMap.SiteLinksIncluded.PropertyName, ADReplicationSiteLinkBridgeFactory<T>.ADReplicationSiteLinkBridgePropertyMap.SiteLinksIncluded.ADAttribute, TypeConstants.ADReplicationSiteLink, true, TypeAdapterAccess.ReadWrite, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(ADTopologyUtil.ToDirectoryFromTopologyObjectNameListToDNList<ADReplicationSiteLinkFactory<ADReplicationSiteLink>, ADReplicationSiteLink>), new ToSearchFilterDelegate(ADTopologyUtil.ToSearchFromTopologyObjectNameToDN<ADReplicationSiteLinkFactory<ADReplicationSiteLink>, ADReplicationSiteLink>));
			ADReplicationSiteLinkBridgeFactory<T>.ADMappingTable = attributeConverterEntry;
			ADReplicationSiteLinkBridgeFactory<T>.ADAMMappingTable = ADReplicationSiteLinkBridgeFactory<T>.ADMappingTable;
			ADFactoryBase<T>.RegisterMappingTable(ADReplicationSiteLinkBridgeFactory<T>.ADMappingTable, ADServerType.ADDS);
			ADFactoryBase<T>.RegisterMappingTable(ADReplicationSiteLinkBridgeFactory<T>.ADAMMappingTable, ADServerType.ADLDS);
		}

		public ADReplicationSiteLinkBridgeFactory()
		{
			base.PreCommitPipeline.InsertAtEnd(new ADFactory<T>.FactoryCommitSubroutine(this.ADReplicationSiteLinkBridgePreCommitFSRoutine));
		}

		private bool ADReplicationSiteLinkBridgePreCommitFSRoutine(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			string parentPath = ADPathModule.GetParentPath(directoryObj.DistinguishedName, null, ADPathFormat.X500);
			if ((!directoryObj.Contains("siteLinkList") || directoryObj["siteLinkList"].Count <= 1) && operation == ADFactory<T>.DirectoryOperation.Create)
			{
				object[] objArray = new object[4];
				objArray[0] = "SiteLinkBridge";
				objArray[1] = 2;
				objArray[2] = "SiteLinks";
				objArray[3] = "SiteLinksIncluded";
				throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.ADInvalidAttributeValueCount, objArray));
			}
			else
			{
				if (directoryObj.Contains("siteLinkList") && directoryObj["siteLinkList"].Count > 0)
				{
					foreach (string item in directoryObj["siteLinkList"])
					{
						string str = ADPathModule.GetParentPath(item, null, ADPathFormat.X500);
						if (string.Compare(str, parentPath, StringComparison.OrdinalIgnoreCase) == 0)
						{
							continue;
						}
						object[] distinguishedName = new object[2];
						distinguishedName[0] = item;
						distinguishedName[1] = directoryObj.DistinguishedName;
						throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.SiteLinkAndSiteLinkBridgeDoNotShareSameTransportType, distinguishedName));
					}
				}
				return false;
			}
		}

		internal static class ADReplicationSiteLinkBridgePropertyMap
		{
			public readonly static PropertyMapEntry SiteLinksIncluded;

			public readonly static PropertyMapEntry InterSiteTransportProtocol;

			static ADReplicationSiteLinkBridgePropertyMap()
			{
				ADReplicationSiteLinkBridgeFactory<T>.ADReplicationSiteLinkBridgePropertyMap.SiteLinksIncluded = new PropertyMapEntry("SiteLinksIncluded", "siteLinkList", "siteLinkList");
				ADReplicationSiteLinkBridgeFactory<T>.ADReplicationSiteLinkBridgePropertyMap.InterSiteTransportProtocol = new PropertyMapEntry("InterSiteTransportProtocol", "distinguishedName", "distinguishedName");
			}
		}
	}
}