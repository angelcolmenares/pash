using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADReplicationFailureFactory<T> : ADXmlAttributeFactory<T>
	where T : ADReplicationFailure, new()
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

		static ADReplicationFailureFactory()
		{
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[7];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADReplicationFailureFactory<T>.ADReplicationFailurePropertyMap.Server.PropertyName, ADReplicationFailureFactory<T>.ADReplicationFailurePropertyMap.Server.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(ADReplicationUtil.ToExtendedServerFromSessionInfo), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADReplicationFailureFactory<T>.ADReplicationFailurePropertyMap.Partner.PropertyName, ADReplicationFailureFactory<T>.ADReplicationFailurePropertyMap.Partner.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADReplicationFailureFactory<T>.ADReplicationFailurePropertyMap.PartnerGuid.PropertyName, ADReplicationFailureFactory<T>.ADReplicationFailurePropertyMap.PartnerGuid.ADAttribute, TypeConstants.Guid, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseGuidFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADReplicationFailureFactory<T>.ADReplicationFailurePropertyMap.FailureCount.PropertyName, ADReplicationFailureFactory<T>.ADReplicationFailurePropertyMap.FailureCount.ADAttribute, TypeConstants.Int, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseIntFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADReplicationFailureFactory<T>.ADReplicationFailurePropertyMap.FailureType.PropertyName, ADReplicationFailureFactory<T>.ADReplicationFailurePropertyMap.FailureType.ADAttribute, TypeConstants.ADPartnerType, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(ADReplicationUtil.ToExtendedReplicationFailureType), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[5] = new AttributeConverterEntry(ADReplicationFailureFactory<T>.ADReplicationFailurePropertyMap.FirstFailureTime.PropertyName, ADReplicationFailureFactory<T>.ADReplicationFailurePropertyMap.FirstFailureTime.ADAttribute, TypeConstants.DateTime, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseDateTimeFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[6] = new AttributeConverterEntry(ADReplicationFailureFactory<T>.ADReplicationFailurePropertyMap.LastError.PropertyName, ADReplicationFailureFactory<T>.ADReplicationFailurePropertyMap.LastError.ADAttribute, TypeConstants.Int, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseIntFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			ADReplicationFailureFactory<T>.ADMappingTable = attributeConverterEntry;
			ADReplicationFailureFactory<T>.ADAMMappingTable = ADReplicationFailureFactory<T>.ADMappingTable;
			ADFactoryBase<T>.RegisterMappingTable(ADReplicationFailureFactory<T>.ADAMMappingTable, ADServerType.ADLDS);
			ADFactoryBase<T>.RegisterMappingTable(ADReplicationFailureFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADReplicationFailureFactory()
		{
		}

		private static class ADReplicationFailurePropertyMap
		{
			public readonly static PropertyMapEntry Server;

			public readonly static PropertyMapEntry Partner;

			public readonly static PropertyMapEntry PartnerGuid;

			public readonly static PropertyMapEntry FailureCount;

			public readonly static PropertyMapEntry FailureType;

			public readonly static PropertyMapEntry FirstFailureTime;

			public readonly static PropertyMapEntry LastError;

			static ADReplicationFailurePropertyMap()
			{
				ADReplicationFailureFactory<T>.ADReplicationFailurePropertyMap.Server = new PropertyMapEntry("Server", string.Empty, string.Empty);
				ADReplicationFailureFactory<T>.ADReplicationFailurePropertyMap.Partner = new PropertyMapEntry("Partner", "pszDsaDN", "pszDsaDN");
				ADReplicationFailureFactory<T>.ADReplicationFailurePropertyMap.PartnerGuid = new PropertyMapEntry("PartnerGuid", "uuidDsaObjGuid", "uuidDsaObjGuid");
				ADReplicationFailureFactory<T>.ADReplicationFailurePropertyMap.FailureCount = new PropertyMapEntry("FailureCount", "cNumFailures", "cNumFailures");
				ADReplicationFailureFactory<T>.ADReplicationFailurePropertyMap.FailureType = new PropertyMapEntry("FailureType", "sourceXmlAttribute", "sourceXmlAttribute");
				ADReplicationFailureFactory<T>.ADReplicationFailurePropertyMap.FirstFailureTime = new PropertyMapEntry("FirstFailureTime", "ftimeFirstFailure", "ftimeFirstFailure");
				ADReplicationFailureFactory<T>.ADReplicationFailurePropertyMap.LastError = new PropertyMapEntry("LastError", "dwLastResult", "dwLastResult");
			}
		}
	}
}