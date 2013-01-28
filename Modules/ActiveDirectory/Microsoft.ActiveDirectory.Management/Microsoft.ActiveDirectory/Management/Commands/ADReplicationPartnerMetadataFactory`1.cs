using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADReplicationPartnerMetadataFactory<T> : ADXmlAttributeFactory<T>
	where T : ADReplicationPartnerMetadata, new()
	{
		private static AttributeConverterEntry[] ADMappingTable;

		private static AttributeConverterEntry[] ADAMMappingTable;

		internal static IDictionary<ADServerType, MappingTable<AttributeConverterEntry>> AttributeTable
		{
			get
			{
				return ADFactoryBase<T>.AttributeTable;
			}
		}

		static ADReplicationPartnerMetadataFactory()
		{
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[24];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.Server.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.Server.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(ADReplicationUtil.ToExtendedServerFromSessionInfo), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.Partner.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.Partner.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.PartnerAddress.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.PartnerAddress.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.PartnerGuid.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.PartnerGuid.ADAttribute, TypeConstants.Guid, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseGuidFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.PartnerInvocationId.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.PartnerInvocationId.ADAttribute, TypeConstants.Guid, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseGuidFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[5] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.PartnerType.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.PartnerType.ADAttribute, TypeConstants.ADPartnerType, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(ADReplicationUtil.ToExtendedReplicationPartnerType), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[6] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.Partition.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.Partition.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[7] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.PartitionGuid.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.PartitionGuid.ADAttribute, TypeConstants.Guid, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[8] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.LastReplicationAttempt.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.LastReplicationAttempt.ADAttribute, TypeConstants.DateTime, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseDateTimeFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[9] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.LastReplicationResult.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.LastReplicationResult.ADAttribute, TypeConstants.Int, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseIntFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[10] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.LastReplicationSuccess.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.LastReplicationSuccess.ADAttribute, TypeConstants.DateTime, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseDateTimeFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[11] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.ConsecutiveReplicationFailures.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.ConsecutiveReplicationFailures.ADAttribute, TypeConstants.Int, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseIntFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[12] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.LastChangeUsn.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.LastChangeUsn.ADAttribute, TypeConstants.Long, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseLongFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[13] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.UsnFilter.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.UsnFilter.ADAttribute, TypeConstants.Long, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseLongFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[14] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.IntersiteTransport.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.IntersiteTransport.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[15] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.IntersiteTransportGuid.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.IntersiteTransportGuid.ADAttribute, TypeConstants.Guid, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseGuidFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[16] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.IntersiteTransportType.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.IntersiteTransportType.ADAttribute, TypeConstants.ADInterSiteTransportProtocolType, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(ADReplicationUtil.ToExtendedTransportTypeFromDrsOptions), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[17] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.CompressChanges.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.CompressChanges.ADAttribute, TypeConstants.Bool, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetDelegateToExtendedFlagFromInt(0x10000000, false), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[18] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.DisableScheduledSync.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.DisableScheduledSync.ADAttribute, TypeConstants.Bool, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetDelegateToExtendedFlagFromInt(0x8000000, false), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[19] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.IgnoreChangeNotifications.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.IgnoreChangeNotifications.ADAttribute, TypeConstants.Bool, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetDelegateToExtendedFlagFromInt(0x4000000, false), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[20] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.ScheduledSync.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.ScheduledSync.ADAttribute, TypeConstants.Bool, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetDelegateToExtendedFlagFromInt(64, false), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[21] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.SyncOnStartup.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.SyncOnStartup.ADAttribute, TypeConstants.Bool, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetDelegateToExtendedFlagFromInt(32, false), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[22] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.TwoWaySync.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.TwoWaySync.ADAttribute, TypeConstants.Bool, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetDelegateToExtendedFlagFromInt(0x200, false), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[23] = new AttributeConverterEntry(ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.Writable.PropertyName, ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.Writable.ADAttribute, TypeConstants.Bool, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetDelegateToExtendedFlagFromInt(16, false), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			ADReplicationPartnerMetadataFactory<T>.ADMappingTable = attributeConverterEntry;
			ADReplicationPartnerMetadataFactory<T>.ADAMMappingTable = ADReplicationPartnerMetadataFactory<T>.ADMappingTable;
			ADFactoryBase<T>.RegisterMappingTable(ADReplicationPartnerMetadataFactory<T>.ADAMMappingTable, ADServerType.ADLDS);
			ADFactoryBase<T>.RegisterMappingTable(ADReplicationPartnerMetadataFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADReplicationPartnerMetadataFactory()
		{
		}

		private static class ADReplicationPartnerMetadataPropertyMap
		{
			public readonly static PropertyMapEntry Server;

			public readonly static PropertyMapEntry Partner;

			public readonly static PropertyMapEntry PartnerAddress;

			public readonly static PropertyMapEntry PartnerGuid;

			public readonly static PropertyMapEntry PartnerInvocationId;

			public readonly static PropertyMapEntry PartnerType;

			public readonly static PropertyMapEntry Partition;

			public readonly static PropertyMapEntry PartitionGuid;

			public readonly static PropertyMapEntry LastReplicationAttempt;

			public readonly static PropertyMapEntry LastReplicationResult;

			public readonly static PropertyMapEntry LastReplicationSuccess;

			public readonly static PropertyMapEntry ConsecutiveReplicationFailures;

			public readonly static PropertyMapEntry LastChangeUsn;

			public readonly static PropertyMapEntry UsnFilter;

			public readonly static PropertyMapEntry IntersiteTransport;

			public readonly static PropertyMapEntry IntersiteTransportGuid;

			public readonly static PropertyMapEntry IntersiteTransportType;

			public readonly static PropertyMapEntry CompressChanges;

			public readonly static PropertyMapEntry DisableScheduledSync;

			public readonly static PropertyMapEntry IgnoreChangeNotifications;

			public readonly static PropertyMapEntry ScheduledSync;

			public readonly static PropertyMapEntry SyncOnStartup;

			public readonly static PropertyMapEntry TwoWaySync;

			public readonly static PropertyMapEntry Writable;

			static ADReplicationPartnerMetadataPropertyMap()
			{
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.Server = new PropertyMapEntry("Server", string.Empty, string.Empty);
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.Partner = new PropertyMapEntry("Partner", "pszSourceDsaDN", "pszSourceDsaDN");
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.PartnerAddress = new PropertyMapEntry("PartnerAddress", "pszSourceDsaAddress", "pszSourceDsaAddress");
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.PartnerGuid = new PropertyMapEntry("PartnerGuid", "uuidSourceDsaObjGuid", "uuidSourceDsaObjGuid");
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.PartnerInvocationId = new PropertyMapEntry("PartnerInvocationId", "uuidSourceDsaInvocationID", "uuidSourceDsaInvocationID");
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.PartnerType = new PropertyMapEntry("PartnerType", "sourceXmlAttribute", "sourceXmlAttribute");
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.Partition = new PropertyMapEntry("Partition", "pszNamingContext", "pszNamingContext");
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.PartitionGuid = new PropertyMapEntry("PartitionGuid", "objectGUID", "objectGUID");
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.LastReplicationAttempt = new PropertyMapEntry("LastReplicationAttempt", "ftimeLastSyncAttempt", "ftimeLastSyncAttempt");
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.LastReplicationResult = new PropertyMapEntry("LastReplicationResult", "dwLastSyncResult", "dwLastSyncResult");
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.LastReplicationSuccess = new PropertyMapEntry("LastReplicationSuccess", "ftimeLastSyncSuccess", "ftimeLastSyncSuccess");
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.ConsecutiveReplicationFailures = new PropertyMapEntry("ConsecutiveReplicationFailures", "cNumConsecutiveSyncFailures", "cNumConsecutiveSyncFailures");
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.LastChangeUsn = new PropertyMapEntry("LastChangeUsn", "usnLastObjChangeSynced", "usnLastObjChangeSynced");
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.UsnFilter = new PropertyMapEntry("UsnFilter", "usnAttributeFilter", "usnAttributeFilter");
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.IntersiteTransport = new PropertyMapEntry("IntersiteTransport", "pszAsyncIntersiteTransportDN", "pszAsyncIntersiteTransportDN");
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.IntersiteTransportGuid = new PropertyMapEntry("IntersiteTransportGuid", "uuidAsyncIntersiteTransportObjGuid", "uuidAsyncIntersiteTransportObjGuid");
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.IntersiteTransportType = new PropertyMapEntry("IntersiteTransportType", "dwReplicaFlags", "dwReplicaFlags");
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.CompressChanges = new PropertyMapEntry("CompressChanges", "dwReplicaFlags", "dwReplicaFlags");
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.DisableScheduledSync = new PropertyMapEntry("DisableScheduledSync", "dwReplicaFlags", "dwReplicaFlags");
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.IgnoreChangeNotifications = new PropertyMapEntry("IgnoreChangeNotifications", "dwReplicaFlags", "dwReplicaFlags");
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.ScheduledSync = new PropertyMapEntry("ScheduledSync", "dwReplicaFlags", "dwReplicaFlags");
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.SyncOnStartup = new PropertyMapEntry("SyncOnStartup", "dwReplicaFlags", "dwReplicaFlags");
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.TwoWaySync = new PropertyMapEntry("TwoWaySync", "dwReplicaFlags", "dwReplicaFlags");
				ADReplicationPartnerMetadataFactory<T>.ADReplicationPartnerMetadataPropertyMap.Writable = new PropertyMapEntry("Writable", "dwReplicaFlags", "dwReplicaFlags");
			}
		}
	}
}