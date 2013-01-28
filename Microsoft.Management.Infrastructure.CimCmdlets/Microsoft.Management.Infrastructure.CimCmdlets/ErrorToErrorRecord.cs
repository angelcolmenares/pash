using Microsoft.Management.Infrastructure;
using System;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal sealed class ErrorToErrorRecord
	{
		public ErrorToErrorRecord()
		{
		}

		internal static ErrorCategory ConvertCimErrorToErrorCategory(CimInstance cimError)
		{
			ErrorCategory errorCategory = ErrorCategory.NotSpecified;
			if (cimError != null)
			{
				CimProperty item = cimError.CimInstanceProperties["Error_Category"];
				if (item != null)
				{
					if (LanguagePrimitives.TryConvertTo<ErrorCategory>(item.Value, CultureInfo.InvariantCulture, out errorCategory))
					{
						return errorCategory;
					}
					else
					{
						return ErrorCategory.NotSpecified;
					}
				}
				else
				{
					return ErrorCategory.NotSpecified;
				}
			}
			else
			{
				return ErrorCategory.NotSpecified;
			}
		}

		internal static ErrorCategory ConvertCimExceptionToErrorCategory(CimException cimException)
		{
			ErrorCategory errorCategory = ErrorCategory.NotSpecified;
			if (cimException.ErrorData != null)
			{
				errorCategory = ErrorToErrorRecord.ConvertCimErrorToErrorCategory(cimException.ErrorData);
			}
			if (errorCategory == ErrorCategory.NotSpecified)
			{
				errorCategory = ErrorToErrorRecord.ConvertCimNativeErrorCodeToErrorCategory(cimException.NativeErrorCode);
			}
			return errorCategory;
		}

		internal static ErrorCategory ConvertCimNativeErrorCodeToErrorCategory(NativeErrorCode nativeErrorCode)
		{
			NativeErrorCode nativeErrorCode1 = nativeErrorCode;
			switch (nativeErrorCode1)
			{
				case NativeErrorCode.Failed:
				{
					return ErrorCategory.NotSpecified;
				}
				case NativeErrorCode.AccessDenied:
				{
					return ErrorCategory.PermissionDenied;
				}
				case NativeErrorCode.InvalidNamespace:
				{
					return ErrorCategory.MetadataError;
				}
				case NativeErrorCode.InvalidParameter:
				{
					return ErrorCategory.InvalidArgument;
				}
				case NativeErrorCode.InvalidClass:
				{
					return ErrorCategory.MetadataError;
				}
				case NativeErrorCode.NotFound:
				{
					return ErrorCategory.ObjectNotFound;
				}
				case NativeErrorCode.NotSupported:
				{
					return ErrorCategory.NotImplemented;
				}
				case NativeErrorCode.ClassHasChildren:
				{
					return ErrorCategory.MetadataError;
				}
				case NativeErrorCode.ClassHasInstances:
				{
					return ErrorCategory.MetadataError;
				}
				case NativeErrorCode.InvalidSuperClass:
				{
					return ErrorCategory.MetadataError;
				}
				case NativeErrorCode.AlreadyExists:
				{
					return ErrorCategory.ResourceExists;
				}
				case NativeErrorCode.NoSuchProperty:
				{
					return ErrorCategory.MetadataError;
				}
				case NativeErrorCode.TypeMismatch:
				{
					return ErrorCategory.InvalidType;
				}
				case NativeErrorCode.QueryLanguageNotSupported:
				{
					return ErrorCategory.NotImplemented;
				}
				case NativeErrorCode.InvalidQuery:
				{
					return ErrorCategory.InvalidArgument;
				}
				case NativeErrorCode.MethodNotAvailable:
				{
					return ErrorCategory.MetadataError;
				}
				case NativeErrorCode.MethodNotFound:
				{
					return ErrorCategory.MetadataError;
				}
				case NativeErrorCode.AccessDenied | NativeErrorCode.MethodNotAvailable:
				case NativeErrorCode.Failed | NativeErrorCode.AccessDenied | NativeErrorCode.InvalidNamespace | NativeErrorCode.MethodNotAvailable | NativeErrorCode.MethodNotFound:
				{
					return ErrorCategory.NotSpecified;
				}
				case NativeErrorCode.NamespaceNotEmpty:
				{
					return ErrorCategory.MetadataError;
				}
				case NativeErrorCode.InvalidEnumerationContext:
				{
					return ErrorCategory.MetadataError;
				}
				case NativeErrorCode.InvalidOperationTimeout:
				{
					return ErrorCategory.InvalidArgument;
				}
				case NativeErrorCode.PullHasBeenAbandoned:
				{
					return ErrorCategory.OperationStopped;
				}
				case NativeErrorCode.PullCannotBeAbandoned:
				{
					return ErrorCategory.CloseError;
				}
				case NativeErrorCode.FilteredEnumerationNotSupported:
				{
					return ErrorCategory.NotImplemented;
				}
				case NativeErrorCode.ContinuationOnErrorNotSupported:
				{
					return ErrorCategory.NotImplemented;
				}
				case NativeErrorCode.ServerLimitsExceeded:
				{
					return ErrorCategory.ResourceBusy;
				}
				case NativeErrorCode.ServerIsShuttingDown:
				{
					return ErrorCategory.ResourceUnavailable;
				}
				default:
				{
					return ErrorCategory.NotSpecified;
				}
			}
		}

		internal static ErrorRecord CreateFromCimException(InvocationContext context, CimException cimException, CimResultContext cimResultContext)
		{
			return ErrorToErrorRecord.InitializeErrorRecord(context, cimException, cimResultContext);
		}

		internal static ErrorRecord ErrorRecordFromAnyException(InvocationContext context, Exception inner, CimResultContext cimResultContext)
		{
			CimException cimException = inner as CimException;
			if (cimException == null)
			{
				IContainsErrorRecord containsErrorRecord = inner as IContainsErrorRecord;
				if (containsErrorRecord == null)
				{
					return ErrorToErrorRecord.InitializeErrorRecord(context, inner, string.Concat("CimCmdlet_", inner.GetType().Name), ErrorCategory.NotSpecified, cimResultContext);
				}
				else
				{
					return ErrorToErrorRecord.InitializeErrorRecord(context, inner, string.Concat("CimCmdlet_", containsErrorRecord.ErrorRecord.FullyQualifiedErrorId), containsErrorRecord.ErrorRecord.CategoryInfo.Category, cimResultContext);
				}
			}
			else
			{
				return ErrorToErrorRecord.CreateFromCimException(context, cimException, cimResultContext);
			}
		}

		internal static ErrorRecord InitializeErrorRecord(InvocationContext context, Exception exception, string errorId, ErrorCategory errorCategory, CimResultContext cimResultContext)
		{
			return ErrorToErrorRecord.InitializeErrorRecordCore(context, exception, errorId, errorCategory, cimResultContext);
		}

		internal static ErrorRecord InitializeErrorRecord(InvocationContext context, CimException cimException, CimResultContext cimResultContext)
		{
			InvocationContext invocationContext = context;
			CimException cimException1 = cimException;
			string messageId = cimException.MessageId;
			string str = messageId;
			if (messageId == null)
			{
				str = string.Concat("MiClientApiError_", cimException.NativeErrorCode);
			}
			ErrorRecord errorSource = ErrorToErrorRecord.InitializeErrorRecordCore(invocationContext, cimException1, str, ErrorToErrorRecord.ConvertCimExceptionToErrorCategory(cimException), cimResultContext);
			if (cimException.ErrorData != null)
			{
				errorSource.CategoryInfo.TargetName = cimException.ErrorSource;
			}
			return errorSource;
		}

		internal static ErrorRecord InitializeErrorRecordCore(InvocationContext context, Exception exception, string errorId, ErrorCategory errorCategory, CimResultContext cimResultContext)
		{
			object errorSource = null;
			if (cimResultContext != null)
			{
				errorSource = cimResultContext.ErrorSource;
			}
			if (errorSource == null && context != null && context.TargetCimInstance != null)
			{
				errorSource = context.TargetCimInstance;
			}
			ErrorRecord errorRecord = new ErrorRecord(exception, errorId, errorCategory, errorSource);
			if (context != null)
			{
				OriginInfo originInfo = new OriginInfo(context.ComputerName, Guid.Empty);
				ErrorRecord remotingErrorRecord = new RemotingErrorRecord(errorRecord, originInfo);
				DebugHelper.WriteLogEx("Created RemotingErrorRecord.", 0);
				return remotingErrorRecord;
			}
			else
			{
				return errorRecord;
			}
		}
	}
}