using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADSchemaObjectFactory<T> : ADObjectFactory<T>
	where T : ADSchemaObject, new()
	{
		private readonly static string _rDNPrefix;

		private readonly static string[] _identityLdapAttributes;

		private readonly static IdentityResolverDelegate[] _identityResolvers;

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
				return ADSchemaObjectFactory<T>._identityResolvers;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				return ADSchemaObjectFactory<T>._rDNPrefix;
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return "*";
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADOPathUtil.CreateFilterClause(ADOperator.Like, "objectClass", this.StructuralObjectClass);
			}
		}

		static ADSchemaObjectFactory()
		{
			ADSchemaObjectFactory<T>._rDNPrefix = "CN";
			string[] strArrays = new string[2];
			strArrays[0] = "name";
			strArrays[1] = "lDAPDisplayName";
			ADSchemaObjectFactory<T>._identityLdapAttributes = strArrays;
			IdentityResolverDelegate[] customIdentityResolver = new IdentityResolverDelegate[2];
			customIdentityResolver[0] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.DistinguishedNameIdentityResolver));
			IdentityResolverDelegate[] genericIdentityResolver = new IdentityResolverDelegate[2];
			genericIdentityResolver[0] = IdentityResolverMethods.GetGenericIdentityResolver(ADSchemaObjectFactory<T>._identityLdapAttributes);
			genericIdentityResolver[1] = new IdentityResolverDelegate(IdentityResolverMethods.GuidSearchFilterIdentityResolver);
			customIdentityResolver[1] = IdentityResolverMethods.GetAggregatedIdentityResolver(ADOperator.Or, genericIdentityResolver);
			ADSchemaObjectFactory<T>._identityResolvers = customIdentityResolver;
		}

		public ADSchemaObjectFactory()
		{
		}
	}
}