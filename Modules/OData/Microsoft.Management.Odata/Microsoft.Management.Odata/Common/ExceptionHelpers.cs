using Microsoft.Management.Odata;
using System;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;

namespace Microsoft.Management.Odata.Common
{
	internal static class ExceptionHelpers
	{
		public static string GetDataServiceExceptionMessage(HttpStatusCode statusCode, string message, object[] args)
		{
			string str = string.Format(CultureInfo.CurrentCulture, message, args);
			TraceHelper.Current.DebugMessage(ExceptionHelpers.GetTraceMessage(str, string.Concat("Throwing Data Service exception. \nHttp Status code = ", statusCode.ToString())).ToString());
			return str;
		}

		public static string GetExceptionMessage(string message, object[] args)
		{
			string str = string.Format(CultureInfo.CurrentCulture, message, args);
			TraceHelper.Current.DebugMessage(ExceptionHelpers.GetTraceMessage(str, "Throwing exception.").ToString());
			return str;
		}

		public static string GetExceptionMessage(Exception innerException, string message, object[] args)
		{
			string str = string.Format(CultureInfo.CurrentCulture, message, args);
			StringBuilder traceMessage = ExceptionHelpers.GetTraceMessage(str, "Throwing exception.");
			if (innerException != null)
			{
				traceMessage = innerException.ToTraceMessage("Inner exception", traceMessage);
			}
			TraceHelper.Current.DebugMessage(traceMessage.ToString());
			return str;
		}

		public static string GetInvalidCastExceptionMessage(Type sourceType, Type destinationType)
		{
			object[] assemblyQualifiedName = new object[2];
			assemblyQualifiedName[0] = sourceType.AssemblyQualifiedName;
			assemblyQualifiedName[1] = destinationType.AssemblyQualifiedName;
			string str = string.Format(CultureInfo.CurrentCulture, Resources.InvalidCastTried, assemblyQualifiedName);
			TraceHelper.Current.DebugMessage(ExceptionHelpers.GetTraceMessage(str, "Throwing exception.").ToString());
			return str;
		}

		private static StringBuilder GetTraceMessage(string exceptionMessage, string traceMessageStart)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(string.Concat(traceMessageStart, "\nMessage = ", exceptionMessage));
			stringBuilder.AppendLine("Stack Trace");
			stringBuilder.AppendLine(StackTraceHelper.GetStrackTrace(2));
			return stringBuilder;
		}

		public static bool IsIgnorablePropertyException(this Exception ex)
		{
			if (ex as TargetInvocationException != null && ex.InnerException != null)
			{
				Exception innerException = ex.InnerException;
				if (innerException as InvalidOperationException != null || innerException as MissingMemberException != null)
				{
					TraceHelper.Current.DebugMessage(string.Concat("ignoring invocation exception ", innerException.ToTraceMessage("Exception")));
					return true;
				}
			}
			return false;
		}

		public static bool IsSevereException(this Exception ex)
		{
			if (ex as AccessViolationException != null || ex as StackOverflowException != null || ex as OutOfMemoryException != null)
			{
				TraceHelper.Current.DebugMessage(string.Concat("Severe exception happened ", ex.ToString()));
				return true;
			}
			else
			{
				return false;
			}
		}

		public static void ThrowArgumentExceptionIf(string parameterName, bool condition, string message, object[] args)
		{
			if (!condition)
			{
				return;
			}
			else
			{
				string exceptionMessage = ExceptionHelpers.GetExceptionMessage(message, args);
				throw new ArgumentException(exceptionMessage, parameterName);
			}
		}

		public static void ThrowArgumentExceptionIf(string parameterName, bool condition, ExceptionHelpers.MessageLoader loader, object[] args)
		{
			if (!condition)
			{
				return;
			}
			else
			{
				string exceptionMessage = ExceptionHelpers.GetExceptionMessage(loader(), args);
				throw new ArgumentException(exceptionMessage, parameterName);
			}
		}

		public delegate string MessageLoader();
	}
}