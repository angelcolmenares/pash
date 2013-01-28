namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Resources;

    internal static class InterpreterError
    {
        private static RuntimeException NewBackupInterpreterException(Type exceptionType, IScriptExtent errorPosition, string errorId, Exception innerException)
        {
            string str;
            if (innerException == null)
            {
                str = StringUtil.Format(ParserStrings.BackupParserMessage, errorId);
            }
            else
            {
                str = StringUtil.Format(ParserStrings.BackupParserMessageWithException, errorId, innerException.Message);
            }
            return NewInterpreterExceptionByMessage(exceptionType, errorPosition, str, errorId, innerException);
        }

        internal static RuntimeException NewInterpreterException(object targetObject, Type exceptionType, IScriptExtent errorPosition, string resourceIdAndErrorId, string resourceString, params object[] args)
        {
            return NewInterpreterExceptionWithInnerException(targetObject, exceptionType, errorPosition, resourceIdAndErrorId, resourceString, null, args);
        }

        internal static RuntimeException NewInterpreterExceptionByMessage(Type exceptionType, IScriptExtent errorPosition, string message, string errorId, Exception innerException)
        {
            RuntimeException exception;
            if (exceptionType == typeof(ParseException))
            {
                exception = new ParseException(message, errorId, innerException);
            }
            else if (exceptionType == typeof(IncompleteParseException))
            {
                exception = new IncompleteParseException(message, errorId, innerException);
            }
            else
            {
                exception = new RuntimeException(message, innerException);
                exception.SetErrorId(errorId);
                exception.SetErrorCategory(ErrorCategory.InvalidOperation);
            }
            if (errorPosition != null)
            {
                exception.ErrorRecord.SetInvocationInfo(new InvocationInfo(null, errorPosition));
            }
            return exception;
        }

        internal static RuntimeException NewInterpreterExceptionWithInnerException(object targetObject, Type exceptionType, IScriptExtent errorPosition, string resourceIdAndErrorId, string resourceString, Exception innerException, params object[] args)
        {
            if (string.IsNullOrEmpty(resourceIdAndErrorId))
            {
                throw PSTraceSource.NewArgumentException("resourceIdAndErrorId");
            }
            RuntimeException exception = null;
            try
            {
                string str;
                if ((args == null) || (args.Length == 0))
                {
                    str = resourceString;
                }
                else
                {
                    str = StringUtil.Format(resourceString, args);
                }
                if (string.IsNullOrEmpty(str))
                {
                    exception = NewBackupInterpreterException(exceptionType, errorPosition, resourceIdAndErrorId, null);
                }
                else
                {
                    exception = NewInterpreterExceptionByMessage(exceptionType, errorPosition, str, resourceIdAndErrorId, innerException);
                }
            }
            catch (InvalidOperationException exception2)
            {
                exception = NewBackupInterpreterException(exceptionType, errorPosition, resourceIdAndErrorId, exception2);
            }
            catch (MissingManifestResourceException exception3)
            {
                exception = NewBackupInterpreterException(exceptionType, errorPosition, resourceIdAndErrorId, exception3);
            }
            catch (FormatException exception4)
            {
                exception = NewBackupInterpreterException(exceptionType, errorPosition, resourceIdAndErrorId, exception4);
            }
            exception.SetTargetObject(targetObject);
            return exception;
        }

        internal static void UpdateExceptionErrorRecordPosition(Exception exception, IScriptExtent extent)
        {
            if ((extent != null) && (extent != PositionUtilities.EmptyExtent))
            {
                IContainsErrorRecord record = exception as IContainsErrorRecord;
                if (record != null)
                {
                    ErrorRecord errorRecord = record.ErrorRecord;
                    InvocationInfo invocationInfo = errorRecord.InvocationInfo;
                    if (invocationInfo == null)
                    {
                        errorRecord.SetInvocationInfo(new InvocationInfo(null, extent));
                    }
                    else if ((invocationInfo.ScriptPosition == null) || (invocationInfo.ScriptPosition == PositionUtilities.EmptyExtent))
                    {
                        invocationInfo.ScriptPosition = extent;
                        errorRecord.LockScriptStackTrace();
                    }
                }
            }
        }
    }
}

