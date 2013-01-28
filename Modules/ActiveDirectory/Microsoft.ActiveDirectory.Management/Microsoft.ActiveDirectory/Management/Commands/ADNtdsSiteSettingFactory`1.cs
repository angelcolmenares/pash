using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADNtdsSiteSettingFactory<T> : ADObjectFactory<T>
	where T : ADNtdsSiteSetting, new()
	{
		internal const int IS_AUTO_TOPOLOGY_DISABLED = 1;

		internal const int IS_TOPL_CLEANUP_DISABLED = 2;

		internal const int IS_TOPL_MIN_HOPS_DISABLED = 4;

		internal const int IS_TOPL_DETECT_STALE_DISABLED = 8;

		internal const int IS_INTER_SITE_AUTO_TOPOLOGY_DISABLED = 16;

		internal const int IS_GROUP_CACHING_ENABLED = 32;

		internal const int FORCE_KCC_WHISTLER_BEHAVIOR = 64;

		internal const int FORCE_KCC_W2K_ELECTION = 128;

		internal const int RAND_BH_SELECTION_DISABLED = 0x100;

		internal const int IS_SCHEDULE_HASHING_ENABLED = 0x200;

		internal const int IS_REDUNDANT_SERVER_TOPOLOGY_ENABLED = 0x400;

		internal const int W2K3_IGNORE_SCHEDULES = 0x800;

		internal const int W2K3_BRIDGES_REQUIRED = 0x1000;

		private readonly static IADOPathNode _structuralObjectFilter;

		private readonly static string _rDNPrefix;

		private readonly static string _structuralObjectClass;

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
				return ADNtdsSiteSettingFactory<T>._identityResolvers;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				return ADNtdsSiteSettingFactory<T>._rDNPrefix;
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return ADNtdsSiteSettingFactory<T>._structuralObjectClass;
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADNtdsSiteSettingFactory<T>._structuralObjectFilter;
			}
		}

		static ADNtdsSiteSettingFactory()
		{
			ADNtdsSiteSettingFactory<T>._structuralObjectFilter = ADOPathUtil.CreateFilterClause(ADOperator.Like, "objectClass", "nTDSSiteSettings");
			ADNtdsSiteSettingFactory<T>._rDNPrefix = "CN";
			ADNtdsSiteSettingFactory<T>._structuralObjectClass = "nTDSSiteSettings";
			string[] strArrays = new string[1];
			strArrays[0] = "name";
			ADNtdsSiteSettingFactory<T>._identityLdapAttributes = strArrays;
			IdentityResolverDelegate[] customIdentityResolver = new IdentityResolverDelegate[2];
			customIdentityResolver[0] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.DistinguishedNameIdentityResolver));
			IdentityResolverDelegate[] genericIdentityResolver = new IdentityResolverDelegate[2];
			genericIdentityResolver[0] = IdentityResolverMethods.GetGenericIdentityResolver(ADNtdsSiteSettingFactory<T>._identityLdapAttributes);
			genericIdentityResolver[1] = new IdentityResolverDelegate(IdentityResolverMethods.GuidSearchFilterIdentityResolver);
			customIdentityResolver[1] = IdentityResolverMethods.GetAggregatedIdentityResolver(ADOperator.Or, genericIdentityResolver);
			ADNtdsSiteSettingFactory<T>._identityResolvers = customIdentityResolver;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[16];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.InterSiteTopologyGenerator.PropertyName, ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.InterSiteTopologyGenerator.ADAttribute, TypeConstants.ADDirectoryServer, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(ADTopologyUtil.ToDirectoryFromServerNameToNTDSSettings), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.ReplicationSchedule.PropertyName, ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.ReplicationSchedule.ADAttribute, TypeConstants.ByteArray, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedADReplicationScheduleFromBlob), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryBlobFromADReplicationSchedule), new ToSearchFilterDelegate(SearchConverters.ToSearchNotSupported));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.UniversalGroupCachingRefreshSite.PropertyName, ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.UniversalGroupCachingRefreshSite.ADAttribute, TypeConstants.ADReplicationSite, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(ADTopologyUtil.ToDirectoryFromSiteNameToDN), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.AutomaticTopologyGenerationEnabled.PropertyName, ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.AutomaticTopologyGenerationEnabled.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(1, true).Invoke), new ToDirectoryFormatDelegate(AttributeConverters.GetDelegateToDirectoryIntFromFlag(1, true).Invoke), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.TopologyCleanupEnabled.PropertyName, ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.TopologyCleanupEnabled.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(2, true).Invoke), new ToDirectoryFormatDelegate(AttributeConverters.GetDelegateToDirectoryIntFromFlag(2, true).Invoke), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[5] = new AttributeConverterEntry(ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.TopologyMinimumHopsEnabled.PropertyName, ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.TopologyMinimumHopsEnabled.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(4, true).Invoke), new ToDirectoryFormatDelegate(AttributeConverters.GetDelegateToDirectoryIntFromFlag(4, true).Invoke), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[6] = new AttributeConverterEntry(ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.TopologyDetectStaleEnabled.PropertyName, ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.TopologyDetectStaleEnabled.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(8, true).Invoke), new ToDirectoryFormatDelegate(AttributeConverters.GetDelegateToDirectoryIntFromFlag(8, true).Invoke), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[7] = new AttributeConverterEntry(ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.RedundantServerTopologyEnabled.PropertyName, ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.RedundantServerTopologyEnabled.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(0x400, false).Invoke), new ToDirectoryFormatDelegate(AttributeConverters.GetDelegateToDirectoryIntFromFlag(0x400, false).Invoke), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[8] = new AttributeConverterEntry(ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.UniversalGroupCachingEnabled.PropertyName, ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.UniversalGroupCachingEnabled.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(32, false).Invoke), new ToDirectoryFormatDelegate(AttributeConverters.GetDelegateToDirectoryIntFromFlag(32, false).Invoke), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[9] = new AttributeConverterEntry(ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.AutomaticInterSiteTopologyGenerationEnabled.PropertyName, ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.AutomaticInterSiteTopologyGenerationEnabled.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(16, true).Invoke), new ToDirectoryFormatDelegate(AttributeConverters.GetDelegateToDirectoryIntFromFlag(16, true).Invoke), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[10] = new AttributeConverterEntry(ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.WindowsServer2003KCCBehaviorEnabled.PropertyName, ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.WindowsServer2003KCCBehaviorEnabled.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(64, false).Invoke), new ToDirectoryFormatDelegate(AttributeConverters.GetDelegateToDirectoryIntFromFlag(64, false).Invoke), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[11] = new AttributeConverterEntry(ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.WindowsServer2000KCCISTGSelectionBehaviorEnabled.PropertyName, ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.WindowsServer2000KCCISTGSelectionBehaviorEnabled.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(128, false).Invoke), new ToDirectoryFormatDelegate(AttributeConverters.GetDelegateToDirectoryIntFromFlag(128, false).Invoke), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[12] = new AttributeConverterEntry(ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.WindowsServer2000BridgeheadSelectionMethodEnabled.PropertyName, ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.WindowsServer2000BridgeheadSelectionMethodEnabled.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(0x100, true).Invoke), new ToDirectoryFormatDelegate(AttributeConverters.GetDelegateToDirectoryIntFromFlag(0x100, true).Invoke), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[13] = new AttributeConverterEntry(ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.ScheduleHashingEnabled.PropertyName, ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.ScheduleHashingEnabled.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(0x200, false).Invoke), new ToDirectoryFormatDelegate(AttributeConverters.GetDelegateToDirectoryIntFromFlag(0x200, false).Invoke), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[14] = new AttributeConverterEntry(ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.WindowsServer2003KCCIgnoreScheduleEnabled.PropertyName, ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.WindowsServer2003KCCIgnoreScheduleEnabled.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(0x800, false).Invoke), new ToDirectoryFormatDelegate(AttributeConverters.GetDelegateToDirectoryIntFromFlag(0x800, false).Invoke), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[15] = new AttributeConverterEntry(ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.WindowsServer2003KCCSiteLinkBridgingEnabled.PropertyName, ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.WindowsServer2003KCCSiteLinkBridgingEnabled.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(0x1000, false).Invoke), new ToDirectoryFormatDelegate(AttributeConverters.GetDelegateToDirectoryIntFromFlag(0x1000, false).Invoke), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			ADNtdsSiteSettingFactory<T>.ADMappingTable = attributeConverterEntry;
			ADNtdsSiteSettingFactory<T>.ADAMMappingTable = ADNtdsSiteSettingFactory<T>.ADMappingTable;
			ADFactoryBase<T>.RegisterMappingTable(ADNtdsSiteSettingFactory<T>.ADMappingTable, ADServerType.ADDS);
			ADFactoryBase<T>.RegisterMappingTable(ADNtdsSiteSettingFactory<T>.ADAMMappingTable, ADServerType.ADLDS);
		}

		public ADNtdsSiteSettingFactory()
		{
		}

		internal static class ADNtdsSiteSettingPropertyMap
		{
			public readonly static PropertyMapEntry InterSiteTopologyGenerator;

			public readonly static PropertyMapEntry ReplicationSchedule;

			public readonly static PropertyMapEntry UniversalGroupCachingRefreshSite;

			public readonly static PropertyMapEntry AutomaticTopologyGenerationEnabled;

			public readonly static PropertyMapEntry TopologyCleanupEnabled;

			public readonly static PropertyMapEntry TopologyMinimumHopsEnabled;

			public readonly static PropertyMapEntry TopologyDetectStaleEnabled;

			public readonly static PropertyMapEntry RedundantServerTopologyEnabled;

			public readonly static PropertyMapEntry UniversalGroupCachingEnabled;

			public readonly static PropertyMapEntry AutomaticInterSiteTopologyGenerationEnabled;

			public readonly static PropertyMapEntry WindowsServer2003KCCBehaviorEnabled;

			public readonly static PropertyMapEntry WindowsServer2000KCCISTGSelectionBehaviorEnabled;

			public readonly static PropertyMapEntry WindowsServer2000BridgeheadSelectionMethodEnabled;

			public readonly static PropertyMapEntry ScheduleHashingEnabled;

			public readonly static PropertyMapEntry WindowsServer2003KCCIgnoreScheduleEnabled;

			public readonly static PropertyMapEntry WindowsServer2003KCCSiteLinkBridgingEnabled;

			static ADNtdsSiteSettingPropertyMap()
			{
				ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.InterSiteTopologyGenerator = new PropertyMapEntry("InterSiteTopologyGenerator", "interSiteTopologyGenerator", "interSiteTopologyGenerator");
				ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.ReplicationSchedule = new PropertyMapEntry("ReplicationSchedule", "schedule", "schedule");
				ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.UniversalGroupCachingRefreshSite = new PropertyMapEntry("UniversalGroupCachingRefreshSite", "msDS-Preferred-GC-Site", "msDS-Preferred-GC-Site");
				ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.AutomaticTopologyGenerationEnabled = new PropertyMapEntry("AutomaticTopologyGenerationEnabled", "Options", "Options");
				ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.TopologyCleanupEnabled = new PropertyMapEntry("TopologyCleanupEnabled", "Options", "Options");
				ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.TopologyMinimumHopsEnabled = new PropertyMapEntry("TopologyMinimumHopsEnabled", "Options", "Options");
				ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.TopologyDetectStaleEnabled = new PropertyMapEntry("TopologyDetectStaleEnabled", "Options", "Options");
				ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.RedundantServerTopologyEnabled = new PropertyMapEntry("RedundantServerTopologyEnabled", "Options", "Options");
				ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.UniversalGroupCachingEnabled = new PropertyMapEntry("UniversalGroupCachingEnabled", "Options", "Options");
				ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.AutomaticInterSiteTopologyGenerationEnabled = new PropertyMapEntry("AutomaticInterSiteTopologyGenerationEnabled", "Options", "Options");
				ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.WindowsServer2003KCCBehaviorEnabled = new PropertyMapEntry("WindowsServer2003KCCBehaviorEnabled", "Options", "Options");
				ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.WindowsServer2000KCCISTGSelectionBehaviorEnabled = new PropertyMapEntry("WindowsServer2000KCCISTGSelectionBehaviorEnabled", "Options", "Options");
				ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.WindowsServer2000BridgeheadSelectionMethodEnabled = new PropertyMapEntry("WindowsServer2000BridgeheadSelectionMethodEnabled", "Options", "Options");
				ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.ScheduleHashingEnabled = new PropertyMapEntry("ScheduleHashingEnabled", "Options", "Options");
				ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.WindowsServer2003KCCIgnoreScheduleEnabled = new PropertyMapEntry("WindowsServer2003KCCIgnoreScheduleEnabled", "Options", "Options");
				ADNtdsSiteSettingFactory<T>.ADNtdsSiteSettingPropertyMap.WindowsServer2003KCCSiteLinkBridgingEnabled = new PropertyMapEntry("WindowsServer2003KCCSiteLinkBridgingEnabled", "Options", "Options");
			}
		}
	}
}