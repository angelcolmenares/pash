using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADFineGrainedPasswordPolicyFactory<T> : ADObjectFactory<T>
	where T : ADFineGrainedPasswordPolicy, new()
	{
		private static IADOPathNode _fgppStructuralFilter;

		private static string[] _fgppIdentityLdapAttributes;

		private readonly static IdentityResolverDelegate[] _identityResolvers;

		private static AttributeConverterEntry[] ADMappingTable;

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
				return ADFineGrainedPasswordPolicyFactory<T>._identityResolvers;
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
				return "msDS-PasswordSettings";
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADFineGrainedPasswordPolicyFactory<T>._fgppStructuralFilter;
			}
		}

		static ADFineGrainedPasswordPolicyFactory()
		{
			ADFineGrainedPasswordPolicyFactory<T>._fgppStructuralFilter = null;
			string[] strArrays = new string[2];
			strArrays[0] = "name";
			strArrays[1] = "distinguishedName";
			ADFineGrainedPasswordPolicyFactory<T>._fgppIdentityLdapAttributes = strArrays;
			IdentityResolverDelegate[] aggregatedIdentityResolver = new IdentityResolverDelegate[1];
			IdentityResolverDelegate[] genericIdentityResolver = new IdentityResolverDelegate[2];
			genericIdentityResolver[0] = IdentityResolverMethods.GetGenericIdentityResolver(ADFineGrainedPasswordPolicyFactory<T>._fgppIdentityLdapAttributes);
			genericIdentityResolver[1] = new IdentityResolverDelegate(IdentityResolverMethods.GuidSearchFilterIdentityResolver);
			aggregatedIdentityResolver[0] = IdentityResolverMethods.GetAggregatedIdentityResolver(ADOperator.Or, genericIdentityResolver);
			ADFineGrainedPasswordPolicyFactory<T>._identityResolvers = aggregatedIdentityResolver;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[11];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.LockoutDuration.PropertyName, ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.LockoutDuration.ADAttribute, TypeConstants.TimeSpan, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedNoExpirationTimeSpan), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryNegativeTimeSpan), new ToSearchFilterDelegate(SearchConverters.ToSearchNegativeTimeSpan));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.LockoutObservationWindow.PropertyName, ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.LockoutObservationWindow.ADAttribute, TypeConstants.TimeSpan, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedNoExpirationTimeSpan), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryNegativeTimeSpan), new ToSearchFilterDelegate(SearchConverters.ToSearchNegativeTimeSpan));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.LockoutThreshold.PropertyName, ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.LockoutThreshold.ADAttribute, TypeConstants.Int, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.MaxPasswordAge.PropertyName, ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.MaxPasswordAge.ADAttribute, TypeConstants.TimeSpan, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedNoExpirationTimeSpan), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryNoExpirationTimeSpan), new ToSearchFilterDelegate(SearchConverters.ToSearchNegativeTimeSpan));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.MinPasswordAge.PropertyName, ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.MinPasswordAge.ADAttribute, TypeConstants.TimeSpan, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedNoExpirationTimeSpan), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryNegativeTimeSpan), new ToSearchFilterDelegate(SearchConverters.ToSearchNegativeTimeSpan));
			attributeConverterEntry[5] = new AttributeConverterEntry(ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.MinPasswordLength.PropertyName, ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.MinPasswordLength.ADAttribute, TypeConstants.Int, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[6] = new AttributeConverterEntry(ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.PasswordHistoryCount.PropertyName, ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.PasswordHistoryCount.ADAttribute, TypeConstants.Int, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[7] = new AttributeConverterEntry(ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.ComplexityEnabled.PropertyName, ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.ComplexityEnabled.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[8] = new AttributeConverterEntry(ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.ReversibleEncryptionEnabled.PropertyName, ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.ReversibleEncryptionEnabled.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[9] = new AttributeConverterEntry(ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.Precedence.PropertyName, ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.Precedence.ADAttribute, TypeConstants.Int, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[10] = new AttributeConverterEntry(ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.AppliesTo.PropertyName, ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.AppliesTo.ADAttribute, TypeConstants.ADPrincipal, true, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryMultivalueObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			ADFineGrainedPasswordPolicyFactory<T>.ADMappingTable = attributeConverterEntry;
			ADFineGrainedPasswordPolicyFactory<T>._fgppStructuralFilter = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "msDS-PasswordSettings");
			ADFactoryBase<T>.RegisterMappingTable(ADFineGrainedPasswordPolicyFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADFineGrainedPasswordPolicyFactory()
		{
		}

		internal override IADOPathNode IdentitySearchConverter(object identity)
		{
			if (identity != null)
			{
				return base.IdentitySearchConverter(identity);
			}
			else
			{
				throw new ArgumentNullException("Identity");
			}
		}

		internal static class ADFineGrainedPasswordPolicyPropertyMap
		{
			internal readonly static PropertyMapEntry LockoutDuration;

			internal readonly static PropertyMapEntry LockoutObservationWindow;

			internal readonly static PropertyMapEntry LockoutThreshold;

			internal readonly static PropertyMapEntry MaxPasswordAge;

			internal readonly static PropertyMapEntry MinPasswordAge;

			internal readonly static PropertyMapEntry MinPasswordLength;

			internal readonly static PropertyMapEntry PasswordHistoryCount;

			internal readonly static PropertyMapEntry ComplexityEnabled;

			internal readonly static PropertyMapEntry ReversibleEncryptionEnabled;

			internal readonly static PropertyMapEntry Precedence;

			internal readonly static PropertyMapEntry AppliesTo;

			static ADFineGrainedPasswordPolicyPropertyMap()
			{
				ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.LockoutDuration = new PropertyMapEntry("LockoutDuration", "msDS-LockoutDuration", null);
				ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.LockoutObservationWindow = new PropertyMapEntry("LockoutObservationWindow", "msDS-LockoutObservationWindow", null);
				ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.LockoutThreshold = new PropertyMapEntry("LockoutThreshold", "msDS-LockoutThreshold", null);
				ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.MaxPasswordAge = new PropertyMapEntry("MaxPasswordAge", "msDS-MaximumPasswordAge", null);
				ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.MinPasswordAge = new PropertyMapEntry("MinPasswordAge", "msDS-MinimumPasswordAge", null);
				ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.MinPasswordLength = new PropertyMapEntry("MinPasswordLength", "msDS-MinimumPasswordLength", null);
				ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.PasswordHistoryCount = new PropertyMapEntry("PasswordHistoryCount", "msDS-PasswordHistoryLength", null);
				ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.ComplexityEnabled = new PropertyMapEntry("ComplexityEnabled", "msDS-PasswordComplexityEnabled", null);
				ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.ReversibleEncryptionEnabled = new PropertyMapEntry("ReversibleEncryptionEnabled", "msDS-PasswordReversibleEncryptionEnabled", null);
				ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.Precedence = new PropertyMapEntry("Precedence", "msDS-PasswordSettingsPrecedence", null);
				ADFineGrainedPasswordPolicyFactory<T>.ADFineGrainedPasswordPolicyPropertyMap.AppliesTo = new PropertyMapEntry("AppliesTo", "msDS-PSOAppliesTo", null);
			}
		}
	}
}