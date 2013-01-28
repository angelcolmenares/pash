using Microsoft.ActiveDirectory.Management;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public abstract class ADPartitionFactory<T> : ADObjectFactory<T>
	where T : ADPartition, new()
	{
		private static AttributeConverterEntry[] ADMappingTable;

		internal static IDictionary<ADServerType, MappingTable<AttributeConverterEntry>> AttributeTable
		{
			get
			{
				return ADObjectFactory<T>.AttributeTable;
			}
		}

		static ADPartitionFactory()
		{
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[7];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADPartitionFactory<T>.ADPartitionPropertyMap.SubordinateReferences.PropertyName, ADPartitionFactory<T>.ADPartitionPropertyMap.SubordinateReferences.ADAttribute, TypeConstants.ADPartition, false, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryMultivalueObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADPartitionFactory<T>.ADPartitionPropertyMap.DNSRoot.PropertyName, ADPartitionFactory<T>.ADPartitionPropertyMap.DNSRoot.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADPartitionFactory<T>.ADPartitionPropertyMap.LostAndFoundContainer.PropertyName, ADPartitionFactory<T>.ADPartitionPropertyMap.LostAndFoundContainer.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADPartitionFactory<T>.ADPartitionPropertyMap.DeletedObjectsContainer.PropertyName, ADPartitionFactory<T>.ADPartitionPropertyMap.DeletedObjectsContainer.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADPartitionFactory<T>.ADPartitionPropertyMap.QuotasContainer.PropertyName, ADPartitionFactory<T>.ADPartitionPropertyMap.QuotasContainer.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[5] = new AttributeConverterEntry(ADPartitionFactory<T>.ADPartitionPropertyMap.ReadOnlyReplicaDirectoryServers.PropertyName, ADPartitionFactory<T>.ADPartitionPropertyMap.ReadOnlyReplicaDirectoryServers.ADAttribute, TypeConstants.ADDirectoryServer, false, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryMultivalueObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[6] = new AttributeConverterEntry(ADPartitionFactory<T>.ADPartitionPropertyMap.ReplicaDirectoryServers.PropertyName, ADPartitionFactory<T>.ADPartitionPropertyMap.ReplicaDirectoryServers.ADAttribute, TypeConstants.ADDirectoryServer, false, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryMultivalueObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			ADPartitionFactory<T>.ADMappingTable = attributeConverterEntry;
			ADFactoryBase<T>.RegisterMappingTable(ADPartitionFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADPartitionFactory()
		{
		}

		internal static class ADPartitionPropertyMap
		{
			internal readonly static PropertyMapEntry SubordinateReferences;

			internal readonly static PropertyMapEntry DNSRoot;

			internal readonly static PropertyMapEntry LostAndFoundContainer;

			internal readonly static PropertyMapEntry DeletedObjectsContainer;

			internal readonly static PropertyMapEntry QuotasContainer;

			internal readonly static PropertyMapEntry ReadOnlyReplicaDirectoryServers;

			internal readonly static PropertyMapEntry ReplicaDirectoryServers;

			static ADPartitionPropertyMap()
			{
				ADPartitionFactory<T>.ADPartitionPropertyMap.SubordinateReferences = new PropertyMapEntry("SubordinateReferences", "SubordinateReferences", "SubordinateReferences");
				ADPartitionFactory<T>.ADPartitionPropertyMap.DNSRoot = new PropertyMapEntry("DNSRoot", "DNSRoot", "DNSRoot");
				ADPartitionFactory<T>.ADPartitionPropertyMap.LostAndFoundContainer = new PropertyMapEntry("LostAndFoundContainer", "LostAndFoundContainer", "LostAndFoundContainer");
				ADPartitionFactory<T>.ADPartitionPropertyMap.DeletedObjectsContainer = new PropertyMapEntry("DeletedObjectsContainer", "DeletedObjectsContainer", "DeletedObjectsContainer");
				ADPartitionFactory<T>.ADPartitionPropertyMap.QuotasContainer = new PropertyMapEntry("QuotasContainer", "QuotasContainer", "QuotasContainer");
				ADPartitionFactory<T>.ADPartitionPropertyMap.ReadOnlyReplicaDirectoryServers = new PropertyMapEntry("ReadOnlyReplicaDirectoryServers", "ReadOnlyReplicaDirectoryServers", "ReadOnlyReplicaDirectoryServers");
				ADPartitionFactory<T>.ADPartitionPropertyMap.ReplicaDirectoryServers = new PropertyMapEntry("ReplicaDirectoryServers", "ReplicaDirectoryServers", "ReplicaDirectoryServers");
			}
		}
	}
}