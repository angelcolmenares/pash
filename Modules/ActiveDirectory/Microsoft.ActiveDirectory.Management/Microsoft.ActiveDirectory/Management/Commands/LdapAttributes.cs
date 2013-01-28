using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class LdapAttributes
	{
		internal const string DistinguishedName = "distinguishedName";

		internal const string ObjectGUID = "objectGUID";

		internal const string ObjectSid = "objectSid";

		internal const string ObjectClass = "objectClass";

		internal const string ObjectCategory = "objectCategory";

		internal const string Name = "name";

		internal const string GroupType = "groupType";

		internal const string ManagedBy = "managedBy";

		internal const string Member = "member";

		internal const string CanonicalName = "canonicalName";

		internal const string CommonName = "cn";

		internal const string CreationTimeStamp = "createTimeStamp";

		internal const string IsDeleted = "isDeleted";

		internal const string Description = "description";

		internal const string DisplayName = "displayName";

		internal const string NTSecurityDescriptor = "nTSecurityDescriptor";

		internal const string SDRightsEffective = "sdRightsEffective";

		internal const string InstanceType = "instanceType";

		internal const string LastKnownParent = "lastKnownParent";

		internal const string LastKnownRDN = "msDS-LastKnownRDN";

		internal const string ModifiedTimeStamp = "modifyTimeStamp";

		internal const string Street = "street";

		internal const string LinkedGroupPolicyObjects = "gpLink";

		internal const string PostalCode = "postalCode";

		internal const string City = "l";

		internal const string State = "st";

		internal const string Country = "c";

		internal const string SamAccountName = "sAMAccountName";

		internal const string UserAccountControl = "userAccountControl";

		internal const string UserAccountControlComputed = "msDS-User-Account-Control-Computed";

		internal const string PrimaryGroup = "primaryGroupID";

		internal const string AccountPassword = "unicodePwd";

		internal const string UserPrincipalName = "userPrincipalName";

		internal const string AccountExpirationDate = "accountExpires";

		internal const string AccountLockoutTime = "lockoutTime";

		internal const string ADAMAllowReversiblePasswordEncryption = "ms-DS-UserEncryptedTextPasswordAllowed";

		internal const string BadLogonCount = "badPwdCount";

		internal const string Certificates = "userCertificate";

		internal const string ADAMDisabled = "msDS-UserAccountDisabled";

		internal const string LastBadPasswordAttempt = "badPasswordTime";

		internal const string LastLogonTimeStamp = "lastLogonTimestamp";

		internal const string PasswordLastSet = "pwdLastSet";

		internal const string ADAMPasswordNeverExpires = "msDS-UserDontExpirePassword";

		internal const string ADAMPasswordNotRequired = "ms-DS-UserPasswordNotRequired";

		internal const string ADAMLockedOut = "ms-DS-UserAccountAutoLocked";

		internal const string ADAMPasswordExpired = "msDS-UserPasswordExpired";

		internal const string ServicePrincipalNames = "servicePrincipalName";

		internal const string MSDSUserAccountControlComputed = "msDS-User-Account-Control-Computed";

		internal const string UnicodePwd = "unicodePwd";

		internal const string StreetAddress = "streetAddress";

		internal const string GivenName = "givenName";

		internal const string Surname = "sn";

		internal const string HomeDrive = "homeDrive";

		internal const string HomeDirectory = "homeDirectory";

		internal const string Manager = "manager";

		internal const string OtherName = "middleName";

		internal const string LogonWorkstations = "userWorkstations";

		internal const string ProfilePath = "profilePath";

		internal const string ScriptPath = "scriptPath";

		internal const string OfficePhone = "telephoneNumber";

		internal const string Company = "company";

		internal const string Department = "department";

		internal const string Fax = "facsimileTelephoneNumber";

		internal const string Initials = "initials";

		internal const string MobilePhone = "mobile";

		internal const string HomePhone = "homePhone";

		internal const string Office = "physicalDeliveryOfficeName";

		internal const string POBox = "postOfficeBox";

		internal const string Title = "title";

		internal const string Division = "division";

		internal const string EmployeeID = "employeeID";

		internal const string EmployeeNumber = "employeeNumber";

		internal const string Organization = "o";

		internal const string EmailAddress = "mail";

		internal const string ResultantPSO = "msDS-ResultantPSO";

		internal const string DNSHostName = "dNSHostName";

		internal const string Location = "location";

		internal const string OS = "operatingSystem";

		internal const string OSHotfix = "operatingSystemHotfix";

		internal const string OSServicePack = "operatingSystemServicePack";

		internal const string OSVersion = "operatingSystemVersion";

		internal const string ServiceAccount = "msDS-HostServiceAccount";

		internal const string HostComputers = "msDS-HostServiceAccountBL";

		internal const string MsaGroupMembership = "msDS-GroupMSAMembership";

		internal const string MsaManagedPasswordInterval = "msDS-ManagedPasswordInterval";

		internal const string SupportedEncryptionTypes = "msDS-SupportedEncryptionTypes";

		internal const string AllowedToActOnBehalfOf = "msDS-AllowedToActOnBehalfOfOtherIdentity";

		internal const string LastLogonReplicationInterval = "msDS-LogonTimeSyncInterval";

		internal const string AllowedDNSSuffixes = "msDS-AllowedDNSSuffixes";

		internal const string BehaviorVersion = "msDS-Behavior-Version";

		internal const string NTMixedDomainMode = "ntMixedDomain";

		internal const string UPNSuffixes = "uPNSuffixes";

		internal const string SPNSuffixes = "msDS-SPNSuffixes";

		internal const string AllowedPasswordReplicationPolicy = "msDS-RevealOnDemandGroup";

		internal const string DeniedPasswordReplicationPolicy = "msDS-NeverRevealGroup";

		internal const string AuthenticatedAccounts = "msDS-AuthenticatedToAccountlist";

		internal const string RevealedAccounts = "msDS-RevealedList";

		internal const string ServerReferenceBL = "serverReferenceBL";

		internal const string NCName = "nCName";

		internal const string NETBIOSName = "nETBIOSName";

		internal const string DNSRoot = "dnsRoot";

		internal const string SystemFlags = "systemFlags";

		internal const string LockoutDuration = "msDS-LockoutDuration";

		internal const string LockoutObservationWindow = "msDS-LockoutObservationWindow";

		internal const string LockoutThreshold = "msDS-LockoutThreshold";

		internal const string MaxPasswordAge = "msDS-MaximumPasswordAge";

		internal const string MinPasswordAge = "msDS-MinimumPasswordAge";

		internal const string MinPasswordLength = "msDS-MinimumPasswordLength";

		internal const string PasswordHistoryCount = "msDS-PasswordHistoryLength";

		internal const string ComplexityEnabled = "msDS-PasswordComplexityEnabled";

		internal const string ReversibleEncryptionEnabled = "msDS-PasswordReversibleEncryptionEnabled";

		internal const string Precedence = "msDS-PasswordSettingsPrecedence";

		internal const string AppliesTo = "msDS-PSOAppliesTo";

		internal const string MSDSPortLDAP = "msDS-PortLDAP";

		internal const string MSDSIsUserCachableAtRodc = "msDS-IsUserCachableAtRodc";

		internal const string ServerReference = "serverReference";

		internal const string DefaultLockoutDuration = "lockoutDuration";

		internal const string DefaultLockoutObservationWindow = "lockoutObservationWindow";

		internal const string DefaultLockoutThreshold = "lockoutThreshold";

		internal const string DefaultMaxPasswordAge = "maxPwdAge";

		internal const string DefaultMinPasswordAge = "minPwdAge";

		internal const string DefaultMinPasswordLength = "minPwdLength";

		internal const string DefaultPasswordHistoryCount = "pwdHistoryLength";

		internal const string DefaultPasswordProperties = "pwdProperties";

		internal const string EnabledScopes = "msDS-EnabledFeatureBL";

		internal const string FeatureScope = "msDS-OptionalFeatureFlags";

		internal const string FeatureGUID = "msDS-OptionalFeatureGUID";

		internal const string RequiredDomainMode = "msDS-RequiredDomainBehaviorVersion";

		internal const string RequiredForestMode = "msDS-RequiredForestBehaviorVersion";

		internal const string InterSiteTopologyGenerator = "interSiteTopologyGenerator";

		internal const string ReplicationSchedule = "schedule";

		internal const string UniversalGroupCachingRefreshSite = "msDS-Preferred-GC-Site";

		internal const string Options = "Options";

		internal const string Subnet = "siteObjectBL";

		internal const string Site = "siteObject";

		internal const string ReplicationFrequency = "replInterval";

		internal const string Cost = "cost";

		internal const string SitesIncluded = "siteList";

		internal const string SiteLinksIncluded = "siteLinkList";

		internal const string SourceXmlAttribute = "sourceXmlAttribute";

		internal const string ReplicateSingleObject = "replicateSingleObject";

		internal const string ReplicationInboundPartners = "msDS-NCReplInboundNeighbors";

		internal const string ReplicationOutboundPartners = "msDS-NCReplOutboundNeighbors";

		internal const string ReplicationPartnersObjectType = "DS_REPL_NEIGHBOR";

		internal const string PszSourceDsaDN = "pszSourceDsaDN";

		internal const string PszSourceDsaAddress = "pszSourceDsaAddress";

		internal const string UuidSourceDsaObjGuid = "uuidSourceDsaObjGuid";

		internal const string UuidSourceDsaInvocationID = "uuidSourceDsaInvocationID";

		internal const string PszNamingContext = "pszNamingContext";

		internal const string FTimeLastSyncAttempt = "ftimeLastSyncAttempt";

		internal const string DwLastSyncResult = "dwLastSyncResult";

		internal const string FTimeLastSyncSuccess = "ftimeLastSyncSuccess";

		internal const string CNumConsecutiveSyncFailures = "cNumConsecutiveSyncFailures";

		internal const string UsnLastObjChangeSynced = "usnLastObjChangeSynced";

		internal const string UsnAttributeFilter = "usnAttributeFilter";

		internal const string PszAsyncIntersiteTransportDN = "pszAsyncIntersiteTransportDN";

		internal const string UuidAsyncIntersiteTransportObjGuid = "uuidAsyncIntersiteTransportObjGuid";

		internal const string DwReplicaFlags = "dwReplicaFlags";

		internal const string ReplicationConnectionFailures = "msDS-ReplConnectionFailures";

		internal const string ReplicationLinkFailures = "msDS-ReplLinkFailures";

		internal const string ReplicationFailuresObjectType = "DS_REPL_KCC_DSA_FAILURE";

		internal const string PszDsaDN = "pszDsaDN";

		internal const string UuidDsaObjGuid = "uuidDsaObjGuid";

		internal const string CNumFailures = "cNumFailures";

		internal const string FTimeFirstFailure = "ftimeFirstFailure";

		internal const string DwLastResult = "dwLastResult";

		internal const string ReplicationQueue = "msDS-ReplPendingOps";

		internal const string ReplicationQueueObjectType = "DS_REPL_OP";

		internal const string PszDsaAddress = "pszDsaAddress";

		internal const string FTimeEnqueued = "ftimeEnqueued";

		internal const string UlSerialNumber = "ulSerialNumber";

		internal const string OpType = "OpType";

		internal const string UlOptions = "ulOptions";

		internal const string UlPriority = "ulPriority";

		internal const string ReplicationAttributeMetadata = "msDS-ReplAttributeMetaData";

		internal const string ReplicationAttributeValueMetadata = "msDS-ReplValueMetaData";

		internal const string ReplicationAttributeMetadataObjectType = "DS_REPL_ATTR_META_DATA";

		internal const string ReplicationAttributeValueMetadataObjectType = "DS_REPL_VALUE_META_DATA";

		internal const string PszAttributeName = "pszAttributeName";

		internal const string PszObjectDn = "pszObjectDn";

		internal const string DwVersion = "dwVersion";

		internal const string UsnOriginatingChange = "usnOriginatingChange";

		internal const string FTimeLastOriginatingChange = "ftimeLastOriginatingChange";

		internal const string UuidLastOriginatingDsaInvocationID = "uuidLastOriginatingDsaInvocationID";

		internal const string PszLastOriginatingDsaDN = "pszLastOriginatingDsaDN";

		internal const string UsnLocalChange = "usnLocalChange";

		internal const string FTimeDeleted = "ftimeDeleted";

		internal const string FTimeCreated = "ftimeCreated";

		internal const string ReplicationUpToDatenessVector = "msDS-NCReplCursors";

		internal const string ReplicationUpToDatenessVectorObjectType = "DS_REPL_CURSOR";

		internal const string MSDShasMasterNCs = "msDS-hasMasterNCs";

		internal const string MSDShasFullReplicaNCs = "msDS-hasFullReplicaNCs";

		internal const string PartiallyReplicatedNamingContexts = "hasPartialReplicaNCs";

		internal const string TransportType = "transportType";

		internal const string ReplicateFromDirectoryServer = "fromServer";

		internal const string MsDSSClaimSharesPossibleValuesWith = "msDS-ClaimSharesPossibleValuesWith";

		internal const string MsDSURI = "msDS-URI";

		internal const string MsDSClaimIsValueSpaceRestricted = "msDS-ClaimIsValueSpaceRestricted";

		internal const string MsDSClaimValueType = "msDS-ClaimValueType";

		internal const string MsDSClaimPossibleValues = "msDS-ClaimPossibleValues";

		internal const string Enabled = "Enabled";

		internal const string MsDSMembersOfResourcePropertyListBL = "msDS-MembersOfResourcePropertyListBL";

		internal const string MsDSClaimIsSingleValued = "msDS-ClaimIsSingleValued";

		internal const string MsDSValueTypeReference = "msDS-ValueTypeReference";

		internal const string MsDSIsUsedAsResourceSecurityAttribute = "msDS-IsUsedAsResourceSecurityAttribute";

		internal const string MsDSAppliesToResourceTypes = "msDS-AppliesToResourceTypes";

		internal const string MsDSClaimAttributeSource = "msDS-ClaimAttributeSource";

		internal const string MsDSClaimSource = "msDS-ClaimSource";

		internal const string MsDSClaimTypeAppliesToClass = "msDS-ClaimTypeAppliesToClass";

		internal const string MsDSClaimSourceType = "msDS-ClaimSourceType";

		internal const string MsDSClaimSharesPossibleValuesWithBL = "msDS-ClaimSharesPossibleValuesWithBL";

		internal const string MSAuthzResourceCondition = "msAuthz-ResourceCondition";

		internal const string MSAuthzEffectiveDACL = "msAuthz-EffectiveSecurityPolicy";

		internal const string MSAuthzProposedDACL = "msAuthz-ProposedSecurityPolicy";

		internal const string MSAuthzLastEffectiveDACL = "msAuthz-LastEffectiveSecurityPolicy";

		internal const string MemberRulesInCAPBL = "msAuthz-MemberRulesInCentralAccessPolicyBL";

		internal const string MemberRulesInCAP = "msAuthz-MemberRulesInCentralAccessPolicy";

		internal const string MSAuthzCentralAccessPolicyID = "msAuthz-CentralAccessPolicyID";

		internal const string LdapDisplayName = "lDAPDisplayName";

		internal const string AttributeSyntax = "attributeSyntax";

		internal const string IsDefunct = "isDefunct";

		internal const string SearchFlags = "searchFlags";

		internal const string MembersOfResourcePropertyList = "msDS-MembersOfResourcePropertyList";

		internal const string MSDSIsPossibleValuesPresent = "msDS-IsPossibleValuesPresent";

		internal const string MSDSValueTypeReferenceBL = "msDS-ValueTypeReferenceBL";

		internal const string Target = "trustPartner";

		internal const string TrustDirection = "trustDirection";

		internal const string TrustType = "trustType";

		internal const string TrustAttributes = "trustAttributes";

		internal const string TrustingPolicy = "msDS-IngressClaimsTransformationPolicy";

		internal const string TrustedPolicy = "msDS-EgressClaimsTransformationPolicy";

		internal const string Rule = "msDS-TransformationRules";

		internal const string IncomingTrust = "msDS-TDOEgressBL";

		internal const string OutgoingTrust = "msDS-TDOIngressBL";

	}
}