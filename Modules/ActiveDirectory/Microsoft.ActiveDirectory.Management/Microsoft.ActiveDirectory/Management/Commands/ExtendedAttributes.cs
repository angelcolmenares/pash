using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class ExtendedAttributes
	{
		internal const string Name = "Name";

		internal const string LogonName = "LogonName";

		internal const string Identity = "Identity";

		internal const string ObjectCategory = "ObjectCategory";

		internal const string ObjectGUID = "ObjectGUID";

		internal const string GroupCategory = "GroupCategory";

		internal const string GroupScope = "GroupScope";

		internal const string HomePage = "HomePage";

		internal const string ManagedBy = "ManagedBy";

		internal const string Members = "Members";

		internal const string MemberOf = "MemberOf";

		internal const string SID = "SID";

		internal const string SIDHistory = "SIDHistory";

		internal const string DistinguishedName = "DistinguishedName";

		internal const string ObjectClass = "ObjectClass";

		internal const string CanonicalName = "CanonicalName";

		internal const string CommonName = "CN";

		internal const string IsDeleted = "Deleted";

		internal const string CreationTimeStamp = "Created";

		internal const string Description = "Description";

		internal const string DisplayName = "DisplayName";

		internal const string ProtectedFromDeletion = "ProtectedFromAccidentalDeletion";

		internal const string LastKnownParent = "LastKnownParent";

		internal const string ModifiedTimeStamp = "Modified";

		internal const string Street = "StreetAddress";

		internal const string PostalCode = "PostalCode";

		internal const string City = "City";

		internal const string State = "State";

		internal const string Country = "Country";

		internal const string SamAccountName = "SamAccountName";

		internal const string AccountExpirationDate = "AccountExpirationDate";

		internal const string AccountPassword = "AccountPassword";

		internal const string Enabled = "Enabled";

		internal const string PrimaryGroup = "PrimaryGroup";

		internal const string UserPrincipalName = "UserPrincipalName";

		internal const string AccountLockoutTime = "AccountLockoutTime";

		internal const string AllowReversiblePasswordEncryption = "AllowReversiblePasswordEncryption";

		internal const string BadLogonCount = "BadLogonCount";

		internal const string CannotChangePassword = "CannotChangePassword";

		internal const string Certificates = "Certificates";

		internal const string AccountNotDelegated = "AccountNotDelegated";

		internal const string LastBadPasswordAttempt = "LastBadPasswordAttempt";

		internal const string LastLogonDate = "LastLogonDate";

		internal const string LockedOut = "LockedOut";

		internal const string PasswordLastSet = "PasswordLastSet";

		internal const string PasswordNeverExpires = "PasswordNeverExpires";

		internal const string PasswordNotRequired = "PasswordNotRequired";

		internal const string ServicePrincipalNames = "ServicePrincipalNames";

		internal const string TrustedForDelegation = "TrustedForDelegation";

		internal const string TrustedToAuthForDelegation = "TrustedToAuthForDelegation";

		internal const string MNSLogonAccount = "MNSLogonAccount";

		internal const string DoesNotRequirePreAuth = "DoesNotRequirePreAuth";

		internal const string PasswordExpired = "PasswordExpired";

		internal const string HomedirRequired = "HomedirRequired";

		internal const string UseDESKeyOnly = "UseDESKeyOnly";

		internal const string HostComputers = "HostComputers";

		internal const string PrincipalsAllowedToRetrieveManagedPassword = "PrincipalsAllowedToRetrieveManagedPassword";

		internal const string ManagedPasswordIntervalInDays = "ManagedPasswordIntervalInDays";

		internal const string KerberosEncryptionType = "KerberosEncryptionType";

		internal const string CompoundIdentitySupported = "CompoundIdentitySupported";

		internal const string PrincipalsAllowedToDelegateToAccount = "PrincipalsAllowedToDelegateToAccount";

		internal const string RestrictToSingleComputer = "RestrictToSingleComputer";

		internal const string RestrictToOutboundAuthenticationOnly = "RestrictToOutboundAuthenticationOnly";

		internal const string GivenName = "GivenName";

		internal const string Surname = "Surname";

		internal const string HomeDirectory = "HomeDirectory";

		internal const string HomeDrive = "HomeDrive";

		internal const string Manager = "Manager";

		internal const string OtherName = "OtherName";

		internal const string LogonWorkstations = "LogonWorkstations";

		internal const string ProfilePath = "ProfilePath";

		internal const string ScriptPath = "ScriptPath";

		internal const string SmartcardLogonRequired = "SmartcardLogonRequired";

		internal const string OfficePhone = "OfficePhone";

		internal const string Company = "Company";

		internal const string Department = "Department";

		internal const string Fax = "Fax";

		internal const string Initials = "Initials";

		internal const string MobilePhone = "MobilePhone";

		internal const string HomePhone = "HomePhone";

		internal const string Office = "Office";

		internal const string POBox = "POBox";

		internal const string Title = "Title";

		internal const string Division = "Division";

		internal const string EmployeeID = "EmployeeID";

		internal const string EmployeeNumber = "EmployeeNumber";

		internal const string Organization = "Organization";

		internal const string EmailAddress = "EmailAddress";

		internal const string StreetAddress = "StreetAddress";

		internal const string DNSHostName = "DNSHostName";

		internal const string IPv4Address = "IPv4Address";

		internal const string IPv6Address = "IPv6Address";

		internal const string Location = "Location";

		internal const string OS = "OperatingSystem";

		internal const string OSHotfix = "OperatingSystemHotfix";

		internal const string OSServicePack = "OperatingSystemServicePack";

		internal const string OSVersion = "OperatingSystemVersion";

		internal const string ServiceAccount = "ServiceAccount";

		internal const string Forest = "Forest";

		internal const string Site = "Site";

		internal const string Partitions = "Partitions";

		internal const string DefaultPartition = "DefaultPartition";

		internal const string HostName = "HostName";

		internal const string LdapPort = "LdapPort";

		internal const string SslPort = "SslPort";

		internal const string NTDSSettingsObjectDN = "NTDSSettingsObjectDN";

		internal const string ServerObjectDN = "ServerObjectDN";

		internal const string OperationMasterRoles = "OperationMasterRoles";

		internal const string ServerObjectGuid = "ServerObjectGuid";

		internal const string InvocationId = "InvocationId";

		internal const string IsReadOnly = "IsReadOnly";

		internal const string IsGlobalCatalog = "IsGlobalCatalog";

		internal const string ComputerObjectDN = "ComputerObjectDN";

		internal const string Domain = "Domain";

		internal const string SubordinateReferences = "SubordinateReferences";

		internal const string DNSRoot = "DNSRoot";

		internal const string LostAndFoundContainer = "LostAndFoundContainer";

		internal const string DeletedObjectsContainer = "DeletedObjectsContainer";

		internal const string QuotasContainer = "QuotasContainer";

		internal const string ReadOnlyReplicaDirectoryServers = "ReadOnlyReplicaDirectoryServers";

		internal const string ReplicaDirectoryServers = "ReplicaDirectoryServers";

		internal const string AllowedDNSSuffixes = "AllowedDNSSuffixes";

		internal const string LinkedGroupPolicyObjects = "LinkedGroupPolicyObjects";

		internal const string ChildDomains = "ChildDomains";

		internal const string ComputersContainer = "ComputersContainer";

		internal const string DomainControllersContainer = "DomainControllersContainer";

		internal const string DomainMode = "DomainMode";

		internal const string DomainSID = "DomainSID";

		internal const string ForeignSecurityPrincipalsContainer = "ForeignSecurityPrincipalsContainer";

		internal const string InfrastructureMaster = "InfrastructureMaster";

		internal const string LastLogonReplicationInterval = "LastLogonReplicationInterval";

		internal const string NetBIOSName = "NetBIOSName";

		internal const string PDCEmulator = "PDCEmulator";

		internal const string ParentDomain = "ParentDomain";

		internal const string RIDMaster = "RIDMaster";

		internal const string SystemsContainer = "SystemsContainer";

		internal const string UsersContainer = "UsersContainer";

		internal const string SPNSuffixes = "SPNSuffixes";

		internal const string UPNSuffixes = "UPNSuffixes";

		internal const string RootDomain = "RootDomain";

		internal const string ForestMode = "ForestMode";

		internal const string PartitionsContainer = "PartitionsContainer";

		internal const string ApplicationPartitions = "ApplicationPartitions";

		internal const string CrossForestReferences = "CrossForestReferences";

		internal const string Domains = "Domains";

		internal const string GlobalCatalogs = "GlobalCatalogs";

		internal const string Sites = "Sites";

		internal const string DomainNamingMaster = "DomainNamingMaster";

		internal const string SchemaMaster = "SchemaMaster";

		internal const string MinPasswordLength = "MinPasswordLength";

		internal const string DefaultDomainPolicy = "DefaultDomainPolicy";

		internal const string PasswordHistoryCount = "PasswordHistoryCount";

		internal const string Precedence = "Precedence";

		internal const string MaxPasswordAge = "MaxPasswordAge";

		internal const string LockoutDuration = "LockoutDuration";

		internal const string MinPasswordAge = "MinPasswordAge";

		internal const string ComplexityEnabled = "ComplexityEnabled";

		internal const string LockoutObservationWindow = "LockoutObservationWindow";

		internal const string LockoutThreshold = "LockoutThreshold";

		internal const string ReversibleEncryptionEnabled = "ReversibleEncryptionEnabled";

		internal const string AppliesTo = "AppliesTo";

		internal const string EnabledScopes = "EnabledScopes";

		internal const string FeatureScope = "FeatureScope";

		internal const string FeatureGUID = "FeatureGUID";

		internal const string RequiredDomainMode = "RequiredDomainMode";

		internal const string RequiredForestMode = "RequiredForestMode";

		internal const string IsDisableable = "IsDisableable";

		internal const string Subnet = "Subnets";

		internal const string InterSiteTopologyGenerator = "InterSiteTopologyGenerator";

		internal const string ReplicationSchedule = "ReplicationSchedule";

		internal const string UniversalGroupCachingRefreshSite = "UniversalGroupCachingRefreshSite";

		internal const string UniversalGroupCachingEnabled = "UniversalGroupCachingEnabled";

		internal const string AutomaticTopologyGenerationEnabled = "AutomaticTopologyGenerationEnabled";

		internal const string AutomaticInterSiteTopologyGenerationEnabled = "AutomaticInterSiteTopologyGenerationEnabled";

		internal const string TopologyCleanupEnabled = "TopologyCleanupEnabled";

		internal const string TopologyMinimumHopsEnabled = "TopologyMinimumHopsEnabled";

		internal const string TopologyDetectStaleEnabled = "TopologyDetectStaleEnabled";

		internal const string WindowsServer2003KCCBehaviorEnabled = "WindowsServer2003KCCBehaviorEnabled";

		internal const string WindowsServer2000KCCISTGSelectionBehaviorEnabled = "WindowsServer2000KCCISTGSelectionBehaviorEnabled";

		internal const string WindowsServer2000BridgeheadSelectionMethodEnabled = "WindowsServer2000BridgeheadSelectionMethodEnabled";

		internal const string ScheduleHashingEnabled = "ScheduleHashingEnabled";

		internal const string RedundantServerTopologyEnabled = "RedundantServerTopologyEnabled";

		internal const string WindowsServer2003KCCIgnoreScheduleEnabled = "WindowsServer2003KCCIgnoreScheduleEnabled";

		internal const string WindowsServer2003KCCSiteLinkBridgingEnabled = "WindowsServer2003KCCSiteLinkBridgingEnabled";

		internal const string Partner = "Partner";

		internal const string PartnerAddress = "PartnerAddress";

		internal const string PartnerGuid = "PartnerGuid";

		internal const string PartnerInvocationId = "PartnerInvocationId";

		internal const string PartnerType = "PartnerType";

		internal const string PartitionGuid = "PartitionGuid";

		internal const string LastReplicationAttempt = "LastReplicationAttempt";

		internal const string LastReplicationResult = "LastReplicationResult";

		internal const string LastReplicationSuccess = "LastReplicationSuccess";

		internal const string ConsecutiveReplicationFailures = "ConsecutiveReplicationFailures";

		internal const string LastChangeUsn = "LastChangeUsn";

		internal const string UsnFilter = "UsnFilter";

		internal const string IntersiteTransport = "IntersiteTransport";

		internal const string IntersiteTransportGuid = "IntersiteTransportGuid";

		internal const string IntersiteTransportType = "IntersiteTransportType";

		internal const string CompressChanges = "CompressChanges";

		internal const string DisableScheduledSync = "DisableScheduledSync";

		internal const string IgnoreChangeNotifications = "IgnoreChangeNotifications";

		internal const string ScheduledSync = "ScheduledSync";

		internal const string SyncOnStartup = "SyncOnStartup";

		internal const string TwoWaySync = "TwoWaySync";

		internal const string Writable = "Writable";

		internal const string ReplicationFrequencyInMinutes = "ReplicationFrequencyInMinutes";

		internal const string Cost = "Cost";

		internal const string SitesIncluded = "SitesIncluded";

		internal const string InterSiteTransportProtocol = "InterSiteTransportProtocol";

		internal const string SiteLinksIncluded = "SiteLinksIncluded";

		internal const string FailureCount = "FailureCount";

		internal const string FailureType = "FailureType";

		internal const string FirstFailureTime = "FirstFailureTime";

		internal const string LastError = "LastError";

		internal const string EnqueueTime = "EnqueueTime";

		internal const string OperationID = "OperationID";

		internal const string OperationType = "OperationType";

		internal const string Options = "Options";

		internal const string Priority = "Priority";

		internal const string Object = "Object";

		internal const string AttributeName = "AttributeName";

		internal const string AttributeValue = "AttributeValue";

		internal const string Version = "Version";

		internal const string LastOriginatingChangeUsn = "LastOriginatingChangeUsn";

		internal const string LastOriginatingChangeTime = "LastOriginatingChangeTime";

		internal const string LastOriginatingChangeDirectoryServerInvocationId = "LastOriginatingChangeDirectoryServerInvocationId";

		internal const string LastOriginatingChangeDirectoryServerIdentity = "LastOriginatingChangeDirectoryServerIdentity";

		internal const string LocalChangeUsn = "LocalChangeUsn";

		internal const string LastOriginatingDeleteTime = "LastOriginatingDeleteTime";

		internal const string FirstOriginatingCreateTime = "FirstOriginatingCreateTime";

		internal const string IsLinkValue = "IsLinkValue";

		internal const string ReplicatedNamingContexts = "ReplicatedNamingContexts";

		internal const string PartiallyReplicatedNamingContexts = "PartiallyReplicatedNamingContexts";

		internal const string ReplicateFromDirectoryServer = "ReplicateFromDirectoryServer";

		internal const string ReplicateToDirectoryServer = "ReplicateToDirectoryServer";

		internal const string AutoGenerated = "AutoGenerated";

		internal const string SharesValuesWith = "SharesValuesWith";

		internal const string RestrictValues = "RestrictValues";

		internal const string ValueType = "ValueType";

		internal const string SuggestedValues = "SuggestedValues";

		internal const string IsSingleValued = "IsSingleValued";

		internal const string ID = "ID";

		internal const string IsSecured = "IsSecured";

		internal const string ResourcePropertyValueType = "ResourcePropertyValueType";

		internal const string AppliesToResourceTypes = "AppliesToResourceTypes";

		internal const string SourceAttribute = "SourceAttribute";

		internal const string SourceOID = "SourceOID";

		internal const string AppliesToClasses = "AppliesToClasses";

		internal const string CompatibleResourceTypes = "CompatibleResourceTypes";

		internal const string SourceTransformPolicy = "SourceTransformPolicy";

		internal const string ClaimSourceType = "ClaimSourceType";

		internal const string ResourceCondition = "ResourceCondition";

		internal const string CurrentAcl = "CurrentAcl";

		internal const string ProposedAcl = "ProposedAcl";

		internal const string PreviousAcl = "PreviousAcl";

		internal const string PolicyID = "PolicyID";

		internal const string IsSuggestedValuesPresent = "IsSuggestedValuesPresent";

		internal const string ResourceProperties = "ResourceProperties";

		internal const string Source = "Source";

		internal const string Target = "Target";

		internal const string Direction = "Direction";

		internal const string Policy = "Policy";

		internal const string TrustType = "TrustType";

		internal const string TrustAttributes = "TrustAttributes";

		internal const string TrustingPolicy = "TrustingPolicy";

		internal const string TrustedPolicy = "TrustedPolicy";

		internal const string DisallowTransivity = "DisallowTransivity";

		internal const string UplevelOnly = "UplevelOnly";

		internal const string SIDFilteringQuarantined = "SIDFilteringQuarantined";

		internal const string ForestTransitive = "ForestTransitive";

		internal const string SelectiveAuthentication = "SelectiveAuthentication";

		internal const string IntraForest = "IntraForest";

		internal const string SIDFilteringForestAware = "SIDFilteringForestAware";

		internal const string UsesRC4Encryption = "UsesRC4Encryption";

		internal const string UsesAESKeys = "UsesAESKeys";

		internal const string IsTreeParent = "IsTreeParent";

		internal const string IsTreeRoot = "IsTreeRoot";

		internal const string TGTDelegation = "TGTDelegation";

		internal const string InputObject = "InputObject";

		internal const string TrustRole = "TrustRole";

		internal const string Rule = "Rule";

		internal const string DenyAll = "DenyAll";

		internal const string DenyAllExcept = "DenyAllExcept";

		internal const string AllowAll = "AllowAll";

		internal const string AllowAllExcept = "AllowAllExcept";

		internal const string IncomingTrust = "IncomingTrust";

		internal const string OutgoingTrust = "OutgoingTrust";

		internal const string GenerateXml = "GenerateXml";

		internal const string CloneComputerName = "CloneComputerName";

		internal const string PreferredWINSServer = "PreferredWINSServer";

		internal const string IPv4SubnetMask = "IPv4SubnetMask";

		internal const string AlternateWINSServer = "AlternateWINSServer";

		internal const string Static = "Static";

		internal const string Offline = "Offline";

		internal const string IPv4DefaultGateway = "IPv4DefaultGateway";

		internal const string IPv4DNSResolver = "IPv4DNSResolver";

		internal const string IPv6DNSResolver = "IPv6DNSResolver";

	}
}