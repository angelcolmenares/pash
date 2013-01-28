namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Provider;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Xml;
    using System.Xml.XPath;

    [OutputType(new Type[] { typeof(FileInfo) }, ProviderCmdlet="Get-Item"), OutputType(new Type[] { typeof(string), typeof(FileInfo) }, ProviderCmdlet="New-Item"), OutputType(new Type[] { typeof(PathInfo) }, ProviderCmdlet="Push-Location"), OutputType(new Type[] { typeof(bool), typeof(string), typeof(FileInfo), typeof(DirectoryInfo) }, ProviderCmdlet="Get-Item"), CmdletProvider("FileSystem", ProviderCapabilities.Credentials | ProviderCapabilities.ShouldProcess | ProviderCapabilities.Filter), OutputType(new Type[] { typeof(FileSecurity) }, ProviderCmdlet="Set-Acl"), OutputType(new Type[] { typeof(string), typeof(PathInfo) }, ProviderCmdlet="Resolve-Path"), OutputType(new Type[] { typeof(bool), typeof(string), typeof(DateTime), typeof(FileInfo), typeof(DirectoryInfo) }, ProviderCmdlet="Get-ItemProperty"), OutputType(new Type[] { typeof(byte), typeof(string) }, ProviderCmdlet="Get-Content"), OutputType(new Type[] { typeof(FileInfo), typeof(DirectoryInfo) }, ProviderCmdlet="Get-ChildItem"), OutputType(new Type[] { typeof(FileSecurity), typeof(DirectorySecurity) }, ProviderCmdlet="Get-Acl")]
    public sealed class FileSystemProvider : NavigationCmdletProvider, IContentCmdletProvider, IPropertyCmdletProvider, ISecurityDescriptorCmdletProvider, ICmdletProviderSupportsHelp
    {
        public const string ProviderName = "FileSystem";
        [TraceSource("FileSystemProvider", "The namespace navigation provider for the file system")]
        private static PSTraceSource tracer = PSTraceSource.GetTracer("FileSystemProvider", "The namespace navigation provider for the file system");

        public void ClearContent(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            path = NormalizePath(path);
            try
            {
                bool flag = false;
                FileSystemClearContentDynamicParameters dynamicParameters = null;
                FileSystemContentWriterDynamicParameters parameters2 = null;
                string stream = null;
                if (base.DynamicParameters != null)
                {
                    dynamicParameters = base.DynamicParameters as FileSystemClearContentDynamicParameters;
                    parameters2 = base.DynamicParameters as FileSystemContentWriterDynamicParameters;
                    if (dynamicParameters != null)
                    {
                        if ((dynamicParameters.Stream != null) && (dynamicParameters.Stream.Length > 0))
                        {
                            flag = true;
                        }
                        stream = dynamicParameters.Stream;
                    }
                    else if (parameters2 != null)
                    {
                        if ((parameters2.Stream != null) && (parameters2.Stream.Length > 0))
                        {
                            flag = true;
                        }
                        stream = parameters2.Stream;
                    }
                    if (string.IsNullOrEmpty(stream))
                    {
                        int index = path.IndexOf(':');
                        int startIndex = path.IndexOf(':', index + 1);
                        if (startIndex > 0)
                        {
                            stream = path.Substring(startIndex + 1);
                            path = path.Remove(startIndex);
                            flag = true;
                        }
                    }
                }
                if (string.Equals(":$DATA", stream, StringComparison.OrdinalIgnoreCase))
                {
                    flag = false;
                }
                if (flag)
                {
                    string target = string.Format(CultureInfo.InvariantCulture, FileSystemProviderStrings.StreamAction, new object[] { stream, path });
                    if (base.ShouldProcess(target))
                    {
                        if (dynamicParameters != null)
                        {
                            AlternateDataStreamUtilities.CreateFileStream(path, stream, FileMode.Open, FileAccess.Write, FileShare.Write).Close();
                        }
                        AlternateDataStreamUtilities.CreateFileStream(path, stream, FileMode.Create, FileAccess.Write, FileShare.Write).Close();
                    }
                }
                else
                {
                    string clearContentActionFile = FileSystemProviderStrings.ClearContentActionFile;
                    string str4 = StringUtil.Format(FileSystemProviderStrings.ClearContentesourceTemplate, path);
                    if (!base.ShouldProcess(str4, clearContentActionFile))
                    {
                        return;
                    }
                    new FileStream(path, FileMode.Truncate, FileAccess.Write, FileShare.Write).Close();
                }
                base.WriteItemObject("", path, false);
            }
            catch (ArgumentException exception)
            {
                base.WriteError(new ErrorRecord(exception, "ClearContentArgumentError", ErrorCategory.InvalidArgument, path));
            }
            catch (IOException exception2)
            {
                base.WriteError(new ErrorRecord(exception2, "ClearContentIOError", ErrorCategory.WriteError, path));
            }
            catch (UnauthorizedAccessException exception3)
            {
                if (base.Force != 0)
                {
                    System.IO.FileAttributes fileAttributes = File.GetAttributes(path);
                    try
                    {
                        try
                        {
                            File.SetAttributes(path, File.GetAttributes(path) & ~(System.IO.FileAttributes.Hidden | System.IO.FileAttributes.ReadOnly));
                            new FileStream(path, FileMode.Truncate, FileAccess.Write, FileShare.Write).Close();
                            base.WriteItemObject("", path, false);
                        }
                        catch (UnauthorizedAccessException exception4)
                        {
                            base.WriteError(new ErrorRecord(exception4, "RemoveFileSystemItemUnAuthorizedAccess", ErrorCategory.PermissionDenied, path));
                        }
                        return;
                    }
                    finally
                    {
                        File.SetAttributes(path, fileAttributes);
                    }
                }
                base.WriteError(new ErrorRecord(exception3, "ClearContentUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
            }
        }

        public object ClearContentDynamicParameters(string path)
        {
            return new FileSystemClearContentDynamicParameters();
        }

        public void ClearProperty(string path, Collection<string> propertiesToClear)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            path = NormalizePath(path);
            if ((propertiesToClear == null) || (propertiesToClear.Count == 0))
            {
                throw PSTraceSource.NewArgumentNullException("propertiesToClear");
            }
            if ((propertiesToClear.Count > 1) || (string.Compare("Attributes", propertiesToClear[0], true, base.Host.CurrentCulture) != 0))
            {
                throw PSTraceSource.NewArgumentException("propertiesToClear", "FileSystemProviderStrings", "CannotClearProperty", new object[0]);
            }
            try
            {
                FileSystemInfo info = null;
                string action = null;
                if (this.IsItemContainer(path))
                {
                    action = FileSystemProviderStrings.ClearPropertyActionDirectory;
                    info = new DirectoryInfo(path);
                }
                else
                {
                    action = FileSystemProviderStrings.ClearPropertyActionFile;
                    info = new FileInfo(path);
                }
                string clearPropertyResourceTemplate = FileSystemProviderStrings.ClearPropertyResourceTemplate;
                string target = string.Format(base.Host.CurrentCulture, clearPropertyResourceTemplate, new object[] { info.FullName, propertiesToClear[0] });
                if (base.ShouldProcess(target, action))
                {
                    info.Attributes = System.IO.FileAttributes.Normal;
                    PSObject propertyValue = new PSObject();
                    propertyValue.Properties.Add(new PSNoteProperty(propertiesToClear[0], info.Attributes));
                    base.WritePropertyObject(propertyValue, path);
                }
            }
            catch (UnauthorizedAccessException exception)
            {
                base.WriteError(new ErrorRecord(exception, "ClearPropertyUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
            }
            catch (ArgumentException exception2)
            {
                base.WriteError(new ErrorRecord(exception2, "ClearPropertyArgumentError", ErrorCategory.InvalidArgument, path));
            }
            catch (IOException exception3)
            {
                base.WriteError(new ErrorRecord(exception3, "ClearPropertyIOError", ErrorCategory.WriteError, path));
            }
        }

        public object ClearPropertyDynamicParameters(string path, Collection<string> propertiesToClear)
        {
            return null;
        }

        protected override bool ConvertPath(string path, string filter, ref string updatedPath, ref string updatedFilter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                char ch = '\\';
                if (!path.Contains(ch.ToString()))
                {
                    char ch2 = '/';
                    if (!path.Contains(ch2.ToString()) && !path.Contains("`"))
                    {
                        updatedPath = path;
                        updatedFilter = Regex.Replace(path, @"\[.*?\]", "?");
                        return true;
                    }
                }
            }
            return false;
        }

        private void CopyAndDelete(DirectoryInfo directory, string destination, bool force)
        {
            if (!this.ItemExists(destination))
            {
                this.CreateDirectory(destination, false);
            }
            else if (this.ItemExists(destination) && !this.IsItemContainer(destination))
            {
                Exception exception = new IOException(StringUtil.Format(FileSystemProviderStrings.DirectoryExist, destination));
                base.WriteError(new ErrorRecord(exception, "DirectoryExist", ErrorCategory.ResourceExists, destination));
                return;
            }
            foreach (FileInfo info in directory.EnumerateFiles())
            {
                this.MoveFileInfoItem(info, Path.Combine(destination, info.Name), force, false);
            }
            foreach (DirectoryInfo info2 in directory.EnumerateDirectories())
            {
                this.CopyAndDelete(info2, Path.Combine(destination, info2.Name), force);
            }
            if (!directory.EnumerateDirectories().Any<DirectoryInfo>() && !directory.EnumerateFiles().Any<FileInfo>())
            {
                this.RemoveItem(directory.FullName, false);
            }
        }

        private void CopyDirectoryInfoItem(DirectoryInfo directory, string destination, bool recurse, bool force)
        {
            if (this.IsItemContainer(destination))
            {
                destination = this.MakePath(destination, directory.Name);
            }
            tracer.WriteLine("destination = {0}", new object[] { destination });
            string copyItemActionDirectory = FileSystemProviderStrings.CopyItemActionDirectory;
            string target = StringUtil.Format(FileSystemProviderStrings.CopyItemResourceFileTemplate, directory.FullName, destination);
            if (base.ShouldProcess(target, copyItemActionDirectory))
            {
                this.CreateDirectory(destination, true);
                if (recurse)
                {
                    IEnumerable<FileInfo> enumerable = null;
                    if (string.IsNullOrEmpty(base.Filter))
                    {
                        enumerable = directory.EnumerateFiles();
                    }
                    else
                    {
                        enumerable = directory.EnumerateFiles(base.Filter);
                    }
                    foreach (FileInfo info in enumerable)
                    {
                        if (base.Stopping)
                        {
                            return;
                        }
                        if (info != null)
                        {
                            try
                            {
                                this.CopyFileInfoItem(info, destination, force);
                            }
                            catch (ArgumentException exception)
                            {
                                base.WriteError(new ErrorRecord(exception, "CopyDirectoryInfoItemArgumentError", ErrorCategory.InvalidArgument, info));
                            }
                            catch (IOException exception2)
                            {
                                base.WriteError(new ErrorRecord(exception2, "CopyDirectoryInfoItemIOError", ErrorCategory.WriteError, info));
                            }
                            catch (UnauthorizedAccessException exception3)
                            {
                                base.WriteError(new ErrorRecord(exception3, "CopyDirectoryInfoItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, info));
                            }
                        }
                    }
                    foreach (DirectoryInfo info2 in directory.EnumerateDirectories())
                    {
                        if (base.Stopping)
                        {
                            break;
                        }
                        if (info2 != null)
                        {
                            try
                            {
                                this.CopyDirectoryInfoItem(info2, destination, recurse, force);
                            }
                            catch (ArgumentException exception4)
                            {
                                base.WriteError(new ErrorRecord(exception4, "CopyDirectoryInfoItemArgumentError", ErrorCategory.InvalidArgument, info2));
                            }
                            catch (IOException exception5)
                            {
                                base.WriteError(new ErrorRecord(exception5, "CopyDirectoryInfoItemIOError", ErrorCategory.WriteError, info2));
                            }
                            catch (UnauthorizedAccessException exception6)
                            {
                                base.WriteError(new ErrorRecord(exception6, "CopyDirectoryInfoItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, info2));
                            }
                        }
                    }
                }
            }
        }

        private void CopyFileInfoItem(FileInfo file, string destinationPath, bool force)
        {
            if (this.IsItemContainer(destinationPath))
            {
                destinationPath = this.MakePath(destinationPath, file.Name);
            }
            if (destinationPath.Equals(file.FullName, StringComparison.OrdinalIgnoreCase))
            {
                Exception exception = new IOException(StringUtil.Format(FileSystemProviderStrings.CopyError, destinationPath));
                base.WriteError(new ErrorRecord(exception, "CopyError", ErrorCategory.WriteError, destinationPath));
            }
            else if (IsReservedDeviceName(destinationPath))
            {
                Exception exception2 = new IOException(StringUtil.Format(FileSystemProviderStrings.TargetCannotContainDeviceName, destinationPath));
                base.WriteError(new ErrorRecord(exception2, "CopyError", ErrorCategory.WriteError, destinationPath));
            }
            else
            {
                string copyItemActionFile = FileSystemProviderStrings.CopyItemActionFile;
                string target = StringUtil.Format(FileSystemProviderStrings.CopyItemResourceFileTemplate, file.FullName, destinationPath);
                if (base.ShouldProcess(target, copyItemActionFile))
                {
                    try
                    {
                        file.CopyTo(destinationPath, true);
                        FileInfo item = new FileInfo(destinationPath);
                        base.WriteItemObject(item, destinationPath, false);
                    }
                    catch (UnauthorizedAccessException exception3)
                    {
                        if (force)
                        {
                            try
                            {
                                FileInfo info2 = null;
                                info2 = new FileInfo(destinationPath) {
                                    Attributes = info2.Attributes & ~(System.IO.FileAttributes.Hidden | System.IO.FileAttributes.ReadOnly)
                                };
                            }
                            catch (Exception exception4)
                            {
                                if ((!(exception4 is FileNotFoundException) && !(exception4 is DirectoryNotFoundException)) && ((!(exception4 is SecurityException) && !(exception4 is ArgumentException)) && !(exception4 is IOException)))
                                {
                                    throw;
                                }
                                base.WriteError(new ErrorRecord(exception3, "CopyFileInfoItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, file));
                            }
                            file.CopyTo(destinationPath, true);
                            FileInfo info3 = new FileInfo(destinationPath);
                            base.WriteItemObject(info3, destinationPath, false);
                        }
                        else
                        {
                            base.WriteError(new ErrorRecord(exception3, "CopyFileInfoItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, file));
                        }
                    }
                }
            }
        }

        protected override void CopyItem(string path, string destinationPath, bool recurse)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            if (string.IsNullOrEmpty(destinationPath))
            {
                throw PSTraceSource.NewArgumentException("destinationPath");
            }
            path = NormalizePath(path);
            destinationPath = NormalizePath(destinationPath);
            if (path.Equals(destinationPath, StringComparison.OrdinalIgnoreCase))
            {
                Exception exception = new IOException(StringUtil.Format(FileSystemProviderStrings.CopyError, path));
                base.WriteError(new ErrorRecord(exception, "CopyError", ErrorCategory.WriteError, path));
            }
            else if (this.IsItemContainer(path))
            {
                DirectoryInfo directory = new DirectoryInfo(path);
                this.CopyDirectoryInfoItem(directory, destinationPath, recurse, (bool) base.Force);
            }
            else
            {
                FileInfo file = new FileInfo(path);
                this.CopyFileInfoItem(file, destinationPath, (bool) base.Force);
            }
        }

        private void CreateDirectory(string path, bool streamOutput)
        {
            string parentPath = this.GetParentPath(path, null);
            string childName = this.GetChildName(path);
            ErrorRecord error = null;
            if ((base.Force == 0) && this.ItemExists(path, out error))
            {
                Exception exception = new IOException(StringUtil.Format(FileSystemProviderStrings.DirectoryExist, path));
                base.WriteError(new ErrorRecord(exception, "DirectoryExist", ErrorCategory.ResourceExists, path));
            }
            else if (error != null)
            {
                base.WriteError(error);
            }
            else
            {
                try
                {
                    string newItemActionDirectory = FileSystemProviderStrings.NewItemActionDirectory;
                    string target = StringUtil.Format(FileSystemProviderStrings.NewItemActionTemplate, path);
                    if (base.ShouldProcess(target, newItemActionDirectory))
                    {
                        DirectoryInfo item = new DirectoryInfo(parentPath).CreateSubdirectory(childName);
                        if (streamOutput)
                        {
                            base.WriteItemObject(item, path, true);
                        }
                    }
                }
                catch (ArgumentException exception2)
                {
                    base.WriteError(new ErrorRecord(exception2, "CreateDirectoryArgumentError", ErrorCategory.InvalidArgument, path));
                }
                catch (IOException exception3)
                {
                    if (base.Force == 0)
                    {
                        base.WriteError(new ErrorRecord(exception3, "CreateDirectoryIOError", ErrorCategory.WriteError, path));
                    }
                }
                catch (UnauthorizedAccessException exception4)
                {
                    base.WriteError(new ErrorRecord(exception4, "CreateDirectoryUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
                }
            }
        }

        private static ErrorRecord CreateErrorRecord(string path, string errorId)
        {
            return new ErrorRecord(new FileNotFoundException(StringUtil.Format(FileSystemProviderStrings.FileNotFound, path)), errorId, ErrorCategory.ObjectNotFound, null);
        }

        private bool CreateIntermediateDirectories(string path)
        {
            bool flag = false;
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            try
            {
                string root;
                Stack<string> stack = new Stack<string>();
                string strB = path;
            Label_001D:
                root = string.Empty;
                if (base.PSDriveInfo != null)
                {
                    root = base.PSDriveInfo.Root;
                }
                string parentPath = this.GetParentPath(path, root);
                if ((!string.IsNullOrEmpty(parentPath) && (string.Compare(parentPath, strB, StringComparison.OrdinalIgnoreCase) != 0)) && !this.ItemExists(parentPath))
                {
                    stack.Push(parentPath);
                    strB = parentPath;
                    if (!string.IsNullOrEmpty(strB))
                    {
                        goto Label_001D;
                    }
                }
                foreach (string str4 in stack)
                {
                    this.CreateDirectory(str4, false);
                }
                flag = true;
            }
            catch (ArgumentException exception)
            {
                base.WriteError(new ErrorRecord(exception, "CreateIntermediateDirectoriesArgumentError", ErrorCategory.InvalidArgument, path));
            }
            catch (IOException exception2)
            {
                base.WriteError(new ErrorRecord(exception2, "CreateIntermediateDirectoriesIOError", ErrorCategory.WriteError, path));
            }
            catch (UnauthorizedAccessException exception3)
            {
                base.WriteError(new ErrorRecord(exception3, "CreateIntermediateDirectoriesUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        private string CreateNormalizedRelativePathFromStack(Stack<string> normalizedPathStack)
        {
            string str = string.Empty;
            while (normalizedPathStack.Count > 0)
            {
                if (string.IsNullOrEmpty(str))
                {
                    str = normalizedPathStack.Pop();
                }
                else
                {
                    string parent = normalizedPathStack.Pop();
                    str = this.MakePath(parent, str);
                }
            }
            tracer.WriteLine("result = {0}", new object[] { str });
            return str;
        }

        private void Dir(DirectoryInfo directory, bool recurse, bool nameOnly, ReturnContainers returnContainers)
        {
            List<IEnumerable<FileSystemInfo>> list = new List<IEnumerable<FileSystemInfo>>();
            try
            {
                if ((base.Filter != null) && (base.Filter.Length > 0))
                {
                    if (returnContainers == ReturnContainers.ReturnAllContainers)
                    {
                        list.Add(directory.EnumerateDirectories());
                    }
                    else
                    {
                        list.Add(directory.EnumerateDirectories(base.Filter));
                    }
                    if (base.Stopping)
                    {
                        return;
                    }
                    list.Add(directory.EnumerateFiles(base.Filter));
                }
                else
                {
                    list.Add(directory.EnumerateDirectories());
                    if (base.Stopping)
                    {
                        return;
                    }
                    list.Add(directory.EnumerateFiles());
                }
                FlagsExpression<System.IO.FileAttributes> attributes = null;
                FlagsExpression<System.IO.FileAttributes> expression2 = null;
                GetChildDynamicParameters dynamicParameters = base.DynamicParameters as GetChildDynamicParameters;
                if (dynamicParameters != null)
                {
                    attributes = dynamicParameters.Attributes;
                    expression2 = this.FormatAttributeSwitchParamters();
                }
                foreach (IEnumerable<FileSystemInfo> enumerable in list)
                {
                    foreach (FileSystemInfo info in enumerable)
                    {
                        if (base.Stopping)
                        {
                            return;
                        }
                        bool flag = true;
                        bool flag2 = true;
                        bool flag3 = false;
                        bool flag4 = false;
                        if (attributes != null)
                        {
                            flag = attributes.Evaluate(info.Attributes);
                            flag3 = attributes.ExistsInExpression(System.IO.FileAttributes.Hidden);
                        }
                        if (expression2 != null)
                        {
                            flag2 = expression2.Evaluate(info.Attributes);
                            flag4 = expression2.ExistsInExpression(System.IO.FileAttributes.Hidden);
                        }
                        bool flag5 = false;
                        if (base.Force == 0)
                        {
                            flag5 = (info.Attributes & System.IO.FileAttributes.Hidden) != 0;
                        }
                        if (((flag && flag2) || ((returnContainers == ReturnContainers.ReturnAllContainers) && ((info.Attributes & System.IO.FileAttributes.Directory) != 0))) && ((flag3 || flag4) || ((base.Force != 0) || !flag5)))
                        {
                            if (nameOnly)
                            {
                                base.WriteItemObject(info.Name, info.FullName, false);
                            }
                            else if (info is FileInfo)
                            {
                                base.WriteItemObject(info, info.FullName, false);
                            }
                            else
                            {
                                base.WriteItemObject(info, info.FullName, true);
                            }
                        }
                    }
                }
                bool flag7 = false;
                bool flag8 = false;
                if (attributes != null)
                {
                    flag7 = attributes.ExistsInExpression(System.IO.FileAttributes.Hidden);
                }
                if (expression2 != null)
                {
                    flag8 = expression2.ExistsInExpression(System.IO.FileAttributes.Hidden);
                }
                if (recurse)
                {
                    foreach (DirectoryInfo info2 in directory.EnumerateDirectories())
                    {
                        if (base.Stopping)
                        {
                            return;
                        }
                        bool flag9 = false;
                        if (base.Force == 0)
                        {
                            flag9 = (info2.Attributes & System.IO.FileAttributes.Hidden) != 0;
                        }
                        if (((base.Force != 0) || !flag9) || (flag7 || flag8))
                        {
                            this.Dir(info2, recurse, nameOnly, returnContainers);
                        }
                    }
                }
            }
            catch (ArgumentException exception)
            {
                base.WriteError(new ErrorRecord(exception, "DirArgumentError", ErrorCategory.InvalidArgument, directory.FullName));
            }
            catch (IOException exception2)
            {
                base.WriteError(new ErrorRecord(exception2, "DirIOError", ErrorCategory.ReadError, directory.FullName));
            }
            catch (UnauthorizedAccessException exception3)
            {
                base.WriteError(new ErrorRecord(exception3, "DirUnauthorizedAccessError", ErrorCategory.PermissionDenied, directory.FullName));
            }
        }

        private static bool DirectoryInfoHasChildItems(DirectoryInfo directory)
        {
            bool flag = false;
            if (directory.EnumerateFileSystemInfos().Any<FileSystemInfo>())
            {
                flag = true;
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        private static string EnsureDriveIsRooted (string path)
		{
			string str = path;
			if (OSHelper.IsWindows) {
				int index = path.IndexOf (':');
				if ((index != -1) && ((index + 1) == path.Length)) {
					str = path + '\\';
				}
			}
            tracer.WriteLine("result = {0}", new object[] { str });
            return str;
        }

        private FlagsExpression<System.IO.FileAttributes> FormatAttributeSwitchParamters()
        {
            FlagsExpression<System.IO.FileAttributes> expression = null;
            StringBuilder builder = new StringBuilder();
            if (((GetChildDynamicParameters) base.DynamicParameters).Directory != 0)
            {
                builder.Append("+Directory");
            }
            if (((GetChildDynamicParameters) base.DynamicParameters).File != 0)
            {
                builder.Append("+!Directory");
            }
            if (((GetChildDynamicParameters) base.DynamicParameters).System != 0)
            {
                builder.Append("+System");
            }
            if (((GetChildDynamicParameters) base.DynamicParameters).ReadOnly != 0)
            {
                builder.Append("+ReadOnly");
            }
            if (((GetChildDynamicParameters) base.DynamicParameters).Hidden != 0)
            {
                builder.Append("+Hidden");
            }
            string str = builder.ToString();
            if (!string.IsNullOrEmpty(str))
            {
                expression = new FlagsExpression<System.IO.FileAttributes>(str.Substring(1));
            }
            return expression;
        }

        protected override void GetChildItems(string path, bool recurse)
        {
            this.GetPathItems(path, recurse, false, ReturnContainers.ReturnMatchingContainers);
        }

        protected override object GetChildItemsDynamicParameters(string path, bool recurse)
        {
            return new GetChildDynamicParameters();
        }

        protected override string GetChildName (string path)
		{
			if (string.IsNullOrEmpty (path)) {
				throw PSTraceSource.NewArgumentException ("path");
			}
			char ch1 = (OSHelper.IsUnix ? '/' : '\\');
			if (OSHelper.IsWindows) {
				path = path.Replace ('/', '\\');
			}
			path = path.TrimEnd(new char[] { ch1 });
            string str = null;
			int num = path.LastIndexOf(ch1);
            if (num == -1)
            {
                str = EnsureDriveIsRooted(path);
            }
            else if (IsUNCPath(path))
            {
                if (IsUNCRoot(path))
                {
                    str = string.Empty;
                }
                else
                {
                    str = path.Substring(num + 1);
                }
            }
            else
            {
                str = path.Substring(num + 1);
            }
            tracer.WriteLine("Result = {0}", new object[] { str });
            return str;
        }

        protected override void GetChildNames(string path, ReturnContainers returnContainers)
        {
            this.GetPathItems(path, false, true, returnContainers);
        }

        protected override object GetChildNamesDynamicParameters(string path)
        {
            return new GetChildDynamicParameters();
        }

        private string GetCommonBase(string path1, string path2)
        {
            while (!string.Equals(path1, path2, StringComparison.OrdinalIgnoreCase))
            {
                if (path2.Length > path1.Length)
                {
                    path2 = this.GetParentPath(path2, null);
                }
                else
                {
                    path1 = this.GetParentPath(path1, null);
                }
            }
            return path1;
        }

        public IContentReader GetContentReader(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            path = NormalizePath(path);
            string delimiter = "\n";
            Encoding encodingType = Encoding.Default;
            bool waitForChanges = false;
            bool usingByteEncoding = false;
            bool delimiterSpecified = false;
            bool isRawStream = false;
            string streamName = null;
            if (base.DynamicParameters != null)
            {
                FileSystemContentReaderDynamicParameters dynamicParameters = base.DynamicParameters as FileSystemContentReaderDynamicParameters;
                if (dynamicParameters != null)
                {
                    this.ValidateParameters((bool) dynamicParameters.Raw);
                    isRawStream = (bool) dynamicParameters.Raw;
                    delimiterSpecified = dynamicParameters.DelimiterSpecified;
                    if (delimiterSpecified)
                    {
                        delimiter = dynamicParameters.Delimiter;
                    }
                    usingByteEncoding = dynamicParameters.UsingByteEncoding;
                    if (dynamicParameters.WasStreamTypeSpecified)
                    {
                        encodingType = dynamicParameters.EncodingType;
                    }
                    waitForChanges = (bool) dynamicParameters.Wait;
                    streamName = dynamicParameters.Stream;
                }
            }
            int index = path.IndexOf(':');
            int startIndex = path.IndexOf(':', index + 1);
            if (startIndex > 0)
            {
                streamName = path.Substring(startIndex + 1);
                path = path.Remove(startIndex);
            }
            FileSystemContentReaderWriter writer = null;
            try
            {
                if (delimiterSpecified)
                {
                    if (usingByteEncoding)
                    {
                        Exception exception = new ArgumentException(FileSystemProviderStrings.DelimiterError, "delimiter");
                        base.WriteError(new ErrorRecord(exception, "GetContentReaderArgumentError", ErrorCategory.InvalidArgument, path));
                        return writer;
                    }
                    return new FileSystemContentReaderWriter(path, streamName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, delimiter, encodingType, waitForChanges, this, isRawStream);
                }
                writer = new FileSystemContentReaderWriter(path, streamName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, encodingType, usingByteEncoding, waitForChanges, this, isRawStream);
            }
            catch (PathTooLongException exception2)
            {
                base.WriteError(new ErrorRecord(exception2, "GetContentReaderPathTooLongError", ErrorCategory.InvalidArgument, path));
            }
            catch (FileNotFoundException exception3)
            {
                base.WriteError(new ErrorRecord(exception3, "GetContentReaderFileNotFoundError", ErrorCategory.ObjectNotFound, path));
            }
            catch (DirectoryNotFoundException exception4)
            {
                base.WriteError(new ErrorRecord(exception4, "GetContentReaderDirectoryNotFoundError", ErrorCategory.ObjectNotFound, path));
            }
            catch (ArgumentException exception5)
            {
                base.WriteError(new ErrorRecord(exception5, "GetContentReaderArgumentError", ErrorCategory.InvalidArgument, path));
            }
            catch (IOException exception6)
            {
                base.WriteError(new ErrorRecord(exception6, "GetContentReaderIOError", ErrorCategory.ReadError, path));
            }
            catch (SecurityException exception7)
            {
                base.WriteError(new ErrorRecord(exception7, "GetContentReaderSecurityError", ErrorCategory.PermissionDenied, path));
            }
            catch (UnauthorizedAccessException exception8)
            {
                base.WriteError(new ErrorRecord(exception8, "GetContentReaderUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
            }
            return writer;
        }

        public object GetContentReaderDynamicParameters(string path)
        {
            return new FileSystemContentReaderDynamicParameters();
        }

        public IContentWriter GetContentWriter(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            path = NormalizePath(path);
            bool usingByteEncoding = false;
            Encoding encodingType = Encoding.Default;
            FileMode openOrCreate = FileMode.OpenOrCreate;
            string streamName = null;
            if (base.DynamicParameters != null)
            {
                FileSystemContentWriterDynamicParameters dynamicParameters = base.DynamicParameters as FileSystemContentWriterDynamicParameters;
                if (dynamicParameters != null)
                {
                    usingByteEncoding = dynamicParameters.UsingByteEncoding;
                    if (dynamicParameters.WasStreamTypeSpecified)
                    {
                        encodingType = dynamicParameters.EncodingType;
                    }
                    streamName = dynamicParameters.Stream;
                }
            }
            int index = path.IndexOf(':');
            int startIndex = path.IndexOf(':', index + 1);
            if (startIndex > 0)
            {
                streamName = path.Substring(startIndex + 1);
                path = path.Remove(startIndex);
            }
            FileSystemContentReaderWriter writer = null;
            try
            {
                writer = new FileSystemContentReaderWriter(path, streamName, openOrCreate, FileAccess.Write, FileShare.Write, encodingType, usingByteEncoding, false, this, false);
            }
            catch (PathTooLongException exception)
            {
                base.WriteError(new ErrorRecord(exception, "GetContentWriterPathTooLongError", ErrorCategory.InvalidArgument, path));
            }
            catch (FileNotFoundException exception2)
            {
                base.WriteError(new ErrorRecord(exception2, "GetContentWriterFileNotFoundError", ErrorCategory.ObjectNotFound, path));
            }
            catch (DirectoryNotFoundException exception3)
            {
                base.WriteError(new ErrorRecord(exception3, "GetContentWriterDirectoryNotFoundError", ErrorCategory.ObjectNotFound, path));
            }
            catch (ArgumentException exception4)
            {
                base.WriteError(new ErrorRecord(exception4, "GetContentWriterArgumentError", ErrorCategory.InvalidArgument, path));
            }
            catch (IOException exception5)
            {
                base.WriteError(new ErrorRecord(exception5, "GetContentWriterIOError", ErrorCategory.WriteError, path));
            }
            catch (SecurityException exception6)
            {
                base.WriteError(new ErrorRecord(exception6, "GetContentWriterSecurityError", ErrorCategory.PermissionDenied, path));
            }
            catch (UnauthorizedAccessException exception7)
            {
                base.WriteError(new ErrorRecord(exception7, "GetContentWriterUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
            }
            return writer;
        }

        public object GetContentWriterDynamicParameters(string path)
        {
            return new FileSystemContentWriterDynamicParameters();
        }

        private static FileSystemInfo GetFileSystemInfo(string path, ref bool isContainer)
        {
            isContainer = false;
            if (NativeFileExists(path))
            {
                return new FileInfo(path);
            }
            if (NativeDirectoryExists(path))
            {
                isContainer = true;
                return new DirectoryInfo(path);
            }
            return null;
        }

        private FileSystemInfo GetFileSystemItem(string path, ref bool isContainer, bool showHidden)
        {
            path = NormalizePath(path);
            FileSystemInfo info = null;
            int num = SafeGetFileAttributes(path);
            bool flag = num != -1;
            bool flag2 = (num & 0x10) == 0x10;
            bool flag3 = (num & 2) == 2;
            FlagsExpression<System.IO.FileAttributes> attributes = null;
            FlagsExpression<System.IO.FileAttributes> expression2 = null;
            GetChildDynamicParameters dynamicParameters = base.DynamicParameters as GetChildDynamicParameters;
            if (dynamicParameters != null)
            {
                attributes = dynamicParameters.Attributes;
                expression2 = this.FormatAttributeSwitchParamters();
            }
            bool flag4 = false;
            bool flag5 = false;
            if (attributes != null)
            {
                flag4 = attributes.ExistsInExpression(System.IO.FileAttributes.Hidden);
            }
            if (expression2 != null)
            {
                flag5 = expression2.ExistsInExpression(System.IO.FileAttributes.Hidden);
            }
            if ((flag && !flag2) && ((!flag3 || (base.Force != 0)) || ((showHidden || flag4) || flag5)))
            {
                FileInfo info2 = new FileInfo(path);
                info = info2;
                tracer.WriteLine("Got FileInfo: {0}", new object[] { info2 });
                return info;
            }
            DirectoryInfo info3 = new DirectoryInfo(path);
            bool flag6 = string.Compare(Path.GetPathRoot(path), info3.FullName, StringComparison.OrdinalIgnoreCase) == 0;
            if (flag && (((flag6 || !flag3) || ((base.Force != 0) || showHidden)) || (flag4 || flag5)))
            {
                info = info3;
                isContainer = true;
                tracer.WriteLine("Got DirectoryInfo: {0}", new object[] { info3 });
            }
            return info;
        }

        public string GetHelpMaml(string helpItemName, string path)
        {
            string verb = null;
            string noun = null;
            try
            {
                if (!string.IsNullOrEmpty(helpItemName))
                {
                    CmdletInfo.SplitCmdletName(helpItemName, out verb, out noun);
                }
                else
                {
                    return string.Empty;
                }
                if (string.IsNullOrEmpty(verb) || string.IsNullOrEmpty(noun))
                {
                    return string.Empty;
                }
                XmlDocument document = new XmlDocument();
                string inputUri = Path.Combine(string.IsNullOrEmpty(base.ProviderInfo.ApplicationBase) ? "" : base.ProviderInfo.ApplicationBase, Thread.CurrentThread.CurrentUICulture.ToString(), string.IsNullOrEmpty(base.ProviderInfo.HelpFile) ? "" : base.ProviderInfo.HelpFile);
                XmlReaderSettings settings = new XmlReaderSettings {
                    XmlResolver = null
                };
				if (!File.Exists (inputUri)) return string.Empty;
                XmlReader reader = XmlReader.Create(inputUri, settings);
                document.Load(reader);
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
                nsmgr.AddNamespace("command", HelpCommentsParser.commandURI);
                string xpath = string.Format(CultureInfo.InvariantCulture, HelpCommentsParser.ProviderHelpCommandXPath, new object[] { "[@id='FileSystem']", verb, noun });
                System.Xml.XmlNode node = document.SelectSingleNode(xpath, nsmgr);
                if (node != null)
                {
                    return node.OuterXml;
                }
            }
            catch (XmlException)
            {
                return string.Empty;
            }
            catch (PathTooLongException)
            {
                return string.Empty;
            }
            catch (IOException)
            {
                return string.Empty;
            }
            catch (UnauthorizedAccessException)
            {
                return string.Empty;
            }
            catch (NotSupportedException)
            {
                return string.Empty;
            }
            catch (SecurityException)
            {
                return string.Empty;
            }
            catch (XPathException)
            {
                return string.Empty;
            }
            return string.Empty;
        }

        protected override void GetItem(string path)
        {
            bool isContainer = false;
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            try
            {
                bool flag2 = false;
                FileSystemProviderGetItemDynamicParameters dynamicParameters = null;
                if (base.DynamicParameters != null)
                {
                    dynamicParameters = base.DynamicParameters as FileSystemProviderGetItemDynamicParameters;
                    if (dynamicParameters != null)
                    {
                        if ((dynamicParameters.Stream != null) && (dynamicParameters.Stream.Length > 0))
                        {
                            flag2 = true;
                        }
                        else
                        {
                            int index = path.IndexOf(':');
                            int startIndex = path.IndexOf(':', index + 1);
                            if (startIndex > 0)
                            {
                                string str = path.Substring(startIndex + 1);
                                path = path.Remove(startIndex);
                                flag2 = true;
                                dynamicParameters = new FileSystemProviderGetItemDynamicParameters {
                                    Stream = new string[] { str }
                                };
                            }
                        }
                    }
                }
                FileSystemInfo item = this.GetFileSystemItem(path, ref isContainer, false);
                if (item != null)
                {
                    if (flag2)
                    {
                        if (!isContainer)
                        {
                            foreach (string str2 in dynamicParameters.Stream)
                            {
                                WildcardPattern pattern = new WildcardPattern(str2, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
                                bool flag3 = false;
                                foreach (AlternateStreamData data in AlternateDataStreamUtilities.GetStreams(item.FullName))
                                {
                                    if (pattern.IsMatch(data.Stream))
                                    {
                                        string str3 = item.FullName + ":" + data.Stream;
                                        base.WriteItemObject(data, str3, isContainer);
                                        flag3 = true;
                                    }
                                }
                                if (!WildcardPattern.ContainsWildcardCharacters(str2) && !flag3)
                                {
                                    Exception exception = new FileNotFoundException(StringUtil.Format(FileSystemProviderStrings.AlternateDataStreamNotFound, str2, item.FullName), item.FullName);
                                    base.WriteError(new ErrorRecord(exception, "AlternateDataStreamNotFound", ErrorCategory.ObjectNotFound, path));
                                }
                            }
                        }
                    }
                    else
                    {
                        base.WriteItemObject(item, item.FullName, isContainer);
                    }
                }
                else
                {
                    Exception exception2 = new IOException(StringUtil.Format(FileSystemProviderStrings.ItemNotFound, path));
                    base.WriteError(new ErrorRecord(exception2, "ItemNotFound", ErrorCategory.ObjectNotFound, path));
                }
            }
            catch (IOException exception3)
            {
                ErrorRecord errorRecord = new ErrorRecord(exception3, "GetItemIOError", ErrorCategory.ReadError, path);
                base.WriteError(errorRecord);
            }
            catch (UnauthorizedAccessException exception4)
            {
                base.WriteError(new ErrorRecord(exception4, "GetItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
            }
        }

        protected override object GetItemDynamicParameters(string path)
        {
            return new FileSystemProviderGetItemDynamicParameters();
        }

        private static ItemType GetItemType(string input)
        {
            ItemType unknown = ItemType.Unknown;
            WildcardPattern pattern = new WildcardPattern(input + "*", WildcardOptions.IgnoreCase | WildcardOptions.Compiled);
            if (pattern.IsMatch("directory") || pattern.IsMatch("container"))
            {
                return ItemType.Directory;
            }
            if (pattern.IsMatch("file"))
            {
                unknown = ItemType.File;
            }
            return unknown;
        }

        protected override string GetParentPath(string path, string root)
        {
            string parentPath = base.GetParentPath(path, root);
            if (IsUNCPath(path))
            {
                if (parentPath.LastIndexOf('\\') < 3)
                {
                    parentPath = string.Empty;
                }
                return parentPath;
            }
            return EnsureDriveIsRooted(parentPath);
        }

        private void GetPathItems(string path, bool recurse, bool nameOnly, ReturnContainers returnContainers)
        {
            bool flag;
			if (string.IsNullOrEmpty (path) && OSHelper.IsUnix) path = "/";
            else if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            path = NormalizePath(path);
            if (NativeItemExists(path, out flag))
            {
                if (flag)
                {
                    DirectoryInfo directory = new DirectoryInfo(path);
                    this.Dir(directory, recurse, nameOnly, returnContainers);
                }
                else
                {
                    FileInfo item = new FileInfo(path);
                    FlagsExpression<System.IO.FileAttributes> attributes = null;
                    FlagsExpression<System.IO.FileAttributes> expression2 = null;
                    GetChildDynamicParameters dynamicParameters = base.DynamicParameters as GetChildDynamicParameters;
                    if (dynamicParameters != null)
                    {
                        attributes = dynamicParameters.Attributes;
                        expression2 = this.FormatAttributeSwitchParamters();
                    }
                    bool flag3 = true;
                    bool flag4 = true;
                    bool flag5 = false;
                    bool flag6 = false;
                    if (attributes != null)
                    {
                        flag3 = attributes.Evaluate(item.Attributes);
                        flag5 = attributes.ExistsInExpression(System.IO.FileAttributes.Hidden);
                    }
                    if (expression2 != null)
                    {
                        flag4 = expression2.Evaluate(item.Attributes);
                        flag6 = expression2.ExistsInExpression(System.IO.FileAttributes.Hidden);
                    }
                    bool flag7 = (item.Attributes & System.IO.FileAttributes.Hidden) != 0;
                    if ((flag3 && flag4) && ((flag5 || flag6) || ((base.Force != 0) || !flag7)))
                    {
                        if (nameOnly)
                        {
                            base.WriteItemObject(item.Name, item.FullName, false);
                        }
                        else
                        {
                            base.WriteItemObject(item, path, false);
                        }
                    }
                }
            }
            else
            {
                Exception exception = new IOException(StringUtil.Format(FileSystemProviderStrings.ItemDoesNotExist, path));
                base.WriteError(new ErrorRecord(exception, "ItemDoesNotExist", ErrorCategory.ObjectNotFound, path));
            }
        }

        public void GetProperty(string path, Collection<string> providerSpecificPickList)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            path = NormalizePath(path);
            PSObject propertyValue = null;
            try
            {
                bool flag;
                FileSystemInfo info = null;
                if (NativeItemExists(path, out flag))
                {
                    if (flag)
                    {
                        info = new DirectoryInfo(path);
                    }
                    else
                    {
                        info = new FileInfo(path);
                    }
                }
                if (info == null)
                {
                    Exception exception = new IOException(StringUtil.Format(FileSystemProviderStrings.ItemDoesNotExist, path));
                    base.WriteError(new ErrorRecord(exception, "ItemDoesNotExist", ErrorCategory.ObjectNotFound, path));
                }
                else if ((providerSpecificPickList == null) || (providerSpecificPickList.Count == 0))
                {
                    propertyValue = PSObject.AsPSObject(info);
                }
                else
                {
                    foreach (string str2 in providerSpecificPickList)
                    {
                        if ((str2 != null) && (str2.Length > 0))
                        {
                            try
                            {
                                PSMemberInfo info2 = PSObject.AsPSObject(info).Properties[str2];
                                if (info2 != null)
                                {
                                    object obj4 = info2.Value;
                                    if (propertyValue == null)
                                    {
                                        propertyValue = new PSObject();
                                    }
                                    propertyValue.Properties.Add(new PSNoteProperty(str2, obj4));
                                }
                                else
                                {
                                    Exception exception2 = new IOException(StringUtil.Format(FileSystemProviderStrings.PropertyNotFound, str2));
                                    base.WriteError(new ErrorRecord(exception2, "GetValueError", ErrorCategory.ReadError, str2));
                                }
                            }
                            catch (GetValueException exception3)
                            {
                                base.WriteError(new ErrorRecord(exception3, "GetValueError", ErrorCategory.ReadError, str2));
                            }
                        }
                    }
                }
            }
            catch (ArgumentException exception4)
            {
                base.WriteError(new ErrorRecord(exception4, "GetPropertyArgumentError", ErrorCategory.InvalidArgument, path));
            }
            catch (IOException exception5)
            {
                base.WriteError(new ErrorRecord(exception5, "GetPropertyIOError", ErrorCategory.ReadError, path));
            }
            catch (UnauthorizedAccessException exception6)
            {
                base.WriteError(new ErrorRecord(exception6, "GetPropertyUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
            }
            if (propertyValue != null)
            {
                base.WritePropertyObject(propertyValue, path);
            }
        }

        public object GetPropertyDynamicParameters(string path, Collection<string> providerSpecificPickList)
        {
            return null;
        }

        public void GetSecurityDescriptor(string path, AccessControlSections sections)
        {
            ObjectSecurity securityDescriptor = null;
            path = NormalizePath(path);
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if ((sections & ~AccessControlSections.All) != AccessControlSections.None)
            {
                throw PSTraceSource.NewArgumentException("sections");
            }
            try
            {
                if (Directory.Exists(path))
                {
                    securityDescriptor = new DirectorySecurity(path, sections);
                }
                else
                {
                    securityDescriptor = new FileSecurity(path, sections);
                }
            }
            catch (SecurityException exception)
            {
                base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.PermissionDenied, path));
            }
            base.WriteSecurityDescriptorObject(securityDescriptor, path);
        }

        internal static string GetUNCForNetworkDrive(string driveName)
        {
            string str = null;
            if (string.IsNullOrEmpty(driveName) || (driveName.Length != 1))
            {
                return str;
            }
            int capacity = 300;
            StringBuilder remoteName = new StringBuilder(capacity);
            driveName = driveName + ':';
            int error = NativeMethods.WNetGetConnection(driveName, remoteName, ref capacity);
            if (error == 0xea)
            {
                remoteName = new StringBuilder(capacity);
                error = NativeMethods.WNetGetConnection(driveName, remoteName, ref capacity);
            }
            if (error != 0)
            {
                throw new Win32Exception(error);
            }
            return remoteName.ToString();
        }

        protected override bool HasChildItems(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            path = NormalizePath(path);
            try
            {
                DirectoryInfo directory = new DirectoryInfo(path);
                return DirectoryInfoHasChildItems(directory);
            }
            catch (ArgumentNullException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            Collection<PSDriveInfo> collection = new Collection<PSDriveInfo>();
            DriveInfo[] drives = DriveInfo.GetDrives();
            if (drives != null)
            {
                foreach (DriveInfo info in drives)
                {
                    if (base.Stopping)
                    {
                        collection.Clear();
                        return collection;
                    }
					string driveName = null;
					if (OSHelper.IsUnix)
					{
						driveName = info.Name;
					}
					else {
						driveName = info.Name.Substring(0, 1);
					}
                    string description = string.Empty;
                    string name = info.Name;
                    string displayRoot = null;
                    if (info.DriveType == DriveType.Fixed)
                    {
                        try
                        {
                            description = info.VolumeLabel;
                        }
                        catch (IOException)
                        {
                        }
                        catch (SecurityException)
                        {
                        }
                        catch (UnauthorizedAccessException)
                        {
                        }
                    }
                    if (info.DriveType == DriveType.Network)
                    {
                        displayRoot = GetUNCForNetworkDrive(driveName);
                    }
                    try
                    {
						if (info.DriveType == DriveType.Fixed) /*  && Environment.OSVersion.Platform != PlatformID.MacOSX && Environment.OSVersion.Platform != PlatformID.Unix */
                        {
                            if (!info.RootDirectory.Exists)
                            {
                                continue;
                            }
                            name = info.RootDirectory.FullName;
                        }
                        PSDriveInfo item = new PSDriveInfo(driveName, base.ProviderInfo, name, description, null, displayRoot);
                        if (info.DriveType == DriveType.Network)
                        {
                            item.IsNetworkDrive = true;
                        }
                        if (info.DriveType != DriveType.Fixed)
                        {
                            item.IsAutoMounted = true;
                        }
                        collection.Add(item);
                    }
                    catch (IOException)
                    {
                    }
                    catch (SecurityException)
                    {
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                }
            }
            return collection;
        }

        protected override void InvokeDefaultAction(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            path = NormalizePath(path);
            string invokeItemAction = FileSystemProviderStrings.InvokeItemAction;
            string target = StringUtil.Format(FileSystemProviderStrings.InvokeItemResourceFileTemplate, path);
            if (base.ShouldProcess(target, invokeItemAction))
            {
                Process.Start(path);
            }
        }

        private static bool IsAbsolutePath (string path)
		{
			bool flag = false;
			if (OSHelper.IsUnix) {
				flag = path.StartsWith("/", StringComparison.OrdinalIgnoreCase);
			}
            else if (path.IndexOf(':') != -1)
            {
                flag = true;
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        internal override bool IsFilterSet()
        {
            bool flag = false;
            GetChildDynamicParameters dynamicParameters = base.DynamicParameters as GetChildDynamicParameters;
            if (dynamicParameters != null)
            {
                flag = ((((dynamicParameters.Attributes != null) || (dynamicParameters.Directory != 0)) || ((dynamicParameters.File != 0) || (dynamicParameters.Hidden != 0))) || (dynamicParameters.ReadOnly != 0)) || ((bool) dynamicParameters.System);
            }
            if (!flag)
            {
                return base.IsFilterSet();
            }
            return true;
        }

        protected override bool IsItemContainer(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            path = NormalizePath(path);
            return NativeDirectoryExists(path);
        }

        private static bool IsPathRoot(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            bool flag = string.Equals(path, Path.GetPathRoot(path), StringComparison.OrdinalIgnoreCase);
            bool flag2 = IsUNCRoot(path);
            bool flag3 = flag || flag2;
            tracer.WriteLine("result = {0}; isDriveRoot = {1}; isUNCRoot = {2}", new object[] { flag3, flag, flag2 });
            return flag3;
        }

        private static bool IsReservedDeviceName(string destinationPath)
        {
            string[] strArray = new string[] { 
                "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", 
                "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
             };
            string fileName = Path.GetFileName(destinationPath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(destinationPath);
            if (((fileName.Length >= 3) && (fileName.Length <= 6)) || ((fileNameWithoutExtension.Length >= 3) && (fileNameWithoutExtension.Length <= 6)))
            {
                foreach (string str3 in strArray)
                {
                    if (string.Equals(str3, fileName, StringComparison.OrdinalIgnoreCase) || string.Equals(str3, fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsSameVolume(string source, string destination)
        {
            FileInfo info = new FileInfo(source);
            FileInfo info2 = new FileInfo(destination);
            return (info.Directory.Root.Name == info2.Directory.Root.Name);
        }

        private bool IsSupportedDriveForPersistence(PSDriveInfo drive)
        {
            bool flag = false;
            if (((drive != null) && !string.IsNullOrEmpty(drive.Name)) && (drive.Name.Length == 1))
            {
                char c = Convert.ToChar(drive.Name, CultureInfo.InvariantCulture);
                if ((char.ToUpper(c, CultureInfo.InvariantCulture) >= 'A') && (char.ToUpper(c, CultureInfo.InvariantCulture) <= 'Z'))
                {
                    flag = true;
                }
            }
            return flag;
        }

        private static bool IsUNCPath(string path)
        {
            bool flag = false;
            if (path.StartsWith(@"\\", StringComparison.Ordinal))
            {
                flag = true;
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        private static bool IsUNCRoot(string path)
        {
            bool flag = false;
            if (!string.IsNullOrEmpty(path) && IsUNCPath(path))
            {
                int startIndex = path.Length - 1;
                if (path[path.Length - 1] == '\\')
                {
                    startIndex--;
                }
                int num2 = 0;
                do
                {
                    startIndex = path.LastIndexOf('\\', startIndex);
                    if (startIndex == -1)
                    {
                        break;
                    }
                    startIndex--;
                    if (startIndex < 3)
                    {
                        break;
                    }
                    num2++;
                }
                while (startIndex > 3);
                if (num2 == 1)
                {
                    flag = true;
                }
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        protected override bool IsValidPath (string path)
		{
			if (string.IsNullOrEmpty (path)) {
				return false;
			}
			path = NormalizePath (path);
			path = EnsureDriveIsRooted (path);
			if (OSHelper.IsWindows) {
				int index = path.IndexOf (':');
				int length = path.IndexOf (':', index + 1);
				if (length > 0) {
					path = path.Substring (0, length);
				}
			}
            if (!IsAbsolutePath(path) && !IsUNCPath(path))
            {
                return false;
            }
            try
            {
                new FileInfo(path);
            }
            catch (Exception exception)
            {
                if (((!(exception is ArgumentNullException) && !(exception is ArgumentException)) && (!(exception is SecurityException) && !(exception is UnauthorizedAccessException))) && (!(exception is PathTooLongException) && !(exception is NotSupportedException)))
                {
                    throw;
                }
                return false;
            }
            return true;
        }

        protected override bool ItemExists(string path)
        {
            ErrorRecord error = null;
            bool flag = this.ItemExists(path, out error);
            if (error != null)
            {
                base.WriteError(error);
            }
            return flag;
        }

        private bool ItemExists(string path, out ErrorRecord error)
        {
            error = null;
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            bool flag = false;
            path = NormalizePath(path);
            try
            {
                bool flag2;
                if (NativeItemExists(path, out flag2))
                {
                    flag = true;
                }
                FileSystemItemProviderDynamicParameters dynamicParameters = base.DynamicParameters as FileSystemItemProviderDynamicParameters;
                if (!flag || (dynamicParameters == null))
                {
                    return flag;
                }
                DateTime lastWriteTime = File.GetLastWriteTime(path);
                if (dynamicParameters.OlderThan.HasValue)
                {
                    flag = lastWriteTime < dynamicParameters.OlderThan.Value;
                }
                if (dynamicParameters.NewerThan.HasValue)
                {
                    flag = lastWriteTime > dynamicParameters.NewerThan.Value;
                }
            }
            catch (SecurityException exception)
            {
                error = new ErrorRecord(exception, "ItemExistsSecurityError", ErrorCategory.PermissionDenied, path);
            }
            catch (ArgumentException exception2)
            {
                error = new ErrorRecord(exception2, "ItemExistsArgumentError", ErrorCategory.InvalidArgument, path);
            }
            catch (UnauthorizedAccessException exception3)
            {
                error = new ErrorRecord(exception3, "ItemExistsUnauthorizedAccessError", ErrorCategory.PermissionDenied, path);
            }
            catch (PathTooLongException exception4)
            {
                error = new ErrorRecord(exception4, "ItemExistsPathTooLongError", ErrorCategory.InvalidArgument, path);
            }
            catch (NotSupportedException exception5)
            {
                error = new ErrorRecord(exception5, "ItemExistsNotSupportedError", ErrorCategory.InvalidOperation, path);
            }
            return flag;
        }

        protected override object ItemExistsDynamicParameters(string path)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return new FileSystemItemProviderDynamicParameters();
            }
        }

        private void MapNetworkDrive(PSDriveInfo drive)
        {
            if ((drive != null) && !string.IsNullOrEmpty(drive.Root))
            {
                int flags = 0;
                string str = null;
                byte[] password = null;
                string username = null;
                if (drive.Persist)
                {
                    if (this.IsSupportedDriveForPersistence(drive))
                    {
                        flags = 1;
                        str = drive.Name + ":";
                        drive.DisplayRoot = drive.Root;
                    }
                    else
                    {
                        ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(FileSystemProviderStrings.InvalidDriveName), "DriveNameNotSupportedForPersistence", ErrorCategory.InvalidOperation, drive);
                        base.ThrowTerminatingError(errorRecord);
                    }
                }
                if ((drive.Credential != null) && !drive.Credential.Equals(PSCredential.Empty))
                {
                    username = drive.Credential.UserName;
                    password = SecureStringHelper.GetData(drive.Credential.Password);
                }
                try
                {
                    NetResource netResource = new NetResource {
                        Comment = null,
                        DisplayType = 0,
                        LocalName = str,
                        Provider = null,
                        RemoteName = drive.Root,
                        Scope = 2,
                        Type = 0,
                        Usage = 1
                    };
                    int error = NativeMethods.WNetAddConnection2(ref netResource, password, username, flags);
                    if (error != 0)
                    {
                        ErrorRecord record2 = new ErrorRecord(new Win32Exception(error), "CouldNotMapNetworkDrive", ErrorCategory.InvalidOperation, drive);
                        base.ThrowTerminatingError(record2);
                    }
                    if (flags == 1)
                    {
                        drive.IsNetworkDrive = true;
                        drive.Root = str + @"\";
                    }
                }
                finally
                {
                    if (password != null)
                    {
                        Array.Clear(password, 0, password.Length - 1);
                    }
                }
            }
        }

        public static string Mode(PSObject instance)
        {
            if (instance == null)
            {
                return string.Empty;
            }
            FileSystemInfo baseObject = (FileSystemInfo) instance.BaseObject;
            if (baseObject == null)
            {
                return string.Empty;
            }
            string str = "";
            if ((baseObject.Attributes & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory)
            {
                str = str + "d";
            }
            else
            {
                str = str + "-";
            }
            if ((baseObject.Attributes & System.IO.FileAttributes.Archive) == System.IO.FileAttributes.Archive)
            {
                str = str + "a";
            }
            else
            {
                str = str + "-";
            }
            if ((baseObject.Attributes & System.IO.FileAttributes.ReadOnly) == System.IO.FileAttributes.ReadOnly)
            {
                str = str + "r";
            }
            else
            {
                str = str + "-";
            }
            if ((baseObject.Attributes & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden)
            {
                str = str + "h";
            }
            else
            {
                str = str + "-";
            }
            if ((baseObject.Attributes & System.IO.FileAttributes.System) == System.IO.FileAttributes.System)
            {
                return (str + "s");
            }
            return (str + "-");
        }

        private void MoveDirectoryInfoItem(DirectoryInfo directory, string destination, bool force)
        {
            try
            {
                if (!this.IsSameVolume(directory.FullName, destination))
                {
                    this.CopyAndDelete(directory, destination, force);
                }
                else
                {
                    directory.MoveTo(destination);
                }
                base.WriteItemObject(directory, directory.FullName, true);
            }
            catch (UnauthorizedAccessException exception)
            {
                if (force)
                {
                    try
                    {
                        directory.Attributes &= ~(System.IO.FileAttributes.Hidden | System.IO.FileAttributes.ReadOnly);
                        if (!this.IsSameVolume(directory.FullName, destination))
                        {
                            this.CopyAndDelete(directory, destination, force);
                        }
                        else
                        {
                            directory.MoveTo(destination);
                        }
                        base.WriteItemObject(directory, directory.FullName, true);
                    }
                    catch (IOException)
                    {
                        base.WriteError(new ErrorRecord(exception, "MoveDirectoryItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, directory));
                    }
                    catch (Exception exception2)
                    {
                        if ((!(exception2 is FileNotFoundException) && !(exception2 is ArgumentNullException)) && ((!(exception2 is DirectoryNotFoundException) && !(exception2 is SecurityException)) && !(exception2 is ArgumentException)))
                        {
                            throw;
                        }
                        base.WriteError(new ErrorRecord(exception, "MoveDirectoryItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, directory));
                    }
                }
                else
                {
                    base.WriteError(new ErrorRecord(exception, "MoveDirectoryItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, directory));
                }
            }
            catch (ArgumentException exception3)
            {
                base.WriteError(new ErrorRecord(exception3, "MoveDirectoryItemArgumentError", ErrorCategory.InvalidArgument, directory));
            }
            catch (IOException exception4)
            {
                base.WriteError(new ErrorRecord(exception4, "MoveDirectoryItemIOError", ErrorCategory.WriteError, directory));
            }
        }

        private void MoveFileInfoItem(FileInfo file, string destination, bool force, bool output)
        {
            try
            {
                file.MoveTo(destination);
                if (output)
                {
                    base.WriteItemObject(file, file.FullName, false);
                }
            }
            catch (UnauthorizedAccessException exception)
            {
                if (force)
                {
                    try
                    {
                        file.Attributes &= ~(System.IO.FileAttributes.Hidden | System.IO.FileAttributes.ReadOnly);
                        file.MoveTo(destination);
                        if (output)
                        {
                            base.WriteItemObject(file, file.FullName, false);
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (((!(exception2 is IOException) && !(exception2 is ArgumentNullException)) && (!(exception2 is ArgumentException) && !(exception2 is SecurityException))) && ((!(exception2 is UnauthorizedAccessException) && !(exception2 is FileNotFoundException)) && ((!(exception2 is DirectoryNotFoundException) && !(exception2 is PathTooLongException)) && !(exception2 is NotSupportedException))))
                        {
                            throw;
                        }
                        base.WriteError(new ErrorRecord(exception, "MoveFileInfoItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, file));
                    }
                }
                else
                {
                    base.WriteError(new ErrorRecord(exception, "MoveFileInfoItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, file));
                }
            }
            catch (ArgumentException exception3)
            {
                base.WriteError(new ErrorRecord(exception3, "MoveFileInfoItemArgumentError", ErrorCategory.InvalidArgument, file));
            }
            catch (IOException exception4)
            {
                if (force && File.Exists(destination))
                {
                    FileInfo targetObject = new FileInfo(destination);
                    if (targetObject != null)
                    {
                        try
                        {
                            targetObject.Attributes &= ~(System.IO.FileAttributes.Hidden | System.IO.FileAttributes.ReadOnly);
                            targetObject.Delete();
                            file.MoveTo(destination);
                            if (output)
                            {
                                base.WriteItemObject(file, file.FullName, false);
                            }
                        }
                        catch (Exception exception5)
                        {
                            if (((!(exception5 is FileNotFoundException) && !(exception5 is DirectoryNotFoundException)) && (!(exception5 is UnauthorizedAccessException) && !(exception5 is SecurityException))) && ((!(exception5 is ArgumentException) && !(exception5 is PathTooLongException)) && ((!(exception5 is NotSupportedException) && !(exception5 is ArgumentNullException)) && !(exception5 is IOException))))
                            {
                                throw;
                            }
                            base.WriteError(new ErrorRecord(exception4, "MoveFileInfoItemIOError", ErrorCategory.WriteError, targetObject));
                        }
                    }
                    else
                    {
                        base.WriteError(new ErrorRecord(exception4, "MoveFileInfoItemIOError", ErrorCategory.WriteError, file));
                    }
                }
                else
                {
                    base.WriteError(new ErrorRecord(exception4, "MoveFileInfoItemIOError", ErrorCategory.WriteError, file));
                }
            }
        }

        protected override void MoveItem(string path, string destination)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            if (string.IsNullOrEmpty(destination))
            {
                throw PSTraceSource.NewArgumentException("destination");
            }
            path = NormalizePath(path);
            destination = NormalizePath(destination);
            if (IsReservedDeviceName(destination))
            {
                Exception exception = new IOException(StringUtil.Format(FileSystemProviderStrings.TargetCannotContainDeviceName, destination));
                base.WriteError(new ErrorRecord(exception, "MoveError", ErrorCategory.WriteError, destination));
            }
            else
            {
                try
                {
                    bool flag = this.IsItemContainer(path);
                    tracer.WriteLine("Moving {0} to {1}", new object[] { path, destination });
                    if (flag)
                    {
                        DirectoryInfo directory = new DirectoryInfo(path);
                        if (this.ItemExists(destination) && this.IsItemContainer(destination))
                        {
                            destination = this.MakePath(destination, directory.Name);
                        }
                        string moveItemActionDirectory = FileSystemProviderStrings.MoveItemActionDirectory;
                        string target = StringUtil.Format(FileSystemProviderStrings.MoveItemResourceFileTemplate, directory.FullName, destination);
                        if (base.ShouldProcess(target, moveItemActionDirectory))
                        {
                            this.MoveDirectoryInfoItem(directory, destination, (bool) base.Force);
                        }
                    }
                    else
                    {
                        FileInfo file = new FileInfo(path);
                        if (this.IsItemContainer(destination))
                        {
                            destination = this.MakePath(destination, file.Name);
                        }
                        string moveItemActionFile = FileSystemProviderStrings.MoveItemActionFile;
                        string str5 = StringUtil.Format(FileSystemProviderStrings.MoveItemResourceFileTemplate, file.FullName, destination);
                        if (base.ShouldProcess(str5, moveItemActionFile))
                        {
                            this.MoveFileInfoItem(file, destination, (bool) base.Force, true);
                        }
                    }
                }
                catch (ArgumentException exception2)
                {
                    base.WriteError(new ErrorRecord(exception2, "MoveItemArgumentError", ErrorCategory.InvalidArgument, path));
                }
                catch (IOException exception3)
                {
                    base.WriteError(new ErrorRecord(exception3, "MoveItemIOError", ErrorCategory.WriteError, path));
                }
                catch (UnauthorizedAccessException exception4)
                {
                    base.WriteError(new ErrorRecord(exception4, "MoveItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
                }
            }
        }

        private static bool NativeDirectoryExists(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            int fileAttributes = NativeMethods.GetFileAttributes(path);
            if (fileAttributes == -1)
            {
                int error = Marshal.GetLastWin32Error();
                if (error == 5)
                {
                    Win32Exception inner = new Win32Exception(error);
                    throw new UnauthorizedAccessException(inner.Message, inner);
                }
                return false;
            }
            return ((fileAttributes & 0x10) == 0x10);
        }

        private static bool NativeFileExists(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            if (IsReservedDeviceName(path))
            {
                return false;
            }
            int fileAttributes = NativeMethods.GetFileAttributes(path);
            if (fileAttributes == -1)
            {
                int error = Marshal.GetLastWin32Error();
                if (error == 5)
                {
                    Win32Exception inner = new Win32Exception(error);
                    throw new UnauthorizedAccessException(inner.Message, inner);
                }
                return false;
            }
            bool flag = (fileAttributes & 0x10) == 0x10;
            return !flag;
        }

        private static bool NativeItemExists(string path, out bool directory)
        {
            if (string.IsNullOrEmpty(path))
            {
                directory = false;
                return false;
            }
            if (IsReservedDeviceName(path))
            {
                directory = false;
                return false;
            }
            int fileAttributes = NativeMethods.GetFileAttributes(path);
            if (fileAttributes == -1)
            {
                int error = Marshal.GetLastWin32Error();
                if (error == 5)
                {
                    Win32Exception inner = new Win32Exception(error);
                    throw new UnauthorizedAccessException(inner.Message, inner);
                }
                directory = false;
                return false;
            }
            directory = (fileAttributes & 0x10) == 0x10;
            return true;
        }

        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            if (drive == null)
            {
                throw PSTraceSource.NewArgumentNullException("drive");
            }
            if (string.IsNullOrEmpty(drive.Root))
            {
                throw PSTraceSource.NewArgumentException("drive.Root");
            }
            if (drive.Persist && !NativeMethods.PathIsNetworkPath(drive.Root))
            {
                ErrorRecord errorRecord = new ErrorRecord(new NotSupportedException(FileSystemProviderStrings.PersistNotSupported), "DriveRootNotNetworkPath", ErrorCategory.InvalidArgument, drive);
                base.ThrowTerminatingError(errorRecord);
            }
            if (this.ShouldMapNetworkDrive(drive))
            {
                this.MapNetworkDrive(drive);
            }
            bool flag = true;
            PSDriveInfo info = null;
            try
            {
                DriveInfo info2 = new DriveInfo(Path.GetPathRoot(drive.Root));
                if (info2.DriveType != DriveType.Fixed)
                {
                    flag = false;
                }
            }
            catch (ArgumentException)
            {
            }
            bool flag2 = true;
            if (flag)
            {
                try
                {
                    flag2 = NativeDirectoryExists(drive.Root);
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
            if (flag2)
            {
                info = drive;
            }
            else
            {
                Exception exception = new IOException(StringUtil.Format(FileSystemProviderStrings.DriveRootError, drive.Root));
                base.WriteError(new ErrorRecord(exception, "DriveRootError", ErrorCategory.ReadError, drive));
            }
            drive.Trace();
            return info;
        }

        protected override void NewItem(string path, string type, object value)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            if (string.IsNullOrEmpty(type))
            {
                type = this.PromptNewItemType();
                if (string.IsNullOrEmpty(type))
                {
                    throw PSTraceSource.NewArgumentException("type");
                }
            }
            path = NormalizePath(path);
            if ((base.Force == 0) || this.CreateIntermediateDirectories(path))
            {
                switch (GetItemType(type))
                {
                    case ItemType.Directory:
                        this.CreateDirectory(path, true);
                        return;

                    case ItemType.File:
                        try
                        {
                            FileMode createNew = FileMode.CreateNew;
                            if (base.Force != 0)
                            {
                                createNew = FileMode.Create;
                            }
                            string newItemActionFile = FileSystemProviderStrings.NewItemActionFile;
                            string target = StringUtil.Format(FileSystemProviderStrings.NewItemActionTemplate, path);
                            if (base.ShouldProcess(target, newItemActionFile))
                            {
                                using (FileStream stream = new FileStream(path, createNew, FileAccess.Write, FileShare.None))
                                {
                                    if (value != null)
                                    {
                                        StreamWriter writer = new StreamWriter(stream);
                                        writer.Write(value.ToString());
                                        writer.Flush();
                                        writer.Close();
                                    }
                                }
                                FileInfo item = new FileInfo(path);
                                base.WriteItemObject(item, path, false);
                            }
                        }
                        catch (IOException exception)
                        {
                            base.WriteError(new ErrorRecord(exception, "NewItemIOError", ErrorCategory.WriteError, path));
                        }
                        catch (UnauthorizedAccessException exception2)
                        {
                            base.WriteError(new ErrorRecord(exception2, "NewItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
                        }
                        return;
                }
                throw PSTraceSource.NewArgumentException("type", "FileSystemProviderStrings", "UnknownType", new object[0]);
            }
        }

        private static ObjectSecurity NewSecurityDescriptor(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.File:
                    return new FileSecurity();

                case ItemType.Directory:
                    return new DirectorySecurity();
            }
            return null;
        }

        public ObjectSecurity NewSecurityDescriptorFromPath(string path, AccessControlSections sections)
        {
            ItemType unknown = ItemType.Unknown;
            if (this.IsItemContainer(path))
            {
                unknown = ItemType.Directory;
            }
            else
            {
                unknown = ItemType.File;
            }
            return NewSecurityDescriptor(unknown);
        }

        public ObjectSecurity NewSecurityDescriptorOfType(string type, AccessControlSections sections)
        {
            return NewSecurityDescriptor(GetItemType(type));
        }

        private static string NormalizePath (string path)
		{
			string str = path;
			if (OSHelper.IsWindows) {
				str = path.Replace ('/', '\\');
			} else {
				if (string.IsNullOrEmpty (path) && OSHelper.IsUnix) path = "/";
				int index = path.IndexOf ("::", StringComparison.OrdinalIgnoreCase);
				if (index != -1)
				{
					str = path.Substring (index + 2);
				}
				index = path.IndexOf (":", StringComparison.OrdinalIgnoreCase);
				if (index != -1)
				{
					str = path.Substring (index + 1);
				}
			}
            tracer.WriteLine("result = {0}", new object[] { str });
            return str;
        }

        protected override string NormalizeRelativePath(string path, string basePath)
        {
			if (string.IsNullOrEmpty (path) && OSHelper.IsUnix) path = "/";
			path = NormalizePath(path);
			basePath = NormalizePath(basePath);
            if (string.IsNullOrEmpty(path) || !this.IsValidPath(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            if (basePath == null)
            {
                basePath = string.Empty;
            }
            tracer.WriteLine("basePath = {0}", new object[] { basePath });
            string str = path;
            
            path = EnsureDriveIsRooted(path);
            path = this.NormalizeRelativePathHelper(path, basePath);
            basePath = EnsureDriveIsRooted(basePath);
            str = path;
            if (!string.IsNullOrEmpty(str))
            {
                try
                {
                    string str2 = path;
                    if (!str2.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
                    {
                        str2 = str2 + '\\';
                    }
                    string str3 = basePath;
                    if (!str3.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
                    {
                        str3 = str3 + '\\';
                    }
                    if (str2.StartsWith(str3, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!IsUNCPath(str) && !str.StartsWith(basePath, StringComparison.CurrentCulture))
                        {
                            str = this.MakePath(basePath, str);
                        }
                        if (IsPathRoot(str))
                        {
                            str = EnsureDriveIsRooted(str);
                        }
                        else
                        {
                            string parentPath = this.GetParentPath(str, string.Empty);
                            if (string.IsNullOrEmpty(parentPath))
                            {
                                return string.Empty;
                            }
                            string childName = this.GetChildName(str);
                            IEnumerable<string> source = Directory.EnumerateFiles(parentPath, childName);
                            if ((source == null) || !source.Any<string>())
                            {
                                source = Directory.EnumerateDirectories(parentPath, childName);
                            }
                            if ((source == null) || !source.Any<string>())
                            {
                                Exception exception = new IOException(StringUtil.Format(FileSystemProviderStrings.ItemDoesNotExist, path));
                                base.WriteError(new ErrorRecord(exception, "ItemDoesNotExist", ErrorCategory.ObjectNotFound, path));
                            }
                            else
                            {
                                str = source.First<string>();
                                if (str.StartsWith(basePath, StringComparison.CurrentCulture))
                                {
                                    str = str.Substring(basePath.Length);
                                }
                                else
                                {
                                    Exception exception2 = new ArgumentException(StringUtil.Format(FileSystemProviderStrings.PathOutSideBasePath, path));
                                    base.WriteError(new ErrorRecord(exception2, "PathOutSideBasePath", ErrorCategory.InvalidArgument, null));
                                }
                            }
                        }
                    }
                }
                catch (ArgumentException exception3)
                {
                    base.WriteError(new ErrorRecord(exception3, "NormalizeRelativePathArgumentError", ErrorCategory.InvalidArgument, path));
                }
                catch (DirectoryNotFoundException exception4)
                {
                    base.WriteError(new ErrorRecord(exception4, "NormalizeRelativePathDirectoryNotFoundError", ErrorCategory.ObjectNotFound, path));
                }
                catch (IOException exception5)
                {
                    base.WriteError(new ErrorRecord(exception5, "NormalizeRelativePathIOError", ErrorCategory.ReadError, path));
                }
                catch (UnauthorizedAccessException exception6)
                {
                    base.WriteError(new ErrorRecord(exception6, "NormalizeRelativePathUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
                }
            }
            tracer.WriteLine("result = {0}", new object[] { str });
            return str;
        }

        private string NormalizeRelativePathHelper (string path, string basePath)
		{
			if (path == null) {
				throw PSTraceSource.NewArgumentNullException ("path");
			}
			if (path.Length == 0) {
				return string.Empty;
			}
			if (basePath == null) {
				basePath = string.Empty;
			}
			tracer.WriteLine ("basePath = {0}", new object[] { basePath });
			string str = string.Empty;
			int index = path.IndexOf (':');
			int length = path.IndexOf (':', index + 1);
			if (length > 0) {
				string oldValue = path.Substring (0, length);
				str = path.Replace (oldValue, "");
				path = oldValue;
			}
			string child = path;
			var ch1 = OSHelper.IsUnix ? '/' : '\\';
			if (OSHelper.IsWindows) {
				path = path.Replace ('/', '\\');
			}
			string str4 = path;
			path = path.TrimEnd (new char[] { ch1 });
			if (OSHelper.IsWindows) {
				basePath = basePath.Replace ('/', '\\');
			}
            basePath = basePath.TrimEnd(new char[] { ch1 });
            path = this.RemoveRelativeTokens(path);
			if (string.Equals(path, basePath, StringComparison.OrdinalIgnoreCase) && !str4.EndsWith((OSHelper.IsUnix ? "/" : "\\"), StringComparison.OrdinalIgnoreCase))
            {
                string childName = this.GetChildName(path);
                child = this.MakePath("..", childName);
            }
            else
            {
                Stack<string> tokenizedPathStack = null;
				if (!(path + (OSHelper.IsUnix ? "/" : "\\")).StartsWith(basePath + (OSHelper.IsUnix ? "/" : "\\"), StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(basePath))
                {
                    child = string.Empty;
                    string commonBase = this.GetCommonBase(path, basePath);
                    int count = this.TokenizePathToStack(basePath, commonBase).Count;
                    if (string.IsNullOrEmpty(commonBase))
                    {
                        count--;
                    }
                    for (int i = 0; i < count; i++)
                    {
                        child = this.MakePath("..", child);
                    }
                    if (!string.IsNullOrEmpty(commonBase))
                    {
                        if (string.Equals(path, commonBase, StringComparison.OrdinalIgnoreCase) && !path.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
                        {
                            string str7 = this.GetChildName(path);
                            child = this.MakePath("..", child);
                            child = this.MakePath(child, str7);
                        }
                        else
                        {
                            string[] strArray = this.TokenizePathToStack(path, commonBase).ToArray();
                            for (int j = 0; j < strArray.Length; j++)
                            {
                                child = this.MakePath(child, strArray[j]);
                            }
                        }
                    }
                }
                else if (IsPathRoot(path))
                {
                    if (string.IsNullOrEmpty(basePath))
                    {
                        child = path;
                    }
                    else
                    {
                        child = string.Empty;
                    }
                }
                else
                {
                    tokenizedPathStack = this.TokenizePathToStack(path, basePath);
                    Stack<string> normalizedPathStack = new Stack<string>();
                    try
                    {
                        normalizedPathStack = this.NormalizeThePath(basePath, tokenizedPathStack);
                    }
                    catch (ArgumentException exception)
                    {
                        base.WriteError(new ErrorRecord(exception, "NormalizeRelativePathHelperArgumentError", ErrorCategory.InvalidArgument, null));
                        child = null;
                        goto Label_0281;
                    }
                    child = this.CreateNormalizedRelativePathFromStack(normalizedPathStack);
                }
            }
        Label_0281:
            if (!string.IsNullOrEmpty(str))
            {
                child = child + str;
            }
			if (string.IsNullOrEmpty (child) && OSHelper.IsUnix) child = "/";
            tracer.WriteLine("result = {0}", new object[] { child });
            return child;
        }

        private Stack<string> NormalizeThePath(string basepath, Stack<string> tokenizedPathStack)
        {
            Stack<string> stack = new Stack<string>();
            string parent = basepath;
            while (tokenizedPathStack.Count > 0)
            {
                string child = tokenizedPathStack.Pop();
                tracer.WriteLine("childName = {0}", new object[] { child });
                if (!child.Equals(".", StringComparison.OrdinalIgnoreCase))
                {
                    if (child.Equals("..", StringComparison.OrdinalIgnoreCase))
                    {
                        if (stack.Count <= 0)
                        {
                            throw PSTraceSource.NewArgumentException("path", "FileSystemProviderStrings", "PathOutSideBasePath", new object[0]);
                        }
                        string str3 = stack.Pop();
                        if (parent.Length > str3.Length)
                        {
                            parent = parent.Substring(0, (parent.Length - str3.Length) - 1);
                        }
                        else
                        {
                            parent = "";
                        }
                        tracer.WriteLine("normalizedPathStack.Pop() : {0}", new object[] { str3 });
                    }
                    else
                    {
                        parent = this.MakePath(parent, child);
                        bool isContainer = false;
                        FileSystemInfo fileSystemInfo = GetFileSystemInfo(parent, ref isContainer);
                        if (fileSystemInfo != null)
                        {
                            if (fileSystemInfo.FullName.Length < parent.Length)
                            {
                                throw PSTraceSource.NewArgumentException("path", "FileSystemProviderStrings", "ItemDoesNotExist", new object[] { parent });
                            }
                            if (fileSystemInfo.Name.Length >= child.Length)
                            {
                                child = fileSystemInfo.Name;
                            }
                        }
                        else if (!isContainer && (tokenizedPathStack.Count == 0))
                        {
                            throw PSTraceSource.NewArgumentException("path", "FileSystemProviderStrings", "ItemDoesNotExist", new object[] { parent });
                        }
                        tracer.WriteLine("normalizedPathStack.Push({0})", new object[] { child });
                        stack.Push(child);
                    }
                }
            }
            return stack;
        }

        private string PromptNewItemType()
        {
            string str = null;
            if (base.Host != null)
            {
                FieldDescription description = new FieldDescription("Type");
                description.SetParameterType(typeof(string));
                Collection<FieldDescription> descriptions = new Collection<FieldDescription> {
                    description
                };
                try
                {
                    Dictionary<string, PSObject> dictionary = null;
                    dictionary = base.Host.UI.Prompt(string.Empty, string.Empty, descriptions);
                    if ((dictionary != null) && (dictionary.Count > 0))
                    {
                        foreach (PSObject obj2 in dictionary.Values)
                        {
                            return (string) LanguagePrimitives.ConvertTo(obj2, typeof(string), Thread.CurrentThread.CurrentCulture);
                        }
                    }
                    return str;
                }
                catch (NotImplementedException)
                {
                }
            }
            return str;
        }

        private void RemoveDirectoryInfoItem(DirectoryInfo directory, bool recurse, bool force, bool rootOfRemoval)
        {
            bool flag = true;
            if (rootOfRemoval || recurse)
            {
                string removeItemActionDirectory = FileSystemProviderStrings.RemoveItemActionDirectory;
                flag = base.ShouldProcess(directory.FullName, removeItemActionDirectory);
            }
            if (((directory.Attributes & System.IO.FileAttributes.ReparsePoint) != 0) && (base.Force == 0))
            {
                Exception exception = new IOException(StringUtil.Format(FileSystemProviderStrings.DirectoryReparsePoint, directory.FullName));
                base.WriteError(new ErrorRecord(exception, "DirectoryNotEmpty", ErrorCategory.WriteError, directory));
            }
            else if (flag)
            {
                foreach (DirectoryInfo info in directory.EnumerateDirectories())
                {
                    if (base.Stopping)
                    {
                        return;
                    }
                    if (info != null)
                    {
                        this.RemoveDirectoryInfoItem(info, recurse, force, false);
                    }
                }
                IEnumerable<FileInfo> enumerable = null;
                if (!string.IsNullOrEmpty(base.Filter))
                {
                    enumerable = directory.EnumerateFiles(base.Filter);
                }
                else
                {
                    enumerable = directory.EnumerateFiles();
                }
                foreach (FileInfo info2 in enumerable)
                {
                    if (base.Stopping)
                    {
                        return;
                    }
                    if (info2 != null)
                    {
                        if (recurse)
                        {
                            this.RemoveFileInfoItem(info2, force);
                        }
                        else
                        {
                            this.RemoveFileSystemItem(info2, force);
                        }
                    }
                }
                if (DirectoryInfoHasChildItems(directory) && !force)
                {
                    Exception exception2 = new IOException(StringUtil.Format(FileSystemProviderStrings.DirectoryNotEmpty, directory.FullName));
                    base.WriteError(new ErrorRecord(exception2, "DirectoryNotEmpty", ErrorCategory.WriteError, directory));
                }
                else
                {
                    this.RemoveFileSystemItem(directory, force);
                }
            }
        }

        protected override PSDriveInfo RemoveDrive(PSDriveInfo drive)
        {
            if (drive.IsNetworkDrive)
            {
                int error = NativeMethods.WNetCancelConnection2(drive.Name + ":", 1, true);
                if (error != 0)
                {
                    ErrorRecord errorRecord = new ErrorRecord(new Win32Exception(error), "CouldRemoveNetworkDrive", ErrorCategory.InvalidOperation, drive);
                    base.ThrowTerminatingError(errorRecord);
                }
            }
            return drive;
        }

        private void RemoveFileInfoItem(FileInfo file, bool force)
        {
            string removeItemActionFile = FileSystemProviderStrings.RemoveItemActionFile;
            if (base.ShouldProcess(file.FullName, removeItemActionFile))
            {
                this.RemoveFileSystemItem(file, force);
            }
        }

        private void RemoveFileSystemItem(FileSystemInfo fileSystemInfo, bool force)
        {
            if ((base.Force == 0) && ((fileSystemInfo.Attributes & (System.IO.FileAttributes.System | System.IO.FileAttributes.Hidden | System.IO.FileAttributes.ReadOnly)) != 0))
            {
                Exception exception = new IOException(StringUtil.Format(FileSystemProviderStrings.PermissionError, new object[0]));
                ErrorDetails details = new ErrorDetails(this, "FileSystemProviderStrings", "CannotRemoveItem", new object[] { fileSystemInfo.FullName, exception.Message });
                ErrorRecord errorRecord = new ErrorRecord(exception, "RemoveFileSystemItemUnAuthorizedAccess", ErrorCategory.PermissionDenied, fileSystemInfo) {
                    ErrorDetails = details
                };
                base.WriteError(errorRecord);
            }
            else
            {
                System.IO.FileAttributes attributes = fileSystemInfo.Attributes;
                bool flag = false;
                try
                {
                    if (force)
                    {
                        fileSystemInfo.Attributes &= ~(System.IO.FileAttributes.System | System.IO.FileAttributes.Hidden | System.IO.FileAttributes.ReadOnly);
                        flag = true;
                    }
                    fileSystemInfo.Delete();
                    if (force)
                    {
                        flag = false;
                    }
                }
                catch (Exception exception2)
                {
                    CommandProcessorBase.CheckForSevereException(exception2);
                    ErrorDetails details2 = new ErrorDetails(this, "FileSystemProviderStrings", "CannotRemoveItem", new object[] { fileSystemInfo.FullName, exception2.Message });
                    if ((exception2 is SecurityException) || (exception2 is UnauthorizedAccessException))
                    {
                        ErrorRecord record2 = new ErrorRecord(exception2, "RemoveFileSystemItemUnAuthorizedAccess", ErrorCategory.PermissionDenied, fileSystemInfo) {
                            ErrorDetails = details2
                        };
                        base.WriteError(record2);
                    }
                    else if (exception2 is ArgumentException)
                    {
                        ErrorRecord record3 = new ErrorRecord(exception2, "RemoveFileSystemItemArgumentError", ErrorCategory.InvalidArgument, fileSystemInfo) {
                            ErrorDetails = details2
                        };
                        base.WriteError(record3);
                    }
                    else
                    {
                        if ((!(exception2 is IOException) && !(exception2 is FileNotFoundException)) && !(exception2 is DirectoryNotFoundException))
                        {
                            throw;
                        }
                        ErrorRecord record4 = new ErrorRecord(exception2, "RemoveFileSystemItemIOError", ErrorCategory.WriteError, fileSystemInfo) {
                            ErrorDetails = details2
                        };
                        base.WriteError(record4);
                    }
                }
                finally
                {
                    if (flag)
                    {
                        try
                        {
                            if (fileSystemInfo.Exists)
                            {
                                fileSystemInfo.Attributes = attributes;
                            }
                        }
                        catch (Exception exception3)
                        {
                            CommandProcessorBase.CheckForSevereException(exception3);
                            if ((!(exception3 is DirectoryNotFoundException) && !(exception3 is SecurityException)) && ((!(exception3 is ArgumentException) && !(exception3 is FileNotFoundException)) && !(exception3 is IOException)))
                            {
                                throw;
                            }
                            ErrorDetails details3 = new ErrorDetails(this, "FileSystemProviderStrings", "CannotRestoreAttributes", new object[] { fileSystemInfo.FullName, exception3.Message });
                            ErrorRecord record5 = new ErrorRecord(exception3, "RemoveFileSystemItemCannotRestoreAttributes", ErrorCategory.PermissionDenied, fileSystemInfo) {
                                ErrorDetails = details3
                            };
                            base.WriteError(record5);
                        }
                    }
                }
            }
        }

        protected override void RemoveItem(string path, bool recurse)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            try
            {
                path = NormalizePath(path);
                bool flag = false;
                FileSystemProviderRemoveItemDynamicParameters dynamicParameters = null;
                if (base.DynamicParameters != null)
                {
                    dynamicParameters = base.DynamicParameters as FileSystemProviderRemoveItemDynamicParameters;
                    if (dynamicParameters != null)
                    {
                        if ((dynamicParameters.Stream != null) && (dynamicParameters.Stream.Length > 0))
                        {
                            flag = true;
                        }
                        else
                        {
                            int index = path.IndexOf(':');
                            int startIndex = path.IndexOf(':', index + 1);
                            if (startIndex > 0)
                            {
                                string str = path.Substring(startIndex + 1);
                                path = path.Remove(startIndex);
                                flag = true;
                                dynamicParameters = new FileSystemProviderRemoveItemDynamicParameters {
                                    Stream = new string[] { str }
                                };
                            }
                        }
                    }
                }
                bool isContainer = false;
                FileSystemInfo fileSystemInfo = GetFileSystemInfo(path, ref isContainer);
                if (fileSystemInfo == null)
                {
                    Exception exception = new IOException(StringUtil.Format(FileSystemProviderStrings.ItemDoesNotExist, path));
                    base.WriteError(new ErrorRecord(exception, "ItemDoesNotExist", ErrorCategory.ObjectNotFound, path));
                }
                else if (!flag && isContainer)
                {
                    this.RemoveDirectoryInfoItem((DirectoryInfo) fileSystemInfo, recurse, (bool) base.Force, true);
                }
                else if (flag)
                {
                    foreach (string str3 in dynamicParameters.Stream)
                    {
                        WildcardPattern pattern = new WildcardPattern(str3, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
                        bool flag3 = false;
                        foreach (AlternateStreamData data in AlternateDataStreamUtilities.GetStreams(fileSystemInfo.FullName))
                        {
                            if (pattern.IsMatch(data.Stream))
                            {
                                flag3 = true;
                                string target = string.Format(CultureInfo.InvariantCulture, FileSystemProviderStrings.StreamAction, new object[] { data.Stream, fileSystemInfo.FullName });
                                if (base.ShouldProcess(target))
                                {
                                    AlternateDataStreamUtilities.DeleteFileStream(fileSystemInfo.FullName, data.Stream);
                                }
                            }
                        }
                        if (!WildcardPattern.ContainsWildcardCharacters(str3) && !flag3)
                        {
                            Exception exception2 = new FileNotFoundException(StringUtil.Format(FileSystemProviderStrings.AlternateDataStreamNotFound, str3, fileSystemInfo.FullName), fileSystemInfo.FullName);
                            base.WriteError(new ErrorRecord(exception2, "AlternateDataStreamNotFound", ErrorCategory.ObjectNotFound, path));
                        }
                    }
                }
                else
                {
                    this.RemoveFileInfoItem((FileInfo) fileSystemInfo, (bool) base.Force);
                }
            }
            catch (IOException exception3)
            {
                base.WriteError(new ErrorRecord(exception3, "RemoveItemIOError", ErrorCategory.WriteError, path));
            }
            catch (UnauthorizedAccessException exception4)
            {
                base.WriteError(new ErrorRecord(exception4, "RemoveItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
            }
        }

        protected override object RemoveItemDynamicParameters(string path, bool recurse)
        {
            if (!recurse)
            {
                return new FileSystemProviderRemoveItemDynamicParameters();
            }
            return null;
        }

        private string RemoveRelativeTokens(string path)
        {
            string str = OSHelper.IsUnix ? path : path.Replace('/', '\\');
            if ((((str.IndexOf(@"\", StringComparison.OrdinalIgnoreCase) < 0) || str.StartsWith(@".\", StringComparison.OrdinalIgnoreCase)) || (str.StartsWith(@"..\", StringComparison.OrdinalIgnoreCase) || str.EndsWith(@"\.", StringComparison.OrdinalIgnoreCase))) || ((str.EndsWith(@"\..", StringComparison.OrdinalIgnoreCase) || (str.IndexOf(@"\.\", StringComparison.OrdinalIgnoreCase) > 0)) || (str.IndexOf(@"\..\", StringComparison.OrdinalIgnoreCase) > 0)))
            {
                try
                {
                    Stack<string> tokenizedPathStack = this.TokenizePathToStack(path, "");
                    Stack<string> normalizedPathStack = this.NormalizeThePath("", tokenizedPathStack);
                    return this.CreateNormalizedRelativePathFromStack(normalizedPathStack);
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
            return path;
        }

        protected override void RenameItem(string path, string newName)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            path = NormalizePath(path);
            if (string.IsNullOrEmpty(newName))
            {
                throw PSTraceSource.NewArgumentException("newName");
            }
            if (newName.StartsWith(@".\", StringComparison.OrdinalIgnoreCase) || newName.StartsWith("./", StringComparison.OrdinalIgnoreCase))
            {
                newName = newName.Remove(0, 2);
            }
            else if (string.Equals(Path.GetDirectoryName(path), Path.GetDirectoryName(newName), StringComparison.OrdinalIgnoreCase))
            {
                newName = Path.GetFileName(newName);
            }
            if (string.Compare(Path.GetFileName(newName), newName, StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw PSTraceSource.NewArgumentException("newName", "FileSystemProviderStrings", "RenameError", new object[0]);
            }
            if (IsReservedDeviceName(newName))
            {
                Exception exception = new IOException(StringUtil.Format(FileSystemProviderStrings.TargetCannotContainDeviceName, newName));
                base.WriteError(new ErrorRecord(exception, "RenameError", ErrorCategory.WriteError, newName));
            }
            else
            {
                try
                {
                    bool isContainer = this.IsItemContainer(path);
                    FileSystemInfo item = null;
                    if (isContainer)
                    {
                        DirectoryInfo info2 = new DirectoryInfo(path);
                        string fullName = info2.Parent.FullName;
                        string str3 = this.MakePath(fullName, newName);
                        string renameItemActionDirectory = FileSystemProviderStrings.RenameItemActionDirectory;
                        string target = StringUtil.Format(FileSystemProviderStrings.RenameItemResourceFileTemplate, info2.FullName, str3);
                        if (base.ShouldProcess(target, renameItemActionDirectory))
                        {
                            info2.MoveTo(str3);
                            item = info2;
                            base.WriteItemObject(item, item.FullName, isContainer);
                        }
                    }
                    else
                    {
                        FileInfo info3 = new FileInfo(path);
                        string directoryName = info3.DirectoryName;
                        string str7 = this.MakePath(directoryName, newName);
                        string renameItemActionFile = FileSystemProviderStrings.RenameItemActionFile;
                        string str9 = StringUtil.Format(FileSystemProviderStrings.RenameItemResourceFileTemplate, info3.FullName, str7);
                        if (base.ShouldProcess(str9, renameItemActionFile))
                        {
                            info3.MoveTo(str7);
                            item = info3;
                            base.WriteItemObject(item, item.FullName, isContainer);
                        }
                    }
                }
                catch (ArgumentException exception2)
                {
                    base.WriteError(new ErrorRecord(exception2, "RenameItemArgumentError", ErrorCategory.InvalidArgument, path));
                }
                catch (IOException exception3)
                {
                    base.WriteError(new ErrorRecord(exception3, "RenameItemIOError", ErrorCategory.WriteError, path));
                }
                catch (UnauthorizedAccessException exception4)
                {
                    base.WriteError(new ErrorRecord(exception4, "RenameItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
                }
            }
        }

        internal static int SafeGetFileAttributes(string path)
        {
            int fileAttributes = NativeMethods.GetFileAttributes(path);
            if (fileAttributes == -1)
            {
                int error = Marshal.GetLastWin32Error();
                if (error == 5)
                {
                    Win32Exception inner = new Win32Exception(error);
                    throw new UnauthorizedAccessException(inner.Message, inner);
                }
            }
            return fileAttributes;
        }

        public void SetProperty(string path, PSObject propertyToSet)
        {
            bool flag2;
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            if (propertyToSet == null)
            {
                throw PSTraceSource.NewArgumentNullException("propertyToSet");
            }
            path = NormalizePath(path);
            PSObject propertyValue = new PSObject();
            PSObject obj3 = null;
            bool flag = false;
            if (NativeItemExists(path, out flag2))
            {
                if (flag2)
                {
                    flag = true;
                    obj3 = PSObject.AsPSObject(new DirectoryInfo(path));
                }
                else
                {
                    obj3 = PSObject.AsPSObject(new FileInfo(path));
                }
            }
            if (obj3 != null)
            {
                bool flag4 = false;
                foreach (PSMemberInfo info in propertyToSet.Properties)
                {
                    object obj4 = info.Value;
                    string action = null;
                    if (flag)
                    {
                        action = FileSystemProviderStrings.SetPropertyActionDirectory;
                    }
                    else
                    {
                        action = FileSystemProviderStrings.SetPropertyActionFile;
                    }
                    string setPropertyResourceTemplate = FileSystemProviderStrings.SetPropertyResourceTemplate;
                    string str3 = obj4.ToString();
                    try
                    {
                        str3 = PSObject.AsPSObject(obj4).ToString();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    string target = string.Format(base.Host.CurrentCulture, setPropertyResourceTemplate, new object[] { path, info.Name, str3 });
                    if (base.ShouldProcess(target, action))
                    {
                        PSMemberInfo info2 = PSObject.AsPSObject(obj3).Properties[info.Name];
                        if (info2 != null)
                        {
                            if (string.Compare(info.Name, "attributes", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                System.IO.FileAttributes attributes;
                                if (obj4 is System.IO.FileAttributes)
                                {
                                    attributes = (System.IO.FileAttributes) obj4;
                                }
                                else
                                {
                                    attributes = (System.IO.FileAttributes) Enum.Parse(typeof(System.IO.FileAttributes), str3, true);
                                }
                                if ((attributes & ~(System.IO.FileAttributes.Normal | System.IO.FileAttributes.Archive | System.IO.FileAttributes.System | System.IO.FileAttributes.Hidden | System.IO.FileAttributes.ReadOnly)) != 0)
                                {
                                    Exception exception = new IOException(StringUtil.Format(FileSystemProviderStrings.AttributesNotSupported, info));
                                    base.WriteError(new ErrorRecord(exception, "SetPropertyError", ErrorCategory.ReadError, info));
                                    continue;
                                }
                            }
                            info2.Value = obj4;
                            propertyValue.Properties.Add(new PSNoteProperty(info.Name, obj4));
                            flag4 = true;
                        }
                        else
                        {
                            Exception exception2 = new IOException(StringUtil.Format(FileSystemProviderStrings.PropertyNotFound, info));
                            base.WriteError(new ErrorRecord(exception2, "SetPropertyError", ErrorCategory.ReadError, info));
                        }
                    }
                }
                if (flag4)
                {
                    base.WritePropertyObject(propertyValue, path);
                }
            }
            else
            {
                Exception exception3 = new IOException(StringUtil.Format(FileSystemProviderStrings.ItemDoesNotExist, path));
                base.WriteError(new ErrorRecord(exception3, "ItemDoesNotExist", ErrorCategory.ObjectNotFound, path));
            }
        }

        public object SetPropertyDynamicParameters(string path, PSObject propertyValue)
        {
            return null;
        }

        public void SetSecurityDescriptor(string path, ObjectSecurity securityDescriptor)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            path = NormalizePath(path);
            if (securityDescriptor == null)
            {
                throw PSTraceSource.NewArgumentNullException("securityDescriptor");
            }
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                base.ThrowTerminatingError(CreateErrorRecord(path, "SetSecurityDescriptor_FileNotFound"));
            }
            FileSystemSecurity sd = securityDescriptor as FileSystemSecurity;
            if (sd == null)
            {
                throw PSTraceSource.NewArgumentException("securityDescriptor");
            }
            try
            {
                this.SetSecurityDescriptor(path, sd, AccessControlSections.All);
            }
            catch (PrivilegeNotHeldException)
            {
                ObjectSecurity accessControl = File.GetAccessControl(path);
                Type targetType = typeof(NTAccount);
                AccessControlSections all = AccessControlSections.All;
                if ((sd.GetAuditRules(true, true, targetType).Count == 0) && (sd.AreAuditRulesProtected == accessControl.AreAccessRulesProtected))
                {
                    all &= ~AccessControlSections.Audit;
                }
                if (sd.GetOwner(targetType) == accessControl.GetOwner(targetType))
                {
                    all &= ~AccessControlSections.Owner;
                }
                if (sd.GetGroup(targetType) == accessControl.GetGroup(targetType))
                {
                    all &= ~AccessControlSections.Group;
                }
                this.SetSecurityDescriptor(path, sd, all);
            }
        }

        private void SetSecurityDescriptor(string path, ObjectSecurity sd, AccessControlSections sections)
        {
            byte[] securityDescriptorBinaryForm = sd.GetSecurityDescriptorBinaryForm();
            if (Directory.Exists(path))
            {
                DirectorySecurity directorySecurity = new DirectorySecurity();
                directorySecurity.SetSecurityDescriptorBinaryForm(securityDescriptorBinaryForm, sections);
                Directory.SetAccessControl(path, directorySecurity);
                base.WriteSecurityDescriptorObject(directorySecurity, path);
            }
            else
            {
                FileSecurity fileSecurity = new FileSecurity();
                fileSecurity.SetSecurityDescriptorBinaryForm(securityDescriptorBinaryForm, sections);
                File.SetAccessControl(path, fileSecurity);
                base.WriteSecurityDescriptorObject(fileSecurity, path);
            }
        }

        private bool ShouldMapNetworkDrive(PSDriveInfo drive)
        {
            bool flag = false;
            if ((((drive == null) || string.IsNullOrEmpty(drive.Root)) || !NativeMethods.PathIsNetworkPath(drive.Root)) || (!drive.Persist && ((drive.Credential == null) || drive.Credential.Equals(PSCredential.Empty))))
            {
                return flag;
            }
            return true;
        }

        protected override ProviderInfo Start(ProviderInfo providerInfo)
        {
            if ((providerInfo != null) && string.IsNullOrEmpty(providerInfo.Home))
            {
                string environmentVariable = Environment.GetEnvironmentVariable("HOMEDRIVE");
                string str2 = Environment.GetEnvironmentVariable("HOMEPATH");
                if (string.IsNullOrEmpty(environmentVariable) || string.IsNullOrEmpty(str2))
                {
                    return providerInfo;
                }
                string path = this.MakePath(environmentVariable, str2);
                if (Directory.Exists(path))
                {
                    tracer.WriteLine("Home = {0}", new object[] { path });
                    providerInfo.Home = path;
                    return providerInfo;
                }
                tracer.WriteLine("Not setting home directory {0} - does not exist", new object[] { path });
            }
            return providerInfo;
        }

        private Stack<string> TokenizePathToStack(string path, string basePath)
        {
            Stack<string> stack = new Stack<string>();
            string parentPath = path;
            for (string str2 = path; parentPath.Length > basePath.Length; str2 = parentPath)
            {
                string childName = this.GetChildName(parentPath);
                if (string.IsNullOrEmpty(childName))
                {
                    tracer.WriteLine("tokenizedPathStack.Push({0})", new object[] { parentPath });
                    stack.Push(parentPath);
                    return stack;
                }
                tracer.WriteLine("tokenizedPathStack.Push({0})", new object[] { childName });
                stack.Push(childName);
                parentPath = this.GetParentPath(parentPath, basePath);
                if ((parentPath.Length >= str2.Length) || IsPathRoot(parentPath))
                {
                    if (string.IsNullOrEmpty(basePath))
                    {
                        tracer.WriteLine("tokenizedPathStack.Push({0})", new object[] { parentPath });
                        stack.Push(parentPath);
                    }
                    return stack;
                }
            }
            return stack;
        }

        private void ValidateParameters(bool isRawSpecified)
        {
            if (isRawSpecified)
            {
                if (base.Context.MyInvocation.BoundParameters.ContainsKey("TotalCount"))
                {
                    throw new PSInvalidOperationException(StringUtil.Format(FileSystemProviderStrings.NoFirstLastWaitForRaw, "Raw", "TotalCount"));
                }
                if (base.Context.MyInvocation.BoundParameters.ContainsKey("Tail"))
                {
                    throw new PSInvalidOperationException(StringUtil.Format(FileSystemProviderStrings.NoFirstLastWaitForRaw, "Raw", "Tail"));
                }
                if (base.Context.MyInvocation.BoundParameters.ContainsKey("Wait"))
                {
                    throw new PSInvalidOperationException(StringUtil.Format(FileSystemProviderStrings.NoFirstLastWaitForRaw, "Raw", "Wait"));
                }
                if (base.Context.MyInvocation.BoundParameters.ContainsKey("Delimiter"))
                {
                    throw new PSInvalidOperationException(StringUtil.Format(FileSystemProviderStrings.NoFirstLastWaitForRaw, "Raw", "Delimiter"));
                }
            }
        }

        private enum ItemType
        {
            Unknown,
            File,
            Directory
        }

        private static class NativeMethods
        {
			/*
            [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
            internal static extern int GetFileAttributes(string lpFileName);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("shlwapi.dll", CharSet=CharSet.Unicode)]
            internal static extern bool PathIsNetworkPath(string path);
            [DllImport("mpr.dll", CharSet=CharSet.Unicode)]
            internal static extern int WNetAddConnection2(ref FileSystemProvider.NetResource netResource, byte[] password, string username, int flags);
            [DllImport("mpr.dll", CharSet=CharSet.Unicode)]
            internal static extern int WNetCancelConnection2(string driveName, int flags, bool force);
            [DllImport("mpr.dll", CharSet=CharSet.Unicode)]
            internal static extern int WNetGetConnection(string localName, StringBuilder remoteName, ref int remoteNameLength);
            */

			internal static int GetFileAttributes (string lpFileName)
			{
				FileInfo fi = new FileInfo (lpFileName);
				if (fi.Exists) {
					return (int)fi.Attributes;
				}
				DirectoryInfo di = new DirectoryInfo(lpFileName);
				return (int)di.Attributes;
			}

			[return: MarshalAs(UnmanagedType.Bool)]
			internal static bool PathIsNetworkPath(string path)
			{
				return false;
			}

			internal static int WNetAddConnection2(ref FileSystemProvider.NetResource netResource, byte[] password, string username, int flags)
			{
				return 0;
			}

			internal static int WNetCancelConnection2(string driveName, int flags, bool force)
			{
				return 0;
			}

			internal static int WNetGetConnection(string localName, StringBuilder remoteName, ref int remoteNameLength)
			{
				return 0;
			}

            [Flags]
            internal enum FileAttributes
            {
                Directory = 0x10,
                Hidden = 2
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NetResource
        {
            public int Scope;
            public int Type;
            public int DisplayType;
            public int Usage;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string LocalName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string RemoteName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Comment;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Provider;
        }
    }
}

