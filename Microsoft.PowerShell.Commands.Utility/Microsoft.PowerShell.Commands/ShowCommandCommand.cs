namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    [Cmdlet("Show", "Command", HelpUri="http://go.microsoft.com/fwlink/?LinkID=217448")]
    public class ShowCommandCommand : PSCmdlet, IDisposable
    {
        private string commandName;
        private List<CommandInfo> commands;
        private object commandViewModelObj;
        private SwitchParameter errorPopup;
        private PSDataCollection<ErrorRecord> errors = new PSDataCollection<ErrorRecord>();
        private bool hasOpenedWindow;
        private double height;
        private Dictionary<string, PSModuleInfo> importedModules;
        private SwitchParameter noCommonParameter;
        private bool passThrough;
        private ShowCommandProxy showCommandProxy;
        private double width;

        protected override void BeginProcessing()
        {
            this.showCommandProxy = new ShowCommandProxy(this);
            if (this.showCommandProxy.ScreenHeight < this.Height)
            {
                ErrorRecord errorRecord = new ErrorRecord(new NotSupportedException(string.Format(CultureInfo.CurrentUICulture, FormatAndOut_out_gridview.PropertyValidate, new object[] { "Height", this.showCommandProxy.ScreenHeight })), "PARAMETER_DATA_ERROR", ErrorCategory.InvalidData, null);
                base.ThrowTerminatingError(errorRecord);
            }
            if (this.showCommandProxy.ScreenWidth < this.Width)
            {
                ErrorRecord record2 = new ErrorRecord(new NotSupportedException(string.Format(CultureInfo.CurrentUICulture, FormatAndOut_out_gridview.PropertyValidate, new object[] { "Width", this.showCommandProxy.ScreenWidth })), "PARAMETER_DATA_ERROR", ErrorCategory.InvalidData, null);
                base.ThrowTerminatingError(record2);
            }
        }

        private bool CanProcessRecordForAllCommands()
        {
            Collection<PSObject> collection = base.InvokeCommand.InvokeScript(this.showCommandProxy.GetShowAllModulesCommand());
            this.commands = this.showCommandProxy.GetCommandList((object[]) collection[0].BaseObject);
            this.importedModules = this.showCommandProxy.GetImportedModulesDictionary((object[]) collection[1].BaseObject);
            try
            {
                this.showCommandProxy.ShowAllModulesWindow(this.importedModules, this.commands, this.noCommonParameter.ToBool(), this.passThrough);
            }
            catch (TargetInvocationException exception)
            {
                base.WriteError(new ErrorRecord(exception.InnerException, "CannotProcessRecordForAllCommands", ErrorCategory.InvalidOperation, this.commandName));
                return false;
            }
            return true;
        }

        private bool CanProcessRecordForOneCommand()
        {
            CommandInfo info;
            this.GetCommandInfoAndModules(out info, out this.importedModules);
            try
            {
                this.commandViewModelObj = this.showCommandProxy.GetCommandViewModel(info, this.noCommonParameter.ToBool(), this.importedModules, this.Name.IndexOf('\\') != -1);
                this.showCommandProxy.ShowCommandWindow(this.commandViewModelObj, this.passThrough);
            }
            catch (TargetInvocationException exception)
            {
                base.WriteError(new ErrorRecord(exception.InnerException, "CannotProcessRecordForOneCommand", ErrorCategory.InvalidOperation, this.commandName));
                return false;
            }
            return true;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {
            if (isDisposing && (this.errors != null))
            {
                this.errors.Dispose();
                this.errors = null;
            }
        }

        protected override void EndProcessing()
        {
            if (this.hasOpenedWindow)
            {
                this.showCommandProxy.WindowLoaded.WaitOne();
                this.showCommandProxy.ActivateWindow();
                this.WaitForWindowClosedOrHelpNeeded();
                this.RunScript(this.showCommandProxy.GetScript());
                if ((this.errors.Count != 0) && (this.errorPopup != 0))
                {
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < this.errors.Count; i++)
                    {
                        if (i != 0)
                        {
                            builder.AppendLine();
                        }
                        ErrorRecord record = this.errors[i];
                        builder.Append(record.Exception.Message);
                    }
                    this.showCommandProxy.ShowErrorString(builder.ToString());
                }
            }
        }

        private void Error_DataAdded(object sender, DataAddedEventArgs e)
        {
            base.WriteError(((PSDataCollection<ErrorRecord>) sender)[e.Index]);
        }

        ~ShowCommandCommand()
        {
            this.Dispose(false);
        }

        private void GetCommandInfoAndModules(out CommandInfo command, out Dictionary<string, PSModuleInfo> modules)
        {
            command = null;
            modules = null;
            string showCommandCommand = this.showCommandProxy.GetShowCommandCommand(this.commandName, true);
            Collection<PSObject> collection = base.InvokeCommand.InvokeScript(showCommandCommand);
            object[] baseObject = (object[]) collection[0].BaseObject;
            object[] moduleObjects = (object[]) collection[1].BaseObject;
            if (((collection == null) || (moduleObjects == null)) || (baseObject.Length == 0))
            {
                this.IssueErrorForNoCommand();
            }
            else
            {
                if (baseObject.Length > 1)
                {
                    this.IssueErrorForMoreThanOneCommand();
                }
                command = ((PSObject) baseObject[0]).BaseObject as CommandInfo;
                if (command == null)
                {
                    this.IssueErrorForNoCommand();
                }
                else
                {
                    if (command.CommandType == CommandTypes.Alias)
                    {
                        showCommandCommand = this.showCommandProxy.GetShowCommandCommand(command.Definition, false);
                        collection = base.InvokeCommand.InvokeScript(showCommandCommand);
                        if ((collection == null) || (collection.Count != 1))
                        {
                            this.IssueErrorForNoCommand();
                        }
                        command = (CommandInfo) collection[0].BaseObject;
                    }
                    modules = this.showCommandProxy.GetImportedModulesDictionary(moduleObjects);
                }
            }
        }

        private void IssueErrorForMoreThanOneCommand()
        {
            InvalidOperationException exception = new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, FormatAndOut_out_gridview.MoreThanOneCommand, new object[] { this.commandName, "Show-Command" }));
            base.ThrowTerminatingError(new ErrorRecord(exception, "MoreThanOneCommand", ErrorCategory.InvalidOperation, this.commandName));
        }

        private void IssueErrorForNoCommand()
        {
            InvalidOperationException exception = new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, FormatAndOut_out_gridview.CommandNotFound, new object[] { this.commandName }));
            base.ThrowTerminatingError(new ErrorRecord(exception, "NoCommand", ErrorCategory.InvalidOperation, this.commandName));
        }

        private void Output_DataAdded(object sender, DataAddedEventArgs e)
        {
            base.WriteObject(((PSDataCollection<object>) sender)[e.Index]);
        }

        protected override void ProcessRecord()
        {
            if (this.commandName == null)
            {
                this.hasOpenedWindow = this.CanProcessRecordForAllCommands();
            }
            else
            {
                this.hasOpenedWindow = this.CanProcessRecordForOneCommand();
            }
        }

        public void RunScript(string script)
        {
            if ((this.showCommandProxy != null) && !string.IsNullOrEmpty(script))
            {
                if (this.passThrough)
                {
                    base.WriteObject(script);
                }
                else if (this.errorPopup != 0)
                {
                    this.RunScriptSilentlyAndWithErrorHookup(script);
                }
                else if (this.showCommandProxy.HasHostWindow)
                {
                    if (!this.showCommandProxy.SetPendingISECommand(script))
                    {
                        this.RunScriptSilentlyAndWithErrorHookup(script);
                    }
                }
                else if (!ConsoleInputWithNativeMethods.AddToConsoleInputBuffer(script, true))
                {
                    base.WriteDebug(FormatAndOut_out_gridview.CannotWriteToConsoleInputBuffer);
                    this.RunScriptSilentlyAndWithErrorHookup(script);
                }
            }
        }

        private void RunScriptSilentlyAndWithErrorHookup(string script)
        {
            PSDataCollection<object> output = new PSDataCollection<object>();
            output.DataAdded += new EventHandler<DataAddedEventArgs>(this.Output_DataAdded);
            this.errors.DataAdded += new EventHandler<DataAddedEventArgs>(this.Error_DataAdded);
            PowerShell shell = PowerShell.Create(RunspaceMode.CurrentRunspace);
            shell.Streams.Error = this.errors;
            shell.Commands.AddScript(script);
            shell.Invoke<object>(null, output, null);
        }

        protected override void StopProcessing()
        {
            this.showCommandProxy.CloseWindow();
        }

        private void WaitForWindowClosedOrHelpNeeded()
        {
            Collection<PSObject> collection2;
        Label_0000:;
            switch (WaitHandle.WaitAny(new WaitHandle[] { this.showCommandProxy.WindowClosed, this.showCommandProxy.HelpNeeded, this.showCommandProxy.ImportModuleNeeded }))
            {
                case 0:
                    return;

                case 1:
                {
                    Collection<PSObject> helpResults = base.InvokeCommand.InvokeScript(this.showCommandProxy.GetHelpCommand(this.showCommandProxy.CommandNeedingHelp));
                    this.showCommandProxy.DisplayHelp(helpResults);
                    goto Label_0000;
                }
            }
            string importModuleCommand = this.showCommandProxy.GetImportModuleCommand(this.showCommandProxy.ParentModuleNeedingImportModule);
            try
            {
                collection2 = base.InvokeCommand.InvokeScript(importModuleCommand);
            }
            catch (RuntimeException exception)
            {
                this.showCommandProxy.ImportModuleFailed(exception);
                goto Label_0000;
            }
            this.commands = this.showCommandProxy.GetCommandList((object[]) collection2[0].BaseObject);
            this.importedModules = this.showCommandProxy.GetImportedModulesDictionary((object[]) collection2[1].BaseObject);
            this.showCommandProxy.ImportModuleDone(this.importedModules, this.commands);
            goto Label_0000;
        }

        [Parameter]
        public SwitchParameter ErrorPopup
        {
            get
            {
                return this.errorPopup;
            }
            set
            {
                this.errorPopup = value;
            }
        }

        [ValidateRange(300, 0x7fffffff), Parameter]
        public double Height
        {
            get
            {
                return this.height;
            }
            set
            {
                this.height = value;
            }
        }

        [Alias(new string[] { "CommandName" }), Parameter(Position=0)]
        public string Name
        {
            get
            {
                return this.commandName;
            }
            set
            {
                this.commandName = value;
            }
        }

        [Parameter]
        public SwitchParameter NoCommonParameter
        {
            get
            {
                return this.noCommonParameter;
            }
            set
            {
                this.noCommonParameter = value;
            }
        }

        [Parameter]
        public SwitchParameter PassThru
        {
            get
            {
                return this.passThrough;
            }
            set
            {
                this.passThrough = (bool) value;
            }
        }

        [Parameter, ValidateRange(300, 0x7fffffff)]
        public double Width
        {
            get
            {
                return this.width;
            }
            set
            {
                this.width = value;
            }
        }

        internal static class ConsoleInputWithNativeMethods
        {
            internal const int STD_INPUT_HANDLE = -10;

            internal static bool AddToConsoleInputBuffer(string str, bool newLine)
            {
                uint num3;
                IntPtr stdHandle = GetStdHandle(-10);
                if (stdHandle == IntPtr.Zero)
                {
                    return false;
                }
                uint length = (uint) str.Length;
                INPUT_RECORD[] lpBuffer = new INPUT_RECORD[length + (newLine ? ((long) 1) : ((long) 0))];
                for (int i = 0; i < length; i++)
                {
                    INPUT_RECORD.SetInputRecord(ref lpBuffer[i], str[i]);
                }
                if (!WriteConsoleInput(stdHandle, lpBuffer, length, out num3) || (num3 != length))
                {
                    return false;
                }
                if (newLine)
                {
                    INPUT_RECORD[] input_recordArray2 = new INPUT_RECORD[1];
                    INPUT_RECORD.SetInputRecord(ref input_recordArray2[0], '\r');
                    num3 = 0;
                    if (!WriteConsoleInput(stdHandle, input_recordArray2, 1, out num3))
                    {
                        return false;
                    }
                }
                return true;
            }

            [DllImport("kernel32.dll", SetLastError=true)]
            internal static extern IntPtr GetStdHandle(int nStdHandle);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("kernel32.dll", SetLastError=true)]
            internal static extern bool WriteConsoleInput(IntPtr hConsoleInput, INPUT_RECORD[] lpBuffer, uint nLength, out uint lpNumberOfEventsWritten);

            [StructLayout(LayoutKind.Sequential)]
            internal struct INPUT_RECORD
            {
                internal const int KEY_EVENT = 1;
                internal ushort EventType;
                internal ShowCommandCommand.ConsoleInputWithNativeMethods.KEY_EVENT_RECORD KeyEvent;
                internal static void SetInputRecord(ref ShowCommandCommand.ConsoleInputWithNativeMethods.INPUT_RECORD inputRecord, char character)
                {
                    inputRecord.EventType = 1;
                    inputRecord.KeyEvent.bKeyDown = true;
                    inputRecord.KeyEvent.UnicodeChar = character;
                }
            }

            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
            internal struct KEY_EVENT_RECORD
            {
                internal bool bKeyDown;
                internal ushort wRepeatCount;
                internal ushort wVirtualKeyCode;
                internal ushort wVirtualScanCode;
                internal char UnicodeChar;
                internal uint dwControlKeyState;
            }
        }
    }
}

