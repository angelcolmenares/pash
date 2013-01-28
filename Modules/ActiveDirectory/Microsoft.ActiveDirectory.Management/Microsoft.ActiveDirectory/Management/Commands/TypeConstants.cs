using Microsoft.ActiveDirectory.Management;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class TypeConstants
	{
		internal readonly static Type Object;

		internal readonly static Type String;

		internal readonly static Type DateTime;

		internal readonly static Type Bool;

		internal readonly static Type Int;

		internal readonly static Type Guid;

		internal readonly static Type Long;

		internal readonly static Type SID;

		internal readonly static Type TimeSpan;

		internal readonly static Type X509Certificate;

		internal readonly static Type ByteArray;

		internal readonly static Type ADGroup;

		internal readonly static Type ADPrincipal;

		internal readonly static Type ADComputer;

		internal readonly static Type ADUser;

		internal readonly static Type ADDomain;

		internal readonly static Type ADForest;

		internal readonly static Type ADDomainController;

		internal readonly static Type ADPartition;

		internal readonly static Type ADDirectoryServer;

		internal readonly static Type ADReplicationSite;

		internal readonly static Type ADReplicationSiteLink;

		internal readonly static Type ADReplicationSubnet;

		internal readonly static Type ADClaimTypeBase;

		internal readonly static Type ADCentralAccessRule;

		internal readonly static Type ADCentralAccessPolicy;

		internal readonly static Type ADResourcePropertyList;

		internal readonly static Type ADResourcePropertyValueType;

		internal readonly static Type ADResourceProperty;

		internal readonly static Type ADClaimTransformPolicy;

		internal readonly static Type ADGroupScope;

		internal readonly static Type ADGroupCategory;

		internal readonly static Type ADOperationMasterRole;

		internal readonly static Type ADDomainMode;

		internal readonly static Type ADForestMode;

		internal readonly static Type ADPartnerType;

		internal readonly static Type ADInterSiteTransportProtocolType;

		internal readonly static Type ADReplicationOperationType;

		internal readonly static Type ADClaimValueType;

		internal readonly static Type ADKerberosEncryptionType;

		internal readonly static Type ADSuggestedValueEntry;

		internal readonly static Type ADClaimType;

		internal readonly static Type ADTrustDirection;

		internal readonly static Type ADTrustType;

		static TypeConstants()
		{
			TypeConstants.Object = typeof(object);
			TypeConstants.String = typeof(string);
			TypeConstants.DateTime = typeof(DateTime);
			TypeConstants.Bool = typeof(bool);
			TypeConstants.Int = typeof(int);
			TypeConstants.Guid = typeof(Guid);
			TypeConstants.Long = typeof(long);
			TypeConstants.SID = typeof(SecurityIdentifier);
			TypeConstants.TimeSpan = typeof(TimeSpan);
			TypeConstants.X509Certificate = typeof(X509Certificate);
			TypeConstants.ByteArray = typeof(string);
			TypeConstants.ADGroup = typeof(string);
			TypeConstants.ADPrincipal = typeof(string);
			TypeConstants.ADComputer = typeof(string);
			TypeConstants.ADUser = typeof(string);
			TypeConstants.ADDomain = typeof(string);
			TypeConstants.ADForest = typeof(string);
			TypeConstants.ADDomainController = typeof(string);
			TypeConstants.ADPartition = typeof(string);
			TypeConstants.ADDirectoryServer = typeof(string);
			TypeConstants.ADReplicationSite = typeof(string);
			TypeConstants.ADReplicationSiteLink = typeof(string);
			TypeConstants.ADReplicationSubnet = typeof(string);
			TypeConstants.ADClaimTypeBase = typeof(string);
			TypeConstants.ADCentralAccessRule = typeof(string);
			TypeConstants.ADCentralAccessPolicy = typeof(string);
			TypeConstants.ADResourcePropertyList = typeof(string);
			TypeConstants.ADResourcePropertyValueType = typeof(string);
			TypeConstants.ADResourceProperty = typeof(string);
			TypeConstants.ADClaimTransformPolicy = typeof(string);
			TypeConstants.ADGroupScope = typeof(ADGroupScope);
			TypeConstants.ADGroupCategory = typeof(ADGroupCategory);
			TypeConstants.ADOperationMasterRole = typeof(ADOperationMasterRole);
			TypeConstants.ADDomainMode = typeof(ADDomainMode);
			TypeConstants.ADForestMode = typeof(ADForestMode);
			TypeConstants.ADPartnerType = typeof(ADPartnerType);
			TypeConstants.ADInterSiteTransportProtocolType = typeof(ADInterSiteTransportProtocolType);
			TypeConstants.ADReplicationOperationType = typeof(ADReplicationOperationType);
			TypeConstants.ADClaimValueType = typeof(ADClaimValueType);
			TypeConstants.ADKerberosEncryptionType = typeof(ADKerberosEncryptionType);
			TypeConstants.ADSuggestedValueEntry = typeof(ADSuggestedValueEntry);
			TypeConstants.ADClaimType = typeof(ADClaimType);
			TypeConstants.ADTrustDirection = typeof(ADTrustDirection);
			TypeConstants.ADTrustType = typeof(ADTrustType);
		}
	}
}