using System.Globalization;
using System.Resources;
using System.Threading;

namespace System
{
	internal sealed class SR
	{
		internal const string ContextNoWellKnownObjects = "ContextNoWellKnownObjects";

		internal const string ContextNoContainerForMachineCtx = "ContextNoContainerForMachineCtx";

		internal const string ContextNoContainerForApplicationDirectoryCtx = "ContextNoContainerForApplicationDirectoryCtx";

		internal const string ContextBadUserPwdCombo = "ContextBadUserPwdCombo";

		internal const string StoreNotSupportMethod = "StoreNotSupportMethod";

		internal const string PrincipalCantSetContext = "PrincipalCantSetContext";

		internal const string PrincipalUnsupportedProperty = "PrincipalUnsupportedProperty";

		internal const string PrincipalUnsupportPropertyForPlatform = "PrincipalUnsupportPropertyForPlatform";

		internal const string PrincipalUnsupportPropertyForType = "PrincipalUnsupportPropertyForType";

		internal const string PrincipalMustSetContextForSave = "PrincipalMustSetContextForSave";

		internal const string PrincipalMustSetContextForNative = "PrincipalMustSetContextForNative";

		internal const string PrincipalMustSetContextForProperty = "PrincipalMustSetContextForProperty";

		internal const string PrincipalCantDeleteUnpersisted = "PrincipalCantDeleteUnpersisted";

		internal const string PrincipalDeleted = "PrincipalDeleted";

		internal const string PrincipalNotSupportedOnFakePrincipal = "PrincipalNotSupportedOnFakePrincipal";

		internal const string PrincipalMustPersistFirst = "PrincipalMustPersistFirst";

		internal const string PrincipalIdentityTypeNotAllowed = "PrincipalIdentityTypeNotAllowed";

		internal const string PrincipalIdentityTypeNotRemovable = "PrincipalIdentityTypeNotRemovable";

		internal const string PrincipalCantChangeSamNameOnPersistedSAM = "PrincipalCantChangeSamNameOnPersistedSAM";

		internal const string EmptyIdentityType = "EmptyIdentityType";

		internal const string PrincipalSearcherPersistedPrincipal = "PrincipalSearcherPersistedPrincipal";

		internal const string PrincipalSearcherMustSetContext = "PrincipalSearcherMustSetContext";

		internal const string PrincipalSearcherMustSetFilter = "PrincipalSearcherMustSetFilter";

		internal const string PrincipalSearcherMustSetContextForUnderlying = "PrincipalSearcherMustSetContextForUnderlying";

		internal const string PrincipalSearcherNoUnderlying = "PrincipalSearcherNoUnderlying";

		internal const string PrincipalSearcherNonReferentialProps = "PrincipalSearcherNonReferentialProps";

		internal const string FindResultEnumInvalidPos = "FindResultEnumInvalidPos";

		internal const string TrackedCollectionNotOneDimensional = "TrackedCollectionNotOneDimensional";

		internal const string TrackedCollectionIndexNotInArray = "TrackedCollectionIndexNotInArray";

		internal const string TrackedCollectionArrayTooSmall = "TrackedCollectionArrayTooSmall";

		internal const string TrackedCollectionEnumHasChanged = "TrackedCollectionEnumHasChanged";

		internal const string TrackedCollectionEnumInvalidPos = "TrackedCollectionEnumInvalidPos";

		internal const string MultipleMatchesExceptionText = "MultipleMatchesExceptionText";

		internal const string MultipleMatchingPrincipals = "MultipleMatchingPrincipals";

		internal const string NoMatchingPrincipalExceptionText = "NoMatchingPrincipalExceptionText";

		internal const string NoMatchingGroupExceptionText = "NoMatchingGroupExceptionText";

		internal const string PrincipalExistsExceptionText = "PrincipalExistsExceptionText";

		internal const string PrincipalCollectionNotOneDimensional = "PrincipalCollectionNotOneDimensional";

		internal const string PrincipalCollectionIndexNotInArray = "PrincipalCollectionIndexNotInArray";

