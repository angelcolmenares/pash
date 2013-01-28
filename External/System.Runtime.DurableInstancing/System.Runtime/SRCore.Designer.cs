using System;
using System.Globalization;
using System.Resources;

namespace System.Runtime
{
	internal class SRCore
	{
		private static ResourceManager resourceManager;

		private static CultureInfo resourceCulture;

		internal static string AlreadyBoundToInstance
		{
			get
			{
				return SRCore.ResourceManager.GetString("AlreadyBoundToInstance", SRCore.Culture);
			}
		}

		internal static string AlreadyBoundToOwner
		{
			get
			{
				return SRCore.ResourceManager.GetString("AlreadyBoundToOwner", SRCore.Culture);
			}
		}

		internal static string AsyncTransactionException
		{
			get
			{
				return SRCore.ResourceManager.GetString("AsyncTransactionException", SRCore.Culture);
			}
		}

		internal static string BindLockRequiresCommandFlag
		{
			get
			{
				return SRCore.ResourceManager.GetString("BindLockRequiresCommandFlag", SRCore.Culture);
			}
		}

		internal static string BindReclaimedLockException
		{
			get
			{
				return SRCore.ResourceManager.GetString("BindReclaimedLockException", SRCore.Culture);
			}
		}

		internal static string BindReclaimSucceeded
		{
			get
			{
				return SRCore.ResourceManager.GetString("BindReclaimSucceeded", SRCore.Culture);
			}
		}

		internal static string CannotAcquireLockDefault
		{
			get
			{
				return SRCore.ResourceManager.GetString("CannotAcquireLockDefault", SRCore.Culture);
			}
		}

		internal static string CannotCompleteWithKeys
		{
			get
			{
				return SRCore.ResourceManager.GetString("CannotCompleteWithKeys", SRCore.Culture);
			}
		}

		internal static string CannotCreateContextWithNullId
		{
			get
			{
				return SRCore.ResourceManager.GetString("CannotCreateContextWithNullId", SRCore.Culture);
			}
		}

		internal static string CannotInvokeBindingFromNonBinding
		{
			get
			{
				return SRCore.ResourceManager.GetString("CannotInvokeBindingFromNonBinding", SRCore.Culture);
			}
		}

		internal static string CannotInvokeTransactionalFromNonTransactional
		{
			get
			{
				return SRCore.ResourceManager.GetString("CannotInvokeTransactionalFromNonTransactional", SRCore.Culture);
			}
		}

		internal static string CannotReplaceTransaction
		{
			get
			{
				return SRCore.ResourceManager.GetString("CannotReplaceTransaction", SRCore.Culture);
			}
		}

		internal static string CommandExecutionCannotOverlap
		{
			get
			{
				return SRCore.ResourceManager.GetString("CommandExecutionCannotOverlap", SRCore.Culture);
			}
		}

		internal static string CompletedMustNotHaveAssociatedKeys
		{
			get
			{
				return SRCore.ResourceManager.GetString("CompletedMustNotHaveAssociatedKeys", SRCore.Culture);
			}
		}

		internal static string ContextAlreadyBoundToInstance
		{
			get
			{
				return SRCore.ResourceManager.GetString("ContextAlreadyBoundToInstance", SRCore.Culture);
			}
		}

		internal static string ContextAlreadyBoundToLock
		{
			get
			{
				return SRCore.ResourceManager.GetString("ContextAlreadyBoundToLock", SRCore.Culture);
			}
		}

		internal static string ContextAlreadyBoundToOwner
		{
			get
			{
				return SRCore.ResourceManager.GetString("ContextAlreadyBoundToOwner", SRCore.Culture);
			}
		}

		internal static string ContextMustBeBoundToInstance
		{
			get
			{
				return SRCore.ResourceManager.GetString("ContextMustBeBoundToInstance", SRCore.Culture);
			}
		}

		internal static string ContextMustBeBoundToOwner
		{
			get
			{
				return SRCore.ResourceManager.GetString("ContextMustBeBoundToOwner", SRCore.Culture);
			}
		}

		internal static string ContextNotFromThisStore
		{
			get
			{
				return SRCore.ResourceManager.GetString("ContextNotFromThisStore", SRCore.Culture);
			}
		}

