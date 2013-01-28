using System;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Microsoft.ActiveDirectory
{
	internal class StringResources
	{
		public static string NoNegativeTime;

		public static string TimeoutError;

		public static string NoNegativeSizeLimit;

		public static string NoNegativePageSize;

		public static string SearchSizeLimitExceeded;

		public static string ExceedMax;

		public static string NotSupportedSessionOption;

		public static string InvalidType;

		public static string InvalidYear;

		public static string EmptyStringParameter;

		public static string ServerDown;

		public static string DefaultServerNotFound;

		public static string DefaultADWSServerNotFound;

		public static string ServerOutOfMemory;

		public static string NoMixedType;

		public static string ForestIdDoesNotMatch;

		public static string NotSupportedGCPort;

		public static string InvalidUriFormat;

		public static string InvalidPartitionMustBelongToValidSet;

		public static string InvalidDNMustBelongToValidPartitionSet;

		public static string ADFilterOperatorNotSupported;

		public static string ADFilterParsingErrorMessage;

		public static string ADFilterVariableNotDefinedMessage;

		public static string ADFilterExprListLessThanTwo;

		public static string ADFilterPropertyNotFoundInObject;

		public static string ADWSXmlParserInvalidelement;

		public static string ADWSXmlParserUnexpectedElement;

		public static string ADWSXmlParserInvalidAttribute;

		public static string ADWSXmlParserMandatoryHeaderNotUnderstood;

		public static string ADWSXmlParserEmptyMessageReceived;

		public static string ADWSXmlParserInvalidActionForMessage;

		public static string ADProviderSetItemNotSupported;

		public static string ADProviderClearItemNotSupported;

		public static string ADProviderExecuteItemNotSupported;

		public static string ADProviderCopyItemNotSupported;

		public static string ADProviderPathNotFound;

		public static string ADProviderPropertiesNotSpecified;

		public static string ADProviderPropertyValueCannotBeNull;

		public static string ADProviderPropertiesToClearNotSpecified;

		public static string ADProviderSDNotSet;

		public static string ADProviderGCInvalidForADLDS;

		public static string ADProviderGCInvalidWithAppendedPort;

		public static string ADProviderUnableToGetPartition;

		public static string ADProviderOperationNotSupportedForRootDSE;

		public static string ADProviderOperationNotSupportedForRootDSEUnlessGC;

		public static string ADProviderInvalidPropertyName;

		public static string ADProviderUnableToReadProperty;

		public static string ADProviderErrorInitializingDefaultDrive;

		public static string ProviderUtilInvalidDrivePath;

		public static string TranslateNameError;

		public static string LoadingDriveProgressMessage;

		public static string UnspecifiedError;

		public static string OperationSuccessful;

		public static string NoSchemaClassInSchemaCache;

		public static string SessionRequired;

		public static string CustomAttributeCollision;

		public static string MultipleKeywords;

		public static string InvalidParameterValue;

		public static string InvalidHashtableKey;

		public static string InvalidNullValue;

		public static string InvalidEmptyCollection;

		public static string InvalidNullInCollection;

		public static string InvalidParameterType;

		public static string ParameterRequired;

		public static string ParameterRequiredMultiple;

		public static string ParameterRequiredOnlyOne;

		public static string ObjectNotFound;

		public static string InstanceMustBeOfType;

		public static string InvalidObjectClass;

		public static string InvalidObjectClasses;

		public static string MultipleMatches;

		public static string InvalidHashtableKeyType;

		public static string InvalidTypeInCollection;

		public static string ObjectTypeNotEqualToExpectedType;

		public static string DistinguishedNameCannotBeNull;

		public static string FSMORoleNotFoundInDomain;

		public static string FSMORoleNotFoundInForest;

		public static string SearchConverterRHSNotDataNode;

		public static string SearchConverterNotBinaryNode;

		public static string SearchConverterSupportedOperatorListErrorMessage;

		public static string SearchConverterInvalidValue;

		public static string SearchConverterUnrecognizedObjectType;

		public static string SearchConverterIdentityAttributeNotSet;

		public static string SearchConverterRHSInvalidType;

		public static string SearchConverterAttributeNotSupported;

		public static string SearchConverterUseSearchADAccount;

		public static string SearchConverterRHSNotMatchEnumValue;

		public static string AttributeConverterUnrecognizedObjectType;

		public static string InsufficientPermissionsToProtectObject;

		public static string InsufficientPermissionsToProtectObjectParent;

		public static string CannotResolveIPAddressToHostName;

		public static string WarningSamAccountNameClauseLacksDollarSign;

		public static string FailedAddingMembersToGroup;

		public static string FailedAddingMembersToOneOrMoreGroup;

		public static string FailedRemovingMembersFromGroup;

		public static string FailedRemovingMembersFromOneOrMoreGroup;

		public static string PasswordRestrictionErrorMessage;

		public static string ChangePasswordErrorMessage;

		public static string UserPressedBreakDuringPasswordEntry;

		public static string PromptForCurrentPassword;

		public static string PromptForNewPassword;

		public static string PromptForRepeatPassword;

		public static string PasswordsDidNotMatch;

		public static string PasswordChangeSuccessful;

		public static string MethodNotSupportedForObjectType;

		public static string UnsupportedObjectClass;

		public static string AttributeNotFoundOnObject;

		public static string WarningPolicyUsageNotAccurateOnRODC;

		public static string WarningResultantPRPNotAccurateOnRODC;

		public static string ErrorResultantPRPSpecifyWindows2008OrAbove;

		public static string UnsupportedOptionSpecified;

		public static string MoveOperationMasterRoleCaption;

		public static string MoveOperationMasterRoleWarning;

		public static string MoveOperationMasterRoleDescription;

		public static string MoveOperationMasterRoleNotApplicableForADLDS;

		public static string DuplicateValuesSpecified;

		public static string CouldNotDetermineLoggedOnUserDomain;

		public static string CouldNotDetermineLocalComputerDomain;

		public static string RequiresDomainCredentials;

		public static string UseSetADDomainMode;

		public static string UseSetADForestMode;

		public static string ADInvalidQuantizationDays;

		public static string ADInvalidQuantizationHours;

		public static string ADInvalidQuantizationMinutes;

		public static string ADInvalidQuantizationFifteenMinuteIntervals;

		public static string CannotInstallServiceAccount;

		public static string ServiceAccountIsNotInstalled;

		public static string CannotResetPasswordOfServiceAccount;

		public static string CannotUninstallServiceAccount;

		public static string CannotTestServiceAccount;

		public static string NetAddServiceAccountFailed;

		public static string CannotReachHostingDC;

		public static string OtherBackLinkDescription;

		public static string OtherBackLinkCaption;

		public static string ServiceAccountNameLengthInvalid;

		public static string AcctChangePwdNotWorksWhenPwdNotExpires;

		public static string AddADPrincipalGroupMembershipShouldProcessCaption;

		public static string AddADPrincipalGroupMembershipShouldProcessWarning;

		public static string AddADPrincipalGroupMembershipShouldProcessDescription;

		public static string RemoveADPrincipalGroupMembershipShouldProcessCaption;

		public static string RemoveADPrincipalGroupMembershipShouldProcessWarning;

		public static string RemoveADPrincipalGroupMembershipShouldProcessDescription;

		public static string ParameterValueNotSearchResult;

		public static string GetGroupMembershipResourceContextParameterCheck;

		public static string IdentityResolutionPartitionRequired;

		public static string IdentityInExtendedAttributeCannotBeResolved;

		public static string IdentityNotFound;

		public static string IdentityInWrongPartition;

		public static string DirectoryServerNotFound;

		public static string EnablingIsIrreversible;

		public static string CouldNotSetForestMode;

		public static string CouldNotFindForestIdentity;

		public static string ConnectedToWrongForest;

		public static string EmptySearchBaseNotSupported;

		public static string PromptForRemove;

		public static string PromptForRecursiveRemove;

		public static string PerformingRecursiveRemove;

		public static string ConfigSetNotFound;

		public static string ADInvalidAttributeValueCount;

		public static string ServerContainerNotEmpty;

		public static string InvalidClaimValueType;

		public static string InvalidPossibleValuesXml;

		public static string NextVersionPossibleValuesXml;

		public static string CannotOverwriteNextVersionXml;

		public static string SPCTNoSourceWarning;

		public static string SPCTBothSourceWarning;

		public static string SPCTBothSourceOIDPossibleValuesWarning;

		public static string CTBothPossibleValuesShareValueWarning;

		public static string SPCTInvalidAppliesToClassWarning;

		public static string CTParameterValidationFailure;

		public static string SPCTInvalidSourceAttributeName;

		public static string SPCTBlockedSourceAttribute;

		public static string SPCTNonREPLSourceAttrError;

		public static string SPCTRODCFilteredSourceAttr;

		public static string SPCTDefuctSourceAttr;

		public static string SPCTInvalidAttributeSyntax;

		public static string CTSourceOIDValueTypeError;

		public static string CTSourceAttributeValueTypeError;

		public static string RCTNoResourcePropertyValueTypeError;

		public static string InvalidValueTypeForPossibleValueXml;

		public static string SPCTSourceAttributeLdapDisplayNameError;

		public static string SPCTAttributeNotFoundInSchemaClass;

		public static string CAPIDCreationFailure;

		public static string CAPMemberMaximumExceeded;

		public static string SPCTInvalidSourceAttribute;

		public static string RCTSuggestedValueNotPresentError;

		public static string RCTSuggestedValuePresentError;

		public static string ResourcePropertySharesValueWithValueTypeError;

		public static string SuggestedValueNotUniqueError;

		public static string ADTrustNoDirectionAndPolicyError;

		public static string ClaimPolicyXmlWarning;

		public static string ClaimPolicyXmlNodeError;

		public static string ServerParameterNotSupported;

		public static string XmlFormattingError;

		public static string RuleValidationFailure;

		public static string ResouceConditionValidationFailed;

		public static string SDDLValidationFailed;

		public static string DisplayNameNotUniqueError;

		public static string RemoveClaimTypeSharesValueWithError;

		public static string SharesValueWithIdentityError;

		public static string ServerTargetParameterNotSpecified;

		public static string TargetParameterHM;

		public static string ClaimIDValidationError;

		public static string ResourceIDValidationError;

		public static string ClaimTypeRestrictValueError;

		public static string PropertyIsReadonly;

		public static string NoConversionExists;

		public static string TypeConversionError;

		public static string TypeAdapterForADEntityOnly;

		public static string EnumConversionError;

		public static string ServerActionNotSupportedFault;

		public static string ServerCannotProcessFilter;

		public static string ServerEncodingLimit;

		public static string ServerEnumerationContextLimitExceeded;

		public static string ServerFilterDialectRequestedUnavailable;

		public static string ServerFragmentDialectNotSupported;

		public static string ServerInvalidEnumerationContext;

		public static string ServerInvalidExpirationTime;

		public static string ServerSchemaValidationError;

		public static string ServerUnsupportedSelectOrSortDialectFault;

		public static string ServerAnonymousNotAllowed;

		public static string ServerInvalidInstance;

		public static string ServerMultipleMatchingSecurityPrincipals;

		public static string ServerNoMatchingSecurityPrincipal;

		public static string InvalidProperty;

		public static string InvalidFilter;

		public static string AsqResponseError;

		public static string ADAccountRPRPIdentityHM;

		public static string ADAccountRPRPDomainControllerHM;

		public static string ADDCPRPUIdentityHM;

		public static string ADObjectFilterHM;

		public static string ADOUFilterHM;

		public static string ADComputerServiceAccountIdentityHM;

		public static string ADFineGrainedPPFilterHM;

		public static string ADFGPPSubjectIdentityHM;

		public static string ADGroupFilterHM;

		public static string ADGroupMemberIdentityHM;

		public static string ADDCPRPIdentityHM;

		public static string ADPrincipalGMIdentityHM;

		public static string ADOFFilterHM;

		public static string ADUserFilterHM;

		public static string ADAccountAuthGroupIdentityHM;

		public static string ADUserResultantPPIdentityHM;

		public static string ADServiceAccountFilterHM;

		public static string ADComputerFilterHM;

		public static string NullOrEmptyIdentityPropertyArgument;

		public static string DelegatePipelineEmptyError;

		public static string DelegatePipelineUnsupportedTypeError;

		public static string DelegatePipelineMulticastDelegatesNotAllowedError;

		public static string DelegatePipelineReferenceDelegateNotFoundError;

		public static string ValidateRangeLessThanMinValue;

		public static string ValidateRangeGreaterThanMaxValue;

		public static string ObjectToReplicateNotFoundOnSource;

		public static string SourceServerDown;

		public static string PasswordOnlySwitchAllowedOnlyOnRODC;

		public static string DestinationDoesNotTargetDirectoryServer;

		public static string SourceDoesNotTargetDirectoryServer;

		public static string DestinationServerDown;

		public static string SourceServerObjNotFoundOrObjToReplicateNotFound;

		public static string DestinationServerDoesNotSupportSynchronizingObject;

		public static string SiteLinkAndSiteLinkBridgeDoNotShareSameTransportType;

		public static string NoMatchingResultsForTarget;

		public static string OnlySearchResultsSupported;

		public static string UnsupportedParameterType;

		public static string ServerDoesNotHaveFriendlyPartition;

		public static string ServerIsNotDirectoryServer;

		public static string UnableToFindSiteForLocalMachine;

		public static string MsaStandloneNotLinked;

		public static string MsaStandaloneLinkedToAlternateComputer;

		public static string MsaDoesNotExist;

		public static string MsaNotServiceAccount;

		public static string InvalidACEInSecDesc;

		public static string ADDCCloningExcludedApplicationListErrorMessage;

		public static string ADDCCloningExcludedApplicationListCustomerAllowListFileNameMessage;

		public static string ADDCCloningExcludedApplicationListNoCustomerAllowListFileMessage;

		public static string ADDCCloningExcludedApplicationListInvalidPath;

		public static string ADDCCloningExcludedApplicationListNoAppsFound;

		public static string ADDCCloningExcludedApplicationListFilePriority;

		public static string ADDCCloningExcludedApplicationListPathPriority;

		public static string ADDCCloningExcludedApplicationListFileExists;

		public static string ADDCCloningExcludedApplicationListNewAllowList;

		public static string ADDCCloningExcludedApplicationListLocalMachineNotADCMessage;

		public static string NewADDCCloneConfigFileLocatingWin8PDCMessage;

		public static string NewADDCCloneConfigFileNoWin8PDCMessage;

		public static string NewADDCCloneConfigFileFoundWin8PDCMessage;

		public static string NewADDCCloneConfigFileCheckCloningPrivilegeMessage;

		public static string NewADDCCloneConfigFileNoLocalDCMessage;

		public static string NewADDCCloneConfigFileFoundLocalDCMessage;

		public static string NewADDCCloneConfigFileNoLocalDCMembershipMessage;

		public static string NewADDCCloneConfigFileNoCloningPrivilegeMessage;

		public static string NewADDCCloneConfigFileGetCloneableGroupMessage;

		public static string NewADDCCloneConfigFileNoCloneableGroupMessage;

		public static string NewADDCCloneConfigFileHasCloningPrivilegeMessage;

		public static string NewADDCCloneConfigFileTestWhiteListMessage;

		public static string NewADDCCloneConfigFileWhiteListCompleteMessage;

		public static string NewADDCCloneConfigFileWhiteListNotCompleteMessage;

		public static string NewADDCCloneConfigFileOfflineModeMessage;

		public static string NewADDCCloneConfigFileLocalModeMessage;

		public static string NewADDCCloneConfigFileGenerateFileMessage;

		public static string NewADDCCloneConfigFileNoGenerateFileMessage;

		public static string NewADDCCloneConfigFileGetDitLocationMessage;

		public static string NewADDCCloneConfigFileNoDitLocationMessage;

		public static string NewADDCCloneConfigFileGenerationMessage;

		public static string NewADDCCloneConfigFileFullNameMessage;

		public static string NewADDCCloneConfigFileGeneratingContentMessage;

		public static string NewADDCCloneConfigFileGeneratedMessage;

		public static string NewADDCCloneConfigFileExistingMessage;

		public static string NewADDCCloneConfigFileNotExistingMessage;

		public static string NewADDCCloneConfigFileFoundMessage;

		public static string NewADDCCloneConfigFileAtWrongLocationMessage;

		public static string NewADDCCloneConfigFileInvalidIpv4StaticMessage;

		public static string NewADDCCloneConfigFileMoreDnsMessage;

		public static string NewADDCCloneConfigFileLocalModeNoLocalDCMessage;

		static StringResources()
		{
			ResourceManager resourceManager = new ResourceManager("Microsoft.ActiveDirectory.Management", Assembly.GetExecutingAssembly());
			Type type = typeof(StringResources);
			MemberInfo[] members = type.GetMembers(BindingFlags.Static | BindingFlags.Public);
			MemberInfo[] memberInfoArray = members;
			for (int i = 0; i < (int)memberInfoArray.Length; i++)
			{
				MemberInfo memberInfo = memberInfoArray[i];
				object[] str = new object[1];
				str[0] = resourceManager.GetString(memberInfo.Name, CultureInfo.CurrentUICulture);
				type.InvokeMember(memberInfo.Name, BindingFlags.SetField, null, null, str, CultureInfo.CurrentCulture);
			}
		}

		public StringResources()
		{
		}
	}
}