		internal const string PrincipalCollectionArrayTooSmall = "PrincipalCollectionArrayTooSmall";

		internal const string PrincipalCollectionEnumHasChanged = "PrincipalCollectionEnumHasChanged";

		internal const string PrincipalCollectionEnumInvalidPos = "PrincipalCollectionEnumInvalidPos";

		internal const string PrincipalCollectionAlreadyMember = "PrincipalCollectionAlreadyMember";

		internal const string AuthenticablePrincipalMustBeSubtypeOfAuthPrinc = "AuthenticablePrincipalMustBeSubtypeOfAuthPrinc";

		internal const string PasswordInfoChangePwdOnUnpersistedPrinc = "PasswordInfoChangePwdOnUnpersistedPrinc";

		internal const string UserMustSetContextForMethod = "UserMustSetContextForMethod";

		internal const string UserDomainNotSupportedOnPlatform = "UserDomainNotSupportedOnPlatform";

		internal const string UserLocalNotSupportedOnPlatform = "UserLocalNotSupportedOnPlatform";

		internal const string UserCouldNotFindCurrent = "UserCouldNotFindCurrent";

		internal const string UnableToRetrieveDomainInfo = "UnableToRetrieveDomainInfo";

		internal const string UnableToOpenToken = "UnableToOpenToken";

		internal const string UnableToRetrieveTokenInfo = "UnableToRetrieveTokenInfo";

		internal const string UnableToRetrievePolicy = "UnableToRetrievePolicy";

		internal const string UnableToImpersonateCredentials = "UnableToImpersonateCredentials";

		internal const string StoreCtxUnsupportedPrincipalTypeForSave = "StoreCtxUnsupportedPrincipalTypeForSave";

		internal const string StoreCtxUnsupportedPrincipalTypeForGroupInsert = "StoreCtxUnsupportedPrincipalTypeForGroupInsert";

		internal const string StoreCtxUnsupportedPrincipalTypeForQuery = "StoreCtxUnsupportedPrincipalTypeForQuery";

		internal const string StoreCtxUnsupportedPropertyForQuery = "StoreCtxUnsupportedPropertyForQuery";

		internal const string StoreCtxUnsupportedIdentityClaimForQuery = "StoreCtxUnsupportedIdentityClaimForQuery";

		internal const string StoreCtxIdentityClaimMustHaveScheme = "StoreCtxIdentityClaimMustHaveScheme";

		internal const string StoreCtxSecurityIdentityClaimBadFormat = "StoreCtxSecurityIdentityClaimBadFormat";

		internal const string StoreCtxGuidIdentityClaimBadFormat = "StoreCtxGuidIdentityClaimBadFormat";

		internal const string StoreCtxNT4IdentityClaimWrongForm = "StoreCtxNT4IdentityClaimWrongForm";

		internal const string StoreCtxCantSetTimeLimitOnIdentityClaim = "StoreCtxCantSetTimeLimitOnIdentityClaim";

		internal const string StoreCtxGroupHasUnpersistedInsertedPrincipal = "StoreCtxGroupHasUnpersistedInsertedPrincipal";

		internal const string StoreCtxNeedValueSecurityIdentityClaimToQuery = "StoreCtxNeedValueSecurityIdentityClaimToQuery";

		internal const string StoreCtxExceptionUpdatingGroup = "StoreCtxExceptionUpdatingGroup";

		internal const string StoreCtxExceptionCommittingChanges = "StoreCtxExceptionCommittingChanges";

		internal const string ADStoreCtxUnsupportedPrincipalContextForGroupInsert = "ADStoreCtxUnsupportedPrincipalContextForGroupInsert";

		internal const string ADStoreCtxCouldntGetSIDForGroupMember = "ADStoreCtxCouldntGetSIDForGroupMember";

		internal const string ADStoreCtxMustBeContainer = "ADStoreCtxMustBeContainer";

		internal const string ADStoreCtxCantRetrieveObjectSidForCrossStore = "ADStoreCtxCantRetrieveObjectSidForCrossStore";

