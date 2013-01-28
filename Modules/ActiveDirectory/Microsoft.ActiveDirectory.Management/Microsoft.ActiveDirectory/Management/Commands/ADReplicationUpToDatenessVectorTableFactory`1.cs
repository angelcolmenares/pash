using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADReplicationUpToDatenessVectorTableFactory<T> : ADXmlAttributeFactory<T>
	where T : ADReplicationUpToDatenessVectorTable, new()
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

		static ADReplicationUpToDatenessVectorTableFactory()
		{
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[7];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADReplicationUpToDatenessVectorTableFactory<T>.ADReplicationUpToDatenessVectorTablePropertyMap.Server.PropertyName, ADReplicationUpToDatenessVectorTableFactory<T>.ADReplicationUpToDatenessVectorTablePropertyMap.Server.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(ADReplicationUtil.ToExtendedServerFromSessionInfo), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADReplicationUpToDatenessVectorTableFactory<T>.ADReplicationUpToDatenessVectorTablePropertyMap.Partner.PropertyName, ADReplicationUpToDatenessVectorTableFactory<T>.ADReplicationUpToDatenessVectorTablePropertyMap.Partner.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADReplicationUpToDatenessVectorTableFactory<T>.ADReplicationUpToDatenessVectorTablePropertyMap.PartnerInvocationId.PropertyName, ADReplicationUpToDatenessVectorTableFactory<T>.ADReplicationUpToDatenessVectorTablePropertyMap.PartnerInvocationId.ADAttribute, TypeConstants.Guid, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseGuidFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADReplicationUpToDatenessVectorTableFactory<T>.ADReplicationUpToDatenessVectorTablePropertyMap.Partition.PropertyName, ADReplicationUpToDatenessVectorTableFactory<T>.ADReplicationUpToDatenessVectorTablePropertyMap.Partition.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADReplicationUpToDatenessVectorTableFactory<T>.ADReplicationUpToDatenessVectorTablePropertyMap.PartitionGuid.PropertyName, ADReplicationUpToDatenessVectorTableFactory<T>.ADReplicationUpToDatenessVectorTablePropertyMap.PartitionGuid.ADAttribute, TypeConstants.Guid, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[5] = new AttributeConverterEntry(ADReplicationUpToDatenessVectorTableFactory<T>.ADReplicationUpToDatenessVectorTablePropertyMap.LastReplicationSuccess.PropertyName, ADReplicationUpToDatenessVectorTableFactory<T>.ADReplicationUpToDatenessVectorTablePropertyMap.LastReplicationSuccess.ADAttribute, TypeConstants.DateTime, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseDateTimeFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[6] = new AttributeConverterEntry(ADReplicationUpToDatenessVectorTableFactory<T>.ADReplicationUpToDatenessVectorTablePropertyMap.UsnFilter.PropertyName, ADReplicationUpToDatenessVectorTableFactory<T>.ADReplicationUpToDatenessVectorTablePropertyMap.UsnFilter.ADAttribute, TypeConstants.Long, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseLongFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			ADReplicationUpToDatenessVectorTableFactory<T>.ADMappingTable = attributeConverterEntry;
			ADReplicationUpToDatenessVectorTableFactory<T>.ADAMMappingTable = ADReplicationUpToDatenessVectorTableFactory<T>.ADMappingTable;
			ADFactoryBase<T>.RegisterMappingTable(ADReplicationUpToDatenessVectorTableFactory<T>.ADAMMappingTable, ADServerType.ADLDS);
			ADFactoryBase<T>.RegisterMappingTable(ADReplicationUpToDatenessVectorTableFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADReplicationUpToDatenessVectorTableFactory()
		{
		}

		private static class ADReplicationUpToDatenessVectorTablePropertyMap
		{
			public readonly static PropertyMapEntry Server;

			public readonly static PropertyMapEntry Partner;

			public readonly static PropertyMapEntry PartnerInvocationId;

			public readonly static PropertyMapEntry Partition;

			public readonly static PropertyMapEntry PartitionGuid;

			public readonly static PropertyMapEntry LastReplicationSuccess;

			public readonly static PropertyMapEntry UsnFilter;

			static ADReplicationUpToDatenessVectorTablePropertyMap()
			{
				ADReplicationUpToDatenessVectorTableFactory<T>.ADReplicationUpToDatenessVectorTablePropertyMap.Server = new PropertyMapEntry("Server", string.Empty, string.Empty);
				ADReplicationUpToDatenessVectorTableFactory<T>.ADReplicationUpToDatenessVectorTablePropertyMap.Partner = new PropertyMapEntry("Partner", "pszSourceDsaDN", "pszSourceDsaDN");
				ADReplicationUpToDatenessVectorTableFactory<T>.ADReplicationUpToDatenessVectorTablePropertyMap.PartnerInvocationId = new PropertyMapEntry("PartnerInvocationId", "uuidSourceDsaInvocationID", "uuidSourceDsaInvocationID");
				ADReplicationUpToDatenessVectorTableFactory<T>.ADReplicationUpToDatenessVectorTablePropertyMap.Partition = new PropertyMapEntry("Partition", "distinguishedName", "distinguishedName");
				ADReplicationUpToDatenessVectorTableFactory<T>.ADReplicationUpToDatenessVectorTablePropertyMap.PartitionGuid = new PropertyMapEntry("PartitionGuid", "objectGUID", "objectGUID");
				ADReplicationUpToDatenessVectorTableFactory<T>.ADReplicationUpToDatenessVectorTablePropertyMap.LastReplicationSuccess = new PropertyMapEntry("LastReplicationSuccess", "ftimeLastSyncSuccess", "ftimeLastSyncSuccess");
				ADReplicationUpToDatenessVectorTableFactory<T>.ADReplicationUpToDatenessVectorTablePropertyMap.UsnFilter = new PropertyMapEntry("UsnFilter", "usnAttributeFilter", "usnAttributeFilter");
			}
		}
	}
}