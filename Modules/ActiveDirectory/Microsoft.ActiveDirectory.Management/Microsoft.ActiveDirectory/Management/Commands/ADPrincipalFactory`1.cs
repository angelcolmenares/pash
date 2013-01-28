using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADPrincipalFactory<T> : ADObjectFactory<T>
	where T : ADPrincipal, new()
	{
		private static IADOPathNode _principalStructuralFilter;

		private static string[] _principalIdentityLdapAttributes;

		private readonly static IdentityResolverDelegate[] _identityResolvers;

		private static AttributeConverterEntry[] ADMappingTable;

		private static AttributeConverterEntry[] ADAMMappingTable;

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
				return ADPrincipalFactory<T>._identityResolvers;
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
				return "top";
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADPrincipalFactory<T>._principalStructuralFilter;
			}
		}

		static ADPrincipalFactory()
		{
			ADPrincipalFactory<T>._principalStructuralFilter = null;
			string[] strArrays = new string[1];
			strArrays[0] = "sAMAccountName";
			ADPrincipalFactory<T>._principalIdentityLdapAttributes = strArrays;
			IdentityResolverDelegate[] customIdentityResolver = new IdentityResolverDelegate[5];
			customIdentityResolver[0] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.DistinguishedNameIdentityResolver));
			customIdentityResolver[1] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.GuidIdentityResolver));
			customIdentityResolver[2] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.SidIdentityResolver));
			customIdentityResolver[3] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.SamAccountNameIdentityResolver));
			customIdentityResolver[4] = IdentityResolverMethods.GetGenericIdentityResolver(ADPrincipalFactory<T>._principalIdentityLdapAttributes);
			ADPrincipalFactory<T>._identityResolvers = customIdentityResolver;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[5];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADPrincipalFactory<T>.ADPrincipalPropertyMap.SamAccountName.PropertyName, ADPrincipalFactory<T>.ADPrincipalPropertyMap.SamAccountName.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADPrincipalFactory<T>.ADPrincipalPropertyMap.SID.PropertyName, ADPrincipalFactory<T>.ADPrincipalPropertyMap.SID.ADAttribute, TypeConstants.SID, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADPrincipalFactory<T>.ADPrincipalPropertyMap.HomePage.PropertyName, ADPrincipalFactory<T>.ADPrincipalPropertyMap.HomePage.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADPrincipalFactory<T>.ADPrincipalPropertyMap.SIDHistory.PropertyName, ADPrincipalFactory<T>.ADPrincipalPropertyMap.SIDHistory.ADAttribute, TypeConstants.SID, false, TypeAdapterAccess.Read, false, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADPrincipalFactory<T>.ADPrincipalPropertyMap.MemberOf.PropertyName, ADPrincipalFactory<T>.ADPrincipalPropertyMap.MemberOf.ADAttribute, TypeConstants.ADGroup, false, TypeAdapterAccess.Read, false, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			ADPrincipalFactory<T>.ADMappingTable = attributeConverterEntry;
			AttributeConverterEntry[] attributeConverterEntryArray = new AttributeConverterEntry[3];
			attributeConverterEntryArray[0] = new AttributeConverterEntry(ADPrincipalFactory<T>.ADPrincipalPropertyMap.SID.PropertyName, ADPrincipalFactory<T>.ADPrincipalPropertyMap.SID.ADAMAttribute, TypeConstants.SID, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[1] = new AttributeConverterEntry(ADPrincipalFactory<T>.ADPrincipalPropertyMap.HomePage.PropertyName, ADPrincipalFactory<T>.ADPrincipalPropertyMap.HomePage.ADAMAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[2] = new AttributeConverterEntry(ADPrincipalFactory<T>.ADPrincipalPropertyMap.MemberOf.PropertyName, ADPrincipalFactory<T>.ADPrincipalPropertyMap.MemberOf.ADAMAttribute, TypeConstants.ADGroup, false, TypeAdapterAccess.Read, false, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			ADPrincipalFactory<T>.ADAMMappingTable = attributeConverterEntryArray;
			IADOPathNode[] aDOPathNodeArray = new IADOPathNode[2];
			aDOPathNodeArray[0] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "group");
			aDOPathNodeArray[1] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "user");
			ADPrincipalFactory<T>._principalStructuralFilter = ADOPathUtil.CreateOrClause(aDOPathNodeArray);
			ADFactoryBase<T>.RegisterMappingTable(ADPrincipalFactory<T>.ADAMMappingTable, ADServerType.ADLDS);
			ADFactoryBase<T>.RegisterMappingTable(ADPrincipalFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADPrincipalFactory()
		{
		}

		internal override T Construct(ADEntity directoryObj, AttributeSetRequest requestedAttributes)
		{
			T t = base.Construct(directoryObj, requestedAttributes);
			string[] internalLdapAttributes = DirectoryAttrConstants.InternalLdapAttributes;
			for (int i = 0; i < (int)internalLdapAttributes.Length; i++)
			{
				string str = internalLdapAttributes[i];
				if (!t.InternalProperties.Contains(str) && directoryObj.Contains(str) && directoryObj[str] != null)
				{
					t.InternalProperties.SetValue(str, directoryObj[str]);
				}
			}
			return t;
		}

		internal override IADOPathNode IdentitySearchConverter(object identity)
		{
			if (identity != null)
			{
				if ((int)this.IdentityLdapAttributes.Length <= 0)
				{
					return null;
				}
				else
				{
					SecurityIdentifier securityIdentifier = identity as SecurityIdentifier;
					if (securityIdentifier == null)
					{
						IADOPathNode aDOPathNode = base.IdentitySearchConverter(identity);
						ADPrincipal aDPrincipal = identity as ADPrincipal;
						if (aDPrincipal == null)
						{
							return aDOPathNode;
						}
						else
						{
							List<IADOPathNode> aDOPathNodes = new List<IADOPathNode>();
							if (aDPrincipal.SamAccountName != null)
							{
								aDOPathNodes.Add(ADOPathUtil.CreateFilterClause(ADOperator.Eq, "sAMAccountName", aDPrincipal.SamAccountName));
							}
							if (aDPrincipal.SID != null)
							{
								aDOPathNodes.Add(ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectSid", aDPrincipal.SID));
							}
							aDOPathNodes.Add(aDOPathNode);
							return ADOPathUtil.CreateAndClause(aDOPathNodes.ToArray());
						}
					}
					else
					{
						byte[] numArray = new byte[securityIdentifier.BinaryLength];
						securityIdentifier.GetBinaryForm(numArray, 0);
						return ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectSid", numArray);
					}
				}
			}
			else
			{
				throw new ArgumentNullException("identity");
			}
		}

		internal static class ADPrincipalPropertyMap
		{
			internal readonly static PropertyMapEntry SamAccountName;

			internal readonly static PropertyMapEntry SID;

			internal readonly static PropertyMapEntry SIDHistory;

			internal readonly static PropertyMapEntry HomePage;

			internal readonly static PropertyMapEntry MemberOf;

			static ADPrincipalPropertyMap()
			{
				ADPrincipalFactory<T>.ADPrincipalPropertyMap.SamAccountName = new PropertyMapEntry("SamAccountName", "sAMAccountName", null);
				ADPrincipalFactory<T>.ADPrincipalPropertyMap.SID = new PropertyMapEntry("SID", "objectSid", "objectSid");
				ADPrincipalFactory<T>.ADPrincipalPropertyMap.SIDHistory = new PropertyMapEntry("SIDHistory", "sIDHistory", null);
				ADPrincipalFactory<T>.ADPrincipalPropertyMap.HomePage = new PropertyMapEntry("HomePage", "wWWHomePage", "wWWHomePage");
				ADPrincipalFactory<T>.ADPrincipalPropertyMap.MemberOf = new PropertyMapEntry("MemberOf", "memberOf", "memberOf");
			}
		}
	}
}