using System;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace System.DirectoryServices.AccountManagement
{
	internal class StringResources
	{
		public static string ContextNoWellKnownObjects;

		public static string ContextNoContainerForMachineCtx;

		public static string ContextBadUserPwdCombo;

		public static string StoreNotSupportMethod;

		public static string PrincipalCantSetContext;

		public static string PrincipalUnsupportedProperty;

		public static string PrincipalUnsupportPropertyForPlatform;

		public static string PrincipalUnsupportPropertyForType;

		public static string PrincipalMustSetContextForSave;

		public static string PrincipalMustSetContextForNative;

		public static string PrincipalMustSetContextForProperty;

		public static string PrincipalCantDeleteUnpersisted;

		public static string PrincipalDeleted;

		public static string PrincipalNotSupportedOnFakePrincipal;

		public static string PrincipalMustPersistFirst;

		public static string PrincipalIdentityTypeNotAllowed;

		public static string PrincipalIdentityTypeNotRemovable;

		public static string PrincipalCantChangeSamNameOnPersistedSAM;

		public static string EmptyIdentityType;

		public static string PrincipalSearcherPersistedPrincipal;

		public static string PrincipalSearcherMustSetContext;

		public static string PrincipalSearcherMustSetContextForUnderlying;

		public static string PrincipalSearcherNoUnderlying;

		public static string PrincipalSearcherNonReferentialProps;

		public static string FindResultEnumInvalidPos;

		public static string TrackedCollectionNotOneDimensional;

		public static string TrackedCollectionIndexNotInArray;

		public static string TrackedCollectionArrayTooSmall;

		public static string TrackedCollectionEnumHasChanged;

		public static string TrackedCollectionEnumInvalidPos;

		public static string MultipleMatchesExceptionText;

		public static string MultipleMatchingPrincipals;

		public static string NoMatchingPrincipalExceptionText;

		public static string NoMatchingGroupExceptionText;

		public static string PrincipalExistsExceptionText;

		public static string IdentityClaimCollectionNullFields;

		public static string PrincipalCollectionNotOneDimensional;

		public static string PrincipalCollectionIndexNotInArray;

		public static string PrincipalCollectionArrayTooSmall;

		public static string PrincipalCollectionEnumHasChanged;

		public static string PrincipalCollectionEnumInvalidPos;

		public static string PrincipalCollectionAlreadyMember;

		public static string AuthenticablePrincipalMustBeSubtypeOfAuthPrinc;

		public static string PasswordInfoChangePwdOnUnpersistedPrinc;

		public static string UserMustSetContextForMethod;

		public static string UserDomainNotSupportedOnPlatform;

		public static string UserLocalNotSupportedOnPlatform;

		public static string UserCouldNotFindCurrent;

		public static string UnableToRetrieveDomainInfo;

		public static string UnableToOpenToken;

		public static string UnableToRetrieveTokenInfo;

		public static string UnableToRetrievePolicy;

		public static string UnableToImpersonateCredentials;

		public static string StoreCtxUnsupportedPrincipalTypeForSave;

		public static string StoreCtxUnsupportedPrincipalTypeForGroupInsert;

		public static string StoreCtxUnsupportedPrincipalTypeForQuery;

		public static string StoreCtxUnsupportedPropertyForQuery;

		public static string StoreCtxUnsupportedIdentityClaimForQuery;

		public static string StoreCtxIdentityClaimMustHaveScheme;

		public static string StoreCtxSecurityIdentityClaimBadFormat;

		public static string StoreCtxGuidIdentityClaimBadFormat;

		public static string StoreCtxNT4IdentityClaimWrongForm;

		public static string StoreCtxCantSetTimeLimitOnIdentityClaim;

		public static string StoreCtxGroupHasUnpersistedInsertedPrincipal;

		public static string StoreCtxNeedValueSecurityIdentityClaimToQuery;

		public static string StoreCtxExceptionUpdatingGroup;

		public static string StoreCtxExceptionCommittingChanges;

		public static string ADStoreCtxUnsupportedPrincipalContextForGroupInsert;

		public static string ADStoreCtxCouldntGetSIDForGroupMember;

		public static string ADStoreCtxMustBeContainer;

		public static string ADStoreCtxCantRetrieveObjectSidForCrossStore;

		public static string ADStoreCtxCantResolveSidForCrossStore;

		public static string ADStoreCtxFailedFindCrossStoreTarget;

		public static string ADStoreCtxCantClearGroup;

		public static string ADStoreCtxCantRemoveMemberFromGroup;

		public static string ADStoreCtxUnableToReadExistingAccountControlFlagsToEnable;

		public static string ADStoreCtxUnableToReadExistingAccountControlFlagsForUpdate;

		public static string ADStoreCtxUnableToReadExistingGroupTypeFlagsForUpdate;

		public static string ADStoreCtxNoComputerPasswordChange;

		public static string SAMStoreCtxUnableToRetrieveVersion;

		public static string SAMStoreCtxUnableToRetrieveMachineName;

		public static string SAMStoreCtxUnableToRetrieveFlatMachineName;

		public static string SAMStoreCtxNoComputerPasswordSet;

		public static string SAMStoreCtxNoComputerPasswordExpire;

		public static string SAMStoreCtxIdentityClaimsImmutable;

		public static string SAMStoreCtxCouldntGetSIDForGroupMember;

		public static string SAMStoreCtxFailedToClearGroup;

		public static string SAMStoreCtxCantRetrieveObjectSidForCrossStore;

		public static string SAMStoreCtxCantResolveSidForCrossStore;

		public static string SAMStoreCtxFailedFindCrossStoreTarget;

		public static string SAMStoreCtxErrorEnumeratingGroup;

		public static string SAMStoreCtxLocalGroupsOnly;

		public static string AuthZFailedToRetrieveGroupList;

		public static string AuthZNotSupported;

		public static string AuthZErrorEnumeratingGroups;

		public static string AuthZCantFindGroup;

		public static string ConfigHandlerConfigSectionsUnique;

		public static string ConfigHandlerInvalidBoolAttribute;

		public static string ConfigHandlerInvalidEnumAttribute;

		public static string ConfigHandlerInvalidStringAttribute;

		public static string ConfigHandlerUnknownConfigSection;

		public static string PrincipalPermWrongType;

		public static string PrincipalPermXmlNotPermission;

		public static string PrincipalPermXmlBadVersion;

		public static string PrincipalPermXmlBadContents;

		public static string ExtensionInvalidClassAttributes;

		public static string ExtensionInvalidClassDefinitionConstructor;

		public static string AdsiNotInstalled;

		public static string DSUnknown;

		public static string ContextOptionsNotValidForMachineStore;

		public static string ContextNoContainerForApplicationDirectoryCtx;

		public static string PassedContextTypeDoesNotMatchDetectedType;

		public static string NullArguments;

		public static string InvalidStringValueForStore;

		public static string InvalidNullArgument;

		public static string ServerDown;

		public static string PrincipalSearcherMustSetFilter;

		public static string InvalidPropertyForStore;

		public static string InvalidOperationForStore;

		public static string NameMustBeSetToPersistPrincipal;

		public static string SaveToMustHaveSamecontextType;

		public static string SaveToNotSupportedAgainstMachineStore;

		public static string ComputerInvalidForAppDirectoryStore;

		public static string InvalidContextOptionsForMachine;

		public static string InvalidContextOptionsForAD;

		public static string InvalidExtensionCollectionType;

		public static string ADAMStoreUnableToPopulateSchemaList;

		public static string StoreCtxMultipleFiltersForPropertyUnsupported;

		static StringResources()
		{
			Type type = typeof(StringResources);
			ResourceManager resourceManager = new ResourceManager(type);
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