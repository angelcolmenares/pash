using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Management.PowerShellWebAccess
{
	internal class PswaAuthorizationRuleCommandHelper
	{
		private PswaAuthorizationRuleCommandHelper()
		{
		}

		internal static SortedList<int, PswaAuthorizationRule> LoadFromFile(Cmdlet cmdlet, string operationName)
		{
			SortedList<int, PswaAuthorizationRule> nums;
			bool flag;
			SortedList<int, PswaAuthorizationRule> nums1;
			try
			{
				ArrayList arrayLists = new ArrayList();
				SortedList<int, PswaAuthorizationRule> nums2 = PswaAuthorizationRuleManager.Instance.LoadFromFile(arrayLists);
				if (!PswaAuthorizationRuleCommandHelper.WriteLoadError(cmdlet, arrayLists, string.Concat(operationName, "RuleError")))
				{
					flag = false;
				}
				else
				{
					flag = nums2 != null;
				}
				bool flag1 = flag;
				if (flag1)
				{
					nums1 = nums2;
				}
				else
				{
					nums1 = null;
				}
				nums = nums1;
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				cmdlet.WriteError(new ErrorRecord(exception, string.Concat(operationName, "RuleError"), ErrorCategory.InvalidOperation, null));
				nums = null;
			}
			return nums;
		}

		private static bool WriteLoadError(Cmdlet cmdlet, ArrayList loadError, string errorId)
		{
			bool flag;
			IEnumerator enumerator = loadError.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					DataFileLoadError current = (DataFileLoadError)enumerator.Current;
					if (current.status != DataFileLoadError.ErrorStatus.Warning)
					{
						if (current.status != DataFileLoadError.ErrorStatus.Error)
						{
							continue;
						}
						cmdlet.WriteError(new ErrorRecord(current.exception, errorId, ErrorCategory.InvalidOperation, null));
						flag = false;
						return flag;
					}
					else
					{
						cmdlet.WriteWarning(string.Concat(current.message, "\n\n"));
					}
				}
				return true;
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			return flag;
		}
	}
}