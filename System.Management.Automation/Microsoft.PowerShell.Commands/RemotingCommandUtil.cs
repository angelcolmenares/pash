namespace Microsoft.PowerShell.Commands
{
    using Microsoft.Win32;
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Security;

    internal static class RemotingCommandUtil
    {
        internal static bool? isWinPEHost = null;
        internal static string WinPEIdentificationRegKey = @"System\CurrentControlSet\Control\MiniNT";

        internal static void CheckHostRemotingPrerequisites()
        {
            if (IsWinPEHost())
            {
                ErrorRecord record = new ErrorRecord(new InvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.WinPERemotingNotSupported, new object[0])), null, ErrorCategory.InvalidOperation, null);
                throw new InvalidOperationException(record.ToString());
            }
        }

        internal static void CheckRemotingCmdletPrerequisites ()
		{
			bool flag = true;
			if (OSHelper.IsUnix) {
			} else { 
				string name = @"Software\Microsoft\Windows\CurrentVersion\WSMAN\";
				CheckHostRemotingPrerequisites ();
				try {
					RegistryKey key = Registry.LocalMachine.OpenSubKey (name);
					if (key != null) {
						string str2 = (string)key.GetValue ("StackVersion", string.Empty);
						Version version = new Version (str2.Trim ());
						if (version >= new Version (2, 0)) {
							flag = false;
						}
					}
				} catch (FormatException) {
					flag = true;
				} catch (OverflowException) {
					flag = true;
				} catch (ArgumentException) {
					flag = true;
				} catch (SecurityException) {
					flag = true;
				} catch (ObjectDisposedException) {
					flag = true;
				}
				if (flag) {
					throw new InvalidOperationException ("Unix PowerShell remoting features are not enabled or not supported on this machine.\nThis may be because you do not have the correct version of WS-Management installed or this version of Windows does not support remoting currently.\n For more information, type 'get-help about_remote_requirements'.");
				}
			}
        }

        internal static bool ExceedMaximumAllowableRunspaces(PSSession[] runspaceInfos)
        {
            if (runspaceInfos == null)
            {
                throw PSTraceSource.NewArgumentNullException("runspaceInfos");
            }
            if (runspaceInfos.GetLength(0) == 0)
            {
                throw PSTraceSource.NewArgumentException("runspaceInfos");
            }
            return false;
        }

        internal static bool HasRepeatingRunspaces(PSSession[] runspaceInfos)
        {
            if (runspaceInfos == null)
            {
                throw PSTraceSource.NewArgumentNullException("runspaceInfos");
            }
            if (runspaceInfos.GetLength(0) == 0)
            {
                throw PSTraceSource.NewArgumentException("runspaceInfos");
            }
            for (int i = 0; i < runspaceInfos.GetLength(0); i++)
            {
                for (int j = 0; j < runspaceInfos.GetLength(0); j++)
                {
                    if ((i != j) && (runspaceInfos[i].Runspace.InstanceId == runspaceInfos[j].Runspace.InstanceId))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static bool IsWinPEHost()
        {
            RegistryKey key = null;
			if (OSHelper.IsUnix) {
				return false;
			}
            else if (!isWinPEHost.HasValue)
            {
                try
                {
                    key = Registry.LocalMachine.OpenSubKey(WinPEIdentificationRegKey);
                    if (key != null)
                    {
                        isWinPEHost = true;
                    }
                    else
                    {
                        isWinPEHost = false;
                    }
                }
                catch (ArgumentException)
                {
                }
                catch (SecurityException)
                {
                }
                catch (ObjectDisposedException)
                {
                }
                finally
                {
                    if (key != null)
                    {
                        key.Close();
                    }
                }
            }
            if (isWinPEHost != true)
            {
                return false;
            }
            return true;
        }
    }
}

