using Microsoft.Management.Infrastructure;
using Microsoft.PowerShell.Commands.Management;
using System;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.Cmdletization.Cim
{
	[Serializable]
	public class CimJobException : Exception, IContainsErrorRecord
	{
		private ErrorRecord errorRecord;

		public ErrorRecord ErrorRecord
		{
			get
			{
				return this.errorRecord;
			}
		}

		internal bool IsTerminatingError
		{
			get
			{
				CimException innerException = base.InnerException as CimException;
				if (innerException == null || innerException.ErrorData == null)
				{
					return false;
				}
				else
				{
					CimProperty item = innerException.ErrorData.CimInstanceProperties["PerceivedSeverity"];
					if (item == null || item.CimType != CimType.UInt16 || item.Value == null)
					{
						return false;
					}
					else
					{
						ushort value = (ushort)item.Value;
						if (value == 7)
						{
							return true;
						}
						else
						{
							return false;
						}
					}
				}
			}
		}

		public CimJobException() : this(null, null)
		{
		}

		public CimJobException(string message) : this(message, null)
		{
		}

		public CimJobException(string message, Exception inner) : base(message, inner)
		{
			this.InitializeErrorRecord(null, "CimJob_ExternalError", ErrorCategory.NotSpecified);
		}

		protected CimJobException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info != null)
			{
				this.errorRecord = (ErrorRecord)info.GetValue("errorRecord", typeof(ErrorRecord));
				return;
			}
			else
			{
				throw new ArgumentNullException("info");
			}
		}

		private static string BuildErrorMessage(string jobDescription, CimJobContext jobContext, string errorMessage)
		{
			if (!string.IsNullOrEmpty(jobDescription))
			{
				object[] objArray = new object[2];
				objArray[0] = errorMessage;
				objArray[1] = jobDescription;
				string str = string.Format(CultureInfo.InvariantCulture, CmdletizationResources.CimJob_GenericCimFailure, objArray);
				return jobContext.PrependComputerNameToMessage(str);
			}
			else
			{
				return jobContext.PrependComputerNameToMessage(errorMessage);
			}
		}

		private static ErrorCategory ConvertCimErrorToErrorCategory(CimInstance cimError)
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

		private static ErrorCategory ConvertCimExceptionToErrorCategory(CimException cimException)
		{
			ErrorCategory errorCategory = ErrorCategory.NotSpecified;
			if (cimException.ErrorData != null)
			{
				errorCategory = CimJobException.ConvertCimErrorToErrorCategory(cimException.ErrorData);
			}
			if (errorCategory == ErrorCategory.NotSpecified)
			{
				errorCategory = CimJobException.ConvertCimNativeErrorCodeToErrorCategory(cimException.NativeErrorCode);
			}
			return errorCategory;
		}

		private static ErrorCategory ConvertCimNativeErrorCodeToErrorCategory(NativeErrorCode nativeErrorCode)
		{
			NativeErrorCode nativeErrorCode1 = nativeErrorCode;
			switch (nativeErrorCode1)
			{
				case NativeErrorCode.Ok:
				{
					return ErrorCategory.NotSpecified;
				}
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

		internal static CimJobException CreateFromAnyException(string jobDescription, CimJobContext jobContext, Exception inner)
		{
			CimException cimException = inner as CimException;
			if (cimException == null)
			{
				string str = CimJobException.BuildErrorMessage(jobDescription, jobContext, inner.Message);
				CimJobException cimJobException = new CimJobException(str, inner);
				IContainsErrorRecord containsErrorRecord = inner as IContainsErrorRecord;
				if (containsErrorRecord == null)
				{
					cimJobException.InitializeErrorRecord(jobContext, string.Concat("CimJob_", inner.GetType().Name), ErrorCategory.NotSpecified);
				}
				else
				{
					cimJobException.InitializeErrorRecord(jobContext, string.Concat("CimJob_", containsErrorRecord.ErrorRecord.FullyQualifiedErrorId), containsErrorRecord.ErrorRecord.CategoryInfo.Category);
				}
				return cimJobException;
			}
			else
			{
				return CimJobException.CreateFromCimException(jobDescription, jobContext, cimException);
			}
		}

		internal static CimJobException CreateFromCimException(string jobDescription, CimJobContext jobContext, CimException cimException)
		{
			string str = CimJobException.BuildErrorMessage(jobDescription, jobContext, cimException.Message);
			CimJobException cimJobException = new CimJobException(str, cimException);
			cimJobException.InitializeErrorRecord(jobContext, cimException);
			return cimJobException;
		}

		internal static CimJobException CreateFromMethodErrorCode(string jobDescription, CimJobContext jobContext, string methodName, string errorCodeFromMethod)
		{
			object[] objArray = new object[1];
			objArray[0] = errorCodeFromMethod;
			string str = string.Format(CultureInfo.InvariantCulture, CmdletizationResources.CimJob_ErrorCodeFromMethod, objArray);
			string str1 = CimJobException.BuildErrorMessage(jobDescription, jobContext, str);
			CimJobException cimJobException = new CimJobException(str1);
			cimJobException.InitializeErrorRecord(jobContext, string.Concat("CimJob_", methodName, "_", errorCodeFromMethod), ErrorCategory.InvalidResult);
			return cimJobException;
		}

		internal static CimJobException CreateWithFullControl(CimJobContext jobContext, string message, string errorId, ErrorCategory errorCategory, Exception inner = null)
		{
			CimJobException cimJobException = new CimJobException(jobContext.PrependComputerNameToMessage(message), inner);
			cimJobException.InitializeErrorRecord(jobContext, errorId, errorCategory);
			return cimJobException;
		}

		internal static CimJobException CreateWithoutJobContext(string message, string errorId, ErrorCategory errorCategory, Exception inner = null)
		{
			CimJobException cimJobException = new CimJobException(message, inner);
			cimJobException.InitializeErrorRecord(null, errorId, errorCategory);
			return cimJobException;
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info != null)
			{
				base.GetObjectData(info, context);
				info.AddValue("errorRecord", this.errorRecord);
				return;
			}
			else
			{
				throw new ArgumentNullException("info");
			}
		}

		private void InitializeErrorRecord(CimJobContext jobContext, string errorId, ErrorCategory errorCategory)
		{
			this.InitializeErrorRecordCore(jobContext, this, errorId, errorCategory);
		}

		private void InitializeErrorRecord(CimJobContext jobContext, CimException cimException)
		{
			string cmdletizationClassName;
			CimJobException cimJobException = this;
			CimJobContext cimJobContext = jobContext;
			CimException cimException1 = cimException;
			string messageId = cimException.MessageId;
			string str = messageId;
			if (messageId == null)
			{
				str = string.Concat("MiClientApiError_", cimException.NativeErrorCode);
			}
			cimJobException.InitializeErrorRecordCore(cimJobContext, cimException1, str, CimJobException.ConvertCimExceptionToErrorCategory(cimException));
			if (cimException.ErrorData != null)
			{
				this.errorRecord.CategoryInfo.TargetName = cimException.ErrorSource;
				ErrorCategoryInfo categoryInfo = this.errorRecord.CategoryInfo;
				if (jobContext != null)
				{
					cmdletizationClassName = jobContext.CmdletizationClassName;
				}
				else
				{
					cmdletizationClassName = null;
				}
				categoryInfo.TargetType = cmdletizationClassName;
			}
		}

		private void InitializeErrorRecordCore(CimJobContext jobContext, Exception exception, string errorId, ErrorCategory errorCategory)
		{
			object targetObject;
			Exception exception1 = exception;
			string str = errorId;
			ErrorCategory errorCategory1 = errorCategory;
			if (jobContext != null)
			{
				targetObject = jobContext.TargetObject;
			}
			else
			{
				targetObject = null;
			}
			ErrorRecord errorRecord = new ErrorRecord(exception1, str, errorCategory1, targetObject);
			if (jobContext == null)
			{
				this.errorRecord = errorRecord;
				return;
			}
			else
			{
				OriginInfo originInfo = new OriginInfo(jobContext.Session.ComputerName, Guid.Empty);
				this.errorRecord = new RemotingErrorRecord(errorRecord, originInfo);
				this.errorRecord.SetInvocationInfo(jobContext.CmdletInvocationInfo);
				this.errorRecord.PreserveInvocationInfoOnce = true;
				return;
			}
		}
	}
}