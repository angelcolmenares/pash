using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class ADPasswordUtil
	{
		private CommandInvocationIntrinsics _invokeCommand;

		internal ADPasswordUtil(CommandInvocationIntrinsics invokeCommand)
		{
			this._invokeCommand = invokeCommand;
		}

		internal void ChangePassword(string partitionDN, ADObject directoryObj, SecureString oldPassword, SecureString newPassword)
		{
			if (oldPassword == null)
			{
				oldPassword = this.PromptOldPassword(directoryObj);
			}
			if (oldPassword != null)
			{
				if (newPassword == null)
				{
					newPassword = this.PromptNewPassword(directoryObj);
				}
				if (newPassword != null)
				{
					ADPasswordUtil.PerformChangePassword(partitionDN, directoryObj, oldPassword, newPassword);
					return;
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}

		private void ConsoleWrite(string output, bool noNewLine)
		{
			object[] objArray = new object[2];
			objArray[0] = noNewLine;
			objArray[1] = output;
			this._invokeCommand.InvokeScript("Write-Host -NoNewLine:$args[0] $args[1]", false, PipelineResultTypes.Output, null, objArray);
		}

		private void ConsoleWriteLine(string output)
		{
			this.ConsoleWrite(output, false);
		}

		private void ConsoleWritePrompt(string output)
		{
			this.ConsoleWrite(string.Concat(output, " "), true);
		}

		internal static void PerformChangePassword(string partitionDN, ADObject directoryObj, SecureString oldPassword, SecureString newPassword)
		{
			ADSessionInfo sessionInfo = directoryObj.SessionInfo;
			using (ADAccountManagement aDAccountManagement = new ADAccountManagement(sessionInfo))
			{
				IntPtr bSTR = Marshal.SecureStringToBSTR(oldPassword);
				string stringUni = Marshal.PtrToStringUni(bSTR);
				IntPtr intPtr = Marshal.SecureStringToBSTR(newPassword);
				string str = Marshal.PtrToStringUni(intPtr);
				aDAccountManagement.ChangePassword(partitionDN, directoryObj.DistinguishedName, stringUni, str);
			}
		}

		internal static void PerformSetPassword(string partitionDN, ADObject directoryObj, SecureString newPassword)
		{
			ADSessionInfo sessionInfo = directoryObj.SessionInfo;
			using (ADAccountManagement aDAccountManagement = new ADAccountManagement(sessionInfo))
			{
				IntPtr bSTR = Marshal.SecureStringToBSTR(newPassword);
				string stringUni = Marshal.PtrToStringUni(bSTR);
				aDAccountManagement.SetPassword(partitionDN, directoryObj.DistinguishedName, stringUni);
			}
		}

		private SecureString PromptNewPassword(ADObject directoryObj)
		{
			SecureString secureString;
			while (true)
			{
				this.ConsoleWritePrompt(string.Format(StringResources.PromptForNewPassword, directoryObj.DistinguishedName));
				secureString = this.PromptPassword();
				this.ConsoleWritePrompt(StringResources.PromptForRepeatPassword);
				SecureString secureString1 = this.PromptPassword();
				if (ADPasswordUtils.MatchPassword(secureString, secureString1))
				{
					break;
				}
				this.ConsoleWriteLine(StringResources.PasswordsDidNotMatch);
			}
			return secureString;
		}

		internal SecureString PromptOldPassword(ADObject directoryObj)
		{
			this.ConsoleWritePrompt(string.Format(StringResources.PromptForCurrentPassword, directoryObj.DistinguishedName));
			SecureString secureString = this.PromptPassword();
			return secureString;
		}

		private SecureString PromptPassword()
		{
			Collection<PSObject> pSObjects = this._invokeCommand.InvokeScript("Read-Host -AsSecureString");
			if (pSObjects.Count != 0)
			{
				return pSObjects[0].BaseObject as SecureString;
			}
			else
			{
				return null;
			}
		}

		internal void SetPassword(string partitionDN, ADObject directoryObj, SecureString newPassword)
		{
			if (newPassword == null)
			{
				newPassword = this.PromptNewPassword(directoryObj);
			}
			if (newPassword != null)
			{
				ADPasswordUtil.PerformSetPassword(partitionDN, directoryObj, newPassword);
				return;
			}
			else
			{
				return;
			}
		}
	}
}