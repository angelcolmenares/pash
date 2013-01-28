namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal;
    using Microsoft.Win32;
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Provider;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.AccessControl;
    using System.Text;
    using System.Threading;

    [CmdletProvider("Registry", ProviderCapabilities.Transactions | ProviderCapabilities.ShouldProcess), OutputType(new Type[] { typeof(RegistryKey) }, ProviderCmdlet="Get-Item"), OutputType(new Type[] { typeof(RegistrySecurity) }, ProviderCmdlet="Get-Acl"), OutputType(new Type[] { typeof(RegistryKey) }, ProviderCmdlet="New-Item"), OutputType(new Type[] { typeof(string) }, ProviderCmdlet="Move-ItemProperty"), OutputType(new Type[] { typeof(RegistryKey), typeof(string) }, ProviderCmdlet="Get-ChildItem"), OutputType(new Type[] { typeof(RegistryKey) }, ProviderCmdlet="Get-Item"), OutputType(new Type[] { typeof(RegistryKey), typeof(string), typeof(int), typeof(long) }, ProviderCmdlet="Get-ItemProperty"), OutputType(new Type[] { typeof(RegistryKey) }, ProviderCmdlet="Get-ChildItem")]
    public sealed class RegistryProvider : NavigationCmdletProvider, IDynamicPropertyCmdletProvider, IPropertyCmdletProvider, ISecurityDescriptorCmdletProvider
    {
        private const string charactersThatNeedEscaping = ".*?[]:";
        private static readonly string[] hiveNames = new string[] { "HKEY_LOCAL_MACHINE", "HKEY_CURRENT_USER", "HKEY_CLASSES_ROOT", "HKEY_CURRENT_CONFIG", "HKEY_USERS", "HKEY_PERFORMANCE_DATA" };
        private static readonly string[] hiveShortNames = new string[] { "HKLM", "HKCU", "HKCR", "HKCC", "HKU", "HKPD" };
        public const string ProviderName = "Registry";
        [TraceSource("RegistryProvider", "The namespace navigation provider for the Windows Registry")]
        private static PSTraceSource tracer = PSTraceSource.GetTracer("RegistryProvider", "The namespace navigation provider for the Windows Registry");
        private static readonly RegistryKey[] wellKnownHives = new RegistryKey[] { Registry.LocalMachine, Registry.CurrentUser, Registry.ClassesRoot, Registry.CurrentConfig, Registry.Users, Registry.PerformanceData };
        private static readonly TransactedRegistryKey[] wellKnownHivesTx = new TransactedRegistryKey[] { TransactedRegistry.LocalMachine, TransactedRegistry.CurrentUser, TransactedRegistry.ClassesRoot, TransactedRegistry.CurrentConfig, TransactedRegistry.Users };

        private bool CheckOperationNotAllowedOnHiveContainer(string path)
        {
            if (this.IsHiveContainer(path))
            {
                InvalidOperationException exception = new InvalidOperationException(RegistryProviderStrings.ContainerInvalidOperationTemplate);
                base.WriteError(new ErrorRecord(exception, "InvalidContainer", ErrorCategory.InvalidArgument, path));
                return false;
            }
            return true;
        }

        private bool CheckOperationNotAllowedOnHiveContainer(string sourcePath, string destinationPath)
        {
            if (this.IsHiveContainer(sourcePath))
            {
                InvalidOperationException exception = new InvalidOperationException(RegistryProviderStrings.SourceContainerInvalidOperationTemplate);
                base.WriteError(new ErrorRecord(exception, "InvalidContainer", ErrorCategory.InvalidArgument, sourcePath));
                return false;
            }
            if (this.IsHiveContainer(destinationPath))
            {
                InvalidOperationException exception2 = new InvalidOperationException(RegistryProviderStrings.DestinationContainerInvalidOperationTemplate);
                base.WriteError(new ErrorRecord(exception2, "InvalidContainer", ErrorCategory.InvalidArgument, destinationPath));
                return false;
            }
            return true;
        }

        protected override void ClearItem(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            string clearItemAction = RegistryProviderStrings.ClearItemAction;
            string clearItemResourceTemplate = RegistryProviderStrings.ClearItemResourceTemplate;
            string target = string.Format(base.Host.CurrentCulture, clearItemResourceTemplate, new object[] { path });
            if (base.ShouldProcess(target, clearItemAction))
            {
                IRegistryWrapper regkeyForPathWriteIfError = this.GetRegkeyForPathWriteIfError(path, true);
                if (regkeyForPathWriteIfError != null)
                {
                    string[] valueNames = new string[0];
                    try
                    {
                        valueNames = regkeyForPathWriteIfError.GetValueNames();
                    }
                    catch (IOException exception)
                    {
                        base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.ReadError, path));
                        return;
                    }
                    catch (SecurityException exception2)
                    {
                        base.WriteError(new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.PermissionDenied, path));
                        return;
                    }
                    catch (UnauthorizedAccessException exception3)
                    {
                        base.WriteError(new ErrorRecord(exception3, exception3.GetType().FullName, ErrorCategory.PermissionDenied, path));
                        return;
                    }
                    for (int i = 0; i < valueNames.Length; i++)
                    {
                        try
                        {
                            regkeyForPathWriteIfError.DeleteValue(valueNames[i]);
                        }
                        catch (IOException exception4)
                        {
                            base.WriteError(new ErrorRecord(exception4, exception4.GetType().FullName, ErrorCategory.WriteError, path));
                        }
                        catch (SecurityException exception5)
                        {
                            base.WriteError(new ErrorRecord(exception5, exception5.GetType().FullName, ErrorCategory.PermissionDenied, path));
                        }
                        catch (UnauthorizedAccessException exception6)
                        {
                            base.WriteError(new ErrorRecord(exception6, exception6.GetType().FullName, ErrorCategory.PermissionDenied, path));
                        }
                    }
                    this.WriteRegistryItemObject(regkeyForPathWriteIfError, path);
                }
            }
        }

        public void ClearProperty(string path, Collection<string> propertyToClear)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (this.CheckOperationNotAllowedOnHiveContainer(path))
            {
                IRegistryWrapper wrapper;
                Collection<string> collection;
                this.GetFilteredRegistryKeyProperties(path, propertyToClear, false, true, out wrapper, out collection);
                if (wrapper != null)
                {
                    string clearPropertyAction = RegistryProviderStrings.ClearPropertyAction;
                    string clearPropertyResourceTemplate = RegistryProviderStrings.ClearPropertyResourceTemplate;
                    bool flag = false;
                    PSObject propertyValue = new PSObject();
                    foreach (string str3 in collection)
                    {
                        string target = string.Format(base.Host.CurrentCulture, clearPropertyResourceTemplate, new object[] { path, str3 });
                        if (base.ShouldProcess(target, clearPropertyAction))
                        {
                            object obj3 = this.ResetRegistryKeyValue(wrapper, str3);
                            string name = str3;
                            if (string.IsNullOrEmpty(str3))
                            {
                                name = this.GetLocalizedDefaultToken();
                            }
                            propertyValue.Properties.Add(new PSNoteProperty(name, obj3));
                            flag = true;
                        }
                    }
                    wrapper.Close();
                    if (flag)
                    {
                        base.WritePropertyObject(propertyValue, path);
                    }
                }
            }
        }

        public object ClearPropertyDynamicParameters(string path, Collection<string> propertyToClear)
        {
            return null;
        }

        private static object ConvertValueToKind(object value, RegistryValueKind kind)
        {
            switch (kind)
            {
                case RegistryValueKind.String:
                    value = (value != null) ? ((string) LanguagePrimitives.ConvertTo(value, typeof(string), Thread.CurrentThread.CurrentCulture)) : "";
                    return value;

                case RegistryValueKind.ExpandString:
                    value = (value != null) ? ((string) LanguagePrimitives.ConvertTo(value, typeof(string), Thread.CurrentThread.CurrentCulture)) : "";
                    return value;

                case RegistryValueKind.Binary:
                    value = (value != null) ? ((byte[]) LanguagePrimitives.ConvertTo(value, typeof(byte[]), Thread.CurrentThread.CurrentCulture)) : new byte[0];
                    return value;

                case RegistryValueKind.DWord:
                    if (value == null)
                    {
                        value = 0;
                        return value;
                    }
                    try
                    {
                        value = (int) LanguagePrimitives.ConvertTo(value, typeof(int), Thread.CurrentThread.CurrentCulture);
                    }
                    catch (PSInvalidCastException)
                    {
                        value = (int) LanguagePrimitives.ConvertTo(value, typeof(int), Thread.CurrentThread.CurrentCulture);
                    }
                    return value;

                case (RegistryValueKind.DWord | RegistryValueKind.String):
                case (RegistryValueKind.DWord | RegistryValueKind.ExpandString):
                case ((RegistryValueKind) 8):
                case ((RegistryValueKind) 9):
                case ((RegistryValueKind) 10):
                    return value;

                case RegistryValueKind.MultiString:
                    value = (value != null) ? ((string[]) LanguagePrimitives.ConvertTo(value, typeof(string[]), Thread.CurrentThread.CurrentCulture)) : new string[0];
                    return value;

                case RegistryValueKind.QWord:
                    if (value == null)
                    {
                        value = 0;
                        return value;
                    }
                    try
                    {
                        value = (long) LanguagePrimitives.ConvertTo(value, typeof(long), Thread.CurrentThread.CurrentCulture);
                    }
                    catch (PSInvalidCastException)
                    {
                        value = (ulong) LanguagePrimitives.ConvertTo(value, typeof(ulong), Thread.CurrentThread.CurrentCulture);
                    }
                    return value;
            }
            return value;
        }

        protected override void CopyItem(string path, string destination, bool recurse)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            if (string.IsNullOrEmpty(destination))
            {
                throw PSTraceSource.NewArgumentException("destination");
            }
            tracer.WriteLine("destination = {0}", new object[] { destination });
            tracer.WriteLine("recurse = {0}", new object[] { recurse });
            IRegistryWrapper regkeyForPathWriteIfError = this.GetRegkeyForPathWriteIfError(path, false);
            if (regkeyForPathWriteIfError != null)
            {
                try
                {
                    this.CopyRegistryKey(regkeyForPathWriteIfError, path, destination, recurse, true, false);
                }
                catch (IOException exception)
                {
                    base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.WriteError, path));
                }
                catch (SecurityException exception2)
                {
                    base.WriteError(new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.PermissionDenied, path));
                }
                catch (UnauthorizedAccessException exception3)
                {
                    base.WriteError(new ErrorRecord(exception3, exception3.GetType().FullName, ErrorCategory.PermissionDenied, path));
                }
                regkeyForPathWriteIfError.Close();
            }
        }

        public void CopyProperty(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        {
            if (sourcePath == null)
            {
                throw PSTraceSource.NewArgumentNullException("sourcePath");
            }
            if (destinationPath == null)
            {
                throw PSTraceSource.NewArgumentNullException("destinationPath");
            }
            if (this.CheckOperationNotAllowedOnHiveContainer(sourcePath, destinationPath))
            {
                IRegistryWrapper regkeyForPathWriteIfError = this.GetRegkeyForPathWriteIfError(sourcePath, false);
                if (regkeyForPathWriteIfError != null)
                {
                    IRegistryWrapper destinationKey = this.GetRegkeyForPathWriteIfError(destinationPath, true);
                    if (destinationKey != null)
                    {
                        string copyPropertyAction = RegistryProviderStrings.CopyPropertyAction;
                        string copyPropertyResourceTemplate = RegistryProviderStrings.CopyPropertyResourceTemplate;
                        string target = string.Format(base.Host.CurrentCulture, copyPropertyResourceTemplate, new object[] { sourcePath, sourceProperty, destinationPath, destinationProperty });
                        if (base.ShouldProcess(target, copyPropertyAction))
                        {
                            try
                            {
                                this.CopyProperty(regkeyForPathWriteIfError, destinationKey, sourceProperty, destinationProperty, true);
                            }
                            catch (IOException exception)
                            {
                                base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.WriteError, sourcePath));
                            }
                            catch (SecurityException exception2)
                            {
                                base.WriteError(new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.PermissionDenied, sourcePath));
                            }
                            catch (UnauthorizedAccessException exception3)
                            {
                                base.WriteError(new ErrorRecord(exception3, exception3.GetType().FullName, ErrorCategory.PermissionDenied, sourcePath));
                            }
                        }
                        regkeyForPathWriteIfError.Close();
                    }
                }
            }
        }

        private void CopyProperty(IRegistryWrapper sourceKey, IRegistryWrapper destinationKey, string sourceProperty, string destinationProperty, bool writeOnSuccess)
        {
            string propertyName = this.GetPropertyName(sourceProperty);
            this.GetPropertyName(destinationProperty);
            object obj2 = sourceKey.GetValue(sourceProperty);
            RegistryValueKind valueKind = sourceKey.GetValueKind(sourceProperty);
            destinationKey.SetValue(destinationProperty, obj2, valueKind);
            if (writeOnSuccess)
            {
                this.WriteWrappedPropertyObject(obj2, propertyName, sourceKey.Name);
            }
        }

        public object CopyPropertyDynamicParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        {
            return null;
        }

        private bool CopyRegistryKey(IRegistryWrapper key, string path, string destination, bool recurse, bool streamResult, bool streamFirstOnly)
        {
            bool flag = true;
            if (recurse && this.ErrorIfDestinationIsSourceOrChildOfSource(path, destination))
            {
                return false;
            }
            tracer.WriteLine("destination = {0}", new object[] { destination });
            IRegistryWrapper regkeyForPath = this.GetRegkeyForPath(destination, true);
            string childName = this.GetChildName(path);
            string parentPath = destination;
            if (regkeyForPath == null)
            {
                parentPath = this.GetParentPath(destination, null);
                childName = this.GetChildName(destination);
                regkeyForPath = this.GetRegkeyForPathWriteIfError(parentPath, true);
            }
            if (regkeyForPath == null)
            {
                return false;
            }
            string str3 = this.MakePath(parentPath, childName);
            string copyKeyAction = RegistryProviderStrings.CopyKeyAction;
            string copyKeyResourceTemplate = RegistryProviderStrings.CopyKeyResourceTemplate;
            string target = string.Format(base.Host.CurrentCulture, copyKeyResourceTemplate, new object[] { path, destination });
            if (base.ShouldProcess(target, copyKeyAction))
            {
                IRegistryWrapper wrapper2 = null;
                try
                {
                    wrapper2 = regkeyForPath.CreateSubKey(childName);
                }
                catch (NotSupportedException exception)
                {
                    base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.InvalidOperation, childName));
                }
                if (wrapper2 != null)
                {
                    string[] valueNames = key.GetValueNames();
                    for (int i = 0; i < valueNames.Length; i++)
                    {
                        if (base.Stopping)
                        {
                            regkeyForPath.Close();
                            wrapper2.Close();
                            return false;
                        }
                        wrapper2.SetValue(valueNames[i], key.GetValue(valueNames[i], null, RegistryValueOptions.DoNotExpandEnvironmentNames), key.GetValueKind(valueNames[i]));
                    }
                    if (streamResult)
                    {
                        this.WriteRegistryItemObject(wrapper2, str3);
                        if (streamFirstOnly)
                        {
                            streamResult = false;
                        }
                    }
                }
            }
            regkeyForPath.Close();
            if (recurse)
            {
                string[] subKeyNames = key.GetSubKeyNames();
                for (int j = 0; j < subKeyNames.Length; j++)
                {
                    if (base.Stopping)
                    {
                        return false;
                    }
                    string str7 = this.MakePath(path, subKeyNames[j]);
                    string str8 = this.MakePath(str3, subKeyNames[j]);
                    IRegistryWrapper wrapper3 = this.GetRegkeyForPath(str7, false);
                    bool flag2 = this.CopyRegistryKey(wrapper3, str7, str8, recurse, streamResult, streamFirstOnly);
                    wrapper3.Close();
                    if (!flag2)
                    {
                        flag = flag2;
                    }
                }
            }
            return flag;
        }

        private bool CreateIntermediateKeys(string path)
        {
            bool flag = false;
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            try
            {
                path = this.NormalizePath(path);
                int index = path.IndexOf(@"\", StringComparison.Ordinal);
                switch (index)
                {
                    case 0:
                        path = path.Substring(1);
                        index = path.IndexOf(@"\", StringComparison.Ordinal);
                        break;

                    case -1:
                        return true;
                }
                string str = path.Substring(0, index);
                string subkey = path.Substring(index + 1);
                IRegistryWrapper hiveRoot = this.GetHiveRoot(str);
                if ((subkey.Length == 0) || (hiveRoot == null))
                {
                    throw PSTraceSource.NewArgumentException("path");
                }
                IRegistryWrapper wrapper2 = hiveRoot.CreateSubKey(subkey);
                if (wrapper2 == null)
                {
                    throw PSTraceSource.NewArgumentException("path");
                }
                wrapper2.Close();
                flag = true;
            }
            catch (ArgumentException exception)
            {
                base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.OpenError, path));
                return flag;
            }
            catch (IOException exception2)
            {
                base.WriteError(new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.OpenError, path));
                return flag;
            }
            catch (SecurityException exception3)
            {
                base.WriteError(new ErrorRecord(exception3, exception3.GetType().FullName, ErrorCategory.PermissionDenied, path));
                return flag;
            }
            catch (UnauthorizedAccessException exception4)
            {
                base.WriteError(new ErrorRecord(exception4, exception4.GetType().FullName, ErrorCategory.PermissionDenied, path));
                return flag;
            }
            catch (NotSupportedException exception5)
            {
                base.WriteError(new ErrorRecord(exception5, exception5.GetType().FullName, ErrorCategory.InvalidOperation, path));
            }
            return flag;
        }

        private static string EnsureDriveIsRooted(string path)
        {
            string str = path;
            int index = path.IndexOf(':');
            if ((index != -1) && ((index + 1) == path.Length))
            {
                str = path + '\\';
            }
            tracer.WriteLine("result = {0}", new object[] { str });
            return str;
        }

        private bool ErrorIfDestinationIsSourceOrChildOfSource(string sourcePath, string destinationPath)
        {
            tracer.WriteLine("destinationPath = {0}", new object[] { destinationPath });
            bool flag = false;
        Label_001D:
            if (string.Compare(sourcePath, destinationPath, StringComparison.OrdinalIgnoreCase) == 0)
            {
                flag = true;
            }
            else
            {
                string parentPath = this.GetParentPath(destinationPath, null);
                if (!string.IsNullOrEmpty(parentPath) && (string.Compare(parentPath, destinationPath, StringComparison.OrdinalIgnoreCase) != 0))
                {
                    destinationPath = parentPath;
                    goto Label_001D;
                }
            }
            if (flag)
            {
                Exception exception = new ArgumentException(RegistryProviderStrings.DestinationChildOfSource);
                base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.InvalidArgument, destinationPath));
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        private static string EscapeChildName(string name)
        {
            StringBuilder builder = new StringBuilder();
            TextElementEnumerator textElementEnumerator = StringInfo.GetTextElementEnumerator(name);
            while (textElementEnumerator.MoveNext())
            {
                string textElement = textElementEnumerator.GetTextElement();
                if (textElement.Contains(".*?[]:"))
                {
                    builder.Append("`");
                }
                builder.Append(textElement);
            }
            tracer.WriteLine("result = {0}", new object[] { builder });
            return builder.ToString();
        }

        private static string EscapeSpecialChars(string path)
        {
            StringBuilder builder = new StringBuilder();
            TextElementEnumerator textElementEnumerator = StringInfo.GetTextElementEnumerator(path);
            while (textElementEnumerator.MoveNext())
            {
                string textElement = textElementEnumerator.GetTextElement();
                if (textElement.Contains(".*?[]:"))
                {
                    builder.Append("`");
                }
                builder.Append(textElement);
            }
            tracer.WriteLine("result = {0}", new object[] { builder });
            return builder.ToString();
        }

        protected override void GetChildItems(string path, bool recurse)
        {
            tracer.WriteLine("recurse = {0}", new object[] { recurse });
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (this.IsHiveContainer(path))
            {
                foreach (string str in hiveNames)
                {
                    if (base.Stopping)
                    {
                        return;
                    }
                    this.GetItem(str);
                }
            }
            else
            {
                IRegistryWrapper regkeyForPathWriteIfError = this.GetRegkeyForPathWriteIfError(path, false);
                if (regkeyForPathWriteIfError != null)
                {
                    try
                    {
                        string[] subKeyNames = regkeyForPathWriteIfError.GetSubKeyNames();
                        regkeyForPathWriteIfError.Close();
                        if (subKeyNames != null)
                        {
                            foreach (string str2 in subKeyNames)
                            {
                                if (base.Stopping)
                                {
                                    return;
                                }
                                if (!string.IsNullOrEmpty(str2))
                                {
                                    string str3 = path;
                                    try
                                    {
                                        str3 = this.MakePath(path, str2);
                                        if (!string.IsNullOrEmpty(str3))
                                        {
                                            IRegistryWrapper regkeyForPath = this.GetRegkeyForPath(str3, false);
                                            if (regkeyForPath != null)
                                            {
                                                this.WriteRegistryItemObject(regkeyForPath, str3);
                                            }
                                            if (recurse)
                                            {
                                                this.GetChildItems(str3, recurse);
                                            }
                                        }
                                    }
                                    catch (IOException exception)
                                    {
                                        base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.ReadError, str3));
                                    }
                                    catch (SecurityException exception2)
                                    {
                                        base.WriteError(new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.PermissionDenied, str3));
                                    }
                                    catch (UnauthorizedAccessException exception3)
                                    {
                                        base.WriteError(new ErrorRecord(exception3, exception3.GetType().FullName, ErrorCategory.PermissionDenied, str3));
                                    }
                                }
                            }
                        }
                    }
                    catch (IOException exception4)
                    {
                        base.WriteError(new ErrorRecord(exception4, exception4.GetType().FullName, ErrorCategory.ReadError, path));
                    }
                    catch (SecurityException exception5)
                    {
                        base.WriteError(new ErrorRecord(exception5, exception5.GetType().FullName, ErrorCategory.PermissionDenied, path));
                    }
                    catch (UnauthorizedAccessException exception6)
                    {
                        base.WriteError(new ErrorRecord(exception6, exception6.GetType().FullName, ErrorCategory.PermissionDenied, path));
                    }
                }
            }
        }

        protected override string GetChildName(string path)
        {
            return base.GetChildName(path).Replace('\\', '/');
        }

        protected override void GetChildNames(string path, ReturnContainers returnContainers)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                foreach (string str in hiveNames)
                {
                    if (base.Stopping)
                    {
                        return;
                    }
                    base.WriteItemObject(str, str, true);
                }
            }
            else
            {
                IRegistryWrapper regkeyForPathWriteIfError = this.GetRegkeyForPathWriteIfError(path, false);
                if (regkeyForPathWriteIfError != null)
                {
                    try
                    {
                        string[] subKeyNames = regkeyForPathWriteIfError.GetSubKeyNames();
                        regkeyForPathWriteIfError.Close();
                        for (int i = 0; i < subKeyNames.Length; i++)
                        {
                            if (base.Stopping)
                            {
                                return;
                            }
                            string child = EscapeChildName(subKeyNames[i]);
                            string str3 = this.MakePath(path, child);
                            base.WriteItemObject(child, str3, true);
                        }
                    }
                    catch (IOException exception)
                    {
                        base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.ReadError, path));
                    }
                    catch (SecurityException exception2)
                    {
                        base.WriteError(new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.PermissionDenied, path));
                    }
                    catch (UnauthorizedAccessException exception3)
                    {
                        base.WriteError(new ErrorRecord(exception3, exception3.GetType().FullName, ErrorCategory.PermissionDenied, path));
                    }
                }
            }
        }

        private void GetFilteredRegistryKeyProperties(string path, Collection<string> propertyNames, bool getAll, bool writeAccess, out IRegistryWrapper key, out Collection<string> filteredCollection)
        {
            bool flag = false;
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            filteredCollection = new Collection<string>();
            key = this.GetRegkeyForPathWriteIfError(path, writeAccess);
            if (key != null)
            {
                if (propertyNames == null)
                {
                    propertyNames = new Collection<string>();
                }
                if ((propertyNames.Count == 0) && getAll)
                {
                    propertyNames.Add("*");
                    flag = true;
                }
                string[] valueNames = new string[0];
                try
                {
                    valueNames = key.GetValueNames();
                }
                catch (IOException exception)
                {
                    base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.ReadError, path));
                    return;
                }
                catch (SecurityException exception2)
                {
                    base.WriteError(new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.PermissionDenied, path));
                    return;
                }
                catch (UnauthorizedAccessException exception3)
                {
                    base.WriteError(new ErrorRecord(exception3, exception3.GetType().FullName, ErrorCategory.PermissionDenied, path));
                    return;
                }
                foreach (string str in propertyNames)
                {
                    WildcardPattern pattern = new WildcardPattern(str, WildcardOptions.IgnoreCase);
                    bool hadAMatch = false;
                    foreach (string str2 in valueNames)
                    {
                        string input = str2;
                        if (string.IsNullOrEmpty(str2) && !string.IsNullOrEmpty(str))
                        {
                            input = this.GetLocalizedDefaultToken();
                        }
                        if ((flag || (!base.Context.SuppressWildcardExpansion && pattern.IsMatch(input))) || (base.Context.SuppressWildcardExpansion && string.Equals(input, str, StringComparison.OrdinalIgnoreCase)))
                        {
                            if (string.IsNullOrEmpty(input))
                            {
                                input = this.GetLocalizedDefaultToken();
                            }
                            hadAMatch = true;
                            filteredCollection.Add(str2);
                        }
                    }
                    this.WriteErrorIfPerfectMatchNotFound(hadAMatch, path, str);
                }
            }
        }

        private IRegistryWrapper GetHiveRoot(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            if (base.TransactionAvailable())
            {
                for (int i = 0; i < wellKnownHivesTx.Length; i++)
                {
                    if (string.Equals(path, hiveNames[i], StringComparison.OrdinalIgnoreCase) || string.Equals(path, hiveShortNames[i], StringComparison.OrdinalIgnoreCase))
                    {
                        using (base.CurrentPSTransaction)
                        {
                            return new TransactedRegistryWrapper(wellKnownHivesTx[i], this);
                        }
                    }
                }
            }
            else
            {
                for (int j = 0; j < wellKnownHives.Length; j++)
                {
                    if (string.Equals(path, hiveNames[j], StringComparison.OrdinalIgnoreCase) || string.Equals(path, hiveShortNames[j], StringComparison.OrdinalIgnoreCase))
                    {
                        return new RegistryWrapper(wellKnownHives[j]);
                    }
                }
            }
            return null;
        }

        protected override void GetItem(string path)
        {
            IRegistryWrapper regkeyForPathWriteIfError = this.GetRegkeyForPathWriteIfError(path, false);
            if (regkeyForPathWriteIfError != null)
            {
                this.WriteRegistryItemObject(regkeyForPathWriteIfError, path);
            }
        }

        private string GetLocalizedDefaultToken()
        {
            return "(default)";
        }

        protected override string GetParentPath(string path, string root)
        {
            string parentPath = base.GetParentPath(path, root);
            if (string.Equals(parentPath, root, StringComparison.OrdinalIgnoreCase))
            {
                goto Label_0067;
            }
            bool flag = this.ItemExists(path);
            bool flag2 = false;
            if (!flag)
            {
                flag2 = this.ItemExists(this.MakePath(root, path));
            }
            if (string.IsNullOrEmpty(parentPath) || (!flag && !flag2))
            {
                goto Label_0067;
            }
            string str2 = parentPath;
        Label_003F:
            str2 = parentPath;
            if (flag2)
            {
                str2 = this.MakePath(root, parentPath);
            }
            if (!this.ItemExists(str2))
            {
                parentPath = base.GetParentPath(parentPath, root);
                if (!string.IsNullOrEmpty(parentPath))
                {
                    goto Label_003F;
                }
            }
        Label_0067:
            return EnsureDriveIsRooted(parentPath);
        }

        public void GetProperty(string path, Collection<string> providerSpecificPickList)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (this.CheckOperationNotAllowedOnHiveContainer(path))
            {
                IRegistryWrapper wrapper;
                Collection<string> collection;
                this.GetFilteredRegistryKeyProperties(path, providerSpecificPickList, true, false, out wrapper, out collection);
                if (wrapper != null)
                {
                    bool flag = false;
                    PSObject propertyValue = new PSObject();
                    foreach (string str in collection)
                    {
                        string name = str;
                        if (string.IsNullOrEmpty(str))
                        {
                            name = this.GetLocalizedDefaultToken();
                        }
                        propertyValue.Properties.Add(new PSNoteProperty(name, wrapper.GetValue(str)));
                        flag = true;
                    }
                    wrapper.Close();
                    if (flag)
                    {
                        base.WritePropertyObject(propertyValue, path);
                    }
                }
            }
        }

        public object GetPropertyDynamicParameters(string path, Collection<string> providerSpecificPickList)
        {
            return null;
        }

        private string GetPropertyName(string userEnteredPropertyName)
        {
            string str = userEnteredPropertyName;
            if (!string.IsNullOrEmpty(userEnteredPropertyName) && (string.Compare(userEnteredPropertyName, this.GetLocalizedDefaultToken(), true, base.Host.CurrentCulture) == 0))
            {
                str = null;
            }
            tracer.WriteLine("result = {0}", new object[] { str });
            return str;
        }

        private IRegistryWrapper GetRegkeyForPath(string path, bool writeAccess)
        {
            if (string.IsNullOrEmpty(path))
            {
                ArgumentException exception = new ArgumentException(RegistryProviderStrings.KeyDoesNotExist);
                throw exception;
            }
            if (base.Stopping)
            {
                return null;
            }
            tracer.WriteLine("writeAccess = {0}", new object[] { writeAccess });
            IRegistryWrapper wrapper = null;
            int index = path.IndexOf(@"\", StringComparison.Ordinal);
            switch (index)
            {
                case 0:
                    path = path.Substring(1);
                    index = path.IndexOf(@"\", StringComparison.Ordinal);
                    break;

                case -1:
                    return this.GetHiveRoot(path);
            }
            string str = path.Substring(0, index);
            string name = path.Substring(index + 1);
            IRegistryWrapper hiveRoot = this.GetHiveRoot(str);
            if ((name.Length == 0) || (hiveRoot == null))
            {
                return hiveRoot;
            }
            try
            {
                wrapper = hiveRoot.OpenSubKey(name, writeAccess);
            }
            catch (NotSupportedException exception2)
            {
                base.WriteError(new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.InvalidOperation, path));
            }
            if (wrapper != null)
            {
                return wrapper;
            }
            IRegistryWrapper wrapper3 = hiveRoot;
            IRegistryWrapper wrapper4 = null;
            while (!string.IsNullOrEmpty(name))
            {
                bool flag = false;
                foreach (string str3 in wrapper3.GetSubKeyNames())
                {
                    string str4 = this.NormalizePath(str3);
                    if ((name.Equals(str4, StringComparison.OrdinalIgnoreCase) || name.StartsWith(str4 + '/', StringComparison.OrdinalIgnoreCase)) || name.StartsWith(str4 + '\\', StringComparison.OrdinalIgnoreCase))
                    {
                        wrapper4 = wrapper3.OpenSubKey(str3, writeAccess);
                        wrapper3.Close();
                        wrapper3 = wrapper4;
                        flag = true;
                        if (name.Equals(str4, StringComparison.OrdinalIgnoreCase))
                        {
                            name = "";
                        }
                        else
                        {
                            name = name.Substring((str4 + '\\').Length);
                        }
                        break;
                    }
                }
                if (!flag)
                {
                    return null;
                }
            }
            return wrapper3;
        }

        private IRegistryWrapper GetRegkeyForPathWriteIfError(string path, bool writeAccess)
        {
            IRegistryWrapper regkeyForPath = null;
            try
            {
                regkeyForPath = this.GetRegkeyForPath(path, writeAccess);
                if (regkeyForPath == null)
                {
                    ArgumentException exception = new ArgumentException(RegistryProviderStrings.KeyDoesNotExist);
                    base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.InvalidArgument, path));
                    return regkeyForPath;
                }
            }
            catch (ArgumentException exception2)
            {
                base.WriteError(new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.OpenError, path));
                return regkeyForPath;
            }
            catch (IOException exception3)
            {
                base.WriteError(new ErrorRecord(exception3, exception3.GetType().FullName, ErrorCategory.OpenError, path));
                return regkeyForPath;
            }
            catch (SecurityException exception4)
            {
                base.WriteError(new ErrorRecord(exception4, exception4.GetType().FullName, ErrorCategory.PermissionDenied, path));
                return regkeyForPath;
            }
            catch (UnauthorizedAccessException exception5)
            {
                base.WriteError(new ErrorRecord(exception5, exception5.GetType().FullName, ErrorCategory.PermissionDenied, path));
                return regkeyForPath;
            }
            return regkeyForPath;
        }

        public void GetSecurityDescriptor(string path, AccessControlSections sections)
        {
            ObjectSecurity securityDescriptor = null;
            IRegistryWrapper regkeyForPathWriteIfError = null;
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if ((sections & ~AccessControlSections.All) != AccessControlSections.None)
            {
                throw PSTraceSource.NewArgumentException("sections");
            }
            path = this.NormalizePath(path);
            regkeyForPathWriteIfError = this.GetRegkeyForPathWriteIfError(path, false);
            if (regkeyForPathWriteIfError != null)
            {
                try
                {
                    securityDescriptor = regkeyForPathWriteIfError.GetAccessControl(sections);
                }
                catch (SecurityException exception)
                {
                    base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.PermissionDenied, path));
                    return;
                }
                base.WriteSecurityDescriptorObject(securityDescriptor, path);
            }
        }

        private static RegistryValueKind GetValueKindForProperty(IRegistryWrapper key, string valueName)
        {
            try
            {
                return key.GetValueKind(valueName);
            }
            catch (ArgumentException)
            {
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
            return RegistryValueKind.Unknown;
        }

        private static RegistryValueKind GetValueKindFromObject(object value)
        {
            if (value == null)
            {
                throw PSTraceSource.NewArgumentNullException("value");
            }
            RegistryValueKind unknown = RegistryValueKind.Unknown;
            Type type = value.GetType();
            if (type == typeof(byte[]))
            {
                unknown = RegistryValueKind.Binary;
            }
            else if (type == typeof(int))
            {
                unknown = RegistryValueKind.DWord;
            }
            if (type == typeof(string))
            {
                unknown = RegistryValueKind.String;
            }
            if (type == typeof(string[]))
            {
                unknown = RegistryValueKind.MultiString;
            }
            if (type == typeof(long))
            {
                unknown = RegistryValueKind.QWord;
            }
            return unknown;
        }

        protected override bool HasChildItems(string path)
        {
            bool flag = false;
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            try
            {
                if (this.IsHiveContainer(path))
                {
                    flag = hiveNames.Length > 0;
                }
                else
                {
                    IRegistryWrapper regkeyForPath = this.GetRegkeyForPath(path, false);
                    if (regkeyForPath != null)
                    {
                        flag = regkeyForPath.SubKeyCount > 0;
                        regkeyForPath.Close();
                    }
                }
            }
            catch (IOException)
            {
                flag = false;
            }
            catch (SecurityException)
            {
                flag = false;
            }
            catch (UnauthorizedAccessException)
            {
                flag = false;
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        private bool HasRelativePathTokens(string path)
        {
            if ((((path.IndexOf(@"\", StringComparison.OrdinalIgnoreCase) != 0) && !path.Contains(@"\.\")) && (!path.Contains(@"\..\") && !path.EndsWith(@"\..", StringComparison.OrdinalIgnoreCase))) && ((!path.EndsWith(@"\.", StringComparison.OrdinalIgnoreCase) && !path.StartsWith(@"..\", StringComparison.OrdinalIgnoreCase)) && !path.StartsWith(@".\", StringComparison.OrdinalIgnoreCase)))
            {
                return path.StartsWith("~", StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            return new Collection<PSDriveInfo> { new PSDriveInfo("HKLM", base.ProviderInfo, "HKEY_LOCAL_MACHINE", RegistryProviderStrings.HKLMDriveDescription, null), new PSDriveInfo("HKCU", base.ProviderInfo, "HKEY_CURRENT_USER", RegistryProviderStrings.HKCUDriveDescription, null) };
        }

        private bool IsHiveContainer(string path)
        {
            bool flag = false;
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if ((string.IsNullOrEmpty(path) || (string.Compare(path, @"\", StringComparison.OrdinalIgnoreCase) == 0)) || (string.Compare(path, "/", StringComparison.OrdinalIgnoreCase) == 0))
            {
                flag = true;
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        protected override bool IsItemContainer(string path)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            bool flag = false;
            if (this.IsHiveContainer(path))
            {
                flag = true;
            }
            else
            {
                try
                {
                    IRegistryWrapper regkeyForPath = this.GetRegkeyForPath(path, false);
                    if (regkeyForPath != null)
                    {
                        regkeyForPath.Close();
                        flag = true;
                    }
                }
                catch (IOException exception)
                {
                    base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.ReadError, path));
                }
                catch (SecurityException exception2)
                {
                    base.WriteError(new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.PermissionDenied, path));
                }
                catch (UnauthorizedAccessException exception3)
                {
                    base.WriteError(new ErrorRecord(exception3, exception3.GetType().FullName, ErrorCategory.PermissionDenied, path));
                }
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        protected override bool IsValidPath(string path)
        {
            bool flag = true;
            string str = this.NormalizePath(path).TrimStart(new char[] { '\\' }).TrimEnd(new char[] { '\\' });
            int index = str.IndexOf('\\');
            if (index != -1)
            {
                str = str.Substring(0, index);
            }
            if (string.IsNullOrEmpty(str))
            {
                flag = true;
            }
            else if (this.GetHiveRoot(str) == null)
            {
                flag = false;
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        protected override bool ItemExists(string path)
        {
            bool flag = false;
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            try
            {
                if (this.IsHiveContainer(path))
                {
                    flag = true;
                }
                else
                {
                    IRegistryWrapper regkeyForPath = this.GetRegkeyForPath(path, false);
                    if (regkeyForPath != null)
                    {
                        flag = true;
                        regkeyForPath.Close();
                    }
                }
            }
            catch (IOException)
            {
            }
            catch (SecurityException)
            {
                flag = true;
            }
            catch (UnauthorizedAccessException)
            {
                flag = true;
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
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
            tracer.WriteLine("destination = {0}", new object[] { destination });
            string moveItemAction = RegistryProviderStrings.MoveItemAction;
            string moveItemResourceTemplate = RegistryProviderStrings.MoveItemResourceTemplate;
            string target = string.Format(base.Host.CurrentCulture, moveItemResourceTemplate, new object[] { path, destination });
            if (base.ShouldProcess(target, moveItemAction))
            {
                this.MoveRegistryItem(path, destination);
            }
        }

        private void MoveProperty(IRegistryWrapper sourceKey, IRegistryWrapper destinationKey, string sourceProperty, string destinationProperty)
        {
            string propertyName = this.GetPropertyName(sourceProperty);
            string b = this.GetPropertyName(destinationProperty);
            try
            {
                bool flag = true;
                if (string.Equals(sourceKey.Name, destinationKey.Name, StringComparison.OrdinalIgnoreCase) && string.Equals(propertyName, b, StringComparison.OrdinalIgnoreCase))
                {
                    flag = false;
                }
                this.CopyProperty(sourceKey, destinationKey, propertyName, b, false);
                if (flag)
                {
                    sourceKey.DeleteValue(propertyName);
                }
                object obj2 = destinationKey.GetValue(b);
                this.WriteWrappedPropertyObject(obj2, destinationProperty, destinationKey.Name);
            }
            catch (IOException exception)
            {
                base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.WriteError, sourceKey.Name));
            }
            catch (SecurityException exception2)
            {
                base.WriteError(new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.PermissionDenied, sourceKey.Name));
            }
            catch (UnauthorizedAccessException exception3)
            {
                base.WriteError(new ErrorRecord(exception3, exception3.GetType().FullName, ErrorCategory.PermissionDenied, sourceKey.Name));
            }
        }

        public void MoveProperty(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        {
            if (sourcePath == null)
            {
                throw PSTraceSource.NewArgumentNullException("sourcePath");
            }
            if (destinationPath == null)
            {
                throw PSTraceSource.NewArgumentNullException("destinationPath");
            }
            if (this.CheckOperationNotAllowedOnHiveContainer(sourcePath, destinationPath))
            {
                IRegistryWrapper regkeyForPathWriteIfError = this.GetRegkeyForPathWriteIfError(sourcePath, true);
                if (regkeyForPathWriteIfError != null)
                {
                    IRegistryWrapper destinationKey = this.GetRegkeyForPathWriteIfError(destinationPath, true);
                    if (destinationKey != null)
                    {
                        string movePropertyAction = RegistryProviderStrings.MovePropertyAction;
                        string movePropertyResourceTemplate = RegistryProviderStrings.MovePropertyResourceTemplate;
                        string target = string.Format(base.Host.CurrentCulture, movePropertyResourceTemplate, new object[] { sourcePath, sourceProperty, destinationPath, destinationProperty });
                        if (base.ShouldProcess(target, movePropertyAction))
                        {
                            try
                            {
                                this.MoveProperty(regkeyForPathWriteIfError, destinationKey, sourceProperty, destinationProperty);
                            }
                            catch (IOException exception)
                            {
                                base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.WriteError, sourcePath));
                            }
                            catch (SecurityException exception2)
                            {
                                base.WriteError(new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.PermissionDenied, sourcePath));
                            }
                            catch (UnauthorizedAccessException exception3)
                            {
                                base.WriteError(new ErrorRecord(exception3, exception3.GetType().FullName, ErrorCategory.PermissionDenied, sourcePath));
                            }
                        }
                        regkeyForPathWriteIfError.Close();
                        destinationKey.Close();
                    }
                }
            }
        }

        public object MovePropertyDynamicParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        {
            return null;
        }

        private void MoveRegistryItem(string path, string destination)
        {
            IRegistryWrapper regkeyForPathWriteIfError = this.GetRegkeyForPathWriteIfError(path, false);
            if (regkeyForPathWriteIfError != null)
            {
                bool flag = false;
                try
                {
                    flag = this.CopyRegistryKey(regkeyForPathWriteIfError, path, destination, true, true, true);
                }
                catch (IOException exception)
                {
                    base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.WriteError, path));
                    regkeyForPathWriteIfError.Close();
                    return;
                }
                catch (SecurityException exception2)
                {
                    base.WriteError(new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.PermissionDenied, path));
                    regkeyForPathWriteIfError.Close();
                    return;
                }
                catch (UnauthorizedAccessException exception3)
                {
                    base.WriteError(new ErrorRecord(exception3, exception3.GetType().FullName, ErrorCategory.PermissionDenied, path));
                    regkeyForPathWriteIfError.Close();
                    return;
                }
                regkeyForPathWriteIfError.Close();
                if (string.Equals(this.GetParentPath(path, null), destination, StringComparison.OrdinalIgnoreCase))
                {
                    flag = false;
                }
                if (flag)
                {
                    try
                    {
                        this.RemoveItem(path, true);
                    }
                    catch (IOException exception4)
                    {
                        base.WriteError(new ErrorRecord(exception4, exception4.GetType().FullName, ErrorCategory.WriteError, path));
                    }
                    catch (SecurityException exception5)
                    {
                        base.WriteError(new ErrorRecord(exception5, exception5.GetType().FullName, ErrorCategory.PermissionDenied, path));
                    }
                    catch (UnauthorizedAccessException exception6)
                    {
                        base.WriteError(new ErrorRecord(exception6, exception6.GetType().FullName, ErrorCategory.PermissionDenied, path));
                    }
                }
            }
        }

        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            if (drive == null)
            {
                throw PSTraceSource.NewArgumentNullException("drive");
            }
            if (!this.ItemExists(drive.Root))
            {
                Exception exception = new ArgumentException(RegistryProviderStrings.NewDriveRootDoesNotExist);
                base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.InvalidArgument, drive.Root));
            }
            return drive;
        }

        protected override void NewItem(string path, string type, object newItem)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            string newItemAction = RegistryProviderStrings.NewItemAction;
            string newItemResourceTemplate = RegistryProviderStrings.NewItemResourceTemplate;
            string target = string.Format(base.Host.CurrentCulture, newItemResourceTemplate, new object[] { path });
            if (base.ShouldProcess(target, newItemAction))
            {
                IRegistryWrapper regkeyForPath = this.GetRegkeyForPath(path, false);
                if (regkeyForPath != null)
                {
                    if (base.Force == 0)
                    {
                        Exception exception = new IOException(RegistryProviderStrings.KeyAlreadyExists);
                        base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.ResourceExists, regkeyForPath));
                        regkeyForPath.Close();
                        return;
                    }
                    regkeyForPath.Close();
                    this.RemoveItem(path, false);
                }
                if ((base.Force == 0) || this.CreateIntermediateKeys(path))
                {
                    string parentPath = this.GetParentPath(path, null);
                    string childName = this.GetChildName(path);
                    IRegistryWrapper regkeyForPathWriteIfError = this.GetRegkeyForPathWriteIfError(parentPath, true);
                    if (regkeyForPathWriteIfError != null)
                    {
                        try
                        {
                            IRegistryWrapper key = regkeyForPathWriteIfError.CreateSubKey(childName);
                            regkeyForPathWriteIfError.Close();
                            try
                            {
                                if (newItem != null)
                                {
                                    RegistryValueKind kind;
                                    if (!this.ParseKind(type, out kind))
                                    {
                                        return;
                                    }
                                    this.SetRegistryValue(key, string.Empty, newItem, kind, path, false);
                                }
                            }
                            catch (Exception exception2)
                            {
                                if (((!(exception2 is ArgumentException) && !(exception2 is InvalidCastException)) && (!(exception2 is IOException) && !(exception2 is SecurityException))) && (!(exception2 is UnauthorizedAccessException) && !(exception2 is NotSupportedException)))
                                {
                                    throw;
                                }
                                ErrorRecord errorRecord = new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.WriteError, key) {
                                    ErrorDetails = new ErrorDetails(StringUtil.Format(RegistryProviderStrings.KeyCreatedValueFailed, childName))
                                };
                                base.WriteError(errorRecord);
                            }
                            this.WriteRegistryItemObject(key, path);
                        }
                        catch (IOException exception3)
                        {
                            base.WriteError(new ErrorRecord(exception3, exception3.GetType().FullName, ErrorCategory.WriteError, path));
                        }
                        catch (SecurityException exception4)
                        {
                            base.WriteError(new ErrorRecord(exception4, exception4.GetType().FullName, ErrorCategory.PermissionDenied, path));
                        }
                        catch (UnauthorizedAccessException exception5)
                        {
                            base.WriteError(new ErrorRecord(exception5, exception5.GetType().FullName, ErrorCategory.PermissionDenied, path));
                        }
                        catch (ArgumentException exception6)
                        {
                            base.WriteError(new ErrorRecord(exception6, exception6.GetType().FullName, ErrorCategory.InvalidArgument, path));
                        }
                        catch (NotSupportedException exception7)
                        {
                            base.WriteError(new ErrorRecord(exception7, exception7.GetType().FullName, ErrorCategory.InvalidOperation, path));
                        }
                    }
                }
            }
        }

        public void NewProperty(string path, string propertyName, string type, object value)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (this.CheckOperationNotAllowedOnHiveContainer(path))
            {
                IRegistryWrapper regkeyForPathWriteIfError = this.GetRegkeyForPathWriteIfError(path, true);
                if (regkeyForPathWriteIfError != null)
                {
                    string newPropertyAction = RegistryProviderStrings.NewPropertyAction;
                    string newPropertyResourceTemplate = RegistryProviderStrings.NewPropertyResourceTemplate;
                    string target = string.Format(base.Host.CurrentCulture, newPropertyResourceTemplate, new object[] { path, propertyName });
                    if (base.ShouldProcess(target, newPropertyAction))
                    {
                        RegistryValueKind kind;
                        if (!this.ParseKind(type, out kind))
                        {
                            regkeyForPathWriteIfError.Close();
                            return;
                        }
                        try
                        {
                            if ((base.Force != 0) || (regkeyForPathWriteIfError.GetValue(propertyName) == null))
                            {
                                this.SetRegistryValue(regkeyForPathWriteIfError, propertyName, value, kind, path);
                            }
                            else
                            {
                                IOException exception = new IOException(RegistryProviderStrings.PropertyAlreadyExists);
                                base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.ResourceExists, path));
                                regkeyForPathWriteIfError.Close();
                                return;
                            }
                        }
                        catch (ArgumentException exception2)
                        {
                            base.WriteError(new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.WriteError, path));
                        }
                        catch (InvalidCastException exception3)
                        {
                            base.WriteError(new ErrorRecord(exception3, exception3.GetType().FullName, ErrorCategory.WriteError, path));
                        }
                        catch (IOException exception4)
                        {
                            base.WriteError(new ErrorRecord(exception4, exception4.GetType().FullName, ErrorCategory.WriteError, path));
                        }
                        catch (SecurityException exception5)
                        {
                            base.WriteError(new ErrorRecord(exception5, exception5.GetType().FullName, ErrorCategory.PermissionDenied, path));
                        }
                        catch (UnauthorizedAccessException exception6)
                        {
                            base.WriteError(new ErrorRecord(exception6, exception6.GetType().FullName, ErrorCategory.PermissionDenied, path));
                        }
                    }
                    regkeyForPathWriteIfError.Close();
                }
            }
        }

        public object NewPropertyDynamicParameters(string path, string propertyName, string type, object value)
        {
            return null;
        }

        public ObjectSecurity NewSecurityDescriptorFromPath(string path, AccessControlSections sections)
        {
            if (base.TransactionAvailable())
            {
                return new TransactedRegistrySecurity();
            }
            return new RegistrySecurity();
        }

        public ObjectSecurity NewSecurityDescriptorOfType(string type, AccessControlSections sections)
        {
            if (base.TransactionAvailable())
            {
                return new TransactedRegistrySecurity();
            }
            return new RegistrySecurity();
        }

        private string NormalizePath(string path)
        {
            string str = path;
            if (!string.IsNullOrEmpty(path))
            {
                str = path.Replace('/', '\\');
                if (this.HasRelativePathTokens(path))
                {
                    str = this.NormalizeRelativePath(str, null);
                }
            }
            tracer.WriteLine("result = {0}", new object[] { str });
            return str;
        }

        private bool ParseKind(string type, out RegistryValueKind kind)
        {
            kind = RegistryValueKind.Unknown;
            if (string.IsNullOrEmpty(type))
            {
                return true;
            }
            bool flag = true;
            Exception innerException = null;
            try
            {
                kind = (RegistryValueKind) Enum.Parse(typeof(RegistryValueKind), type, true);
            }
            catch (InvalidCastException exception2)
            {
                innerException = exception2;
            }
            catch (ArgumentException exception3)
            {
                innerException = exception3;
            }
            if (innerException != null)
            {
                flag = false;
                string typeParameterBindingFailure = RegistryProviderStrings.TypeParameterBindingFailure;
                Exception exception = new ArgumentException(string.Format(Thread.CurrentThread.CurrentCulture, typeParameterBindingFailure, new object[] { type, typeof(RegistryValueKind).FullName }), innerException);
                base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.InvalidArgument, type));
            }
            tracer.WriteLine("result = {0}", new object[] { (RegistryValueKind) kind });
            return flag;
        }

        private static object ReadExistingKeyValue(IRegistryWrapper key, string valueName)
        {
            try
            {
                return key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
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
            return null;
        }

        protected override void RemoveItem(string path, bool recurse)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            tracer.WriteLine("recurse = {0}", new object[] { recurse });
            string parentPath = this.GetParentPath(path, null);
            string childName = this.GetChildName(path);
            IRegistryWrapper regkeyForPathWriteIfError = this.GetRegkeyForPathWriteIfError(parentPath, true);
            if (regkeyForPathWriteIfError != null)
            {
                string removeKeyAction = RegistryProviderStrings.RemoveKeyAction;
                string removeKeyResourceTemplate = RegistryProviderStrings.RemoveKeyResourceTemplate;
                string target = string.Format(base.Host.CurrentCulture, removeKeyResourceTemplate, new object[] { path });
                if (base.ShouldProcess(target, removeKeyAction))
                {
                    try
                    {
                        regkeyForPathWriteIfError.DeleteSubKeyTree(childName);
                    }
                    catch (ArgumentException exception)
                    {
                        base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.WriteError, path));
                    }
                    catch (IOException exception2)
                    {
                        base.WriteError(new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.WriteError, path));
                    }
                    catch (SecurityException exception3)
                    {
                        base.WriteError(new ErrorRecord(exception3, exception3.GetType().FullName, ErrorCategory.PermissionDenied, path));
                    }
                    catch (UnauthorizedAccessException exception4)
                    {
                        base.WriteError(new ErrorRecord(exception4, exception4.GetType().FullName, ErrorCategory.PermissionDenied, path));
                    }
                    catch (NotSupportedException exception5)
                    {
                        base.WriteError(new ErrorRecord(exception5, exception5.GetType().FullName, ErrorCategory.InvalidOperation, path));
                    }
                }
                regkeyForPathWriteIfError.Close();
            }
        }

        public void RemoveProperty(string path, string propertyName)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (this.CheckOperationNotAllowedOnHiveContainer(path))
            {
                IRegistryWrapper regkeyForPathWriteIfError = this.GetRegkeyForPathWriteIfError(path, true);
                if (regkeyForPathWriteIfError != null)
                {
                    WildcardPattern pattern = new WildcardPattern(propertyName, WildcardOptions.IgnoreCase);
                    bool hadAMatch = false;
                    foreach (string str in regkeyForPathWriteIfError.GetValueNames())
                    {
                        if ((base.Context.SuppressWildcardExpansion || pattern.IsMatch(str)) && (!base.Context.SuppressWildcardExpansion || string.Equals(str, propertyName, StringComparison.OrdinalIgnoreCase)))
                        {
                            string removePropertyAction = RegistryProviderStrings.RemovePropertyAction;
                            string removePropertyResourceTemplate = RegistryProviderStrings.RemovePropertyResourceTemplate;
                            string target = string.Format(base.Host.CurrentCulture, removePropertyResourceTemplate, new object[] { path, str });
                            if (base.ShouldProcess(target, removePropertyAction))
                            {
                                string name = this.GetPropertyName(str);
                                try
                                {
                                    hadAMatch = true;
                                    regkeyForPathWriteIfError.DeleteValue(name);
                                }
                                catch (IOException exception)
                                {
                                    base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.WriteError, name));
                                }
                                catch (SecurityException exception2)
                                {
                                    base.WriteError(new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.PermissionDenied, name));
                                }
                                catch (UnauthorizedAccessException exception3)
                                {
                                    base.WriteError(new ErrorRecord(exception3, exception3.GetType().FullName, ErrorCategory.PermissionDenied, name));
                                }
                            }
                        }
                    }
                    regkeyForPathWriteIfError.Close();
                    this.WriteErrorIfPerfectMatchNotFound(hadAMatch, path, propertyName);
                }
            }
        }

        public object RemovePropertyDynamicParameters(string path, string propertyName)
        {
            return null;
        }

        protected override void RenameItem(string path, string newName)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            if (string.IsNullOrEmpty(newName))
            {
                throw PSTraceSource.NewArgumentException("newName");
            }
            tracer.WriteLine("newName = {0}", new object[] { newName });
            string parentPath = this.GetParentPath(path, null);
            string str2 = this.MakePath(parentPath, newName);
            if (this.ItemExists(str2))
            {
                Exception exception = new ArgumentException(RegistryProviderStrings.RenameItemAlreadyExists);
                base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.InvalidArgument, str2));
            }
            else
            {
                string renameItemAction = RegistryProviderStrings.RenameItemAction;
                string renameItemResourceTemplate = RegistryProviderStrings.RenameItemResourceTemplate;
                string target = string.Format(base.Host.CurrentCulture, renameItemResourceTemplate, new object[] { path, str2 });
                if (base.ShouldProcess(target, renameItemAction))
                {
                    this.MoveRegistryItem(path, str2);
                }
            }
        }

        public void RenameProperty(string path, string sourceProperty, string destinationProperty)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (this.CheckOperationNotAllowedOnHiveContainer(path))
            {
                IRegistryWrapper regkeyForPathWriteIfError = this.GetRegkeyForPathWriteIfError(path, true);
                if (regkeyForPathWriteIfError != null)
                {
                    string renamePropertyAction = RegistryProviderStrings.RenamePropertyAction;
                    string renamePropertyResourceTemplate = RegistryProviderStrings.RenamePropertyResourceTemplate;
                    string target = string.Format(base.Host.CurrentCulture, renamePropertyResourceTemplate, new object[] { path, sourceProperty, destinationProperty });
                    if (base.ShouldProcess(target, renamePropertyAction))
                    {
                        try
                        {
                            this.MoveProperty(regkeyForPathWriteIfError, regkeyForPathWriteIfError, sourceProperty, destinationProperty);
                        }
                        catch (IOException exception)
                        {
                            base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.WriteError, path));
                        }
                        catch (SecurityException exception2)
                        {
                            base.WriteError(new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.PermissionDenied, path));
                        }
                        catch (UnauthorizedAccessException exception3)
                        {
                            base.WriteError(new ErrorRecord(exception3, exception3.GetType().FullName, ErrorCategory.PermissionDenied, path));
                        }
                    }
                    regkeyForPathWriteIfError.Close();
                }
            }
        }

        public object RenamePropertyDynamicParameters(string path, string sourceProperty, string destinationProperty)
        {
            return null;
        }

        private object ResetRegistryKeyValue(IRegistryWrapper key, string valueName)
        {
            RegistryValueKind valueKind = key.GetValueKind(valueName);
            object obj2 = null;
            switch (valueKind)
            {
                case RegistryValueKind.Unknown:
                case RegistryValueKind.Binary:
                    obj2 = new byte[0];
                    break;

                case RegistryValueKind.String:
                case RegistryValueKind.ExpandString:
                    obj2 = "";
                    break;

                case RegistryValueKind.DWord:
                    obj2 = 0;
                    break;

                case RegistryValueKind.MultiString:
                    obj2 = new string[0];
                    break;

                case RegistryValueKind.QWord:
                    obj2 = 0L;
                    break;
            }
            try
            {
                key.SetValue(valueName, obj2, valueKind);
            }
            catch (IOException exception)
            {
                base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.WriteError, valueName));
            }
            catch (SecurityException exception2)
            {
                base.WriteError(new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.PermissionDenied, valueName));
            }
            catch (UnauthorizedAccessException exception3)
            {
                base.WriteError(new ErrorRecord(exception3, exception3.GetType().FullName, ErrorCategory.PermissionDenied, valueName));
            }
            return obj2;
        }

        protected override void SetItem(string path, object value)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            string setItemAction = RegistryProviderStrings.SetItemAction;
            string setItemResourceTemplate = RegistryProviderStrings.SetItemResourceTemplate;
            string target = string.Format(base.Host.CurrentCulture, setItemResourceTemplate, new object[] { path, value });
            if (base.ShouldProcess(target, setItemAction))
            {
                string name = null;
                IRegistryWrapper regkeyForPathWriteIfError = this.GetRegkeyForPathWriteIfError(path, true);
                if (regkeyForPathWriteIfError != null)
                {
                    bool flag = false;
                    if (base.DynamicParameters != null)
                    {
                        RegistryProviderSetItemDynamicParameter dynamicParameters = base.DynamicParameters as RegistryProviderSetItemDynamicParameter;
                        if (dynamicParameters != null)
                        {
                            try
                            {
                                RegistryValueKind type = dynamicParameters.Type;
                                regkeyForPathWriteIfError.SetValue(name, value, type);
                                flag = true;
                            }
                            catch (ArgumentException exception)
                            {
                                base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.InvalidArgument, name));
                                regkeyForPathWriteIfError.Close();
                                return;
                            }
                            catch (IOException exception2)
                            {
                                base.WriteError(new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.WriteError, path));
                                regkeyForPathWriteIfError.Close();
                                return;
                            }
                            catch (SecurityException exception3)
                            {
                                base.WriteError(new ErrorRecord(exception3, exception3.GetType().FullName, ErrorCategory.PermissionDenied, path));
                                regkeyForPathWriteIfError.Close();
                                return;
                            }
                            catch (UnauthorizedAccessException exception4)
                            {
                                base.WriteError(new ErrorRecord(exception4, exception4.GetType().FullName, ErrorCategory.PermissionDenied, path));
                                regkeyForPathWriteIfError.Close();
                                return;
                            }
                        }
                    }
                    if (!flag)
                    {
                        try
                        {
                            regkeyForPathWriteIfError.SetValue(name, value);
                        }
                        catch (IOException exception5)
                        {
                            base.WriteError(new ErrorRecord(exception5, exception5.GetType().FullName, ErrorCategory.WriteError, path));
                            regkeyForPathWriteIfError.Close();
                            return;
                        }
                        catch (SecurityException exception6)
                        {
                            base.WriteError(new ErrorRecord(exception6, exception6.GetType().FullName, ErrorCategory.PermissionDenied, path));
                            regkeyForPathWriteIfError.Close();
                            return;
                        }
                        catch (UnauthorizedAccessException exception7)
                        {
                            base.WriteError(new ErrorRecord(exception7, exception7.GetType().FullName, ErrorCategory.PermissionDenied, path));
                            regkeyForPathWriteIfError.Close();
                            return;
                        }
                    }
                    object item = value;
                    item = ReadExistingKeyValue(regkeyForPathWriteIfError, name);
                    regkeyForPathWriteIfError.Close();
                    base.WriteItemObject(item, path, false);
                }
            }
        }

        protected override object SetItemDynamicParameters(string path, object value)
        {
            return new RegistryProviderSetItemDynamicParameter();
        }

        public void SetProperty(string path, PSObject propertyValue)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (this.CheckOperationNotAllowedOnHiveContainer(path))
            {
                if (propertyValue == null)
                {
                    throw PSTraceSource.NewArgumentNullException("propertyValue");
                }
                IRegistryWrapper regkeyForPathWriteIfError = this.GetRegkeyForPathWriteIfError(path, true);
                if (regkeyForPathWriteIfError != null)
                {
                    RegistryValueKind unknown = RegistryValueKind.Unknown;
                    if (base.DynamicParameters != null)
                    {
                        RegistryProviderSetItemDynamicParameter dynamicParameters = base.DynamicParameters as RegistryProviderSetItemDynamicParameter;
                        if (dynamicParameters != null)
                        {
                            unknown = dynamicParameters.Type;
                        }
                    }
                    string setPropertyAction = RegistryProviderStrings.SetPropertyAction;
                    string setPropertyResourceTemplate = RegistryProviderStrings.SetPropertyResourceTemplate;
                    foreach (PSMemberInfo info in propertyValue.Properties)
                    {
                        object obj2 = info.Value;
                        string target = string.Format(base.Host.CurrentCulture, setPropertyResourceTemplate, new object[] { path, info.Name });
                        if (base.ShouldProcess(target, setPropertyAction))
                        {
                            try
                            {
                                this.SetRegistryValue(regkeyForPathWriteIfError, info.Name, obj2, unknown, path);
                            }
                            catch (InvalidCastException exception)
                            {
                                base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.WriteError, path));
                            }
                            catch (IOException exception2)
                            {
                                base.WriteError(new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.WriteError, info.Name));
                            }
                            catch (SecurityException exception3)
                            {
                                base.WriteError(new ErrorRecord(exception3, exception3.GetType().FullName, ErrorCategory.PermissionDenied, info.Name));
                            }
                            catch (UnauthorizedAccessException exception4)
                            {
                                base.WriteError(new ErrorRecord(exception4, exception4.GetType().FullName, ErrorCategory.PermissionDenied, info.Name));
                            }
                        }
                    }
                    regkeyForPathWriteIfError.Close();
                }
            }
        }

        public object SetPropertyDynamicParameters(string path, PSObject propertyValue)
        {
            return new RegistryProviderSetItemDynamicParameter();
        }

        private void SetRegistryValue(IRegistryWrapper key, string propertyName, object value, RegistryValueKind kind, string path)
        {
            this.SetRegistryValue(key, propertyName, value, kind, path, true);
        }

        private void SetRegistryValue(IRegistryWrapper key, string propertyName, object value, RegistryValueKind kind, string path, bool writeResult)
        {
            string valueName = this.GetPropertyName(propertyName);
            RegistryValueKind unknown = RegistryValueKind.Unknown;
            if (kind == RegistryValueKind.Unknown)
            {
                unknown = GetValueKindForProperty(key, valueName);
            }
            if (unknown != RegistryValueKind.Unknown)
            {
                try
                {
                    value = ConvertValueToKind(value, unknown);
                    kind = unknown;
                }
                catch (InvalidCastException)
                {
                    unknown = RegistryValueKind.Unknown;
                }
            }
            if (unknown == RegistryValueKind.Unknown)
            {
                if (kind == RegistryValueKind.Unknown)
                {
                    if (value != null)
                    {
                        kind = GetValueKindFromObject(value);
                    }
                    else
                    {
                        kind = RegistryValueKind.String;
                    }
                }
                value = ConvertValueToKind(value, kind);
            }
            key.SetValue(valueName, value, kind);
            if (writeResult)
            {
                object obj2 = key.GetValue(valueName);
                this.WriteWrappedPropertyObject(obj2, propertyName, path);
            }
        }

        public void SetSecurityDescriptor(string path, ObjectSecurity securityDescriptor)
        {
            IRegistryWrapper regkeyForPathWriteIfError = null;
            ObjectSecurity security;
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            if (securityDescriptor == null)
            {
                throw PSTraceSource.NewArgumentNullException("securityDescriptor");
            }
            path = this.NormalizePath(path);
            if (base.TransactionAvailable())
            {
                security = securityDescriptor as TransactedRegistrySecurity;
                if (security == null)
                {
                    throw PSTraceSource.NewArgumentException("securityDescriptor");
                }
            }
            else
            {
                security = securityDescriptor as RegistrySecurity;
                if (security == null)
                {
                    throw PSTraceSource.NewArgumentException("securityDescriptor");
                }
            }
            regkeyForPathWriteIfError = this.GetRegkeyForPathWriteIfError(path, true);
            if (regkeyForPathWriteIfError != null)
            {
                try
                {
                    regkeyForPathWriteIfError.SetAccessControl(security);
                }
                catch (SecurityException exception)
                {
                    base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.PermissionDenied, path));
                    return;
                }
                catch (UnauthorizedAccessException exception2)
                {
                    base.WriteError(new ErrorRecord(exception2, exception2.GetType().FullName, ErrorCategory.PermissionDenied, path));
                    return;
                }
                base.WriteSecurityDescriptorObject(security, path);
            }
        }

        private void WriteErrorIfPerfectMatchNotFound(bool hadAMatch, string path, string requestedValueName)
        {
            if (!hadAMatch && !WildcardPattern.ContainsWildcardCharacters(requestedValueName))
            {
                string propertyNotAtPath = RegistryProviderStrings.PropertyNotAtPath;
                Exception exception = new PSArgumentException(string.Format(Thread.CurrentThread.CurrentCulture, propertyNotAtPath, new object[] { requestedValueName, path }), (string)null);
                base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.InvalidArgument, requestedValueName));
            }
        }

        private void WriteRegistryItemObject(IRegistryWrapper key, string path)
        {
            if (key != null)
            {
                path = path.Replace('/', '\\');
                path = EscapeSpecialChars(path);
                PSObject item = PSObject.AsPSObject(key.RegistryKey);
                string[] valueNames = key.GetValueNames();
                for (int i = 0; i < valueNames.Length; i++)
                {
                    if (string.IsNullOrEmpty(valueNames[i]))
                    {
                        valueNames[i] = this.GetLocalizedDefaultToken();
                        break;
                    }
                }
                item.AddOrSetProperty("Property", valueNames);
                base.WriteItemObject(item, path, true);
            }
        }

        private void WriteWrappedPropertyObject(object value, string propertyName, string path)
        {
            PSObject propertyValue = new PSObject();
            string name = propertyName;
            if (string.IsNullOrEmpty(propertyName))
            {
                name = this.GetLocalizedDefaultToken();
            }
            propertyValue.Properties.Add(new PSNoteProperty(name, value));
            base.WritePropertyObject(propertyValue, path);
        }
    }
}

