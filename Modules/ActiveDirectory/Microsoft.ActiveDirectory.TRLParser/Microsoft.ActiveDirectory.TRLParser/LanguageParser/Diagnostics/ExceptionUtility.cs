using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser.Diagnostics
{
	internal static class ExceptionUtility
	{
		public static bool IsFatal(Exception exception)
		{
			Exception innerException = exception;
			while (innerException != null)
			{
				if ((innerException as OutOfMemoryException == null || innerException as InsufficientMemoryException != null) && innerException as ThreadAbortException == null && innerException as AccessViolationException == null && innerException as SEHException == null)
				{
					if (innerException as TypeInitializationException == null && innerException as TargetInvocationException == null)
					{
						break;
					}
					innerException = innerException.InnerException;
				}
				else
				{
					return true;
				}
			}
			return false;
		}
	}
}