namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands;
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Security;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;

    internal static class Utils
    {
        internal static string DefaultPowerShellShellID = "Microsoft.PowerShell";
        internal static char[] DirectorySeparators = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        internal static string ModuleDirectory = "Modules";
        internal static string ProductNameForDirectory = "WindowsPowerShell";
        internal const string WorkflowModule = "PSWorkflow";
        internal const string WorkflowType = "Microsoft.PowerShell.Workflow.AstToWorkflowConverter, Microsoft.PowerShell.Activities, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

        internal static void CheckArgForNull(object arg, string argName)
        {
            if (arg == null)
            {
                throw PSTraceSource.NewArgumentNullException(argName);
            }
        }

        internal static void CheckArgForNullOrEmpty(string arg, string argName)
        {
            if (arg == null)
            {
                throw PSTraceSource.NewArgumentNullException(argName);
            }
            if (arg.Length == 0)
            {
                throw PSTraceSource.NewArgumentException(argName);
            }
        }

        internal static void CheckKeyArg(byte[] arg, string argName)
        {
            if (arg == null)
            {
                throw PSTraceSource.NewArgumentNullException(argName);
            }
            if (((arg.Length != 0x10) && (arg.Length != 0x18)) && (arg.Length != 0x20))
            {
                throw PSTraceSource.NewArgumentException(argName, "Serialization", "InvalidKeyLength", new object[] { argName });
            }
        }

        internal static void CheckSecureStringArg(SecureString arg, string argName)
        {
            if (arg == null)
            {
                throw PSTraceSource.NewArgumentNullException(argName);
            }
            if (arg.Length == 0)
            {
                throw PSTraceSource.NewArgumentException(argName);
            }
        }

        internal static string GetApplicationBase(string shellId)
        {
			return PowerShellConfiguration.PowerShellEngine.ApplicationBase;
			/*
            string name = @"Software\Microsoft\PowerShell\" + PSVersionInfo.RegistryVersionKey + @"\PowerShellEngine";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(name))
            {
                if (key != null)
                {
                    return (key.GetValue("ApplicationBase") as string);
                }
            }
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                return Path.GetDirectoryName(entryAssembly.Location);
            }
            entryAssembly = Assembly.GetAssembly(typeof(PSObject));
            if (entryAssembly != null)
            {
                return Path.GetDirectoryName(entryAssembly.Location);
            }
            return "";
            */
        }

        internal static IAstToWorkflowConverter GetAstToWorkflowConverterAndEnsureWorkflowModuleLoaded(ExecutionContext context)
        {
            IAstToWorkflowConverter converter = null;
            Type type = null;
            if (IsRunningFromSysWOW64())
            {
                throw new NotSupportedException(AutomationExceptions.WorkflowDoesNotSupportWOW64);
            }
            if (((context != null) && (context.LanguageMode == PSLanguageMode.ConstrainedLanguage)) && (SystemPolicy.GetSystemLockdownPolicy() != SystemEnforcementMode.Enforce))
            {
                throw new NotSupportedException(Modules.CannotDefineWorkflowInconsistentLanguageMode);
            }
            if ((context != null) && !context.PSWorkflowModuleLoadingInProgress)
            {
                context.PSWorkflowModuleLoadingInProgress = true;
                List<PSModuleInfo> modules = context.Modules.GetModules(new string[] { "PSWorkflow" }, false);
                if ((modules == null) || (modules.Count == 0))
                {
                    CommandInfo commandInfo = new CmdletInfo("Import-Module", typeof(ImportModuleCommand), null, null, context);
                    Command command = new Command(commandInfo);
                    try
                    {
                        PowerShell.Create(RunspaceMode.CurrentRunspace).AddCommand(command).AddParameter("Name", "PSWorkflow").AddParameter("Scope", "GLOBAL").AddParameter("ErrorAction", ActionPreference.Ignore).AddParameter("PassThru").AddParameter("WarningAction", ActionPreference.Ignore).AddParameter("Verbose", false).AddParameter("Debug", false).Invoke<PSModuleInfo>();
                    }
                    catch (Exception exception)
                    {
                        CommandProcessorBase.CheckForSevereException(exception);
                    }
                }
            }
            type = Type.GetType("Microsoft.PowerShell.Workflow.AstToWorkflowConverter, Microsoft.PowerShell.Activities, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            if (type != null)
            {
                converter = (IAstToWorkflowConverter) type.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
            }
            if (converter == null)
            {
                throw new NotSupportedException(StringUtil.Format(AutomationExceptions.CantLoadWorkflowType, "Microsoft.PowerShell.Workflow.AstToWorkflowConverter, Microsoft.PowerShell.Activities, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "PSWorkflow"));
            }
            return converter;
        }

        internal static string GetCurrentMajorVersion()
        {
            return PSVersionInfo.PSVersion.Major.ToString(CultureInfo.InvariantCulture);
        }

        internal static string GetRegistryConfigurationPath(string shellID)
        {
            return (GetRegistryConfigurationPrefix() + @"\" + shellID);
        }

        internal static string GetRegistryConfigurationPrefix()
        {
            return (@"SOFTWARE\Xamarin\PowerShell\" + PSVersionInfo.RegistryVersion1Key + @"\ShellIds");
        }

        [ArchitectureSensitive]
        internal static string GetStringFromSecureString(SecureString ss)
        {
            IntPtr ptr = Marshal.SecureStringToGlobalAllocUnicode(ss);
            string str = Marshal.PtrToStringUni(ptr);
            Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            return str;
        }

        internal static TypeTable GetTypeTableFromExecutionContextTLS()
        {
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            if (executionContextFromTLS == null)
            {
                return null;
            }
            return executionContextFromTLS.TypeTable;
        }

        internal static bool IsNetFrameworkVersionSupported(Version checkVersion, out bool higherThanKnownHighestVersion)
        {
            higherThanKnownHighestVersion = false;
            bool flag = false;
            if (checkVersion == null)
            {
                return false;
            }
            Version key = new Version(checkVersion.Major, checkVersion.Minor, 0, 0);
            if (checkVersion > PsUtils.FrameworkRegistryInstallation.KnownHighestNetFrameworkVersion)
            {
                flag = true;
                higherThanKnownHighestVersion = true;
                return flag;
            }
            if (PsUtils.FrameworkRegistryInstallation.CompatibleNetFrameworkVersions.ContainsKey(key))
            {
                if (PsUtils.FrameworkRegistryInstallation.IsFrameworkInstalled(key.Major, key.Minor, 0))
                {
                    return true;
                }
                HashSet<Version> set = PsUtils.FrameworkRegistryInstallation.CompatibleNetFrameworkVersions[key];
                foreach (Version version2 in set)
                {
                    if (PsUtils.FrameworkRegistryInstallation.IsFrameworkInstalled(version2.Major, version2.Minor, 0))
                    {
                        return true;
                    }
                }
            }
            return flag;
        }

        internal static bool IsPSVersionSupported(string ver)
        {
            return IsPSVersionSupported(StringToVersion(ver));
        }

        internal static bool IsPSVersionSupported(Version checkVersion)
        {
            if (checkVersion != null)
            {
                foreach (Version version in PSVersionInfo.PSCompatibleVersions)
                {
                    if ((checkVersion.Major == version.Major) && (checkVersion.Minor <= version.Minor))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static bool IsRunningFromSysWOW64()
        {
            return GetApplicationBase(DefaultPowerShellShellID).Contains("SysWOW64");
        }

        internal static Version StringToVersion(string versionString)
        {
            Version version = null;
            if (string.IsNullOrEmpty(versionString))
            {
                return null;
            }
            int num = 0;
            foreach (char ch in versionString)
            {
                if (ch == '.')
                {
                    num++;
                    if (num > 1)
                    {
                        return null;
                    }
                }
            }
            if (num == 0)
            {
                versionString = versionString + ".0";
            }
            try
            {
                version = new Version(versionString);
            }
            catch (ArgumentException)
            {
            }
            catch (FormatException)
            {
            }
            catch (OverflowException)
            {
            }
            return version;
        }
    }
}

