using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADGroupFactory<T> : ADPrincipalFactory<T>
	where T : ADGroup, new()
	{
		private const string _debugCategory = "ADGroupFactory";

		private static IADOPathNode _groupStructuralFilter;

		private static AttributeConverterEntry[] ADMappingTable;

		private static AttributeConverterEntry[] ADAMMappingTable;

		internal static IDictionary<ADServerType, MappingTable<AttributeConverterEntry>> AttributeTable
		{
			get
			{
				return ADPrincipalFactory<T>.AttributeTable;
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
				return "group";
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADGroupFactory<T>._groupStructuralFilter;
			}
		}

		static ADGroupFactory()
		{
			ADGroupFactory<T>._groupStructuralFilter = null;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[4];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADGroupFactory<T>.ADGroupPropertyMap.GroupScope.PropertyName, ADGroupFactory<T>.ADGroupPropertyMap.GroupScope.ADAttribute, TypeConstants.ADGroupScope, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(GroupTypeUtils.ToExtendedGroupScope), new ToDirectoryFormatDelegate(GroupTypeUtils.ToDirectoryGroupScope), new ToSearchFilterDelegate(GroupTypeUtils.ToSearchGroupScope));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADGroupFactory<T>.ADGroupPropertyMap.GroupCategory.PropertyName, ADGroupFactory<T>.ADGroupPropertyMap.GroupCategory.ADAttribute, TypeConstants.ADGroupCategory, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(GroupTypeUtils.ToExtendedGroupCategory), new ToDirectoryFormatDelegate(GroupTypeUtils.ToDirectoryGroupCategory), new ToSearchFilterDelegate(GroupTypeUtils.ToSearchGroupCategory));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADGroupFactory<T>.ADGroupPropertyMap.ManagedBy.PropertyName, ADGroupFactory<T>.ADGroupPropertyMap.ManagedBy.ADAttribute, TypeConstants.ADPrincipal, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryFromADObjectToDN<ADPrincipalFactory<ADPrincipal>, ADPrincipal>), new ToSearchFilterDelegate(SearchConverters.ToSearchFromADObjectToDN<ADPrincipalFactory<ADPrincipal>, ADPrincipal>));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADGroupFactory<T>.ADGroupPropertyMap.Members.PropertyName, ADGroupFactory<T>.ADGroupPropertyMap.Members.ADAttribute, TypeConstants.ADPrincipal, false, TypeAdapterAccess.Read, false, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryMultivalueObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			ADGroupFactory<T>.ADMappingTable = attributeConverterEntry;
			ADGroupFactory<T>.ADAMMappingTable = ADGroupFactory<T>.ADMappingTable;
			ADGroupFactory<T>._groupStructuralFilter = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "group");
			ADFactoryBase<T>.RegisterMappingTable(ADGroupFactory<T>.ADAMMappingTable, ADServerType.ADLDS);
			ADFactoryBase<T>.RegisterMappingTable(ADGroupFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADGroupFactory()
		{
			base.PreCommitPipeline.InsertAtEnd(new ADFactory<T>.FactoryCommitSubroutine(this.ADGroupPreCommitFSRoutine));
		}

		private bool ADGroupPreCommitFSRoutine(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			if (operation != ADFactory<T>.DirectoryOperation.Create || instance != null || parameters.Contains("GroupCategory"))
			{
				return false;
			}
			else
			{
				directoryObj["groupType"].Value = (int)directoryObj["groupType"].Value | -2147483648;
				return true;
			}
		}

		internal override AttributeSetRequest ConstructAttributeSetRequest(ICollection<string> requestedExtendedAttr)
		{
			AttributeSetRequest attributeSetRequest = base.ConstructAttributeSetRequest(requestedExtendedAttr);
			attributeSetRequest.DirectoryAttributes.Add("groupType");
			return attributeSetRequest;
		}

		internal static class ADGroupPropertyMap
		{
			internal readonly static PropertyMapEntry GroupScope;

			internal readonly static PropertyMapEntry GroupCategory;

			internal readonly static PropertyMapEntry Members;

			internal readonly static PropertyMapEntry ManagedBy;

			static ADGroupPropertyMap()
			{
				ADGroupFactory<T>.ADGroupPropertyMap.GroupScope = new PropertyMapEntry("GroupScope", "groupType", "groupType");
				ADGroupFactory<T>.ADGroupPropertyMap.GroupCategory = new PropertyMapEntry("GroupCategory", "groupType", "groupType");
				ADGroupFactory<T>.ADGroupPropertyMap.Members = new PropertyMapEntry("Members", "member", "member");
				ADGroupFactory<T>.ADGroupPropertyMap.ManagedBy = new PropertyMapEntry("ManagedBy", "managedBy", "managedBy");
			}
		}
	}
}