		internal const string ADStoreCtxCantResolveSidForCrossStore = "ADStoreCtxCantResolveSidForCrossStore";

		internal const string ADStoreCtxFailedFindCrossStoreTarget = "ADStoreCtxFailedFindCrossStoreTarget";

		internal const string ADStoreCtxUnableToReadExistingAccountControlFlagsToEnable = "ADStoreCtxUnableToReadExistingAccountControlFlagsToEnable";

		internal const string ADStoreCtxUnableToReadExistingAccountControlFlagsForUpdate = "ADStoreCtxUnableToReadExistingAccountControlFlagsForUpdate";

		internal const string ADStoreCtxUnableToReadExistingGroupTypeFlagsForUpdate = "ADStoreCtxUnableToReadExistingGroupTypeFlagsForUpdate";

		internal const string ADStoreCtxCantClearGroup = "ADStoreCtxCantClearGroup";

		internal const string ADStoreCtxCantRemoveMemberFromGroup = "ADStoreCtxCantRemoveMemberFromGroup";

		internal const string ADStoreCtxNoComputerPasswordChange = "ADStoreCtxNoComputerPasswordChange";

		internal const string SAMStoreCtxUnableToRetrieveVersion = "SAMStoreCtxUnableToRetrieveVersion";

		internal const string SAMStoreCtxUnableToRetrieveMachineName = "SAMStoreCtxUnableToRetrieveMachineName";

		internal const string SAMStoreCtxUnableToRetrieveFlatMachineName = "SAMStoreCtxUnableToRetrieveFlatMachineName";

		internal const string SAMStoreCtxNoComputerPasswordSet = "SAMStoreCtxNoComputerPasswordSet";

		internal const string SAMStoreCtxNoComputerPasswordExpire = "SAMStoreCtxNoComputerPasswordExpire";

		internal const string SAMStoreCtxIdentityClaimsImmutable = "SAMStoreCtxIdentityClaimsImmutable";

		internal const string SAMStoreCtxCouldntGetSIDForGroupMember = "SAMStoreCtxCouldntGetSIDForGroupMember";

		internal const string SAMStoreCtxFailedToClearGroup = "SAMStoreCtxFailedToClearGroup";

		internal const string SAMStoreCtxCantRetrieveObjectSidForCrossStore = "SAMStoreCtxCantRetrieveObjectSidForCrossStore";

		internal const string SAMStoreCtxCantResolveSidForCrossStore = "SAMStoreCtxCantResolveSidForCrossStore";

		internal const string SAMStoreCtxFailedFindCrossStoreTarget = "SAMStoreCtxFailedFindCrossStoreTarget";

		internal const string SAMStoreCtxErrorEnumeratingGroup = "SAMStoreCtxErrorEnumeratingGroup";

		internal const string SAMStoreCtxLocalGroupsOnly = "SAMStoreCtxLocalGroupsOnly";

		internal const string AuthZFailedToRetrieveGroupList = "AuthZFailedToRetrieveGroupList";

		internal const string AuthZNotSupported = "AuthZNotSupported";

		internal const string AuthZErrorEnumeratingGroups = "AuthZErrorEnumeratingGroups";

		internal const string AuthZCantFindGroup = "AuthZCantFindGroup";

		internal const string ConfigHandlerConfigSectionsUnique = "ConfigHandlerConfigSectionsUnique";

		internal const string ConfigHandlerInvalidBoolAttribute = "ConfigHandlerInvalidBoolAttribute";

		internal const string ConfigHandlerInvalidEnumAttribute = "ConfigHandlerInvalidEnumAttribute";

		internal const string ConfigHandlerInvalidStringAttribute = "ConfigHandlerInvalidStringAttribute";

		internal const string ConfigHandlerUnknownConfigSection = "ConfigHandlerUnknownConfigSection";

		internal const string PrincipalPermWrongType = "PrincipalPermWrongType";

		internal const string PrincipalPermXmlNotPermission = "PrincipalPermXmlNotPermission";

