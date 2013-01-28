using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADCentralAccessPolicyFactory<T> : ADObjectFactory<T>
	where T : ADCentralAccessPolicy, new()
	{
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
				return ADCentralAccessPolicyFactory<T>._identityResolvers;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				return ADCentralAccessPolicyFactory<T>._rDNPrefix;
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return "msAuthz-CentralAccessPolicy";
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", this.StructuralObjectClass);
			}
		}

		static ADCentralAccessPolicyFactory()
		{
			ADCentralAccessPolicyFactory<T>._rDNPrefix = "CN";
			string[] strArrays = new string[1];
			strArrays[0] = "name";
			ADCentralAccessPolicyFactory<T>._identityLdapAttributes = strArrays;
			IdentityResolverDelegate[] customIdentityResolver = new IdentityResolverDelegate[2];
			customIdentityResolver[0] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.DistinguishedNameIdentityResolver));
			IdentityResolverDelegate[] genericIdentityResolver = new IdentityResolverDelegate[2];
			genericIdentityResolver[0] = IdentityResolverMethods.GetGenericIdentityResolver(ADCentralAccessPolicyFactory<T>._identityLdapAttributes);
			genericIdentityResolver[1] = new IdentityResolverDelegate(IdentityResolverMethods.GuidSearchFilterIdentityResolver);
			customIdentityResolver[1] = IdentityResolverMethods.GetAggregatedIdentityResolver(ADOperator.Or, genericIdentityResolver);
			ADCentralAccessPolicyFactory<T>._identityResolvers = customIdentityResolver;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[2];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADCentralAccessPolicyFactory<T>.ADCentralAccessPolicyPropertyMap.Members.PropertyName, ADCentralAccessPolicyFactory<T>.ADCentralAccessPolicyPropertyMap.Members.ADAttribute, TypeConstants.ADCentralAccessRule, false, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(ADCentralAccessPolicyFactory<ADCentralAccessPolicy>.ToDirectoryCapMember), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADCentralAccessPolicyFactory<T>.ADCentralAccessPolicyPropertyMap.PolicyID.PropertyName, ADCentralAccessPolicyFactory<T>.ADCentralAccessPolicyPropertyMap.PolicyID.ADAttribute, TypeConstants.SID, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			ADCentralAccessPolicyFactory<T>.ADMappingTable = attributeConverterEntry;
			ADFactoryBase<T>.RegisterMappingTable(ADCentralAccessPolicyFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADCentralAccessPolicyFactory()
		{
			base.PreCommitPipeline.InsertAtEnd(new ADFactory<T>.FactoryCommitSubroutine(this.ADCentralAccessPolicyPreCommitFSRoutine));
		}

		private bool ADCentralAccessPolicyPreCommitFSRoutine(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			if (operation == ADFactory<T>.DirectoryOperation.Create)
			{
				IntPtr zero = IntPtr.Zero;
				int num = UnsafeNativeMethods.GenerateNewCAPID(out zero);
				if (num != 0 || !(IntPtr.Zero != zero))
				{
					throw new ADException(StringResources.CAPIDCreationFailure);
				}
				else
				{
					SecurityIdentifier securityIdentifier = new SecurityIdentifier(zero);
					UnsafeNativeMethods.LocalFree(zero);
					directoryObj["msAuthz-CentralAccessPolicyID"].Value = securityIdentifier;
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		internal static void ToDirectoryCapMember(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (extendedData != null && directoryObj["msAuthz-MemberRulesInCentralAccessPolicy"] != null)
			{
				int count = extendedData.AddedValues.Count - extendedData.DeletedValues.Count;
				if (directoryObj["msAuthz-MemberRulesInCentralAccessPolicy"].Count + count > CentralAccessPolicyConstants.MaximumCARsPerCap)
				{
					object[] maximumCARsPerCap = new object[1];
					maximumCARsPerCap[0] = CentralAccessPolicyConstants.MaximumCARsPerCap;
					throw new ArgumentOutOfRangeException("Members", string.Format(CultureInfo.CurrentCulture, StringResources.CAPMemberMaximumExceeded, maximumCARsPerCap));
				}
			}
			AttributeConverters.ToDirectoryMultivalueObjectConvertor(extendedAttribute, directoryAttributes, extendedData, directoryObj, cmdletSessionInfo, new MultiValueAttributeConvertorDelegate(AttributeConverters.MultiValueNoOpConvertor));
		}

		internal static class ADCentralAccessPolicyPropertyMap
		{
			public readonly static PropertyMapEntry Members;

			public readonly static PropertyMapEntry PolicyID;

			static ADCentralAccessPolicyPropertyMap()
			{
				ADCentralAccessPolicyFactory<T>.ADCentralAccessPolicyPropertyMap.Members = new PropertyMapEntry("Members", "msAuthz-MemberRulesInCentralAccessPolicy", "msAuthz-MemberRulesInCentralAccessPolicy");
				ADCentralAccessPolicyFactory<T>.ADCentralAccessPolicyPropertyMap.PolicyID = new PropertyMapEntry("PolicyID", "msAuthz-CentralAccessPolicyID", "msAuthz-CentralAccessPolicyID");
			}
		}
	}
}