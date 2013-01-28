namespace System.Management.Automation
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class WindowsErrorReporting
    {
        private static string applicationName = "GetMainModuleError";
        private static string applicationPath = "GetMainModuleError";
        private static Process currentProcess = null;
        private static IntPtr hCurrentProcess = IntPtr.Zero;
        private static IntPtr hwndMainWindow = IntPtr.Zero;
        private static bool? isWindowsErrorReportingAvailable;
        private static string nameOfExe = "GetMainModuleError";
        private const string powerShellEventType = "PowerShell";
        private static readonly string[] powerShellModulesWithGlobalMembers = new string[] { "powershell.exe", "powershell_ise.exe", "pspluginwkr-v3.dll", "pwrshplugin.dll", "pwrshsip.dll", "pshmsglh.dll", "PSEvents.dll" };
        private static readonly string[] powerShellModulesWithoutGlobalMembers = new string[] { "Microsoft.PowerShell.Commands.Diagnostics.dll", "Microsoft.PowerShell.Commands.Management.dll", "Microsoft.PowerShell.Commands.Utility.dll", "Microsoft.PowerShell.Security.dll", "System.Management.Automation.dll", "Microsoft.PowerShell.ConsoleHost.dll", "Microsoft.PowerShell.Editor.dll", "Microsoft.PowerShell.GPowerShell.dll", "Microsoft.PowerShell.GraphicalHost.dll" };
        private static bool registered = false;
        private static readonly object registrationLock = new object();
        private static readonly object reportCreationLock = new object();
        private static bool unattendedServerMode = false;
        private static string versionOfPowerShellLibraries = string.Empty;

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exceptionObject = e.ExceptionObject as Exception;
            if (exceptionObject != null)
            {
                SubmitReport(exceptionObject);
            }
        }

        internal static void FailFast(Exception exception)
        {
            try
            {
                if (registered && (exception != null))
                {
                    SubmitReport(exception);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                Environment.FailFast((exception != null) ? exception.Message : string.Empty);
            }
        }

        private static void FindStaticInformation()
        {
            versionOfPowerShellLibraries = FileVersionInfo.GetVersionInfo(typeof(PSObject).Assembly.Location).FileVersion;
            currentProcess = Process.GetCurrentProcess();
            ProcessModule mainModule = PsUtils.GetMainModule(currentProcess);
            if (mainModule != null)
            {
                applicationPath = mainModule.FileName;
            }
            nameOfExe = Path.GetFileName(applicationPath);
            hCurrentProcess = currentProcess.Handle;
            hwndMainWindow = currentProcess.MainWindowHandle;
            applicationName = currentProcess.ProcessName;
        }

        private static string GetDeepestFrame(Exception exception, int maxLength)
        {
            StackTrace trace = new StackTrace(exception);
            return StackFrame2BucketParameter(trace.GetFrame(0), maxLength);
        }

        private static string GetDeepestPowerShellFrame(Exception exception, int maxLength)
        {
            StackTrace trace = new StackTrace(exception);
            foreach (StackFrame frame in trace.GetFrames())
            {
                MethodBase method = frame.GetMethod();
                if (method != null)
                {
                    Module module = method.Module;
                    if (module != null)
                    {
                        Type declaringType = method.DeclaringType;
                        if (IsPowerShellModule(module.Name, declaringType == null))
                        {
                            return StackFrame2BucketParameter(frame, maxLength);
                        }
                    }
                }
            }
            return string.Empty;
        }

        private static string GetThreadName()
        {
            string name = Thread.CurrentThread.Name;
            if (name == null)
            {
                name = string.Empty;
            }
            return name;
        }

        private static void HandleHResult(int hresult)
        {
            Marshal.ThrowExceptionForHR(hresult);
        }

        private static bool IsPowerShellModule(string moduleName, bool globalMember)
        {
            foreach (string str in powerShellModulesWithGlobalMembers)
            {
                if (moduleName.Equals(str, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            if (!globalMember)
            {
                foreach (string str2 in powerShellModulesWithoutGlobalMembers)
                {
                    if (moduleName.Equals(str2, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool IsWindowsErrorReportingAvailable()
        {
            if (!isWindowsErrorReportingAvailable.HasValue)
            {
                isWindowsErrorReportingAvailable = new bool?(Environment.OSVersion.Version.Major >= 6);
            }
            return isWindowsErrorReportingAvailable.Value;
        }

        internal static void RegisterWindowsErrorReporting(bool unattendedServer)
        {
            lock (registrationLock)
            {
                if (!registered && IsWindowsErrorReportingAvailable())
                {
                    try
                    {
                        FindStaticInformation();
                    }
                    catch (Exception exception)
                    {
                        CommandProcessorBase.CheckForSevereException(exception);
                    }
                    try
                    {
                        unattendedServerMode = unattendedServer;
                        if (unattendedServer)
                        {
                            HandleHResult(NativeMethods.WerSetFlags(ReportingFlags.Queue));
                        }
                        else
                        {
                            HandleHResult(NativeMethods.WerSetFlags((ReportingFlags) 0));
                        }
                        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(WindowsErrorReporting.CurrentDomain_UnhandledException);
                        registered = true;
                    }
                    catch (Exception exception2)
                    {
                        CommandProcessorBase.CheckForSevereException(exception2);
                    }
                }
            }
        }

        private static void SetBucketParameter(ReportHandle reportHandle, BucketParameterId bucketParameterId, string value)
        {
            HandleHResult(NativeMethods.WerReportSetParameter(reportHandle, bucketParameterId, bucketParameterId.ToString(), value));
        }

        private static void SetBucketParameters(ReportHandle reportHandle, Exception uncaughtException)
        {
            Exception innerException = uncaughtException;
            while (innerException.InnerException != null)
            {
                innerException = innerException.InnerException;
            }
            SetBucketParameter(reportHandle, BucketParameterId.NameOfExe, TruncateExeName(nameOfExe, 20));
            SetBucketParameter(reportHandle, BucketParameterId.FileVersionOfSystemManagementAutomation, TruncateBucketParameter(versionOfPowerShellLibraries, 0x10));
            SetBucketParameter(reportHandle, BucketParameterId.InnermostExceptionType, TruncateExceptionType(innerException.GetType().FullName, 40));
            SetBucketParameter(reportHandle, BucketParameterId.OutermostExceptionType, TruncateExceptionType(uncaughtException.GetType().FullName, 40));
            SetBucketParameter(reportHandle, BucketParameterId.DeepestFrame, GetDeepestFrame(uncaughtException, 50));
            SetBucketParameter(reportHandle, BucketParameterId.DeepestPowerShellFrame, GetDeepestPowerShellFrame(uncaughtException, 50));
            SetBucketParameter(reportHandle, BucketParameterId.ThreadName, TruncateBucketParameter(GetThreadName(), 20));
        }

        private static string StackFrame2BucketParameter(StackFrame frame, int maxLength)
        {
            MethodBase method = frame.GetMethod();
            if (method == null)
            {
                return string.Empty;
            }
            Type declaringType = method.DeclaringType;
            if (declaringType == null)
            {
                return TruncateBucketParameter(method.Name, maxLength);
            }
            string fullName = declaringType.FullName;
            string str3 = "." + method.Name;
            if (maxLength > str3.Length)
            {
                fullName = TruncateTypeName(fullName, maxLength - str3.Length);
            }
            else
            {
                fullName = TruncateTypeName(fullName, 1);
            }
            return TruncateBucketParameter(fullName + str3, maxLength);
        }

        private static void SubmitReport(Exception uncaughtException)
        {
            lock (reportCreationLock)
            {
                ReportInformation information = null;
                ReportHandle handle;
                if (uncaughtException == null)
                {
                    throw new ArgumentNullException("uncaughtException");
                }
                information = new ReportInformation {
                    dwSize = Marshal.SizeOf(information),
                    hProcess = hCurrentProcess,
                    hwndParent = hwndMainWindow,
                    wzApplicationName = applicationName,
                    wzApplicationPath = applicationPath,
                    wzConsentKey = null,
                    wzDescription = null,
                    wzFriendlyEventName = null
                };
                HandleHResult(NativeMethods.WerReportCreate("PowerShell", ReportType.WerReportCritical, information, out handle));
                using (handle)
                {
                    SetBucketParameters(handle, uncaughtException);
                    HandleHResult(NativeMethods.WerReportAddDump(handle, hCurrentProcess, IntPtr.Zero, DumpType.MiniDump, IntPtr.Zero, IntPtr.Zero, 0));
                    SubmitResult reportFailed = SubmitResult.ReportFailed;
                    SubmitFlags flags = SubmitFlags.HonorRecovery | SubmitFlags.HonorRestart | SubmitFlags.AddRegisteredData | SubmitFlags.OutOfProcess;
                    if (unattendedServerMode)
                    {
                        flags |= SubmitFlags.Queue;
                    }
                    HandleHResult(NativeMethods.WerReportSubmit(handle, Consent.NotAsked, flags, out reportFailed));
                    Environment.Exit((int) reportFailed);
                }
            }
        }

        private static string TruncateBucketParameter(string message, int maxLength)
        {
            if (message == null)
            {
                return string.Empty;
            }
            int length = (maxLength * 30) / 100;
            if (message.Length > maxLength)
            {
                int num2 = (maxLength - length) - "..".Length;
                message = message.Substring(0, length) + ".." + message.Substring(message.Length - num2, num2);
            }
            return message;
        }

        private static string TruncateExceptionType(string exceptionType, int maxLength)
        {
            if ((exceptionType.Length > maxLength) && exceptionType.EndsWith("Exception", StringComparison.OrdinalIgnoreCase))
            {
                exceptionType = exceptionType.Substring(0, exceptionType.Length - "Exception".Length);
            }
            if (exceptionType.Length > maxLength)
            {
                exceptionType = TruncateTypeName(exceptionType, maxLength);
            }
            return TruncateBucketParameter(exceptionType, maxLength);
        }

        private static string TruncateExeName(string nameOfExe, int maxLength)
        {
            nameOfExe = nameOfExe.Trim();
            if ((nameOfExe.Length > maxLength) && nameOfExe.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                nameOfExe = nameOfExe.Substring(0, nameOfExe.Length - ".exe".Length);
            }
            return TruncateBucketParameter(nameOfExe, maxLength);
        }

        private static string TruncateTypeName(string typeName, int maxLength)
        {
            if (typeName.Length > maxLength)
            {
                typeName = typeName.Substring(typeName.Length - maxLength, maxLength);
            }
            return typeName;
        }

        internal static void WaitForPendingReports()
        {
            lock (reportCreationLock)
            {
            }
        }

        internal static void WriteMiniDump(string file)
        {
            WriteMiniDump(file, MiniDumpType.MiniDumpNormal);
        }

        internal static void WriteMiniDump(string file, MiniDumpType dumpType)
        {
            Process currentProcess = Process.GetCurrentProcess();
            using (FileStream stream = new FileStream(file, FileMode.Create))
            {
                NativeMethods.MiniDumpWriteDump(currentProcess.Handle, currentProcess.Id, stream.SafeFileHandle, dumpType, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            }
        }

        private enum BucketParameterId : int
        {
            DeepestFrame = 5,
            DeepestPowerShellFrame = 4,
            FileVersionOfSystemManagementAutomation = 1,
            InnermostExceptionType = 2,
            NameOfExe = 0,
            OutermostExceptionType = 3,
            Param7 = 7,
            Param8 = 8,
            Param9 = 9,
            ThreadName = 6
        }

        private enum Consent : int
        {
            AlwaysPrompt = 4,
            Approved = 2,
            Denied = 3,
            NotAsked = 1
        }

        [Flags]
        private enum DumpFlags : int
        {
            NoHeap_OnQueue = 1
        }

        private enum DumpType : int
        {
            HeapDump = 3,
            MicroDump = 1,
            MiniDump = 2
        }

        [Flags]
        internal enum MiniDumpType : int
        {
            MiniDumpFilterMemory = 8,
            MiniDumpFilterModulePaths = 0x80,
            MiniDumpNormal = 0,
            MiniDumpScanMemory = 0x10,
            MiniDumpWithCodeSegs = 0x2000,
            MiniDumpWithDataSegs = 1,
            MiniDumpWithFullMemory = 2,
            MiniDumpWithFullMemoryInfo = 0x800,
            MiniDumpWithHandleData = 4,
            MiniDumpWithIndirectlyReferencedMemory = 0x40,
            MiniDumpWithoutOptionalData = 0x400,
            MiniDumpWithPrivateReadWriteMemory = 0x200,
            MiniDumpWithProcessThreadData = 0x100,
            MiniDumpWithThreadInfo = 0x1000,
            MiniDumpWithUnloadedModules = 0x20
        }

        private static class NativeMethods
        {
            internal const string WerDll = "wer.dll";

            [DllImport("DbgHelp.dll", SetLastError=true)]
            internal static extern bool MiniDumpWriteDump(IntPtr hProcess, int processId, SafeFileHandle hFile, WindowsErrorReporting.MiniDumpType dumpType, IntPtr exceptionParam, IntPtr userStreamParam, IntPtr callackParam);
            [DllImport("wer.dll", CharSet=CharSet.Unicode)]
            internal static extern int WerReportAddDump(WindowsErrorReporting.ReportHandle reportHandle, IntPtr hProcess, IntPtr hThread, WindowsErrorReporting.DumpType dumpType, IntPtr pExceptionParam, IntPtr dumpCustomOptions, WindowsErrorReporting.DumpFlags dumpFlags);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("wer.dll")]
            internal static extern int WerReportCloseHandle(IntPtr reportHandle);
            [DllImport("wer.dll", CharSet=CharSet.Unicode)]
            internal static extern int WerReportCreate([MarshalAs(UnmanagedType.LPWStr)] string pwzEventType, WindowsErrorReporting.ReportType repType, [MarshalAs(UnmanagedType.LPStruct)] WindowsErrorReporting.ReportInformation reportInformation, out WindowsErrorReporting.ReportHandle reportHandle);
            [DllImport("wer.dll", CharSet=CharSet.Unicode)]
            internal static extern int WerReportSetParameter(WindowsErrorReporting.ReportHandle reportHandle, WindowsErrorReporting.BucketParameterId bucketParameterId, [MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.LPWStr)] string value);
            [DllImport("wer.dll")]
            internal static extern int WerReportSubmit(WindowsErrorReporting.ReportHandle reportHandle, WindowsErrorReporting.Consent consent, WindowsErrorReporting.SubmitFlags flags, out WindowsErrorReporting.SubmitResult result);
            [DllImport("kernel32.dll")]
            internal static extern int WerSetFlags(WindowsErrorReporting.ReportingFlags flags);
        }

        private class ReportHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private ReportHandle() : base(true)
            {
            }

            protected override bool ReleaseHandle()
            {
                return (0 == WindowsErrorReporting.NativeMethods.WerReportCloseHandle(base.handle));
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        private class ReportInformation
        {
            private const int MAX_PATH = 260;
            internal int dwSize;
            internal IntPtr hProcess;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x40)]
            internal string wzConsentKey;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x80)]
            internal string wzFriendlyEventName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x80)]
            internal string wzApplicationName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
            internal string wzApplicationPath;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x200)]
            internal string wzDescription;
            internal IntPtr hwndParent;
        }

        private enum ReportingFlags : int
        {
            DisableThreadSuspension = 4,
            NoHeap = 1,
            Queue = 2,
            QueueUpload = 8
        }

        private enum ReportType : int
        {
            WerReportApplicationCrash = 2,
            WerReportApplicationHang = 3,
            WerReportCritical = 1,
            WerReportInvalid = 5,
            WerReportKernel = 4,
            WerReportNonCritical = 0
        }

        [Flags]
        private enum SubmitFlags : int
        {
            AddRegisteredData = 0x10,
            ArchiveParametersOnly = 0x1000,
            BypassDataThrottling = 0x800,
            HonorRecovery = 1,
            HonorRestart = 2,
            NoArchive = 0x100,
            NoCloseUI = 0x40,
            NoQueue = 0x80,
            OutOfProcesAsync = 0x400,
            OutOfProcess = 0x20,
            Queue = 4,
            ShowDebug = 8,
            StartMinimized = 0x200
        }

        private enum SubmitResult : int
        {
            CustomAction = 9,
            Disabled = 5,
            DisabledQueue = 7,
            ReportAsync = 8,
            ReportCancelled = 6,
            ReportDebug = 3,
            ReportFailed = 4,
            ReportQueued = 1,
            ReportUploaded = 2
        }
    }
}

