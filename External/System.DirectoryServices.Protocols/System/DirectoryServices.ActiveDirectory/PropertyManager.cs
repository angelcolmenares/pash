using System;
using System.DirectoryServices;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.ActiveDirectory
{
	internal class PropertyManager
	{
		public static string DefaultNamingContext;

		public static string SchemaNamingContext;

		public static string ConfigurationNamingContext;

		public static string RootDomainNamingContext;

		public static string MsDSBehaviorVersion;

		public static string FsmoRoleOwner;

		public static string ForestFunctionality;

		public static string NTMixedDomain;

		public static string DomainFunctionality;

		public static string ObjectCategory;

		public static string SystemFlags;

		public static string DnsRoot;

		public static string DistinguishedName;

		public static string TrustParent;

		public static string FlatName;

		public static string Name;

		public static string Flags;

		public static string TrustType;

		public static string TrustAttributes;

		public static string BecomeSchemaMaster;

		public static string BecomeDomainMaster;

		public static string BecomePdc;

		public static string BecomeRidMaster;

		public static string BecomeInfrastructureMaster;

		public static string DnsHostName;

		public static string Options;

		public static string CurrentTime;

		public static string HighestCommittedUSN;

		public static string OperatingSystem;

		public static string HasMasterNCs;

		public static string MsDSHasMasterNCs;

		public static string MsDSHasFullReplicaNCs;

		public static string NCName;

		public static string Cn;

		public static string NETBIOSName;

		public static string DomainDNS;

		public static string InstanceType;

		public static string MsDSSDReferenceDomain;

		public static string MsDSPortLDAP;

		public static string MsDSPortSSL;

		public static string MsDSNCReplicaLocations;

		public static string MsDSNCROReplicaLocations;

		public static string SupportedCapabilities;

		public static string ServerName;

		public static string Enabled;

		public static string ObjectGuid;

		public static string Keywords;

		public static string ServiceBindingInformation;

		public static string MsDSReplAuthenticationMode;

		public static string HasPartialReplicaNCs;

		public static string Container;

		public static string LdapDisplayName;

		public static string AttributeID;

		public static string AttributeSyntax;

		public static string Description;

		public static string SearchFlags;

		public static string OMSyntax;

		public static string OMObjectClass;

		public static string IsSingleValued;

		public static string IsDefunct;

		public static string RangeUpper;

		public static string RangeLower;

		public static string IsMemberOfPartialAttributeSet;

		public static string ObjectVersion;

		public static string LinkID;

		public static string ObjectClassCategory;

		public static string SchemaUpdateNow;

		public static string SubClassOf;

		public static string SchemaIDGuid;

		public static string PossibleSuperiors;

		public static string PossibleInferiors;

		public static string MustContain;

		public static string MayContain;

		public static string SystemMustContain;

		public static string SystemMayContain;

		public static string GovernsID;

		public static string IsGlobalCatalogReady;

		public static string NTSecurityDescriptor;

		public static string DsServiceName;

		public static string ReplicateSingleObject;

		public static string MsDSMasteredBy;

		public static string DefaultSecurityDescriptor;

		public static string NamingContexts;

		public static string MsDSDefaultNamingContext;

		public static string OperatingSystemVersion;

		public static string AuxiliaryClass;

		public static string SystemAuxiliaryClass;

		public static string SystemPossibleSuperiors;

		public static string InterSiteTopologyGenerator;

		public static string FromServer;

		public static string SiteList;

		public static string MsDSHasInstantiatedNCs;

		static PropertyManager()
		{
			PropertyManager.DefaultNamingContext = "defaultNamingContext";
			PropertyManager.SchemaNamingContext = "schemaNamingContext";
			PropertyManager.ConfigurationNamingContext = "configurationNamingContext";
			PropertyManager.RootDomainNamingContext = "rootDomainNamingContext";
			PropertyManager.MsDSBehaviorVersion = "msDS-Behavior-Version";
			PropertyManager.FsmoRoleOwner = "fsmoRoleOwner";
			PropertyManager.ForestFunctionality = "forestFunctionality";
			PropertyManager.NTMixedDomain = "ntMixedDomain";
			PropertyManager.DomainFunctionality = "domainFunctionality";
			PropertyManager.ObjectCategory = "objectCategory";
			PropertyManager.SystemFlags = "systemFlags";
			PropertyManager.DnsRoot = "dnsRoot";
			PropertyManager.DistinguishedName = "distinguishedName";
			PropertyManager.TrustParent = "trustParent";
			PropertyManager.FlatName = "flatName";
			PropertyManager.Name = "name";
			PropertyManager.Flags = "flags";
			PropertyManager.TrustType = "trustType";
			PropertyManager.TrustAttributes = "trustAttributes";
			PropertyManager.BecomeSchemaMaster = "becomeSchemaMaster";
			PropertyManager.BecomeDomainMaster = "becomeDomainMaster";
			PropertyManager.BecomePdc = "becomePdc";
			PropertyManager.BecomeRidMaster = "becomeRidMaster";
			PropertyManager.BecomeInfrastructureMaster = "becomeInfrastructureMaster";
			PropertyManager.DnsHostName = "dnsHostName";
			PropertyManager.Options = "options";
			PropertyManager.CurrentTime = "currentTime";
			PropertyManager.HighestCommittedUSN = "highestCommittedUSN";
			PropertyManager.OperatingSystem = "operatingSystem";
			PropertyManager.HasMasterNCs = "hasMasterNCs";
			PropertyManager.MsDSHasMasterNCs = "msDS-HasMasterNCs";
			PropertyManager.MsDSHasFullReplicaNCs = "msDS-hasFullReplicaNCs";
			PropertyManager.NCName = "nCName";
			PropertyManager.Cn = "cn";
			PropertyManager.NETBIOSName = "nETBIOSName";
			PropertyManager.DomainDNS = "domainDNS";
			PropertyManager.InstanceType = "instanceType";
			PropertyManager.MsDSSDReferenceDomain = "msDS-SDReferenceDomain";
			PropertyManager.MsDSPortLDAP = "msDS-PortLDAP";
			PropertyManager.MsDSPortSSL = "msDS-PortSSL";
			PropertyManager.MsDSNCReplicaLocations = "msDS-NC-Replica-Locations";
			PropertyManager.MsDSNCROReplicaLocations = "msDS-NC-RO-Replica-Locations";
			PropertyManager.SupportedCapabilities = "supportedCapabilities";
			PropertyManager.ServerName = "serverName";
			PropertyManager.Enabled = "Enabled";
			PropertyManager.ObjectGuid = "objectGuid";
			PropertyManager.Keywords = "keywords";
			PropertyManager.ServiceBindingInformation = "serviceBindingInformation";
			PropertyManager.MsDSReplAuthenticationMode = "msDS-ReplAuthenticationMode";
			PropertyManager.HasPartialReplicaNCs = "hasPartialReplicaNCs";
			PropertyManager.Container = "container";
			PropertyManager.LdapDisplayName = "ldapDisplayName";
			PropertyManager.AttributeID = "attributeID";
			PropertyManager.AttributeSyntax = "attributeSyntax";
			PropertyManager.Description = "description";
			PropertyManager.SearchFlags = "searchFlags";
			PropertyManager.OMSyntax = "oMSyntax";
			PropertyManager.OMObjectClass = "oMObjectClass";
			PropertyManager.IsSingleValued = "isSingleValued";
			PropertyManager.IsDefunct = "isDefunct";
			PropertyManager.RangeUpper = "rangeUpper";
			PropertyManager.RangeLower = "rangeLower";
			PropertyManager.IsMemberOfPartialAttributeSet = "isMemberOfPartialAttributeSet";
			PropertyManager.ObjectVersion = "objectVersion";
			PropertyManager.LinkID = "linkID";
			PropertyManager.ObjectClassCategory = "objectClassCategory";
			PropertyManager.SchemaUpdateNow = "schemaUpdateNow";
			PropertyManager.SubClassOf = "subClassOf";
			PropertyManager.SchemaIDGuid = "schemaIDGUID";
			PropertyManager.PossibleSuperiors = "possSuperiors";
			PropertyManager.PossibleInferiors = "possibleInferiors";
			PropertyManager.MustContain = "mustContain";
			PropertyManager.MayContain = "mayContain";
			PropertyManager.SystemMustContain = "systemMustContain";
			PropertyManager.SystemMayContain = "systemMayContain";
			PropertyManager.GovernsID = "governsID";
			PropertyManager.IsGlobalCatalogReady = "isGlobalCatalogReady";
			PropertyManager.NTSecurityDescriptor = "ntSecurityDescriptor";
			PropertyManager.DsServiceName = "dsServiceName";
			PropertyManager.ReplicateSingleObject = "replicateSingleObject";
			PropertyManager.MsDSMasteredBy = "msDS-masteredBy";
			PropertyManager.DefaultSecurityDescriptor = "defaultSecurityDescriptor";
			PropertyManager.NamingContexts = "namingContexts";
			PropertyManager.MsDSDefaultNamingContext = "msDS-DefaultNamingContext";
			PropertyManager.OperatingSystemVersion = "operatingSystemVersion";
			PropertyManager.AuxiliaryClass = "auxiliaryClass";
			PropertyManager.SystemAuxiliaryClass = "systemAuxiliaryClass";
			PropertyManager.SystemPossibleSuperiors = "systemPossSuperiors";
			PropertyManager.InterSiteTopologyGenerator = "interSiteTopologyGenerator";
			PropertyManager.FromServer = "fromServer";
			PropertyManager.SiteList = "siteList";
			PropertyManager.MsDSHasInstantiatedNCs = "msDS-HasInstantiatedNCs";
		}

		public PropertyManager()
		{
		}

		public static object GetPropertyValue(DirectoryEntry directoryEntry, string propertyName)
		{
			return PropertyManager.GetPropertyValue(null, directoryEntry, propertyName);
		}

		public static object GetPropertyValue(DirectoryContext context, DirectoryEntry directoryEntry, string propertyName)
		{
			try
			{
				if (directoryEntry.Properties[propertyName].Count == 0)
				{
					if (directoryEntry.Properties[PropertyManager.DistinguishedName].Count == 0)
					{
						object[] objArray = new object[1];
						objArray[0] = propertyName;
						throw new ActiveDirectoryOperationException(Res.GetString("PropertyNotFound", objArray));
					}
					else
					{
						object[] value = new object[2];
						value[0] = propertyName;
						value[1] = directoryEntry.Properties[PropertyManager.DistinguishedName].Value;
						throw new ActiveDirectoryOperationException(Res.GetString("PropertyNotFoundOnObject", value));
					}
				}
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException);
			}
			return directoryEntry.Properties[propertyName].Value;
		}

		public static object GetSearchResultPropertyValue(SearchResult res, string propertyName)
		{
			ResultPropertyValueCollection item = null;
			try
			{
				item = res.Properties[propertyName];
				if (item == null || item.Count < 1)
				{
					object[] objArray = new object[1];
					objArray[0] = propertyName;
					throw new ActiveDirectoryOperationException(Res.GetString("PropertyNotFound", objArray));
				}
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
			}
			return item[0];
		}
	}
}