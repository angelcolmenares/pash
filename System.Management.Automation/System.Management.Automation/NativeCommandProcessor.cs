namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Management;
    using System.Management.Automation.Internal;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class NativeCommandProcessor : CommandProcessorBase
    {
        private static bool _isServerSide;
        private bool _runStandAlone;
        private ApplicationInfo applicationInfo;
        private ProcessInputWriter inputWriter;
        private bool isMiniShell;
        private bool isPreparedCalled;
        private const int MaxExecutablePath = 0x400;
        private NativeCommandParameterBinderController nativeParameterBinderController;
        private Process nativeProcess;
        private ProcessOutputReader outputReader;
        private const int SCS_32BIT_BINARY = 0;
        private const int SCS_64BIT_BINARY = 6;
        private const int SCS_DOS_BINARY = 1;
        private const int SCS_OS216_BINARY = 5;
        private const int SCS_PIF_BINARY = 3;
        private const int SCS_POSIX_BINARY = 4;
        private const int SCS_WOW_BINARY = 2;
        private const int SHGFI_EXETYPE = 0x2000;
        private bool stopped;
        private object sync;

        internal NativeCommandProcessor(ApplicationInfo applicationInfo, ExecutionContext context) : base(applicationInfo)
        {
            this.sync = new object();
            if (applicationInfo == null)
            {
                throw PSTraceSource.NewArgumentNullException("applicationInfo");
            }
            this.applicationInfo = applicationInfo;
            base._context = context;
            base.Command = new NativeCommand();
            base.Command.CommandInfo = applicationInfo;
            base.Command.Context = context;
            base.Command.commandRuntime = base.commandRuntime = new MshCommandRuntime(context, applicationInfo, base.Command);
            base.CommandScope = context.EngineSessionState.CurrentScope;
            ((NativeCommand) base.Command).MyCommandProcessor = this;
            this.inputWriter = new ProcessInputWriter(base.Command);
        }

        private void CalculateIORedirection(out bool redirectOutput, out bool redirectError, out bool redirectInput)
        {
            redirectInput = true;
            redirectOutput = true;
            redirectError = true;
            if (base.Command.MyInvocation.PipelinePosition == base.Command.MyInvocation.PipelineLength)
            {
                if (base._context.IsTopLevelPipe(base.commandRuntime.OutputPipe))
                {
                    redirectOutput = false;
                }
                else
                {
                    CommandProcessorBase downstreamCmdlet = base.commandRuntime.OutputPipe.DownstreamCmdlet;
                    if ((downstreamCmdlet != null) && string.Equals(downstreamCmdlet.CommandInfo.Name, "Out-Default", StringComparison.OrdinalIgnoreCase))
                    {
                        redirectOutput = false;
                    }
                }
            }
            if (base.CommandRuntime.ErrorMergeTo != MshCommandRuntime.MergeDataStream.Output)
            {
                if (base._context.IsTopLevelPipe(base.commandRuntime.ErrorOutputPipe))
                {
                    redirectError = false;
                }
                else
                {
                    CommandProcessorBase base3 = base.commandRuntime.ErrorOutputPipe.DownstreamCmdlet;
                    if ((base3 != null) && string.Equals(base3.CommandInfo.Name, "Out-Default", StringComparison.OrdinalIgnoreCase))
                    {
                        redirectError = false;
                    }
                }
            }
            if ((!redirectError && redirectOutput) && this.isMiniShell)
            {
                redirectError = true;
            }
            if ((this.inputWriter.Count == 0) && !base.Command.MyInvocation.ExpectingInput)
            {
                redirectInput = false;
            }
            if (IsServerSide)
            {
                redirectInput = true;
                redirectOutput = true;
                redirectError = true;
            }
            else if (IsConsoleApplication(this.Path))
            {
                ConsoleVisibility.AllocateHiddenConsole();
                if (ConsoleVisibility.AlwaysCaptureApplicationIO)
                {
                    redirectOutput = true;
                    redirectError = true;
                }
            }
            if (!redirectInput && !redirectOutput)
            {
                this._runStandAlone = true;
            }
        }

        private void CleanUp()
        {
            try
            {
                if (this.nativeProcess != null)
                {
                    this.nativeProcess.Close();
                }
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
        }

        internal override void Complete()
        {
            bool flag2;
            bool flag3;
            bool flag4;
            if (base.Context._debuggingMode > 0)
            {
                base.Context.Debugger.CheckCommand(base.Command.MyInvocation);
            }
            this.CalculateIORedirection(out flag2, out flag3, out flag4);
            bool soloCommand = base.Command.MyInvocation.PipelineLength == 1;
            ProcessStartInfo info = this.GetProcessStartInfo(flag2, flag3, flag4, soloCommand);
            if (base.Command.Context.CurrentPipelineStopping)
            {
                throw new PipelineStoppedException();
            }
            Exception innerException = null;
            try
            {
                bool flag;
                if (!flag2)
                {
                    base.Command.Context.EngineHostInterface.NotifyBeginApplication();
                }
                lock (this.sync)
                {
                    if (this.stopped)
                    {
                        throw new PipelineStoppedException();
                    }
                    try
                    {
                        this.nativeProcess = new Process();
                        this.nativeProcess.StartInfo = info;
                        this.nativeProcess.Start();
                    }
                    catch (Win32Exception)
                    {
                        string str = FindExecutable(info.FileName);
                        bool flag6 = true;
                        if (!string.IsNullOrEmpty(str))
                        {
                            if (IsConsoleApplication(str))
                            {
                                ConsoleVisibility.AllocateHiddenConsole();
                            }
                            string arguments = info.Arguments;
                            string fileName = info.FileName;
                            info.Arguments = "\"" + info.FileName + "\" " + info.Arguments;
                            info.FileName = str;
                            try
                            {
                                this.nativeProcess.Start();
                                flag6 = false;
                            }
                            catch (Win32Exception)
                            {
                                info.Arguments = arguments;
                                info.FileName = fileName;
                            }
                        }
                        if (flag6)
                        {
                            if (!soloCommand || info.UseShellExecute)
                            {
                                throw;
                            }
                            info.UseShellExecute = true;
                            info.RedirectStandardInput = false;
                            info.RedirectStandardOutput = false;
                            info.RedirectStandardError = false;
                            this.nativeProcess.Start();
                        }
                    }
                }
                if (base.Command.MyInvocation.PipelinePosition < base.Command.MyInvocation.PipelineLength)
                {
                    flag = false;
                }
                else
                {
                    flag = true;
                    if (!info.UseShellExecute)
                    {
                        flag = IsWindowsApplication(this.nativeProcess.StartInfo.FileName);
                    }
                }
                try
                {
                    if (info.RedirectStandardInput)
                    {
                        NativeCommandIOFormat text = NativeCommandIOFormat.Text;
                        if (this.isMiniShell)
                        {
                            text = ((MinishellParameterBinderController) this.NativeParameterBinderController).InputFormat;
                        }
                        lock (this.sync)
                        {
                            if (!this.stopped)
                            {
                                this.inputWriter.Start(this.nativeProcess, text);
                            }
                        }
                    }
                    if (!flag && (info.RedirectStandardOutput || info.RedirectStandardError))
                    {
                        lock (this.sync)
                        {
                            if (!this.stopped)
                            {
                                this.outputReader = new ProcessOutputReader(this.nativeProcess, this.Path, flag2, flag3);
                                this.outputReader.Start();
                            }
                        }
                        if (this.outputReader != null)
                        {
                            this.ProcessOutputHelper();
                        }
                    }
                }
                catch (Exception)
                {
                    this.StopProcessing();
                    throw;
                }
                finally
                {
                    if (!flag)
                    {
                        this.nativeProcess.WaitForExit();
                        this.inputWriter.Done();
                        if (this.outputReader != null)
                        {
                            this.outputReader.Done();
                        }
                        base.Command.Context.SetVariable(SpecialVariables.LastExitCodeVarPath, this.nativeProcess.ExitCode);
                        if (this.nativeProcess.ExitCode != 0)
                        {
                            base.commandRuntime.PipelineProcessor.ExecutionFailed = true;
                        }
                    }
                }
            }
            catch (Win32Exception exception2)
            {
                innerException = exception2;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception exception3)
            {
                CommandProcessorBase.CheckForSevereException(exception3);
                innerException = exception3;
            }
            finally
            {
                if (!flag2)
                {
                    base.Command.Context.EngineHostInterface.NotifyEndApplication();
                }
                this.CleanUp();
            }
            if (innerException != null)
            {
                string message = StringUtil.Format(ParserStrings.ProgramFailedToExecute, new object[] { this.NativeCommandName, innerException.Message, base.Command.MyInvocation.PositionMessage });
                if (message == null)
                {
                    message = StringUtil.Format("Program '{0}' failed to execute: {1}{2}", new object[] { this.NativeCommandName, innerException.Message, base.Command.MyInvocation.PositionMessage });
                }
                ApplicationFailedException exception4 = new ApplicationFailedException(message, innerException);
                throw exception4;
            }
        }

        [ArchitectureSensitive]
        private static string FindExecutable(string filename)
        {
            StringBuilder pathFound = new StringBuilder(0x400);
            IntPtr zero = IntPtr.Zero;
            try
            {
                zero = FindExecutableW(filename, string.Empty, pathFound);
            }
            catch (IndexOutOfRangeException exception)
            {
                WindowsErrorReporting.FailFast(exception);
            }
            if (((long) zero) >= 0x20L)
            {
                return pathFound.ToString();
            }
            return null;
        }

        [DllImport("shell32.dll", EntryPoint="FindExecutable")]
        private static extern IntPtr FindExecutableW(string fileName, string directoryPath, StringBuilder pathFound);
        [DllImport("kernel32.dll", SetLastError=true)]
        private static extern int GetBinaryTypeA(string lpApplicationName, ref int lpBinaryType);
        private ProcessStartInfo GetProcessStartInfo(bool redirectOutput, bool redirectError, bool redirectInput, bool soloCommand)
        {
            ProcessStartInfo info = new ProcessStartInfo {
                FileName = this.Path
            };
            if (this.validateExtension(this.Path))
            {
                info.UseShellExecute = false;
                if (redirectInput)
                {
                    info.RedirectStandardInput = true;
                }
                if (redirectOutput)
                {
                    info.RedirectStandardOutput = true;
                }
                if (redirectError)
                {
                    info.RedirectStandardError = true;
                }
            }
            else
            {
                if (!soloCommand)
                {
                    throw InterpreterError.NewInterpreterException(this.Path, typeof(RuntimeException), base.Command.InvocationExtent, "CantActivateDocumentInPipeline", ParserStrings.CantActivateDocumentInPipeline, new object[] { this.Path });
                }
                info.UseShellExecute = true;
            }
            if (this.isMiniShell)
            {
                MinishellParameterBinderController nativeParameterBinderController = (MinishellParameterBinderController) this.NativeParameterBinderController;
                nativeParameterBinderController.BindParameters(base.arguments, redirectOutput, base.Command.Context.EngineHostInterface.Name);
                info.CreateNoWindow = nativeParameterBinderController.NonInteractive;
            }
            info.Arguments = this.NativeParameterBinderController.Arguments;
            ExecutionContext context = base.Command.Context;
            string providerPath = context.EngineSessionState.GetNamespaceCurrentLocation(context.ProviderNames.FileSystem).ProviderPath;
            info.WorkingDirectory = WildcardPattern.Unescape(providerPath);
            return info;
        }

        private static bool IsConsoleApplication(string fileName)
        {
            return !IsWindowsApplication(fileName);
        }

        private bool IsMiniShell()
        {
            for (int i = 0; i < base.arguments.Count; i++)
            {
                CommandParameterInternal internal2 = base.arguments[i];
                if (!internal2.ParameterNameSpecified && (internal2.ArgumentValue is ScriptBlock))
                {
                    return true;
                }
            }
            return false;
        }

        [ArchitectureSensitive]
        private static bool IsWindowsApplication(string fileName)
        {
            SHFILEINFO psfi = new SHFILEINFO();
            switch (((int) SHGetFileInfo(fileName, 0, ref psfi, (int) Marshal.SizeOf(psfi), 0x2000)))
            {
                case 0:
                    return false;

                case 0x4550:
                    return false;

                case 0x5a4d:
                    return false;
            }
            return true;
        }

        private static void KillChildProcesses(int parentId, ProcessWithParentId[] currentlyRunningProcs)
        {
            foreach (ProcessWithParentId id in currentlyRunningProcs)
            {
                if ((id.ParentId > 0) && (id.ParentId == parentId))
                {
                    KillProcessAndChildProcesses(id.OriginalProcessInstance, currentlyRunningProcs);
                }
            }
        }

        private static void KillProcess(Process processToKill)
        {
            if (IsServerSide)
            {
                ProcessWithParentId[] currentlyRunningProcs = ProcessWithParentId.Construct(Process.GetProcesses());
                KillProcessAndChildProcesses(processToKill, currentlyRunningProcs);
            }
            else
            {
                try
                {
                    processToKill.Kill();
                }
                catch (Win32Exception)
                {
                    try
                    {
                        Process.GetProcessById(processToKill.Id).Kill();
                    }
                    catch (Exception exception)
                    {
                        CommandProcessorBase.CheckForSevereException(exception);
                    }
                }
                catch (Exception exception2)
                {
                    CommandProcessorBase.CheckForSevereException(exception2);
                }
            }
        }

        private static void KillProcessAndChildProcesses(Process processToKill, ProcessWithParentId[] currentlyRunningProcs)
        {
            try
            {
                KillChildProcesses(processToKill.Id, currentlyRunningProcs);
                processToKill.Kill();
            }
            catch (Win32Exception)
            {
                try
                {
                    Process.GetProcessById(processToKill.Id).Kill();
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                }
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
            }
        }

        internal ParameterBinderController NewParameterBinderController(InternalCommand command)
        {
            if (this.isMiniShell)
            {
                this.nativeParameterBinderController = new MinishellParameterBinderController(this.nativeCommand);
            }
            else
            {
                this.nativeParameterBinderController = new NativeCommandParameterBinderController(this.nativeCommand);
            }
            return this.nativeParameterBinderController;
        }

        internal override void Prepare(IDictionary psDefaultParameterValues)
        {
            this.isPreparedCalled = true;
            this.isMiniShell = this.IsMiniShell();
            if (!this.isMiniShell)
            {
                this.NativeParameterBinderController.BindParameters(base.arguments);
            }
        }

        private void ProcessOutputHelper()
        {
            for (object obj2 = this.outputReader.Read(); obj2 != AutomationNull.Value; obj2 = this.outputReader.Read())
            {
                ProcessOutputObject obj3 = obj2 as ProcessOutputObject;
                if (obj3.Stream == MinishellStream.Error)
                {
                    ErrorRecord data = obj3.Data as ErrorRecord;
                    data.SetInvocationInfo(base.Command.MyInvocation);
                    ActionPreference? actionPreference = null;
                    base.commandRuntime._WriteErrorSkipAllowCheck(data, actionPreference);
                }
                else if (obj3.Stream == MinishellStream.Output)
                {
                    base.commandRuntime._WriteObjectSkipAllowCheck(obj3.Data);
                }
                else if (obj3.Stream == MinishellStream.Debug)
                {
                    string message = obj3.Data as string;
                    base.Command.PSHostInternal.UI.WriteDebugLine(message);
                }
                else if (obj3.Stream == MinishellStream.Verbose)
                {
                    string str2 = obj3.Data as string;
                    base.Command.PSHostInternal.UI.WriteVerboseLine(str2);
                }
                else if (obj3.Stream == MinishellStream.Warning)
                {
                    string str3 = obj3.Data as string;
                    base.Command.PSHostInternal.UI.WriteWarningLine(str3);
                }
                else if (obj3.Stream == MinishellStream.Progress)
                {
                    PSObject obj4 = obj3.Data as PSObject;
                    if (obj4 != null)
                    {
                        long sourceId = 0L;
                        PSMemberInfo info = obj4.Properties["SourceId"];
                        if (info != null)
                        {
                            sourceId = (long) info.Value;
                        }
                        info = obj4.Properties["Record"];
                        ProgressRecord record = null;
                        if (info != null)
                        {
                            record = info.Value as ProgressRecord;
                        }
                        if (record != null)
                        {
                            base.Command.PSHostInternal.UI.WriteProgress(sourceId, record);
                        }
                    }
                }
                if (base.Command.Context.CurrentPipelineStopping)
                {
                    this.StopProcessing();
                    return;
                }
            }
        }

        internal override void ProcessRecord()
        {
            while (this.Read())
            {
                this.inputWriter.Add(base.Command.CurrentPipelineObject);
            }
        }

        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, int dwFileAttributes, ref SHFILEINFO psfi, int cbSizeFileInfo, int uFlags);
        internal void StopProcessing()
        {
            lock (this.sync)
            {
                if (this.stopped)
                {
                    return;
                }
                this.stopped = true;
            }
            if ((this.nativeProcess != null) && !this._runStandAlone)
            {
                this.inputWriter.Stop();
                if (this.outputReader != null)
                {
                    this.outputReader.Stop();
                }
                KillProcess(this.nativeProcess);
            }
        }

        private bool validateExtension(string path)
        {
            string[] strArray;
            string extension = System.IO.Path.GetExtension(path);
            string str2 = (string) LanguagePrimitives.ConvertTo(base.Command.Context.GetVariableValue(SpecialVariables.PathExtVarPath), typeof(string), CultureInfo.InvariantCulture);
            if (str2 == null)
            {
                strArray = new string[] { ".exe" };
            }
            else
            {
                strArray = str2.Split(new char[] { ';' });
            }
            foreach (string str3 in strArray)
            {
                if (string.Equals(str3, extension, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsServerSide
        {
            get
            {
                return _isServerSide;
            }
            set
            {
                _isServerSide = value;
            }
        }

        private NativeCommand nativeCommand
        {
            get
            {
                return (base.Command as NativeCommand);
            }
        }

        private string NativeCommandName
        {
            get
            {
                return this.applicationInfo.Name;
            }
        }

        internal NativeCommandParameterBinderController NativeParameterBinderController
        {
            get
            {
                if (this.nativeParameterBinderController == null)
                {
                    this.NewParameterBinderController(base.Command);
                }
                return this.nativeParameterBinderController;
            }
        }

        private string Path
        {
            get
            {
                return this.applicationInfo.Path;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct ProcessWithParentId
        {
            public Process OriginalProcessInstance;
            private int parentId;
            public int ParentId
            {
                get
                {
                    if (-2147483648 == this.parentId)
                    {
                        this.ConstructParentId();
                    }
                    return this.parentId;
                }
            }
            public ProcessWithParentId(Process originalProcess)
            {
                this.OriginalProcessInstance = originalProcess;
                this.parentId = -2147483648;
            }

            public static NativeCommandProcessor.ProcessWithParentId[] Construct(Process[] originalProcCollection)
            {
                NativeCommandProcessor.ProcessWithParentId[] idArray = new NativeCommandProcessor.ProcessWithParentId[originalProcCollection.Length];
                for (int i = 0; i < originalProcCollection.Length; i++)
                {
                    idArray[i] = new NativeCommandProcessor.ProcessWithParentId(originalProcCollection[i]);
                }
                return idArray;
            }

            private void ConstructParentId()
            {
                try
                {
                    this.parentId = -1;
                    Process parentProcess = PsUtils.GetParentProcess(this.OriginalProcessInstance);
                    if (parentProcess != null)
                    {
                        this.parentId = parentProcess.Id;
                    }
                }
                catch (Win32Exception)
                {
                }
                catch (InvalidOperationException)
                {
                }
                catch (ManagementException)
                {
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public int dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=80)]
            public string szTypeName;
        }
    }
}

