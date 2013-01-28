namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Security;

    [Cmdlet("Export", "Console", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113298")]
    public sealed class ExportConsoleCommand : ConsoleCmdletsBase
    {
        private string fileName;
        private bool force;
        private bool noclobber;

        private string GetFileName()
        {
            if (!string.IsNullOrEmpty(this.fileName))
            {
                return this.fileName;
            }
            PSVariable variable = base.Context.SessionState.PSVariable.Get("ConsoleFileName");
            if (variable == null)
            {
                return string.Empty;
            }
            string baseObject = variable.Value as string;
            if (baseObject == null)
            {
                PSObject obj2 = variable.Value as PSObject;
                if ((obj2 != null) && (obj2.BaseObject is string))
                {
                    baseObject = obj2.BaseObject as string;
                }
            }
            if (baseObject == null)
            {
                throw PSTraceSource.NewArgumentException("fileName", "ConsoleInfoErrorStrings", "ConsoleCannotbeConvertedToString", new object[0]);
            }
            return baseObject;
        }

        protected override void ProcessRecord()
        {
            string fileName = this.GetFileName();
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = this.PromptUserForFile();
            }
            if (string.IsNullOrEmpty(fileName))
            {
                PSArgumentException innerException = PSTraceSource.NewArgumentException("file", "ConsoleInfoErrorStrings", "FileNameNotResolved", new object[0]);
                base.ThrowError(fileName, "FileNameNotResolved", innerException, ErrorCategory.InvalidArgument);
            }
            if (WildcardPattern.ContainsWildcardCharacters(fileName))
            {
                base.ThrowError(fileName, "WildCardNotSupported", PSTraceSource.NewInvalidOperationException("ConsoleInfoErrorStrings", "ConsoleFileWildCardsNotSupported", new object[] { fileName }), ErrorCategory.InvalidOperation);
            }
            string str2 = this.ResolveProviderAndPath(fileName);
            if (!string.IsNullOrEmpty(str2))
            {
                if (!str2.EndsWith(".psc1", StringComparison.OrdinalIgnoreCase))
                {
                    str2 = str2 + ".psc1";
                }
                if (base.ShouldProcess(this.Path))
                {
                    if (File.Exists(str2))
                    {
                        if (this.NoClobber != false)
                        {
                            Exception exception = new UnauthorizedAccessException(StringUtil.Format(ConsoleInfoErrorStrings.FileExistsNoClobber, str2, "NoClobber"));
                            ErrorRecord errorRecord = new ErrorRecord(exception, "NoClobber", ErrorCategory.ResourceExists, str2);
                            base.ThrowTerminatingError(errorRecord);
                        }
                        if ((File.GetAttributes(str2) & System.IO.FileAttributes.ReadOnly) == System.IO.FileAttributes.ReadOnly)
                        {
                            if (this.Force != false)
                            {
                                this.RemoveFileThrowIfError(str2);
                            }
                            else
                            {
                                base.ThrowError(fileName, "ConsoleFileReadOnly", PSTraceSource.NewArgumentException(fileName, "ConsoleInfoErrorStrings", "ConsoleFileReadOnly", new object[] { str2 }), ErrorCategory.InvalidArgument);
                            }
                        }
                    }
                    try
                    {
                        if (base.Runspace == null)
                        {
                            if (base.InitialSessionState == null)
                            {
                                throw PSTraceSource.NewInvalidOperationException("ConsoleInfoErrorStrings", "CmdletNotAvailable", new object[] { "" });
                            }
                            base.InitialSessionState.SaveAsConsoleFile(str2);
                        }
                        else
                        {
                            base.Runspace.SaveAsConsoleFile(str2);
                        }
                    }
                    catch (PSArgumentException exception3)
                    {
                        base.ThrowError(str2, "PathNotAbsolute", exception3, ErrorCategory.InvalidArgument);
                    }
                    catch (PSArgumentNullException exception4)
                    {
                        base.ThrowError(str2, "PathNull", exception4, ErrorCategory.InvalidArgument);
                    }
                    catch (ArgumentException exception5)
                    {
                        base.ThrowError(str2, "InvalidCharacetersInPath", exception5, ErrorCategory.InvalidArgument);
                    }
                    Exception exception6 = null;
                    try
                    {
                        base.Context.EngineSessionState.SetConsoleVariable();
                    }
                    catch (ArgumentNullException exception7)
                    {
                        exception6 = exception7;
                    }
                    catch (ArgumentOutOfRangeException exception8)
                    {
                        exception6 = exception8;
                    }
                    catch (ArgumentException exception9)
                    {
                        exception6 = exception9;
                    }
                    catch (SessionStateUnauthorizedAccessException exception10)
                    {
                        exception6 = exception10;
                    }
                    catch (SessionStateOverflowException exception11)
                    {
                        exception6 = exception11;
                    }
                    catch (ProviderNotFoundException exception12)
                    {
                        exception6 = exception12;
                    }
                    catch (System.Management.Automation.DriveNotFoundException exception13)
                    {
                        exception6 = exception13;
                    }
                    catch (NotSupportedException exception14)
                    {
                        exception6 = exception14;
                    }
                    catch (ProviderInvocationException exception15)
                    {
                        exception6 = exception15;
                    }
                    if (exception6 != null)
                    {
                        throw PSTraceSource.NewInvalidOperationException(exception6, "ConsoleInfoErrorStrings", "ConsoleVariableCannotBeSet", new object[] { str2 });
                    }
                }
            }
        }

        private string PromptUserForFile()
        {
            if (base.ShouldContinue(ConsoleInfoErrorStrings.PromptForExportConsole, null))
            {
                string caption = StringUtil.Format(ConsoleInfoErrorStrings.FileNameCaptionForExportConsole, "export-console");
                string fileNamePromptMessage = ConsoleInfoErrorStrings.FileNamePromptMessage;
                Collection<FieldDescription> descriptions = new Collection<FieldDescription> {
                    new FieldDescription("Name")
                };
                Dictionary<string, PSObject> dictionary = base.PSHostInternal.UI.Prompt(caption, fileNamePromptMessage, descriptions);
                if ((dictionary != null) && (dictionary["Name"] != null))
                {
                    return (dictionary["Name"].BaseObject as string);
                }
            }
            return string.Empty;
        }

        private void RemoveFileThrowIfError(string destination)
        {
            FileInfo info = new FileInfo(destination);
            if (info != null)
            {
                Exception innerException = null;
                try
                {
                    info.Attributes &= ~(System.IO.FileAttributes.Hidden | System.IO.FileAttributes.ReadOnly);
                    info.Delete();
                }
                catch (FileNotFoundException exception2)
                {
                    innerException = exception2;
                }
                catch (DirectoryNotFoundException exception3)
                {
                    innerException = exception3;
                }
                catch (UnauthorizedAccessException exception4)
                {
                    innerException = exception4;
                }
                catch (SecurityException exception5)
                {
                    innerException = exception5;
                }
                catch (ArgumentNullException exception6)
                {
                    innerException = exception6;
                }
                catch (ArgumentException exception7)
                {
                    innerException = exception7;
                }
                catch (PathTooLongException exception8)
                {
                    innerException = exception8;
                }
                catch (NotSupportedException exception9)
                {
                    innerException = exception9;
                }
                catch (IOException exception10)
                {
                    innerException = exception10;
                }
                if (innerException != null)
                {
                    throw PSTraceSource.NewInvalidOperationException(innerException, "ConsoleInfoErrorStrings", "ExportConsoleCannotDeleteFile", new object[] { info });
                }
            }
        }

        private PathInfo ResolvePath(string pathToResolve, bool allowNonexistingPaths, CmdletProviderContext currentCommandContext)
        {
            Collection<PathInfo> targetObject = new Collection<PathInfo>();
            try
            {
                foreach (PathInfo info in base.SessionState.Path.GetResolvedPSPathFromPSPath(pathToResolve, currentCommandContext))
                {
                    targetObject.Add(info);
                }
            }
            catch (PSNotSupportedException exception)
            {
                base.WriteError(new ErrorRecord(exception.ErrorRecord, exception));
            }
            catch (System.Management.Automation.DriveNotFoundException exception2)
            {
                base.WriteError(new ErrorRecord(exception2.ErrorRecord, exception2));
            }
            catch (ProviderNotFoundException exception3)
            {
                base.WriteError(new ErrorRecord(exception3.ErrorRecord, exception3));
            }
            catch (ItemNotFoundException exception4)
            {
                if (allowNonexistingPaths)
                {
                    ProviderInfo provider = null;
                    PSDriveInfo drive = null;
                    string path = base.SessionState.Path.GetUnresolvedProviderPathFromPSPath(pathToResolve, currentCommandContext, out provider, out drive);
                    PathInfo item = new PathInfo(drive, provider, path, base.SessionState);
                    targetObject.Add(item);
                }
                else
                {
                    base.WriteError(new ErrorRecord(exception4.ErrorRecord, exception4));
                }
            }
            if (targetObject.Count == 1)
            {
                return targetObject[0];
            }
            if (targetObject.Count > 1)
            {
                Exception exception5 = PSTraceSource.NewNotSupportedException();
                base.WriteError(new ErrorRecord(exception5, "NotSupported", ErrorCategory.NotImplemented, targetObject));
                return null;
            }
            return null;
        }

        private string ResolveProviderAndPath(string path)
        {
            CmdletProviderContext currentCommandContext = new CmdletProviderContext(this);
            PathInfo info = this.ResolvePath(path, true, currentCommandContext);
            if (info == null)
            {
                return null;
            }
            if (info.Provider.ImplementingType != typeof(FileSystemProvider))
            {
                throw PSTraceSource.NewInvalidOperationException("ConsoleInfoErrorStrings", "ProviderNotSupported", new object[] { info.Provider.Name });
            }
            return info.Path;
        }

        [Parameter]
        public SwitchParameter Force
        {
            get
            {
                return this.force;
            }
            set
            {
                this.force = (bool) value;
            }
        }

        [Alias(new string[] { "NoOverwrite" }), Parameter]
        public SwitchParameter NoClobber
        {
            get
            {
                return this.noclobber;
            }
            set
            {
                this.noclobber = (bool) value;
            }
        }

        [Parameter(Position=0, Mandatory=false, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true), Alias(new string[] { "PSPath" })]
        public string Path
        {
            get
            {
                return this.fileName;
            }
            set
            {
                this.fileName = value;
            }
        }
    }
}