		internal const string PrincipalPermXmlBadVersion = "PrincipalPermXmlBadVersion";

		internal const string PrincipalPermXmlBadContents = "PrincipalPermXmlBadContents";

		internal const string ContextOptionsNotValidForMachineStore = "ContextOptionsNotValidForMachineStore";

		internal const string PassedContextTypeDoesNotMatchDetectedType = "PassedContextTypeDoesNotMatchDetectedType";

		internal const string NullArguments = "NullArguments";

		internal const string InvalidStringValueForStore = "InvalidStringValueForStore";

		internal const string InvalidNullArgument = "InvalidNullArgument";

		internal const string ServerDown = "ServerDown";

		internal const string InvalidPropertyForStore = "InvalidPropertyForStore";

		internal const string InvalidOperationForStore = "InvalidOperationForStore";

		internal const string NameMustBeSetToPersistPrincipal = "NameMustBeSetToPersistPrincipal";

		internal const string ExtensionInvalidClassDefinitionConstructor = "ExtensionInvalidClassDefinitionConstructor";

		internal const string ExtensionInvalidClassAttributes = "ExtensionInvalidClassAttributes";

		internal const string SaveToMustHaveSamecontextType = "SaveToMustHaveSamecontextType";

		internal const string ComputerInvalidForAppDirectoryStore = "ComputerInvalidForAppDirectoryStore";

		internal const string SaveToNotSupportedAgainstMachineStore = "SaveToNotSupportedAgainstMachineStore";

		internal const string InvalidContextOptionsForMachine = "InvalidContextOptionsForMachine";

		internal const string InvalidContextOptionsForAD = "InvalidContextOptionsForAD";

		internal const string InvalidExtensionCollectionType = "InvalidExtensionCollectionType";

		internal const string ADAMStoreUnableToPopulateSchemaList = "ADAMStoreUnableToPopulateSchemaList";

		internal const string StoreCtxMultipleFiltersForPropertyUnsupported = "StoreCtxMultipleFiltersForPropertyUnsupported";

		internal const string AdsiNotInstalled = "AdsiNotInstalled";

		internal const string DSUnknown = "DSUnknown";

		private static SR loader;

		private ResourceManager resources;

		private static CultureInfo Culture
		{
			get
			{
				return null;
			}
		}

		public static ResourceManager Resources
		{
			get
			{
				return SR.GetLoader().resources;
			}
		}

		static SR()
		{
		}

		internal SR()
		{
			this.resources = new ResourceManager("System.DirectoryServices.AccountManagement", this.GetType().Assembly);
		}

		private static SR GetLoader()
		{
			if (SR.loader == null)
			{
				SR sR = new SR();
				Interlocked.CompareExchange<SR>(ref SR.loader, sR, null);
			}
			return SR.loader;
		}

		public static object GetObject(string name)
		{
			SR loader = SR.GetLoader();
			if (loader != null)
			{
				return loader.resources.GetObject(name, SR.Culture);
			}
			else
			{
				return null;
			}
		}

		public static string GetString(string name, object[] args)
		{
			SR loader = SR.GetLoader();
			if (loader != null)
			{
				string str = loader.resources.GetString(name, SR.Culture);
				if (args == null || (int)args.Length <= 0)
				{
					return str;
				}
				else
				{
					for (int i = 0; i < (int)args.Length; i++)
					{
						string str1 = args[i] as string;
						if (str1 != null && str1.Length > 0x400)
						{
							args[i] = string.Concat(str1.Substring(0, 0x3fd), "...");
						}
					}
					return string.Format(CultureInfo.CurrentCulture, str, args);
				}
			}
			else
			{
				return null;
			}
		}

		public static string GetString(string name)
		{
			SR loader = SR.GetLoader();
			if (loader != null)
			{
				return loader.resources.GetString(name, SR.Culture);
			}
			else
			{
				return null;
			}
		}

		public static string GetString(string name, out bool usedFallback)
		{
			usedFallback = false;
			return SR.GetString(name);
		}
	}
}