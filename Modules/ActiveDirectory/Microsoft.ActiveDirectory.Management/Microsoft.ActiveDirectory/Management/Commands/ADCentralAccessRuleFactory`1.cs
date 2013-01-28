using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADCentralAccessRuleFactory<T> : ADObjectFactory<T>
	where T : ADCentralAccessRule, new()
	{
		private const string EmptyConditionalDACL = "D:(XA;;GA;;;S-1-1-0;({0}))";

		private readonly static string _rDNPrefix;

		private readonly static string[] _identityLdapAttributes;

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
				return ADCentralAccessRuleFactory<T>._identityResolvers;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				return ADCentralAccessRuleFactory<T>._rDNPrefix;
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return "msAuthz-CentralAccessRule";
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", this.StructuralObjectClass);
			}
		}

		static ADCentralAccessRuleFactory()
		{
			ADCentralAccessRuleFactory<T>._rDNPrefix = "CN";
			string[] strArrays = new string[1];
			strArrays[0] = "name";
			ADCentralAccessRuleFactory<T>._identityLdapAttributes = strArrays;
			IdentityResolverDelegate[] customIdentityResolver = new IdentityResolverDelegate[2];
			customIdentityResolver[0] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.DistinguishedNameIdentityResolver));
			IdentityResolverDelegate[] genericIdentityResolver = new IdentityResolverDelegate[2];
			genericIdentityResolver[0] = IdentityResolverMethods.GetGenericIdentityResolver(ADCentralAccessRuleFactory<T>._identityLdapAttributes);
			genericIdentityResolver[1] = new IdentityResolverDelegate(IdentityResolverMethods.GuidSearchFilterIdentityResolver);
			customIdentityResolver[1] = IdentityResolverMethods.GetAggregatedIdentityResolver(ADOperator.Or, genericIdentityResolver);
			ADCentralAccessRuleFactory<T>._identityResolvers = customIdentityResolver;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[5];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADCentralAccessRuleFactory<T>.ADCentralAccessRulePropertyMap.ResourceCondition.PropertyName, ADCentralAccessRuleFactory<T>.ADCentralAccessRulePropertyMap.ResourceCondition.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(ADCentralAccessRuleFactory<ADCentralAccessRule>.ToDirectoryResouceConditionFromString), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADCentralAccessRuleFactory<T>.ADCentralAccessRulePropertyMap.CurrentAcl.PropertyName, ADCentralAccessRuleFactory<T>.ADCentralAccessRulePropertyMap.CurrentAcl.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectorySDDLStringFromString), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADCentralAccessRuleFactory<T>.ADCentralAccessRulePropertyMap.ProposedAcl.PropertyName, ADCentralAccessRuleFactory<T>.ADCentralAccessRulePropertyMap.ProposedAcl.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectorySDDLStringFromString), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADCentralAccessRuleFactory<T>.ADCentralAccessRulePropertyMap.PreviousAcl.PropertyName, ADCentralAccessRuleFactory<T>.ADCentralAccessRulePropertyMap.PreviousAcl.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADCentralAccessRuleFactory<T>.ADCentralAccessRulePropertyMap.MemberOf.PropertyName, ADCentralAccessRuleFactory<T>.ADCentralAccessRulePropertyMap.MemberOf.ADAttribute, TypeConstants.ADCentralAccessPolicy, false, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			ADCentralAccessRuleFactory<T>.ADMappingTable = attributeConverterEntry;
			ADFactoryBase<T>.RegisterMappingTable(ADCentralAccessRuleFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADCentralAccessRuleFactory()
		{
			base.PreCommitPipeline.InsertAtEnd(new ADFactory<T>.FactoryCommitSubroutine(this.ADCentralAccessRulePreCommitFSRoutine));
		}

		private bool ADCentralAccessRulePreCommitFSRoutine(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			if (ADFactory<T>.DirectoryOperation.Update == operation)
			{
				if (directoryObj.ModifiedProperties.Contains("msAuthz-EffectiveSecurityPolicy") || directoryObj.RemovedProperties.Contains("msAuthz-EffectiveSecurityPolicy") || directoryObj.AddedProperties.Contains("msAuthz-EffectiveSecurityPolicy"))
				{
					string str = ADPathModule.MakePath(base.CmdletSessionInfo.ADRootDSE.ConfigurationNamingContext, "CN=Central Access Rules,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
					string attributeValueFromObjectName = AttributeConverters.GetAttributeValueFromObjectName<ADCentralAccessRuleFactory<ADCentralAccessRule>, ADCentralAccessRule>(directoryObj.DistinguishedName, str, "CurrentAcl", "PreviousAcl", base.CmdletSessionInfo) as string;
					directoryObj["msAuthz-LastEffectiveSecurityPolicy"].Value = attributeValueFromObjectName;
				}
				return true;
			}
			else
			{
				return false;
			}
		}

		internal static void ToDirectoryResouceConditionFromString(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (extendedData == null || extendedData.Value == null)
			{
				directoryObj.ForceRemove(directoryAttributes[0]);
				return;
			}
			else
			{
				object[] value = new object[1];
				value[0] = extendedData.Value as string;
				string str = string.Format(CultureInfo.InvariantCulture, "D:(XA;;GA;;;S-1-1-0;({0}))", value);
				int num = AttributeConverters.ValidateSDDLString(str, cmdletSessionInfo);
				if (num != 0)
				{
					object[] objArray = new object[1];
					objArray[0] = extendedData.Value as string;
					Win32Exception win32Exception = new Win32Exception(num, string.Format(CultureInfo.CurrentCulture, StringResources.ResouceConditionValidationFailed, objArray));
					cmdletSessionInfo.CmdletBase.ThrowTerminatingError(new ErrorRecord(win32Exception, num.ToString(CultureInfo.InvariantCulture), ErrorCategory.InvalidArgument, directoryObj));
				}
				directoryObj.SetValue(directoryAttributes[0], extendedData.Value);
				return;
			}
		}

		internal static class ADCentralAccessRulePropertyMap
		{
			public readonly static PropertyMapEntry ResourceCondition;

			public readonly static PropertyMapEntry CurrentAcl;

			public readonly static PropertyMapEntry ProposedAcl;

			public readonly static PropertyMapEntry PreviousAcl;

			public readonly static PropertyMapEntry MemberOf;

			static ADCentralAccessRulePropertyMap()
			{
				ADCentralAccessRuleFactory<T>.ADCentralAccessRulePropertyMap.ResourceCondition = new PropertyMapEntry("ResourceCondition", "msAuthz-ResourceCondition", "msAuthz-ResourceCondition");
				ADCentralAccessRuleFactory<T>.ADCentralAccessRulePropertyMap.CurrentAcl = new PropertyMapEntry("CurrentAcl", "msAuthz-EffectiveSecurityPolicy", "msAuthz-EffectiveSecurityPolicy");
				ADCentralAccessRuleFactory<T>.ADCentralAccessRulePropertyMap.ProposedAcl = new PropertyMapEntry("ProposedAcl", "msAuthz-ProposedSecurityPolicy", "msAuthz-ProposedSecurityPolicy");
				ADCentralAccessRuleFactory<T>.ADCentralAccessRulePropertyMap.PreviousAcl = new PropertyMapEntry("PreviousAcl", "msAuthz-LastEffectiveSecurityPolicy", "msAuthz-LastEffectiveSecurityPolicy");
				ADCentralAccessRuleFactory<T>.ADCentralAccessRulePropertyMap.MemberOf = new PropertyMapEntry("MemberOf", "msAuthz-MemberRulesInCentralAccessPolicyBL", "msAuthz-MemberRulesInCentralAccessPolicyBL");
			}
		}
	}
}