namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation.Internal;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    internal static class PathUtils
    {
        internal static DirectoryInfo CreateModuleDirectory(PSCmdlet cmdlet, string moduleNameOrPath, bool force)
        {
            DirectoryInfo targetObject = null;
            try
            {
                string str = ModuleCmdletBase.ResolveRootedFilePath(moduleNameOrPath, cmdlet.Context);
                if (string.IsNullOrEmpty(str) && moduleNameOrPath.StartsWith(".", StringComparison.OrdinalIgnoreCase))
                {
                    str = Path.Combine(cmdlet.CurrentProviderLocation(cmdlet.Context.ProviderNames.FileSystem).ProviderPath, moduleNameOrPath);
                }
                if (string.IsNullOrEmpty(str))
                {
                    str = Path.Combine(ModuleIntrinsics.GetPersonalModulePath(), moduleNameOrPath);
                }
                targetObject = new DirectoryInfo(str);
                if (targetObject.Exists)
                {
                    if (!force)
                    {
                        ErrorDetails details = new ErrorDetails(string.Format(CultureInfo.InvariantCulture, PathUtilsStrings.ExportPSSession_ErrorDirectoryExists, new object[] { targetObject.FullName }));
                        ErrorRecord errorRecord = new ErrorRecord(new ArgumentException(details.Message), "ExportProxyCommand_OutputDirectoryExists", ErrorCategory.ResourceExists, targetObject);
                        cmdlet.ThrowTerminatingError(errorRecord);
                    }
                    return targetObject;
                }
                targetObject.Create();
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                ErrorDetails details2 = new ErrorDetails(string.Format(CultureInfo.InvariantCulture, PathUtilsStrings.ExportPSSession_CannotCreateOutputDirectory, new object[] { moduleNameOrPath, exception.Message }));
                ErrorRecord record2 = new ErrorRecord(new ArgumentException(details2.Message, exception), "ExportProxyCommand_CannotCreateOutputDirectory", ErrorCategory.ResourceExists, moduleNameOrPath);
                cmdlet.ThrowTerminatingError(record2);
            }
            return targetObject;
        }

        internal static DirectoryInfo CreateTemporaryDirectory()
        {
            DirectoryInfo info2;
            DirectoryInfo info = new DirectoryInfo(Path.GetTempPath());
            do
            {
                info2 = new DirectoryInfo(Path.Combine(info.FullName, string.Format(null, "tmp_{0}", new object[] { Path.GetRandomFileName() })));
            }
            while (info2.Exists);
            Directory.CreateDirectory(info2.FullName);
            return new DirectoryInfo(info2.FullName);
        }

        internal static void MasterStreamOpen(PSCmdlet cmdlet, string filePath, string encoding, bool defaultEncoding, bool Append, bool Force, bool NoClobber, out FileStream fileStream, out StreamWriter streamWriter, out FileInfo readOnlyFileInfo, bool isLiteralPath)
        {
            Encoding resolvedEncoding = EncodingConversion.Convert(cmdlet, encoding);
            MasterStreamOpen(cmdlet, filePath, resolvedEncoding, defaultEncoding, Append, Force, NoClobber, out fileStream, out streamWriter, out readOnlyFileInfo, isLiteralPath);
        }

        internal static void MasterStreamOpen(PSCmdlet cmdlet, string filePath, Encoding resolvedEncoding, bool defaultEncoding, bool Append, bool Force, bool NoClobber, out FileStream fileStream, out StreamWriter streamWriter, out FileInfo readOnlyFileInfo, bool isLiteralPath)
        {
            fileStream = null;
            streamWriter = null;
            readOnlyFileInfo = null;
            string path = ResolveFilePath(filePath, cmdlet, isLiteralPath);
            try
            {
                FileMode create = FileMode.Create;
                if (Append)
                {
                    create = FileMode.Append;
                }
                else if (NoClobber)
                {
                    create = FileMode.CreateNew;
                }
                if ((Force && (Append || !NoClobber)) && File.Exists(path))
                {
                    FileInfo info = new FileInfo(path);
                    if ((info.Attributes & System.IO.FileAttributes.ReadOnly) == System.IO.FileAttributes.ReadOnly)
                    {
                        readOnlyFileInfo = info;
                        info.Attributes &= ~System.IO.FileAttributes.ReadOnly;
                    }
                }
                FileShare share = Force ? FileShare.ReadWrite : FileShare.Read;
                fileStream = new FileStream(path, create, FileAccess.Write, share);
                if (defaultEncoding)
                {
                    streamWriter = new StreamWriter(fileStream);
                }
                else
                {
                    streamWriter = new StreamWriter(fileStream, resolvedEncoding);
                }
            }
            catch (ArgumentException exception)
            {
                ReportFileOpenFailure(cmdlet, path, exception);
            }
            catch (IOException exception2)
            {
                if (NoClobber && File.Exists(path))
                {
                    ErrorRecord errorRecord = new ErrorRecord(exception2, "NoClobber", ErrorCategory.ResourceExists, path) {
                        ErrorDetails = new ErrorDetails(cmdlet, "PathUtilsStrings", "UtilityFileExistsNoClobber", new object[] { filePath, "NoClobber" })
                    };
                    cmdlet.ThrowTerminatingError(errorRecord);
                }
                ReportFileOpenFailure(cmdlet, path, exception2);
            }
            catch (UnauthorizedAccessException exception3)
            {
                ReportFileOpenFailure(cmdlet, path, exception3);
            }
            catch (NotSupportedException exception4)
            {
                ReportFileOpenFailure(cmdlet, path, exception4);
            }
            catch (SecurityException exception5)
            {
                ReportFileOpenFailure(cmdlet, path, exception5);
            }
        }

        internal static FileStream OpenFileStream(string filePath, PSCmdlet command, bool isLiteralPath)
        {
            string path = ResolveFilePath(filePath, command, isLiteralPath);
            try
            {
                return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            catch (ArgumentException exception)
            {
                ReportFileOpenFailure(command, filePath, exception);
                return null;
            }
            catch (IOException exception2)
            {
                ReportFileOpenFailure(command, filePath, exception2);
                return null;
            }
            catch (UnauthorizedAccessException exception3)
            {
                ReportFileOpenFailure(command, filePath, exception3);
                return null;
            }
            catch (NotSupportedException exception4)
            {
                ReportFileOpenFailure(command, filePath, exception4);
                return null;
            }
            catch (System.Management.Automation.DriveNotFoundException exception5)
            {
                ReportFileOpenFailure(command, filePath, exception5);
                return null;
            }
        }

        internal static StreamReader OpenStreamReader(PSCmdlet command, string filePath, string encoding, bool isLiteralPath)
        {
            FileStream stream = OpenFileStream(filePath, command, isLiteralPath);
            if (encoding == null)
            {
                return new StreamReader(stream);
            }
            return new StreamReader(stream, EncodingConversion.Convert(command, encoding));
        }

        internal static void ReportFileOpenFailure(Cmdlet cmdlet, string filePath, Exception e)
        {
            ErrorRecord errorRecord = new ErrorRecord(e, "FileOpenFailure", ErrorCategory.OpenError, null);
            cmdlet.ThrowTerminatingError(errorRecord);
        }

        internal static void ReportMultipleFilesNotSupported(Cmdlet cmdlet)
        {
            string message = StringUtil.Format(PathUtilsStrings.OutFile_MultipleFilesNotSupported, new object[0]);
            ErrorRecord errorRecord = new ErrorRecord(PSTraceSource.NewInvalidOperationException(), "ReadWriteMultipleFilesNotSupported", ErrorCategory.InvalidArgument, null) {
                ErrorDetails = new ErrorDetails(message)
            };
            cmdlet.ThrowTerminatingError(errorRecord);
        }

        internal static void ReportWildcardingFailure(Cmdlet cmdlet, string filePath)
        {
            string message = StringUtil.Format(PathUtilsStrings.OutFile_DidNotResolveFile, filePath);
            ErrorRecord errorRecord = new ErrorRecord(new FileNotFoundException(), "FileOpenFailure", ErrorCategory.OpenError, filePath) {
                ErrorDetails = new ErrorDetails(message)
            };
            cmdlet.ThrowTerminatingError(errorRecord);
        }

        internal static void ReportWrongProviderType(Cmdlet cmdlet, string providerId)
        {
            string message = StringUtil.Format(PathUtilsStrings.OutFile_ReadWriteFileNotFileSystemProvider, providerId);
            ErrorRecord errorRecord = new ErrorRecord(PSTraceSource.NewInvalidOperationException(), "ReadWriteFileNotFileSystemProvider", ErrorCategory.InvalidArgument, null) {
                ErrorDetails = new ErrorDetails(message)
            };
            cmdlet.ThrowTerminatingError(errorRecord);
        }

        internal static string ResolveFilePath(string filePath, PSCmdlet command)
        {
            return ResolveFilePath(filePath, command, false);
        }

        internal static string ResolveFilePath(string filePath, PSCmdlet command, bool isLiteralPath)
        {
            string str = null;
            try
            {
                ProviderInfo provider = null;
                PSDriveInfo drive = null;
                List<string> list = new List<string>();
                if (isLiteralPath)
                {
                    list.Add(command.SessionState.Path.GetUnresolvedProviderPathFromPSPath(filePath, out provider, out drive));
                }
                else
                {
                    list.AddRange(command.SessionState.Path.GetResolvedProviderPathFromPSPath(filePath, out provider));
                }
                if (!provider.NameEquals(command.Context.ProviderNames.FileSystem))
                {
                    ReportWrongProviderType(command, provider.FullName);
                }
                if (list.Count > 1)
                {
                    ReportMultipleFilesNotSupported(command);
                }
                if (list.Count == 0)
                {
                    ReportWildcardingFailure(command, filePath);
                }
                str = list[0];
            }
            catch (ItemNotFoundException)
            {
                str = null;
            }
            if (string.IsNullOrEmpty(str))
            {
                CmdletProviderContext context = new CmdletProviderContext(command);
                ProviderInfo info3 = null;
                PSDriveInfo info4 = null;
                str = command.SessionState.Path.GetUnresolvedProviderPathFromPSPath(filePath, context, out info3, out info4);
                context.ThrowFirstErrorOrDoNothing();
                if (!info3.NameEquals(command.Context.ProviderNames.FileSystem))
                {
                    ReportWrongProviderType(command, info3.FullName);
                }
            }
            return str;
        }
    }
}

