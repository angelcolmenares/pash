using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADOrganizationalUnitFactory<T> : ADObjectFactory<T>
	where T : ADOrganizationalUnit, new()
	{
		private readonly static IADOPathNode _structuralObjectFilter;

		private readonly static string _rDNPrefix;

		private readonly static string _structuralObjectClass;

		private static AttributeConverterEntry[] ADMappingTable;

		private static AttributeConverterEntry[] ADAMMappingTable;

		internal static IDictionary<ADServerType, MappingTable<AttributeConverterEntry>> AttributeTable
		{
			get
			{
				return ADObjectFactory<T>.AttributeTable;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				return ADOrganizationalUnitFactory<T>._rDNPrefix;
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return ADOrganizationalUnitFactory<T>._structuralObjectClass;
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADOrganizationalUnitFactory<T>._structuralObjectFilter;
			}
		}

		static ADOrganizationalUnitFactory()
		{
			ADOrganizationalUnitFactory<T>._structuralObjectFilter = ADOPathUtil.CreateFilterClause(ADOperator.Like, "objectClass", "organizationalUnit");
			ADOrganizationalUnitFactory<T>._rDNPrefix = "OU";
			ADOrganizationalUnitFactory<T>._structuralObjectClass = "organizationalUnit";
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[7];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.ManagedBy.PropertyName, ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.ManagedBy.ADAttribute, TypeConstants.ADPrincipal, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryFromADObjectToDN<ADPrincipalFactory<ADPrincipal>, ADPrincipal>), new ToSearchFilterDelegate(SearchConverters.ToSearchFromADObjectToDN<ADPrincipalFactory<ADPrincipal>, ADPrincipal>));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.Street.PropertyName, ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.Street.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.PostalCode.PropertyName, ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.PostalCode.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.City.PropertyName, ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.City.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.State.PropertyName, ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.State.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[5] = new AttributeConverterEntry(ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.Country.PropertyName, ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.Country.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[6] = new AttributeConverterEntry(ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.LinkedGroupPolicyObjects.PropertyName, ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.LinkedGroupPolicyObjects.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(GPLinkUtil.ToExtendedGPLink), null, new ToSearchFilterDelegate(GPLinkUtil.ToSearchGPLink));
			ADOrganizationalUnitFactory<T>.ADMappingTable = attributeConverterEntry;
			AttributeConverterEntry[] attributeConverterEntryArray = new AttributeConverterEntry[6];
			attributeConverterEntryArray[0] = new AttributeConverterEntry(ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.ManagedBy.PropertyName, ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.ManagedBy.ADAttribute, TypeConstants.ADPrincipal, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryFromADObjectToDN<ADPrincipalFactory<ADPrincipal>, ADPrincipal>), new ToSearchFilterDelegate(SearchConverters.ToSearchFromADObjectToDN<ADPrincipalFactory<ADPrincipal>, ADPrincipal>));
			attributeConverterEntryArray[1] = new AttributeConverterEntry(ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.Street.PropertyName, ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.Street.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[2] = new AttributeConverterEntry(ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.PostalCode.PropertyName, ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.PostalCode.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[3] = new AttributeConverterEntry(ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.City.PropertyName, ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.City.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[4] = new AttributeConverterEntry(ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.State.PropertyName, ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.State.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[5] = new AttributeConverterEntry(ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.Country.PropertyName, ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.Country.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			ADOrganizationalUnitFactory<T>.ADAMMappingTable = attributeConverterEntryArray;
			ADFactoryBase<T>.RegisterMappingTable(ADOrganizationalUnitFactory<T>.ADAMMappingTable, ADServerType.ADLDS);
			ADFactoryBase<T>.RegisterMappingTable(ADOrganizationalUnitFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADOrganizationalUnitFactory()
		{
		}

		private static class ADOrganizationalUnitPropertyMap
		{
			public readonly static PropertyMapEntry ManagedBy;

			public readonly static PropertyMapEntry Street;

			public readonly static PropertyMapEntry PostalCode;

			public readonly static PropertyMapEntry City;

			public readonly static PropertyMapEntry State;

			public readonly static PropertyMapEntry Country;

			public readonly static PropertyMapEntry LinkedGroupPolicyObjects;

			static ADOrganizationalUnitPropertyMap()
			{
				ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.ManagedBy = new PropertyMapEntry("ManagedBy", "managedBy", "managedBy");
				ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.Street = new PropertyMapEntry("StreetAddress", "street", "street");
				ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.PostalCode = new PropertyMapEntry("PostalCode", "postalCode", "postalCode");
				ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.City = new PropertyMapEntry("City", "l", "l");
				ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.State = new PropertyMapEntry("State", "st", "st");
				ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.Country = new PropertyMapEntry("Country", "c", "c");
				ADOrganizationalUnitFactory<T>.ADOrganizationalUnitPropertyMap.LinkedGroupPolicyObjects = new PropertyMapEntry("LinkedGroupPolicyObjects", "gpLink", null);
			}
		}
	}
}