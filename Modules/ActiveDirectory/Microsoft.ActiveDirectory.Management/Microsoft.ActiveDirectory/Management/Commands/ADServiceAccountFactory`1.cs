using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADServiceAccountFactory<T> : ADAccountFactory<T>
	where T : ADServiceAccount, new()
	{
		private readonly static IADOPathNode _structuralObjectFilter;

		private static string[] _serviceAccountIdentityLdapAttributes;

		private readonly static IdentityResolverDelegate[] _identityResolvers;

		private static AttributeConverterEntry[] ADMappingTable;

		internal static IDictionary<ADServerType, MappingTable<AttributeConverterEntry>> AttributeTable
		{
			get
			{
				return ADAccountFactory<T>.AttributeTable;
			}
		}

		internal override IdentityResolverDelegate[] IdentityResolvers
		{
			get
			{
				return ADServiceAccountFactory<T>._identityResolvers;
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADServiceAccountFactory<T>._structuralObjectFilter;
			}
		}

		static ADServiceAccountFactory()
		{
			IADOPathNode[] aDOPathNodeArray = new IADOPathNode[2];
			IADOPathNode[] aDOPathNodeArray1 = new IADOPathNode[2];
			aDOPathNodeArray1[0] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "msDS-ManagedServiceAccount");
			aDOPathNodeArray1[1] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectCategory", "msDS-ManagedServiceAccount");
			aDOPathNodeArray[0] = ADOPathUtil.CreateAndClause(aDOPathNodeArray1);
			aDOPathNodeArray[1] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "msDS-GroupManagedServiceAccount");
			ADServiceAccountFactory<T>._structuralObjectFilter = ADOPathUtil.CreateOrClause(aDOPathNodeArray);
			string[] strArrays = new string[1];
			strArrays[0] = "sAMAccountName";
			ADServiceAccountFactory<T>._serviceAccountIdentityLdapAttributes = strArrays;
			IdentityResolverDelegate[] customIdentityResolver = new IdentityResolverDelegate[5];
			customIdentityResolver[0] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.DistinguishedNameIdentityResolver));
			customIdentityResolver[1] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.GuidIdentityResolver));
			customIdentityResolver[2] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.SidIdentityResolver));
			customIdentityResolver[3] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.SamAccountNameIdentityResolver));
			customIdentityResolver[4] = ADComputerUtil.GetGenericIdentityResolverWithSamName(ADServiceAccountFactory<T>._serviceAccountIdentityLdapAttributes);
			ADServiceAccountFactory<T>._identityResolvers = customIdentityResolver;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[5];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADPrincipalFactory<T>.ADPrincipalPropertyMap.SamAccountName.PropertyName, ADPrincipalFactory<T>.ADPrincipalPropertyMap.SamAccountName.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(ADComputerUtil.ToDirectoryServiceAccountSamAccountName), new ToSearchFilterDelegate(ADComputerUtil.ToSearchComputerSamAccountName));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADServiceAccountFactory<T>.ADServiceAccountPropertyMap.HostComputers.PropertyName, ADServiceAccountFactory<T>.ADServiceAccountPropertyMap.HostComputers.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.Read, false, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryMultivalueObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADServiceAccountFactory<T>.ADServiceAccountPropertyMap.DNSHostName.PropertyName, ADServiceAccountFactory<T>.ADServiceAccountPropertyMap.DNSHostName.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADServiceAccountFactory<T>.ADServiceAccountPropertyMap.PrincipalsAllowedToRetrieveManagedPassword.PropertyName, ADServiceAccountFactory<T>.ADServiceAccountPropertyMap.PrincipalsAllowedToRetrieveManagedPassword.ADAttribute, TypeConstants.ADPrincipal, true, TypeAdapterAccess.ReadWrite, false, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedPrincipalFromSecDesc), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectorySecDescFromPrincipal), new ToSearchFilterDelegate(SearchConverters.ToSearchNotSupported));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADServiceAccountFactory<T>.ADServiceAccountPropertyMap.ManagedPasswordIntervalInDays.PropertyName, ADServiceAccountFactory<T>.ADServiceAccountPropertyMap.ManagedPasswordIntervalInDays.ADAttribute, TypeConstants.Int, true, TypeAdapterAccess.ReadWrite, false, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			ADServiceAccountFactory<T>.ADMappingTable = attributeConverterEntry;
			ADFactoryBase<T>.RegisterMappingTable(ADServiceAccountFactory<T>.ADMappingTable, ADServerType.ADDS);
			ADAccountFactory<T>.DefaultUserAccessControl = 0x1002;
			ADAccountFactory<T>.UseComputerPasswordGeneration = true;
		}

		public ADServiceAccountFactory()
		{
			base.PreCommitPipeline.InsertAtStart(new ADFactory<T>.FactoryCommitSubroutine(this.ADServiceAccountPreCommitFSRoutine));
		}

		private bool ADServiceAccountPreCommitFSRoutine(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			if (string.CompareOrdinal(directoryObj.ObjectClass, "msDS-ManagedServiceAccount") != 0)
			{
				ADAccountFactory<T>.UseComputerPasswordGeneration = false;
			}
			return false;
		}

		internal override void ValidateObjectClass(T identityObj)
		{
			ADObject aDObject = identityObj;
			if (aDObject == null || !aDObject.IsSearchResult)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.OnlySearchResultsSupported, new object[0]));
			}
			else
			{
				if (aDObject.ObjectTypes.Contains("msDS-ManagedServiceAccount") || aDObject.ObjectTypes.Contains("msDS-GroupManagedServiceAccount"))
				{
					return;
				}
				else
				{
					object[] objArray = new object[2];
					objArray[0] = "msDS-ManagedServiceAccount";
					objArray[1] = "msDS-GroupManagedServiceAccount";
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidObjectClasses, objArray));
				}
			}
		}

		private static class ADServiceAccountPropertyMap
		{
			public readonly static PropertyMapEntry HostComputers;

			public readonly static PropertyMapEntry DNSHostName;

			public readonly static PropertyMapEntry PrincipalsAllowedToRetrieveManagedPassword;

			public readonly static PropertyMapEntry ManagedPasswordIntervalInDays;

			static ADServiceAccountPropertyMap()
			{
				ADServiceAccountFactory<T>.ADServiceAccountPropertyMap.HostComputers = new PropertyMapEntry("HostComputers", "msDS-HostServiceAccountBL", null);
				ADServiceAccountFactory<T>.ADServiceAccountPropertyMap.DNSHostName = new PropertyMapEntry("DNSHostName", "dNSHostName", null);
				ADServiceAccountFactory<T>.ADServiceAccountPropertyMap.PrincipalsAllowedToRetrieveManagedPassword = new PropertyMapEntry("PrincipalsAllowedToRetrieveManagedPassword", "msDS-GroupMSAMembership", null);
				ADServiceAccountFactory<T>.ADServiceAccountPropertyMap.ManagedPasswordIntervalInDays = new PropertyMapEntry("ManagedPasswordIntervalInDays", "msDS-ManagedPasswordInterval", null);
			}
		}
	}
}