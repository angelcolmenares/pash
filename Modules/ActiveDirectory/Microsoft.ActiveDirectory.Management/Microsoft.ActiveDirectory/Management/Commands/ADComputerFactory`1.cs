using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADComputerFactory<T> : ADAccountFactory<T>
	where T : ADComputer, new()
	{
		private readonly static IADOPathNode _structuralObjectFilter;

		private readonly static string _rDNPrefix;

		private readonly static string _structuralObjectClass;

		private static string[] _computerIdentityLdapAttributes;

		private readonly static IdentityResolverDelegate[] _identityResolvers;

		private static AttributeConverterEntry[] ADMappingTable;

		private static AttributeConverterEntry[] ADAMMappingTable;

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
				return ADComputerFactory<T>._identityResolvers;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				return ADComputerFactory<T>._rDNPrefix;
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return ADComputerFactory<T>._structuralObjectClass;
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADComputerFactory<T>._structuralObjectFilter;
			}
		}

		static ADComputerFactory()
		{
			IADOPathNode[] aDOPathNodeArray = new IADOPathNode[2];
			aDOPathNodeArray[0] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "computer");
			aDOPathNodeArray[1] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectCategory", "computer");
			ADComputerFactory<T>._structuralObjectFilter = ADOPathUtil.CreateAndClause(aDOPathNodeArray);
			ADComputerFactory<T>._rDNPrefix = "CN";
			ADComputerFactory<T>._structuralObjectClass = "computer";
			string[] strArrays = new string[1];
			strArrays[0] = "sAMAccountName";
			ADComputerFactory<T>._computerIdentityLdapAttributes = strArrays;
			IdentityResolverDelegate[] customIdentityResolver = new IdentityResolverDelegate[5];
			customIdentityResolver[0] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.DistinguishedNameIdentityResolver));
			customIdentityResolver[1] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.GuidIdentityResolver));
			customIdentityResolver[2] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.SidIdentityResolver));
			customIdentityResolver[3] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.SamAccountNameIdentityResolver));
			customIdentityResolver[4] = ADComputerUtil.GetGenericIdentityResolverWithSamName(ADComputerFactory<T>._computerIdentityLdapAttributes);
			ADComputerFactory<T>._identityResolvers = customIdentityResolver;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[11];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADPrincipalFactory<T>.ADPrincipalPropertyMap.SamAccountName.PropertyName, ADPrincipalFactory<T>.ADPrincipalPropertyMap.SamAccountName.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(ADComputerUtil.ToDirectoryComputerSamAccountName), new ToSearchFilterDelegate(ADComputerUtil.ToSearchComputerSamAccountName));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADComputerFactory<T>.ADComputerPropertyMap.DNSHostName.PropertyName, ADComputerFactory<T>.ADComputerPropertyMap.DNSHostName.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADComputerFactory<T>.ADComputerPropertyMap.ServiceAccount.PropertyName, ADComputerFactory<T>.ADComputerPropertyMap.ServiceAccount.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, false, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryMultivalueObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADComputerFactory<T>.ADComputerPropertyMap.IPv4Address.PropertyName, ADComputerFactory<T>.ADComputerPropertyMap.IPv4Address.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(ADComputerFactory<T>.ToExtendedIPv4), null, new ToSearchFilterDelegate(ADComputerFactory<T>.ToSearchIPv4));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADComputerFactory<T>.ADComputerPropertyMap.IPv6Address.PropertyName, ADComputerFactory<T>.ADComputerPropertyMap.IPv6Address.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(ADComputerFactory<T>.ToExtendedIPv6), null, new ToSearchFilterDelegate(ADComputerFactory<T>.ToSearchIPv6));
			attributeConverterEntry[5] = new AttributeConverterEntry(ADComputerFactory<T>.ADComputerPropertyMap.Location.PropertyName, ADComputerFactory<T>.ADComputerPropertyMap.Location.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[6] = new AttributeConverterEntry(ADComputerFactory<T>.ADComputerPropertyMap.ManagedBy.PropertyName, ADComputerFactory<T>.ADComputerPropertyMap.ManagedBy.ADAttribute, TypeConstants.ADPrincipal, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryFromADObjectToDN<ADPrincipalFactory<ADPrincipal>, ADPrincipal>), new ToSearchFilterDelegate(SearchConverters.ToSearchFromADObjectToDN<ADPrincipalFactory<ADPrincipal>, ADPrincipal>));
			attributeConverterEntry[7] = new AttributeConverterEntry(ADComputerFactory<T>.ADComputerPropertyMap.OS.PropertyName, ADComputerFactory<T>.ADComputerPropertyMap.OS.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[8] = new AttributeConverterEntry(ADComputerFactory<T>.ADComputerPropertyMap.OSHotfix.PropertyName, ADComputerFactory<T>.ADComputerPropertyMap.OSHotfix.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[9] = new AttributeConverterEntry(ADComputerFactory<T>.ADComputerPropertyMap.OSServicePack.PropertyName, ADComputerFactory<T>.ADComputerPropertyMap.OSServicePack.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[10] = new AttributeConverterEntry(ADComputerFactory<T>.ADComputerPropertyMap.OSVersion.PropertyName, ADComputerFactory<T>.ADComputerPropertyMap.OSVersion.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			ADComputerFactory<T>.ADMappingTable = attributeConverterEntry;
			ADComputerFactory<T>.ADAMMappingTable = ADComputerFactory<T>.ADMappingTable;
			ADFactoryBase<T>.RegisterMappingTable(ADComputerFactory<T>.ADAMMappingTable, ADServerType.ADLDS);
			ADFactoryBase<T>.RegisterMappingTable(ADComputerFactory<T>.ADMappingTable, ADServerType.ADDS);
			ADAccountFactory<T>.DefaultUserAccessControl = 0x1002;
			ADAccountFactory<T>.UseComputerPasswordGeneration = true;
		}

		public ADComputerFactory()
		{
		}

		internal override AttributeSetRequest ConstructAttributeSetRequest(ICollection<string> requestedExtendedAttr)
		{
			AttributeSetRequest attributeSetRequest = base.ConstructAttributeSetRequest(requestedExtendedAttr);
			ADSchema aDSchema = new ADSchema(base.CmdletSessionInfo.ADSessionInfo);
			if (!aDSchema.SchemaProperties.ContainsKey("msDS-HostServiceAccount"))
			{
				attributeSetRequest.DirectoryAttributes.Remove("msDS-HostServiceAccount");
			}
			return attributeSetRequest;
		}

		internal static void ToExtendedIPv4(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (!directoryObj.Contains(directoryAttributes[0]))
			{
				userObj.Add(extendedAttribute, new ADPropertyValueCollection());
				return;
			}
			else
			{
				string value = directoryObj[directoryAttributes[0]].Value as string;
				userObj.Add(extendedAttribute, IPUtil.GetIPAddress(value, IPUtil.IPVersion.IPv4));
				return;
			}
		}

		internal static void ToExtendedIPv6(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (!directoryObj.Contains(directoryAttributes[0]))
			{
				userObj.Add(extendedAttribute, new ADPropertyValueCollection());
				return;
			}
			else
			{
				string value = directoryObj[directoryAttributes[0]].Value as string;
				userObj.Add(extendedAttribute, IPUtil.GetIPAddress(value, IPUtil.IPVersion.IPv6));
				return;
			}
		}

		internal static IADOPathNode ToSearchIPv4(string extendedAttribute, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			return IPUtil.BuildIPFilter(extendedAttribute, directoryAttributes[0], filterClause, IPUtil.IPVersion.IPv4);
		}

		internal static IADOPathNode ToSearchIPv6(string extendedAttribute, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			return IPUtil.BuildIPFilter(extendedAttribute, directoryAttributes[0], filterClause, IPUtil.IPVersion.IPv6);
		}

		private static class ADComputerPropertyMap
		{
			public readonly static PropertyMapEntry DNSHostName;

			public readonly static PropertyMapEntry IPv4Address;

			public readonly static PropertyMapEntry IPv6Address;

			public readonly static PropertyMapEntry Location;

			public readonly static PropertyMapEntry ManagedBy;

			public readonly static PropertyMapEntry OS;

			public readonly static PropertyMapEntry OSHotfix;

			public readonly static PropertyMapEntry OSServicePack;

			public readonly static PropertyMapEntry OSVersion;

			public readonly static PropertyMapEntry ServiceAccount;

			static ADComputerPropertyMap()
			{
				ADComputerFactory<T>.ADComputerPropertyMap.DNSHostName = new PropertyMapEntry("DNSHostName", "dNSHostName", "dNSHostName");
				ADComputerFactory<T>.ADComputerPropertyMap.IPv4Address = new PropertyMapEntry("IPv4Address", "dNSHostName", "dNSHostName");
				ADComputerFactory<T>.ADComputerPropertyMap.IPv6Address = new PropertyMapEntry("IPv6Address", "dNSHostName", "dNSHostName");
				ADComputerFactory<T>.ADComputerPropertyMap.Location = new PropertyMapEntry("Location", "location", "location");
				ADComputerFactory<T>.ADComputerPropertyMap.ManagedBy = new PropertyMapEntry("ManagedBy", "managedBy", "managedBy");
				ADComputerFactory<T>.ADComputerPropertyMap.OS = new PropertyMapEntry("OperatingSystem", "operatingSystem", "operatingSystem");
				ADComputerFactory<T>.ADComputerPropertyMap.OSHotfix = new PropertyMapEntry("OperatingSystemHotfix", "operatingSystemHotfix", "operatingSystemHotfix");
				ADComputerFactory<T>.ADComputerPropertyMap.OSServicePack = new PropertyMapEntry("OperatingSystemServicePack", "operatingSystemServicePack", "operatingSystemServicePack");
				ADComputerFactory<T>.ADComputerPropertyMap.OSVersion = new PropertyMapEntry("OperatingSystemVersion", "operatingSystemVersion", "operatingSystemVersion");
				ADComputerFactory<T>.ADComputerPropertyMap.ServiceAccount = new PropertyMapEntry("ServiceAccount", "msDS-HostServiceAccount", null);
			}
		}
	}
}