		internal static CultureInfo Culture
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return SRCore.resourceCulture;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				SRCore.resourceCulture = value;
			}
		}

		internal static string DoNotCompleteTryCommandWithPendingReclaim
		{
			get
			{
				return SRCore.ResourceManager.GetString("DoNotCompleteTryCommandWithPendingReclaim", SRCore.Culture);
			}
		}

		internal static string ExecuteMustBeNested
		{
			get
			{
				return SRCore.ResourceManager.GetString("ExecuteMustBeNested", SRCore.Culture);
			}
		}

		internal static string ExtensionsCannotBeSetByIndex
		{
			get
			{
				return SRCore.ResourceManager.GetString("ExtensionsCannotBeSetByIndex", SRCore.Culture);
			}
		}

		internal static string GenericInstanceCommandNull
		{
			get
			{
				return SRCore.ResourceManager.GetString("GenericInstanceCommandNull", SRCore.Culture);
			}
		}

		internal static string GuidCannotBeEmpty
		{
			get
			{
				return SRCore.ResourceManager.GetString("GuidCannotBeEmpty", SRCore.Culture);
			}
		}

		internal static string HandleFreed
		{
			get
			{
				return SRCore.ResourceManager.GetString("HandleFreed", SRCore.Culture);
			}
		}

		internal static string HandleFreedBeforeInitialized
		{
			get
			{
				return SRCore.ResourceManager.GetString("HandleFreedBeforeInitialized", SRCore.Culture);
			}
		}

		internal static string InstanceCollisionDefault
		{
			get
			{
				return SRCore.ResourceManager.GetString("InstanceCollisionDefault", SRCore.Culture);
			}
		}

		internal static string InstanceCompleteDefault
		{
			get
			{
				return SRCore.ResourceManager.GetString("InstanceCompleteDefault", SRCore.Culture);
			}
		}

		internal static string InstanceHandleConflictDefault
		{
			get
			{
				return SRCore.ResourceManager.GetString("InstanceHandleConflictDefault", SRCore.Culture);
			}
		}

		internal static string InstanceKeyRequiresValidGuid
		{
			get
			{
				return SRCore.ResourceManager.GetString("InstanceKeyRequiresValidGuid", SRCore.Culture);
			}
		}

		internal static string InstanceLockLostDefault
		{
			get
			{
				return SRCore.ResourceManager.GetString("InstanceLockLostDefault", SRCore.Culture);
			}
		}

		internal static string InstanceNotReadyDefault
		{
			get
			{
				return SRCore.ResourceManager.GetString("InstanceNotReadyDefault", SRCore.Culture);
			}
		}

		internal static string InstanceOperationRequiresInstance
		{
			get
			{
				return SRCore.ResourceManager.GetString("InstanceOperationRequiresInstance", SRCore.Culture);
			}
		}

		internal static string InstanceOperationRequiresLock
		{
			get
			{
				return SRCore.ResourceManager.GetString("InstanceOperationRequiresLock", SRCore.Culture);
			}
		}

		internal static string InstanceOperationRequiresNotCompleted
		{
			get
			{
				return SRCore.ResourceManager.GetString("InstanceOperationRequiresNotCompleted", SRCore.Culture);
			}
		}

		internal static string InstanceOperationRequiresNotUninitialized
		{
			get
			{
				return SRCore.ResourceManager.GetString("InstanceOperationRequiresNotUninitialized", SRCore.Culture);
			}
		}

		internal static string InstanceOperationRequiresOwner
		{
			get
			{
				return SRCore.ResourceManager.GetString("InstanceOperationRequiresOwner", SRCore.Culture);
			}
		}

		internal static string InstanceOwnerDefault
		{
			get
			{
				return SRCore.ResourceManager.GetString("InstanceOwnerDefault", SRCore.Culture);
			}
		}

		internal static string InstanceRequired
		{
			get
			{
				return SRCore.ResourceManager.GetString("InstanceRequired", SRCore.Culture);
			}
		}

		internal static string InstanceStoreBoundSameVersionTwice
		{
			get
			{
				return SRCore.ResourceManager.GetString("InstanceStoreBoundSameVersionTwice", SRCore.Culture);
			}
		}

		internal static string InvalidInstanceState
		{
			get
			{
				return SRCore.ResourceManager.GetString("InvalidInstanceState", SRCore.Culture);
			}
		}

		internal static string InvalidKeyArgument
		{
			get
			{
				return SRCore.ResourceManager.GetString("InvalidKeyArgument", SRCore.Culture);
			}
		}

		internal static string InvalidLockToken
		{
			get
			{
				return SRCore.ResourceManager.GetString("InvalidLockToken", SRCore.Culture);
			}
		}

		internal static string InvalidStateInAsyncResult
		{
			get
			{
				return SRCore.ResourceManager.GetString("InvalidStateInAsyncResult", SRCore.Culture);
			}
		}

		internal static string KeyAlreadyAssociated
		{
			get
			{
				return SRCore.ResourceManager.GetString("KeyAlreadyAssociated", SRCore.Culture);
			}
		}

		internal static string KeyAlreadyCompleted
		{
			get
			{
				return SRCore.ResourceManager.GetString("KeyAlreadyCompleted", SRCore.Culture);
			}
		}

		internal static string KeyAlreadyUnassociated
		{
			get
			{
				return SRCore.ResourceManager.GetString("KeyAlreadyUnassociated", SRCore.Culture);
			}
		}

		internal static string KeyCollisionDefault
		{
			get
			{
				return SRCore.ResourceManager.GetString("KeyCollisionDefault", SRCore.Culture);
			}
		}

		internal static string KeyCompleteDefault
		{
			get
			{
				return SRCore.ResourceManager.GetString("KeyCompleteDefault", SRCore.Culture);
			}
		}

		internal static string KeyNotAssociated
		{
			get
			{
				return SRCore.ResourceManager.GetString("KeyNotAssociated", SRCore.Culture);
			}
		}

		internal static string KeyNotCompleted
		{
			get
			{
				return SRCore.ResourceManager.GetString("KeyNotCompleted", SRCore.Culture);
			}
		}

		internal static string KeyNotReadyDefault
		{
			get
			{
				return SRCore.ResourceManager.GetString("KeyNotReadyDefault", SRCore.Culture);
			}
		}

		internal static string LoadedWriteOnlyValue
		{
			get
			{
				return SRCore.ResourceManager.GetString("LoadedWriteOnlyValue", SRCore.Culture);
			}
		}

		internal static string LoadOpAssociateKeysCannotContainLookupKey
		{
			get
			{
				return SRCore.ResourceManager.GetString("LoadOpAssociateKeysCannotContainLookupKey", SRCore.Culture);
			}
		}

		internal static string LoadOpFreeKeyRequiresAcceptUninitialized
		{
			get
			{
				return SRCore.ResourceManager.GetString("LoadOpFreeKeyRequiresAcceptUninitialized", SRCore.Culture);
			}
		}

		internal static string LoadOpKeyMustBeValid
		{
			get
			{
				return SRCore.ResourceManager.GetString("LoadOpKeyMustBeValid", SRCore.Culture);
			}
		}

		internal static string MayBindLockCommandShouldValidateOwner
		{
			get
			{
				return SRCore.ResourceManager.GetString("MayBindLockCommandShouldValidateOwner", SRCore.Culture);
			}
		}

		internal static string MetadataCannotContainNullKey
		{
			get
			{
				return SRCore.ResourceManager.GetString("MetadataCannotContainNullKey", SRCore.Culture);
			}
		}

		internal static string MustSetTransactionOnFirstCall
		{
			get
			{
				return SRCore.ResourceManager.GetString("MustSetTransactionOnFirstCall", SRCore.Culture);
			}
		}

		internal static string OnCancelRequestedThrew
		{
			get
			{
				return SRCore.ResourceManager.GetString("OnCancelRequestedThrew", SRCore.Culture);
			}
		}

		internal static string OnFreeInstanceHandleThrew
		{
			get
			{
				return SRCore.ResourceManager.GetString("OnFreeInstanceHandleThrew", SRCore.Culture);
			}
		}

		internal static string OwnerBelongsToWrongStore
		{
			get
			{
				return SRCore.ResourceManager.GetString("OwnerBelongsToWrongStore", SRCore.Culture);
			}
		}

		internal static string OwnerRequired
		{
			get
			{
				return SRCore.ResourceManager.GetString("OwnerRequired", SRCore.Culture);
			}
		}

		internal static string PersistenceInitializerThrew
		{
			get
			{
				return SRCore.ResourceManager.GetString("PersistenceInitializerThrew", SRCore.Culture);
			}
		}

		internal static ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(SRCore.resourceManager, null))
				{
					ResourceManager resourceManager = new ResourceManager("System.Runtime.SRCore", typeof(SRCore).Assembly);
					SRCore.resourceManager = resourceManager;
				}
				return SRCore.resourceManager;
			}
		}

		internal static string StoreReportedConflictingLockTokens
		{
			get
			{
				return SRCore.ResourceManager.GetString("StoreReportedConflictingLockTokens", SRCore.Culture);
			}
		}

		internal static string TimedOutWaitingForLockResolution
		{
			get
			{
				return SRCore.ResourceManager.GetString("TimedOutWaitingForLockResolution", SRCore.Culture);
			}
		}

		internal static string TransactionInDoubtNonHost
		{
			get
			{
				return SRCore.ResourceManager.GetString("TransactionInDoubtNonHost", SRCore.Culture);
			}
		}

		internal static string TransactionRolledBackNonHost
		{
			get
			{
				return SRCore.ResourceManager.GetString("TransactionRolledBackNonHost", SRCore.Culture);
			}
		}

		internal static string TryCommandCannotExecuteSubCommandsAndReduce
		{
			get
			{
				return SRCore.ResourceManager.GetString("TryCommandCannotExecuteSubCommandsAndReduce", SRCore.Culture);
			}
		}

		internal static string UninitializedCannotHaveData
		{
			get
			{
				return SRCore.ResourceManager.GetString("UninitializedCannotHaveData", SRCore.Culture);
			}
		}

		internal static string ValidateUnlockInstance
		{
			get
			{
				return SRCore.ResourceManager.GetString("ValidateUnlockInstance", SRCore.Culture);
			}
		}

		internal static string WaitAlreadyInProgress
		{
			get
			{
				return SRCore.ResourceManager.GetString("WaitAlreadyInProgress", SRCore.Culture);
			}
		}

		private SRCore()
		{
		}

		internal static string CannotAcquireLockSpecific(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("CannotAcquireLockSpecific", SRCore.Culture), objArray);
		}

		internal static string CannotAcquireLockSpecificWithOwner(object param0, object param1)
		{
			object[] objArray = new object[2];
			objArray[0] = param0;
			objArray[1] = param1;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("CannotAcquireLockSpecificWithOwner", SRCore.Culture), objArray);
		}

		internal static string CouldNotResolveNamespacePrefix(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("CouldNotResolveNamespacePrefix", SRCore.Culture), objArray);
		}

		internal static string GenericInstanceCommand(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("GenericInstanceCommand", SRCore.Culture), objArray);
		}

		internal static string GetParameterTypeMismatch(object param0, object param1)
		{
			object[] objArray = new object[2];
			objArray[0] = param0;
			objArray[1] = param1;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("GetParameterTypeMismatch", SRCore.Culture), objArray);
		}

		internal static string IncorrectValueType(object param0, object param1)
		{
			object[] objArray = new object[2];
			objArray[0] = param0;
			objArray[1] = param1;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("IncorrectValueType", SRCore.Culture), objArray);
		}

		internal static string InitialMetadataCannotBeDeleted(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("InitialMetadataCannotBeDeleted", SRCore.Culture), objArray);
		}

		internal static string InstanceCollisionSpecific(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("InstanceCollisionSpecific", SRCore.Culture), objArray);
		}

		internal static string InstanceCompleteSpecific(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("InstanceCompleteSpecific", SRCore.Culture), objArray);
		}

		internal static string InstanceHandleConflictSpecific(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("InstanceHandleConflictSpecific", SRCore.Culture), objArray);
		}

		internal static string InstanceLockLostSpecific(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("InstanceLockLostSpecific", SRCore.Culture), objArray);
		}

		internal static string InstanceNotReadySpecific(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("InstanceNotReadySpecific", SRCore.Culture), objArray);
		}

		internal static string InstanceOwnerSpecific(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("InstanceOwnerSpecific", SRCore.Culture), objArray);
		}

		internal static string KeyCollisionSpecific(object param0, object param1, object param2)
		{
			object[] objArray = new object[3];
			objArray[0] = param0;
			objArray[1] = param1;
			objArray[2] = param2;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("KeyCollisionSpecific", SRCore.Culture), objArray);
		}

		internal static string KeyCollisionSpecificKeyOnly(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("KeyCollisionSpecificKeyOnly", SRCore.Culture), objArray);
		}

		internal static string KeyCompleteSpecific(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("KeyCompleteSpecific", SRCore.Culture), objArray);
		}

		internal static string KeyNotReadySpecific(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("KeyNotReadySpecific", SRCore.Culture), objArray);
		}

		internal static string MetadataCannotContainNullValue(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("MetadataCannotContainNullValue", SRCore.Culture), objArray);
		}

		internal static string NameCollisionOnCollect(object param0, object param1)
		{
			object[] objArray = new object[2];
			objArray[0] = param0;
			objArray[1] = param1;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("NameCollisionOnCollect", SRCore.Culture), objArray);
		}

		internal static string NameCollisionOnMap(object param0, object param1)
		{
			object[] objArray = new object[2];
			objArray[0] = param0;
			objArray[1] = param1;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("NameCollisionOnMap", SRCore.Culture), objArray);
		}

		internal static string NullAssignedToValueType(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("NullAssignedToValueType", SRCore.Culture), objArray);
		}

		internal static string OutsideInstanceExecutionScope(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("OutsideInstanceExecutionScope", SRCore.Culture), objArray);
		}

		internal static string OutsideTransactionalCommand(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("OutsideTransactionalCommand", SRCore.Culture), objArray);
		}

		internal static string PersistencePipelineAbortThrew(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("PersistencePipelineAbortThrew", SRCore.Culture), objArray);
		}

		internal static string ProviderDoesNotSupportCommand(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("ProviderDoesNotSupportCommand", SRCore.Culture), objArray);
		}

		internal static string WaitForEventsTimedOut(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(SRCore.Culture, SRCore.ResourceManager.GetString("WaitForEventsTimedOut", SRCore.Culture), objArray);
		}
	}
}