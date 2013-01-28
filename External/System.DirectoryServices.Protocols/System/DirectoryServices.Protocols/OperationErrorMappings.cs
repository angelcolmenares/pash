using System;
using System.Collections;

namespace System.DirectoryServices.Protocols
{
	internal class OperationErrorMappings
	{
		private static Hashtable ResultCodeHash;

		static OperationErrorMappings()
		{
			OperationErrorMappings.ResultCodeHash = new Hashtable();
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.Success, Res.GetString("LDAP_SUCCESS"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.OperationsError, Res.GetString("LDAP_OPERATIONS_ERROR"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.ProtocolError, Res.GetString("LDAP_PROTOCOL_ERROR"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.TimeLimitExceeded, Res.GetString("LDAP_TIMELIMIT_EXCEEDED"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.SizeLimitExceeded, Res.GetString("LDAP_SIZELIMIT_EXCEEDED"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.CompareFalse, Res.GetString("LDAP_COMPARE_FALSE"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.CompareTrue, Res.GetString("LDAP_COMPARE_TRUE"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.AuthMethodNotSupported, Res.GetString("LDAP_AUTH_METHOD_NOT_SUPPORTED"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.StrongAuthRequired, Res.GetString("LDAP_STRONG_AUTH_REQUIRED"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.ReferralV2, Res.GetString("LDAP_PARTIAL_RESULTS"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.Referral, Res.GetString("LDAP_REFERRAL"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.AdminLimitExceeded, Res.GetString("LDAP_ADMIN_LIMIT_EXCEEDED"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.UnavailableCriticalExtension, Res.GetString("LDAP_UNAVAILABLE_CRIT_EXTENSION"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.ConfidentialityRequired, Res.GetString("LDAP_CONFIDENTIALITY_REQUIRED"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.SaslBindInProgress, Res.GetString("LDAP_SASL_BIND_IN_PROGRESS"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.NoSuchAttribute, Res.GetString("LDAP_NO_SUCH_ATTRIBUTE"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.UndefinedAttributeType, Res.GetString("LDAP_UNDEFINED_TYPE"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.InappropriateMatching, Res.GetString("LDAP_INAPPROPRIATE_MATCHING"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.ConstraintViolation, Res.GetString("LDAP_CONSTRAINT_VIOLATION"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.AttributeOrValueExists, Res.GetString("LDAP_ATTRIBUTE_OR_VALUE_EXISTS"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.InvalidAttributeSyntax, Res.GetString("LDAP_INVALID_SYNTAX"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.NoSuchObject, Res.GetString("LDAP_NO_SUCH_OBJECT"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.AliasProblem, Res.GetString("LDAP_ALIAS_PROBLEM"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.InvalidDNSyntax, Res.GetString("LDAP_INVALID_DN_SYNTAX"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.AliasDereferencingProblem, Res.GetString("LDAP_ALIAS_DEREF_PROBLEM"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.InappropriateAuthentication, Res.GetString("LDAP_INAPPROPRIATE_AUTH"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.InsufficientAccessRights, Res.GetString("LDAP_INSUFFICIENT_RIGHTS"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.Busy, Res.GetString("LDAP_BUSY"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.Unavailable, Res.GetString("LDAP_UNAVAILABLE"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.UnwillingToPerform, Res.GetString("LDAP_UNWILLING_TO_PERFORM"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.LoopDetect, Res.GetString("LDAP_LOOP_DETECT"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.SortControlMissing, Res.GetString("LDAP_SORT_CONTROL_MISSING"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.OffsetRangeError, Res.GetString("LDAP_OFFSET_RANGE_ERROR"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.NamingViolation, Res.GetString("LDAP_NAMING_VIOLATION"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.ObjectClassViolation, Res.GetString("LDAP_OBJECT_CLASS_VIOLATION"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.NotAllowedOnNonLeaf, Res.GetString("LDAP_NOT_ALLOWED_ON_NONLEAF"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.NotAllowedOnRdn, Res.GetString("LDAP_NOT_ALLOWED_ON_RDN"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.EntryAlreadyExists, Res.GetString("LDAP_ALREADY_EXISTS"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.ObjectClassModificationsProhibited, Res.GetString("LDAP_NO_OBJECT_CLASS_MODS"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.ResultsTooLarge, Res.GetString("LDAP_RESULTS_TOO_LARGE"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.AffectsMultipleDsas, Res.GetString("LDAP_AFFECTS_MULTIPLE_DSAS"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.VirtualListViewError, Res.GetString("LDAP_VIRTUAL_LIST_VIEW_ERROR"));
			OperationErrorMappings.ResultCodeHash.Add(ResultCode.Other, Res.GetString("LDAP_OTHER"));
		}

		public OperationErrorMappings()
		{
		}

		public static string MapResultCode(int errorCode)
		{
			return (string)OperationErrorMappings.ResultCodeHash[(object)((ResultCode)errorCode)];
		}
	}
}