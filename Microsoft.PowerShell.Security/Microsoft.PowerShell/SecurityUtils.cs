using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.PowerShell
{
	internal static class SecurityUtils
	{
		internal static ErrorRecord CreateFileNotFoundErrorRecord(string resourceStr, string errorId, object[] args)
		{
			string str = StringUtil.Format(resourceStr, args);
			FileNotFoundException fileNotFoundException = new FileNotFoundException(str);
			ErrorRecord errorRecord = new ErrorRecord(fileNotFoundException, errorId, ErrorCategory.ObjectNotFound, null);
			return errorRecord;
		}

		internal static ErrorRecord CreateInvalidArgumentErrorRecord(Exception e, string errorId)
		{
			ErrorRecord errorRecord = new ErrorRecord(e, errorId, ErrorCategory.InvalidArgument, null);
			return errorRecord;
		}

		internal static ErrorRecord CreateNotSupportedErrorRecord(string resourceStr, string errorId, object[] args)
		{
			string str = StringUtil.Format(resourceStr, args);
			NotSupportedException notSupportedException = new NotSupportedException(str);
			ErrorRecord errorRecord = new ErrorRecord(notSupportedException, errorId, ErrorCategory.NotImplemented, null);
			return errorRecord;
		}

		internal static ErrorRecord CreatePathNotFoundErrorRecord(string path, string errorId)
		{
			ItemNotFoundException itemNotFoundException = new ItemNotFoundException(path, "PathNotFound", SessionStateStrings.PathNotFound);
			ErrorRecord errorRecord = new ErrorRecord(itemNotFoundException, errorId, ErrorCategory.ObjectNotFound, null);
			return errorRecord;
		}

		internal static string GetFilePathOfExistingFile(PSCmdlet cmdlet, string path)
		{
			string unresolvedProviderPathFromPSPath = cmdlet.SessionState.Path.GetUnresolvedProviderPathFromPSPath(path);
			if (!File.Exists(unresolvedProviderPathFromPSPath))
			{
				return null;
			}
			else
			{
				return unresolvedProviderPathFromPSPath;
			}
		}

		internal static long GetFileSize(string filePath)
		{
			long length;
			using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
			{
				length = fileStream.Length;
			}
			return length;
		}

		[ArchitectureSensitive]
		internal static string GetStringFromSecureString(SecureString ss)
		{
			IntPtr globalAllocUnicode = Marshal.SecureStringToGlobalAllocUnicode(ss);
			string stringUni = Marshal.PtrToStringUni(globalAllocUnicode);
			Marshal.ZeroFreeGlobalAllocUnicode(globalAllocUnicode);
			return stringUni;
		}

		internal static SecureString PromptForSecureString(PSHostUserInterface hostUI, string prompt)
		{
			hostUI.Write(prompt);
			SecureString secureString = hostUI.ReadLineAsSecureString();
			hostUI.WriteLine("");
			return secureString;
		}
	}
}