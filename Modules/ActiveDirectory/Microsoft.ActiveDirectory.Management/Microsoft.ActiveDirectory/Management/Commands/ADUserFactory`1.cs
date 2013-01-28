using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADUserFactory<T> : ADAccountFactory<T>
	where T : ADUser, new()
	{
		private static AttributeConverterEntry[] ADMappingTable;

		private static AttributeConverterEntry[] ADAMMappingTable;

		internal static IDictionary<ADServerType, MappingTable<AttributeConverterEntry>> AttributeTable
		{
			get
			{
				return ADAccountFactory<T>.AttributeTable;
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
				return "user";
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				IADOPathNode aDOPathNode = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "user");
				IADOPathNode aDOPathNode1 = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectCategory", "person");
				IADOPathNode[] aDOPathNodeArray = new IADOPathNode[2];
				aDOPathNodeArray[0] = aDOPathNode;
				aDOPathNodeArray[1] = aDOPathNode1;
				IADOPathNode aDOPathNode2 = ADOPathUtil.CreateAndClause(aDOPathNodeArray);
				return aDOPathNode2;
			}
		}

		static ADUserFactory()
		{
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[30];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.GivenName.PropertyName, ADUserFactory<T>.ADUserPropertyMap.GivenName.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.Surname.PropertyName, ADUserFactory<T>.ADUserPropertyMap.Surname.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.HomeDirectory.PropertyName, ADUserFactory<T>.ADUserPropertyMap.HomeDirectory.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.HomeDrive.PropertyName, ADUserFactory<T>.ADUserPropertyMap.HomeDrive.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.Manager.PropertyName, ADUserFactory<T>.ADUserPropertyMap.Manager.ADAttribute, TypeConstants.ADUser, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryFromADObjectToDN<ADUserFactory<ADUser>, ADUser>), new ToSearchFilterDelegate(SearchConverters.ToSearchFromADObjectToDN<ADUserFactory<ADUser>, ADUser>));
			attributeConverterEntry[5] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.OtherName.PropertyName, ADUserFactory<T>.ADUserPropertyMap.OtherName.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[6] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.LogonWorkstations.PropertyName, ADUserFactory<T>.ADUserPropertyMap.LogonWorkstations.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[7] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.ProfilePath.PropertyName, ADUserFactory<T>.ADUserPropertyMap.ProfilePath.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[8] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.ScriptPath.PropertyName, ADUserFactory<T>.ADUserPropertyMap.ScriptPath.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[9] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.SmartcardLogonRequired.PropertyName, ADUserFactory<T>.ADUserPropertyMap.SmartcardLogonRequired.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(ADAccountFactory<T>.ToExtendedUserAccountControl), new ToDirectoryFormatDelegate(ADAccountFactory<T>.ToDirectoryUserAccountControl), new ToSearchFilterDelegate(ADAccountFactory<T>.ToSearchUserAccountControl));
			attributeConverterEntry[10] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.OfficePhone.PropertyName, ADUserFactory<T>.ADUserPropertyMap.OfficePhone.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[11] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.Company.PropertyName, ADUserFactory<T>.ADUserPropertyMap.Company.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[12] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.Department.PropertyName, ADUserFactory<T>.ADUserPropertyMap.Department.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[13] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.Fax.PropertyName, ADUserFactory<T>.ADUserPropertyMap.Fax.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[14] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.Initials.PropertyName, ADUserFactory<T>.ADUserPropertyMap.Initials.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[15] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.MobilePhone.PropertyName, ADUserFactory<T>.ADUserPropertyMap.MobilePhone.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[16] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.HomePhone.PropertyName, ADUserFactory<T>.ADUserPropertyMap.HomePhone.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[17] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.Office.PropertyName, ADUserFactory<T>.ADUserPropertyMap.Office.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[18] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.PostalCode.PropertyName, ADUserFactory<T>.ADUserPropertyMap.PostalCode.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[19] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.POBox.PropertyName, ADUserFactory<T>.ADUserPropertyMap.POBox.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[20] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.State.PropertyName, ADUserFactory<T>.ADUserPropertyMap.State.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[21] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.StreetAddress.PropertyName, ADUserFactory<T>.ADUserPropertyMap.StreetAddress.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[22] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.Title.PropertyName, ADUserFactory<T>.ADUserPropertyMap.Title.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[23] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.Division.PropertyName, ADUserFactory<T>.ADUserPropertyMap.Division.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[24] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.Country.PropertyName, ADUserFactory<T>.ADUserPropertyMap.Country.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[25] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.City.PropertyName, ADUserFactory<T>.ADUserPropertyMap.City.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[26] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.EmployeeID.PropertyName, ADUserFactory<T>.ADUserPropertyMap.EmployeeID.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[27] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.EmployeeNumber.PropertyName, ADUserFactory<T>.ADUserPropertyMap.EmployeeNumber.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[28] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.Organization.PropertyName, ADUserFactory<T>.ADUserPropertyMap.Organization.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[29] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.EmailAddress.PropertyName, ADUserFactory<T>.ADUserPropertyMap.EmailAddress.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			ADUserFactory<T>.ADMappingTable = attributeConverterEntry;
			AttributeConverterEntry[] attributeConverterEntryArray = new AttributeConverterEntry[24];
			attributeConverterEntryArray[0] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.GivenName.PropertyName, ADUserFactory<T>.ADUserPropertyMap.GivenName.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[1] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.Surname.PropertyName, ADUserFactory<T>.ADUserPropertyMap.Surname.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[2] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.Manager.PropertyName, ADUserFactory<T>.ADUserPropertyMap.Manager.ADAMAttribute, TypeConstants.ADUser, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryFromADObjectToDN<ADUserFactory<ADUser>, ADUser>), new ToSearchFilterDelegate(SearchConverters.ToSearchFromADObjectToDN<ADUserFactory<ADUser>, ADUser>));
			attributeConverterEntryArray[3] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.OtherName.PropertyName, ADUserFactory<T>.ADUserPropertyMap.OtherName.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[4] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.OfficePhone.PropertyName, ADUserFactory<T>.ADUserPropertyMap.OfficePhone.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[5] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.Company.PropertyName, ADUserFactory<T>.ADUserPropertyMap.Company.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[6] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.Department.PropertyName, ADUserFactory<T>.ADUserPropertyMap.Department.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[7] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.Fax.PropertyName, ADUserFactory<T>.ADUserPropertyMap.Fax.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[8] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.Initials.PropertyName, ADUserFactory<T>.ADUserPropertyMap.Initials.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[9] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.MobilePhone.PropertyName, ADUserFactory<T>.ADUserPropertyMap.MobilePhone.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[10] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.HomePhone.PropertyName, ADUserFactory<T>.ADUserPropertyMap.HomePhone.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[11] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.Office.PropertyName, ADUserFactory<T>.ADUserPropertyMap.Office.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[12] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.PostalCode.PropertyName, ADUserFactory<T>.ADUserPropertyMap.PostalCode.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[13] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.POBox.PropertyName, ADUserFactory<T>.ADUserPropertyMap.POBox.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, false, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[14] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.State.PropertyName, ADUserFactory<T>.ADUserPropertyMap.State.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[15] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.StreetAddress.PropertyName, ADUserFactory<T>.ADUserPropertyMap.StreetAddress.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[16] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.Title.PropertyName, ADUserFactory<T>.ADUserPropertyMap.Title.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[17] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.Division.PropertyName, ADUserFactory<T>.ADUserPropertyMap.Division.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[18] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.Country.PropertyName, ADUserFactory<T>.ADUserPropertyMap.Country.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[19] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.City.PropertyName, ADUserFactory<T>.ADUserPropertyMap.City.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[20] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.EmployeeID.PropertyName, ADUserFactory<T>.ADUserPropertyMap.EmployeeID.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[21] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.EmployeeNumber.PropertyName, ADUserFactory<T>.ADUserPropertyMap.EmployeeNumber.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[22] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.Organization.PropertyName, ADUserFactory<T>.ADUserPropertyMap.Organization.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[23] = new AttributeConverterEntry(ADUserFactory<T>.ADUserPropertyMap.EmailAddress.PropertyName, ADUserFactory<T>.ADUserPropertyMap.EmailAddress.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			ADUserFactory<T>.ADAMMappingTable = attributeConverterEntryArray;
			ADFactoryBase<T>.RegisterMappingTable(ADUserFactory<T>.ADAMMappingTable, ADServerType.ADLDS);
			ADFactoryBase<T>.RegisterMappingTable(ADUserFactory<T>.ADMappingTable, ADServerType.ADDS);
			ADAccountFactory<T>.DefaultUserAccessControl = 0x202;
		}

		public ADUserFactory()
		{
		}

		private static class ADUserPropertyMap
		{
			public readonly static PropertyMapEntry GivenName;

			public readonly static PropertyMapEntry Surname;

			public readonly static PropertyMapEntry HomeDirectory;

			public readonly static PropertyMapEntry HomeDrive;

			public readonly static PropertyMapEntry Manager;

			public readonly static PropertyMapEntry OtherName;

			public readonly static PropertyMapEntry LogonWorkstations;

			public readonly static PropertyMapEntry ProfilePath;

			public readonly static PropertyMapEntry ScriptPath;

			public readonly static PropertyMapEntry SmartcardLogonRequired;

			public readonly static PropertyMapEntry OfficePhone;

			public readonly static PropertyMapEntry Company;

			public readonly static PropertyMapEntry Department;

			public readonly static PropertyMapEntry Fax;

			public readonly static PropertyMapEntry Initials;

			public readonly static PropertyMapEntry MobilePhone;

			public readonly static PropertyMapEntry HomePhone;

			public readonly static PropertyMapEntry Office;

			public readonly static PropertyMapEntry PostalCode;

			public readonly static PropertyMapEntry POBox;

			public readonly static PropertyMapEntry State;

			public readonly static PropertyMapEntry StreetAddress;

			public readonly static PropertyMapEntry Title;

			public readonly static PropertyMapEntry Division;

			public readonly static PropertyMapEntry Country;

			public readonly static PropertyMapEntry City;

			public readonly static PropertyMapEntry EmployeeID;

			public readonly static PropertyMapEntry EmployeeNumber;

			public readonly static PropertyMapEntry Organization;

			public readonly static PropertyMapEntry EmailAddress;

			static ADUserPropertyMap()
			{
				ADUserFactory<T>.ADUserPropertyMap.GivenName = new PropertyMapEntry("GivenName", "givenName", "givenName");
				ADUserFactory<T>.ADUserPropertyMap.Surname = new PropertyMapEntry("Surname", "sn", "sn");
				ADUserFactory<T>.ADUserPropertyMap.HomeDirectory = new PropertyMapEntry("HomeDirectory", "homeDirectory", null);
				ADUserFactory<T>.ADUserPropertyMap.HomeDrive = new PropertyMapEntry("HomeDrive", "homeDrive", null);
				ADUserFactory<T>.ADUserPropertyMap.Manager = new PropertyMapEntry("Manager", "manager", "manager");
				ADUserFactory<T>.ADUserPropertyMap.OtherName = new PropertyMapEntry("OtherName", "middleName", "middleName");
				ADUserFactory<T>.ADUserPropertyMap.LogonWorkstations = new PropertyMapEntry("LogonWorkstations", "userWorkstations", null);
				ADUserFactory<T>.ADUserPropertyMap.ProfilePath = new PropertyMapEntry("ProfilePath", "profilePath", null);
				ADUserFactory<T>.ADUserPropertyMap.ScriptPath = new PropertyMapEntry("ScriptPath", "scriptPath", null);
				string[] strArrays = new string[2];
				strArrays[0] = "userAccountControl";
				strArrays[1] = "msDS-User-Account-Control-Computed";
				ADUserFactory<T>.ADUserPropertyMap.SmartcardLogonRequired = new PropertyMapEntry("SmartcardLogonRequired", strArrays, null);
				ADUserFactory<T>.ADUserPropertyMap.OfficePhone = new PropertyMapEntry("OfficePhone", "telephoneNumber", "telephoneNumber");
				ADUserFactory<T>.ADUserPropertyMap.Company = new PropertyMapEntry("Company", "company", "company");
				ADUserFactory<T>.ADUserPropertyMap.Department = new PropertyMapEntry("Department", "department", "department");
				ADUserFactory<T>.ADUserPropertyMap.Fax = new PropertyMapEntry("Fax", "facsimileTelephoneNumber", "facsimileTelephoneNumber");
				ADUserFactory<T>.ADUserPropertyMap.Initials = new PropertyMapEntry("Initials", "initials", "initials");
				ADUserFactory<T>.ADUserPropertyMap.MobilePhone = new PropertyMapEntry("MobilePhone", "mobile", "mobile");
				ADUserFactory<T>.ADUserPropertyMap.HomePhone = new PropertyMapEntry("HomePhone", "homePhone", "homePhone");
				ADUserFactory<T>.ADUserPropertyMap.Office = new PropertyMapEntry("Office", "physicalDeliveryOfficeName", "physicalDeliveryOfficeName");
				ADUserFactory<T>.ADUserPropertyMap.PostalCode = new PropertyMapEntry("PostalCode", "postalCode", "postalCode");
				ADUserFactory<T>.ADUserPropertyMap.POBox = new PropertyMapEntry("POBox", "postOfficeBox", "postOfficeBox");
				ADUserFactory<T>.ADUserPropertyMap.State = new PropertyMapEntry("State", "st", "st");
				ADUserFactory<T>.ADUserPropertyMap.StreetAddress = new PropertyMapEntry("StreetAddress", "streetAddress", "streetAddress");
				ADUserFactory<T>.ADUserPropertyMap.Title = new PropertyMapEntry("Title", "title", "title");
				ADUserFactory<T>.ADUserPropertyMap.Division = new PropertyMapEntry("Division", "division", "division");
				ADUserFactory<T>.ADUserPropertyMap.Country = new PropertyMapEntry("Country", "c", "c");
				ADUserFactory<T>.ADUserPropertyMap.City = new PropertyMapEntry("City", "l", "l");
				ADUserFactory<T>.ADUserPropertyMap.EmployeeID = new PropertyMapEntry("EmployeeID", "employeeID", "employeeID");
				ADUserFactory<T>.ADUserPropertyMap.EmployeeNumber = new PropertyMapEntry("EmployeeNumber", "employeeNumber", "employeeNumber");
				ADUserFactory<T>.ADUserPropertyMap.Organization = new PropertyMapEntry("Organization", "o", "o");
				ADUserFactory<T>.ADUserPropertyMap.EmailAddress = new PropertyMapEntry("EmailAddress", "mail", "mail");
			}
		}
	}
}