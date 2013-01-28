using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADReplicationQueueOperationFactory<T> : ADXmlAttributeFactory<T>
	where T : ADReplicationQueueOperation, new()
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

		static ADReplicationQueueOperationFactory()
		{
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[9];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.Server.PropertyName, ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.Server.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(ADReplicationUtil.ToExtendedServerFromSessionInfo), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.Partner.PropertyName, ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.Partner.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.PartnerAddress.PropertyName, ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.PartnerAddress.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.Partition.PropertyName, ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.Partition.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.EnqueueTime.PropertyName, ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.EnqueueTime.ADAttribute, TypeConstants.DateTime, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseDateTimeFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[5] = new AttributeConverterEntry(ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.OperationID.PropertyName, ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.OperationID.ADAttribute, TypeConstants.Int, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseIntFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[6] = new AttributeConverterEntry(ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.OperationType.PropertyName, ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.OperationType.ADAttribute, TypeConstants.ADReplicationOperationType, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseIntFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[7] = new AttributeConverterEntry(ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.Options.PropertyName, ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.Options.ADAttribute, TypeConstants.Int, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseIntFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[8] = new AttributeConverterEntry(ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.Priority.PropertyName, ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.Priority.ADAttribute, TypeConstants.Int, false, TypeAdapterAccess.Read, true, AttributeSet.Default, AttributeConverters.GetToExtendedFromStringConverter(new AttributeConverters.StringParserDelegate(AttributeConverters.ParseIntFromString)), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			ADReplicationQueueOperationFactory<T>.ADMappingTable = attributeConverterEntry;
			ADReplicationQueueOperationFactory<T>.ADAMMappingTable = ADReplicationQueueOperationFactory<T>.ADMappingTable;
			ADFactoryBase<T>.RegisterMappingTable(ADReplicationQueueOperationFactory<T>.ADAMMappingTable, ADServerType.ADLDS);
			ADFactoryBase<T>.RegisterMappingTable(ADReplicationQueueOperationFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADReplicationQueueOperationFactory()
		{
		}

		internal override IEnumerable<T> ApplyClientSideFilter(IEnumerable<T> objectList)
		{
			return base.ApplyClientSideFilter(this.ApplyPartitionFilter(objectList));
		}

		private IEnumerable<T> ApplyPartitionFilter(IEnumerable<T> objectList)
		{
			string[] item = base.CmdletSessionInfo.CmdletParameters["PartitionFilter"] as string[];
			if (item == null)
			{
				string[] strArrays = new string[1];
				strArrays[0] = "Default";
				item = strArrays;
			}
			if (!item.Contains<string>("*"))
			{
				IEnumerable<string> strs = ADForestPartitionInfo.ConstructPartitionList(base.CmdletSessionInfo.ADRootDSE, item, false);
				List<T> ts = new List<T>();
				foreach (T t in objectList)
				{
					if (!strs.Contains<string>(t.Partition))
					{
						continue;
					}
					ts.Add(t);
				}
				return ts;
			}
			else
			{
				return objectList;
			}
		}

		private static class ADReplicationQueueOperationPropertyMap
		{
			public readonly static PropertyMapEntry Server;

			public readonly static PropertyMapEntry Partner;

			public readonly static PropertyMapEntry PartnerAddress;

			public readonly static PropertyMapEntry Partition;

			public readonly static PropertyMapEntry EnqueueTime;

			public readonly static PropertyMapEntry OperationID;

			public readonly static PropertyMapEntry OperationType;

			public readonly static PropertyMapEntry Options;

			public readonly static PropertyMapEntry Priority;

			static ADReplicationQueueOperationPropertyMap()
			{
				ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.Server = new PropertyMapEntry("Server", string.Empty, string.Empty);
				ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.Partner = new PropertyMapEntry("Partner", "pszDsaDN", "pszDsaDN");
				ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.PartnerAddress = new PropertyMapEntry("PartnerAddress", "pszDsaAddress", "pszDsaAddress");
				ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.Partition = new PropertyMapEntry("Partition", "pszNamingContext", "pszNamingContext");
				ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.EnqueueTime = new PropertyMapEntry("EnqueueTime", "ftimeEnqueued", "ftimeEnqueued");
				ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.OperationID = new PropertyMapEntry("OperationID", "ulSerialNumber", "ulSerialNumber");
				ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.OperationType = new PropertyMapEntry("OperationType", "OpType", "OpType");
				ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.Options = new PropertyMapEntry("Options", "ulOptions", "ulOptions");
				ADReplicationQueueOperationFactory<T>.ADReplicationQueueOperationPropertyMap.Priority = new PropertyMapEntry("Priority", "ulPriority", "ulPriority");
			}
		}
	}
}