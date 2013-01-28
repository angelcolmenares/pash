namespace Microsoft.PowerShell.Commands.Internal
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Permissions;
    using System.Text;
    using System.Transactions;

    [ComVisible(true)]
    public sealed class TransactedRegistryKey : MarshalByRefObject, IDisposable
    {
        private RegistryKeyPermissionCheck checkMode;
        private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
        private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        private const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        private SafeRegistryHandle hkey;
        private static readonly string[] hkeyNames = new string[] { "HKEY_CLASSES_ROOT", "HKEY_CURRENT_USER", "HKEY_LOCAL_MACHINE", "HKEY_USERS", "HKEY_PERFORMANCE_DATA", "HKEY_CURRENT_CONFIG", "HKEY_DYN_DATA" };
        private string keyName;
        private const int MaxKeyLength = 0xff;
        private const int MaxValueDataLength = 0x100000;
        private const int MaxValueNameLength = 0x3fff;
        private Transaction myTransaction;
        private SafeTransactionHandle myTransactionHandle;
        private const string resBaseName = "RegistryProviderStrings";
        private int state;
        private const int STATE_DIRTY = 1;
        private const int STATE_SYSTEMKEY = 2;
        private const int STATE_WRITEACCESS = 4;

        private TransactedRegistryKey(SafeRegistryHandle hkey, bool writable, bool systemkey, Transaction transaction, SafeTransactionHandle txHandle)
        {
            this.hkey = hkey;
            this.keyName = "";
            if (systemkey)
            {
                this.state |= 2;
            }
            if (writable)
            {
                this.state |= 4;
            }
            if (null != transaction)
            {
                this.myTransaction = transaction.Clone();
                this.myTransactionHandle = txHandle;
            }
            else
            {
                this.myTransaction = null;
                this.myTransactionHandle = null;
            }
        }

        private RegistryValueKind CalculateValueKind(object value)
        {
            if (value is int)
            {
                return RegistryValueKind.DWord;
            }
            if (!(value is Array))
            {
                return RegistryValueKind.String;
            }
            if (value is byte[])
            {
                return RegistryValueKind.Binary;
            }
            if (value is string[])
            {
                return RegistryValueKind.MultiString;
            }
            string format = RegistryProviderStrings.Arg_RegSetBadArrType;
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, format, new object[] { value.GetType().Name }));
        }

        private void CheckKeyReadPermission()
        {
            if (this.checkMode == RegistryKeyPermissionCheck.Default)
            {
                new RegistryPermission(RegistryPermissionAccess.Read, this.keyName + @"\.").Demand();
            }
        }

        private void CheckOpenSubKeyPermission(string subkeyName, RegistryKeyPermissionCheck subKeyCheck)
        {
            if ((subKeyCheck == RegistryKeyPermissionCheck.Default) && (this.checkMode == RegistryKeyPermissionCheck.Default))
            {
                this.CheckSubKeyReadPermission(subkeyName);
            }
            this.CheckSubTreePermission(subkeyName, subKeyCheck);
        }

        private void CheckOpenSubKeyPermission(string subkeyName, bool subKeyWritable)
        {
            if (this.checkMode == RegistryKeyPermissionCheck.Default)
            {
                this.CheckSubKeyReadPermission(subkeyName);
            }
            if (subKeyWritable && (this.checkMode == RegistryKeyPermissionCheck.ReadSubTree))
            {
                this.CheckSubTreeReadWritePermission(subkeyName);
            }
        }

        private void CheckSubKeyCreatePermission(string subkeyName)
        {
            if (this.checkMode == RegistryKeyPermissionCheck.Default)
            {
                new RegistryPermission(RegistryPermissionAccess.Create, this.keyName + @"\" + subkeyName + @"\.").Demand();
            }
        }

        private void CheckSubKeyReadPermission(string subkeyName)
        {
            new RegistryPermission(RegistryPermissionAccess.Read, this.keyName + @"\" + subkeyName + @"\.").Demand();
        }

        private void CheckSubKeyWritePermission(string subkeyName)
        {
            if (this.checkMode == RegistryKeyPermissionCheck.Default)
            {
                new RegistryPermission(RegistryPermissionAccess.Write, this.keyName + @"\" + subkeyName + @"\.").Demand();
            }
        }

        private void CheckSubTreePermission(string subkeyName, RegistryKeyPermissionCheck subKeyCheck)
        {
            if (subKeyCheck == RegistryKeyPermissionCheck.ReadSubTree)
            {
                if (this.checkMode == RegistryKeyPermissionCheck.Default)
                {
                    this.CheckSubTreeReadPermission(subkeyName);
                }
            }
            else if ((subKeyCheck == RegistryKeyPermissionCheck.ReadWriteSubTree) && (this.checkMode != RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                this.CheckSubTreeReadWritePermission(subkeyName);
            }
        }

        private void CheckSubTreeReadPermission(string subkeyName)
        {
            if (this.checkMode == RegistryKeyPermissionCheck.Default)
            {
                new RegistryPermission(RegistryPermissionAccess.Read, this.keyName + @"\" + subkeyName + @"\").Demand();
            }
        }

        private void CheckSubTreeReadWritePermission(string subkeyName)
        {
            new RegistryPermission(RegistryPermissionAccess.Write | RegistryPermissionAccess.Read, this.keyName + @"\" + subkeyName).Demand();
        }

        private void CheckSubTreeWritePermission(string subkeyName)
        {
            if (this.checkMode == RegistryKeyPermissionCheck.Default)
            {
                new RegistryPermission(RegistryPermissionAccess.Write, this.keyName + @"\" + subkeyName + @"\").Demand();
            }
        }

        private void CheckValueCreatePermission(string valueName)
        {
            if (this.checkMode == RegistryKeyPermissionCheck.Default)
            {
                new RegistryPermission(RegistryPermissionAccess.Create, this.keyName + @"\" + valueName).Demand();
            }
        }

        private void CheckValueReadPermission(string valueName)
        {
            if (this.checkMode == RegistryKeyPermissionCheck.Default)
            {
                new RegistryPermission(RegistryPermissionAccess.Read, this.keyName + @"\" + valueName).Demand();
            }
        }

        private void CheckValueWritePermission(string valueName)
        {
            if (this.checkMode == RegistryKeyPermissionCheck.Default)
            {
                new RegistryPermission(RegistryPermissionAccess.Write, this.keyName + @"\" + valueName).Demand();
            }
        }

        public void Close()
        {
            this.Dispose(true);
        }

        private bool ContainsRegistryValue(string name)
        {
            int lpType = 0;
            int lpcbData = 0;
            return (Microsoft.PowerShell.Commands.Internal.Win32Native.RegQueryValueEx(this.hkey, name, null, ref lpType, (byte[]) null, ref lpcbData) == 0);
        }

        public TransactedRegistryKey CreateSubKey(string subkey)
        {
            return this.CreateSubKey(subkey, this.checkMode);
        }

        [ComVisible(false)]
        public TransactedRegistryKey CreateSubKey(string subkey, RegistryKeyPermissionCheck permissionCheck)
        {
            return this.CreateSubKeyInternal(subkey, permissionCheck, null);
        }

        [ComVisible(false)]
        public TransactedRegistryKey CreateSubKey(string subkey, RegistryKeyPermissionCheck permissionCheck, TransactedRegistrySecurity registrySecurity)
        {
            return this.CreateSubKeyInternal(subkey, permissionCheck, registrySecurity);
        }

        [ComVisible(false)]
        private unsafe TransactedRegistryKey CreateSubKeyInternal(string subkey, RegistryKeyPermissionCheck permissionCheck, object registrySecurityObj)
        {
            ValidateKeyName(subkey);
            if (string.Empty == subkey)
            {
                throw new ArgumentException(RegistryProviderStrings.Arg_RegKeyStrEmpty);
            }
            ValidateKeyMode(permissionCheck);
            this.EnsureWriteable();
            subkey = FixupName(subkey);
            TransactedRegistryKey key = this.InternalOpenSubKey(subkey, permissionCheck != RegistryKeyPermissionCheck.ReadSubTree);
            if (key != null)
            {
                this.CheckSubKeyWritePermission(subkey);
                this.CheckSubTreePermission(subkey, permissionCheck);
                key.checkMode = permissionCheck;
                return key;
            }
            this.CheckSubKeyCreatePermission(subkey);
            Microsoft.PowerShell.Commands.Internal.Win32Native.SECURITY_ATTRIBUTES structure = null;
            TransactedRegistrySecurity security = registrySecurityObj as TransactedRegistrySecurity;
            if (security != null)
            {
                structure = new Microsoft.PowerShell.Commands.Internal.Win32Native.SECURITY_ATTRIBUTES {
                    nLength = Marshal.SizeOf(structure)
                };
                byte[] securityDescriptorBinaryForm = security.GetSecurityDescriptorBinaryForm();
                byte* pDest = stackalloc byte[securityDescriptorBinaryForm.Length];
                Microsoft.PowerShell.Commands.Internal.Buffer.memcpy(securityDescriptorBinaryForm, 0, pDest, 0, securityDescriptorBinaryForm.Length);
                structure.pSecurityDescriptor = pDest;
            }
            int lpdwDisposition = 0;
            SafeRegistryHandle hkResult = null;
            int errorCode = 0;
            SafeTransactionHandle transactionHandle = this.GetTransactionHandle();
            errorCode = Microsoft.PowerShell.Commands.Internal.Win32Native.RegCreateKeyTransacted(this.hkey, subkey, 0, null, 0, GetRegistryKeyAccess(permissionCheck != RegistryKeyPermissionCheck.ReadSubTree), structure, out hkResult, out lpdwDisposition, transactionHandle, IntPtr.Zero);
            if ((errorCode == 0) && !hkResult.IsInvalid)
            {
                TransactedRegistryKey key2 = new TransactedRegistryKey(hkResult, permissionCheck != RegistryKeyPermissionCheck.ReadSubTree, false, Transaction.Current, transactionHandle);
                this.CheckSubTreePermission(subkey, permissionCheck);
                key2.checkMode = permissionCheck;
                if (subkey.Length == 0)
                {
                    key2.keyName = this.keyName;
                    return key2;
                }
                key2.keyName = this.keyName + @"\" + subkey;
                return key2;
            }
            if (errorCode != 0)
            {
                this.Win32Error(errorCode, this.keyName + @"\" + subkey);
            }
            return null;
        }

        public void DeleteSubKey(string subkey)
        {
            this.DeleteSubKey(subkey, true);
        }

        public void DeleteSubKey(string subkey, bool throwOnMissingSubKey)
        {
            ValidateKeyName(subkey);
            this.EnsureWriteable();
            subkey = FixupName(subkey);
            this.CheckSubKeyWritePermission(subkey);
            TransactedRegistryKey key = this.InternalOpenSubKey(subkey, false);
            if (key != null)
            {
                try
                {
                    if (key.InternalSubKeyCount() > 0)
                    {
                        throw new InvalidOperationException(RegistryProviderStrings.InvalidOperation_RegRemoveSubKey);
                    }
                }
                finally
                {
                    key.Close();
                }
                int errorCode = 0;
                SafeTransactionHandle transactionHandle = this.GetTransactionHandle();
                errorCode = Microsoft.PowerShell.Commands.Internal.Win32Native.RegDeleteKeyTransacted(this.hkey, subkey, 0, 0, transactionHandle, IntPtr.Zero);
                switch (errorCode)
                {
                    case 0:
                        return;

                    case 2:
                        if (throwOnMissingSubKey)
                        {
                            throw new ArgumentException(RegistryProviderStrings.ArgumentException_RegSubKeyAbsent);
                        }
                        return;
                }
                this.Win32Error(errorCode, null);
            }
            else if (throwOnMissingSubKey)
            {
                throw new ArgumentException(RegistryProviderStrings.ArgumentException_RegSubKeyAbsent);
            }
        }

        public void DeleteSubKeyTree(string subkey)
        {
            ValidateKeyName(subkey);
            if ((string.IsNullOrEmpty(subkey) || (subkey.Length == 0)) && this.IsSystemKey())
            {
                throw new ArgumentException(RegistryProviderStrings.ArgRegKeyDelHive);
            }
            this.EnsureWriteable();
            int errorCode = 0;
            SafeTransactionHandle transactionHandle = this.GetTransactionHandle();
            subkey = FixupName(subkey);
            this.CheckSubTreeWritePermission(subkey);
            TransactedRegistryKey key = this.InternalOpenSubKey(subkey, true);
            if (key == null)
            {
                throw new ArgumentException(RegistryProviderStrings.Arg_RegSubKeyAbsent);
            }
            try
            {
                if (key.InternalSubKeyCount() > 0)
                {
                    string[] subKeyNames = key.InternalGetSubKeyNames();
                    for (int i = 0; i < subKeyNames.Length; i++)
                    {
                        key.DeleteSubKeyTreeInternal(subKeyNames[i]);
                    }
                }
            }
            finally
            {
                key.Close();
            }
            errorCode = Microsoft.PowerShell.Commands.Internal.Win32Native.RegDeleteKeyTransacted(this.hkey, subkey, 0, 0, transactionHandle, IntPtr.Zero);
            if (errorCode != 0)
            {
                this.Win32Error(errorCode, null);
            }
        }

        private void DeleteSubKeyTreeInternal(string subkey)
        {
            int errorCode = 0;
            SafeTransactionHandle transactionHandle = this.GetTransactionHandle();
            TransactedRegistryKey key = this.InternalOpenSubKey(subkey, true);
            if (key == null)
            {
                throw new ArgumentException(RegistryProviderStrings.Arg_RegSubKeyAbsent);
            }
            try
            {
                if (key.InternalSubKeyCount() > 0)
                {
                    string[] subKeyNames = key.InternalGetSubKeyNames();
                    for (int i = 0; i < subKeyNames.Length; i++)
                    {
                        key.DeleteSubKeyTreeInternal(subKeyNames[i]);
                    }
                }
            }
            finally
            {
                key.Close();
            }
            errorCode = Microsoft.PowerShell.Commands.Internal.Win32Native.RegDeleteKeyTransacted(this.hkey, subkey, 0, 0, transactionHandle, IntPtr.Zero);
            if (errorCode != 0)
            {
                this.Win32Error(errorCode, null);
            }
        }

        public void DeleteValue(string name)
        {
            this.DeleteValue(name, true);
        }

        public void DeleteValue(string name, bool throwOnMissingValue)
        {
            this.EnsureWriteable();
            this.CheckValueWritePermission(name);
            this.VerifyTransaction();
            int errorCode = Microsoft.PowerShell.Commands.Internal.Win32Native.RegDeleteValue(this.hkey, name);
            switch (errorCode)
            {
                case 2:
                case 0xce:
                    if (throwOnMissingValue)
                    {
                        throw new ArgumentException(RegistryProviderStrings.Arg_RegSubKeyValueAbsent);
                    }
                    errorCode = 0;
                    break;
            }
            if (errorCode != 0)
            {
                this.Win32Error(errorCode, null);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if ((this.hkey != null) && !this.IsSystemKey())
            {
                try
                {
                    this.hkey.Dispose();
                }
                catch (IOException)
                {
                }
                finally
                {
                    this.hkey = null;
                }
            }
            if (null != this.myTransaction)
            {
                try
                {
                    this.myTransaction.Dispose();
                }
                catch (TransactionException)
                {
                }
                finally
                {
                    this.myTransaction = null;
                }
            }
        }

        private void EnsureNotDisposed()
        {
            if (this.hkey == null)
            {
                throw new ObjectDisposedException(this.keyName, RegistryProviderStrings.ObjectDisposed_RegKeyClosed);
            }
        }

        private void EnsureWriteable()
        {
            this.EnsureNotDisposed();
            if (!this.IsWritable())
            {
                throw new UnauthorizedAccessException(RegistryProviderStrings.UnauthorizedAccess_RegistryNoWrite);
            }
        }

        internal static string FixupName(string name)
        {
            if (name.IndexOf('\\') == -1)
            {
                return name;
            }
            StringBuilder path = new StringBuilder(name);
            FixupPath(path);
            int num = path.Length - 1;
            if (path[num] == '\\')
            {
                path.Length = num;
            }
            return path.ToString();
        }

        private static void FixupPath(StringBuilder path)
        {
            int num2;
            int length = path.Length;
            bool flag = false;
            char ch = Convert.ToChar(0xffff);
            for (num2 = 1; num2 < (length - 1); num2++)
            {
                if (path[num2] == '\\')
                {
                    num2++;
                    while (num2 < length)
                    {
                        if (path[num2] != '\\')
                        {
                            break;
                        }
                        path[num2] = ch;
                        num2++;
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                num2 = 0;
                int num3 = 0;
                while (num2 < length)
                {
                    if (path[num2] == ch)
                    {
                        num2++;
                    }
                    else
                    {
                        path[num3] = path[num2];
                        num2++;
                        num3++;
                    }
                }
                path.Length += num3 - num2;
            }
        }

        public void Flush()
        {
            this.VerifyTransaction();
            if ((this.hkey != null) && this.IsDirty())
            {
                int hresult = Microsoft.PowerShell.Commands.Internal.Win32Native.RegFlushKey(this.hkey);
                if (hresult != 0)
                {
                    throw new IOException(Microsoft.PowerShell.Commands.Internal.Win32Native.GetMessage(hresult), hresult);
                }
            }
        }

        public TransactedRegistrySecurity GetAccessControl()
        {
            return this.GetAccessControl(AccessControlSections.Group | AccessControlSections.Owner | AccessControlSections.Access);
        }

        public TransactedRegistrySecurity GetAccessControl(AccessControlSections includeSections)
        {
            this.EnsureNotDisposed();
            return new TransactedRegistrySecurity(this.hkey, this.keyName, includeSections);
        }

        internal static TransactedRegistryKey GetBaseKey(IntPtr hKey)
        {
            int index = ((int) hKey) & 0xfffffff;
            return new TransactedRegistryKey(new SafeRegistryHandle(hKey, false), true, true, null, null) { checkMode = RegistryKeyPermissionCheck.Default, keyName = hkeyNames[index] };
        }

        private static int GetRegistryKeyAccess(RegistryKeyPermissionCheck mode)
        {
            switch (mode)
            {
                case RegistryKeyPermissionCheck.Default:
                case RegistryKeyPermissionCheck.ReadSubTree:
                    return 0x20019;

                case RegistryKeyPermissionCheck.ReadWriteSubTree:
                    return 0x2001f;
            }
            return 0;
        }

        private static int GetRegistryKeyAccess(bool isWritable)
        {
            if (!isWritable)
            {
                return 0x20019;
            }
            return 0x2001f;
        }

        public string[] GetSubKeyNames()
        {
            this.CheckKeyReadPermission();
            return this.InternalGetSubKeyNames();
        }

        private RegistryKeyPermissionCheck GetSubKeyPermissonCheck(bool subkeyWritable)
        {
            if (this.checkMode == RegistryKeyPermissionCheck.Default)
            {
                return this.checkMode;
            }
            if (subkeyWritable)
            {
                return RegistryKeyPermissionCheck.ReadWriteSubTree;
            }
            return RegistryKeyPermissionCheck.ReadSubTree;
        }

        private SafeTransactionHandle GetTransactionHandle()
        {
            if (null != this.myTransaction)
            {
                if (!this.myTransaction.Equals(Transaction.Current))
                {
                    throw new InvalidOperationException(RegistryProviderStrings.InvalidOperation_MustUseSameTransaction);
                }
                return this.myTransactionHandle;
            }
            return SafeTransactionHandle.Create();
        }

        public object GetValue(string name)
        {
            this.CheckValueReadPermission(name);
            return this.InternalGetValue(name, null, false, true);
        }

        public object GetValue(string name, object defaultValue)
        {
            this.CheckValueReadPermission(name);
            return this.InternalGetValue(name, defaultValue, false, true);
        }

        [ComVisible(false)]
        public object GetValue(string name, object defaultValue, RegistryValueOptions options)
        {
            if ((options < RegistryValueOptions.None) || (options > RegistryValueOptions.DoNotExpandEnvironmentNames))
            {
                string format = RegistryProviderStrings.Arg_EnumIllegalVal;
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, format, new object[] { options.ToString() }));
            }
            bool doNotExpand = options == RegistryValueOptions.DoNotExpandEnvironmentNames;
            this.CheckValueReadPermission(name);
            return this.InternalGetValue(name, defaultValue, doNotExpand, true);
        }

        [ComVisible(false)]
        public RegistryValueKind GetValueKind(string name)
        {
            this.CheckValueReadPermission(name);
            this.EnsureNotDisposed();
            int lpType = 0;
            int lpcbData = 0;
            int errorCode = Microsoft.PowerShell.Commands.Internal.Win32Native.RegQueryValueEx(this.hkey, name, null, ref lpType, (byte[]) null, ref lpcbData);
            if (errorCode != 0)
            {
                this.Win32Error(errorCode, null);
            }
            if (!Enum.IsDefined(typeof(RegistryValueKind), lpType))
            {
                return RegistryValueKind.Unknown;
            }
            return (RegistryValueKind) lpType;
        }

        public string[] GetValueNames()
        {
            this.CheckKeyReadPermission();
            this.EnsureNotDisposed();
            int num = this.InternalValueCount();
            string[] strArray = new string[num];
            if (num > 0)
            {
                StringBuilder lpValueName = new StringBuilder(0x100);
                for (int i = 0; i < num; i++)
                {
                    int capacity = lpValueName.Capacity;
                    int errorCode = 0xea;
                    while (0xea == errorCode)
                    {
                        int lpcbValueName = capacity;
                        errorCode = Microsoft.PowerShell.Commands.Internal.Win32Native.RegEnumValue(this.hkey, i, lpValueName, ref lpcbValueName, Microsoft.PowerShell.Commands.Internal.Win32Native.NULL, null, null, null);
                        if (errorCode != 0)
                        {
                            if (errorCode != 0xea)
                            {
                                this.Win32Error(errorCode, null);
                            }
                            if (0x3fff == capacity)
                            {
                                this.Win32Error(errorCode, null);
                            }
                            capacity *= 2;
                            if (0x3fff < capacity)
                            {
                                capacity = 0x3fff;
                            }
                            lpValueName = new StringBuilder(capacity);
                        }
                    }
                    strArray[i] = lpValueName.ToString();
                }
            }
            return strArray;
        }

        internal string[] InternalGetSubKeyNames()
        {
            this.EnsureNotDisposed();
            int num = this.InternalSubKeyCount();
            string[] strArray = new string[num];
            if (num > 0)
            {
                StringBuilder lpName = new StringBuilder(0x100);
                for (int i = 0; i < num; i++)
                {
                    int capacity = lpName.Capacity;
                    int errorCode = Microsoft.PowerShell.Commands.Internal.Win32Native.RegEnumKeyEx(this.hkey, i, lpName, out capacity, null, null, null, null);
                    if (errorCode != 0)
                    {
                        this.Win32Error(errorCode, null);
                    }
                    strArray[i] = lpName.ToString();
                }
            }
            return strArray;
        }

        internal object InternalGetValue(string name, object defaultValue, bool doNotExpand, bool checkSecurity)
        {
            byte[] buffer;
            if (checkSecurity)
            {
                this.EnsureNotDisposed();
            }
            object obj2 = defaultValue;
            int lpType = 0;
            int lpcbData = 0;
            int num3 = Microsoft.PowerShell.Commands.Internal.Win32Native.RegQueryValueEx(this.hkey, name, null, ref lpType, (byte[]) null, ref lpcbData);
            switch (num3)
            {
                case 0:
                case 0xea:
                    switch (lpType)
                    {
                        case 0:
                        case 6:
                        case 8:
                        case 9:
                        case 10:
                            return obj2;

                        case 1:
                        {
                            StringBuilder builder = new StringBuilder(lpcbData / 2);
                            num3 = Microsoft.PowerShell.Commands.Internal.Win32Native.RegQueryValueEx(this.hkey, name, null, ref lpType, builder, ref lpcbData);
                            return builder.ToString();
                        }
                        case 2:
                        {
                            StringBuilder builder2 = new StringBuilder(lpcbData / 2);
                            num3 = Microsoft.PowerShell.Commands.Internal.Win32Native.RegQueryValueEx(this.hkey, name, null, ref lpType, builder2, ref lpcbData);
                            if (!doNotExpand)
                            {
                                return Environment.ExpandEnvironmentVariables(builder2.ToString());
                            }
                            return builder2.ToString();
                        }
                        case 3:
                        case 5:
                            goto Label_006F;

                        case 4:
                        {
                            if (lpcbData > 4)
                            {
                                goto Label_0093;
                            }
                            int num5 = 0;
                            num3 = Microsoft.PowerShell.Commands.Internal.Win32Native.RegQueryValueEx(this.hkey, name, null, ref lpType, ref num5, ref lpcbData);
                            return num5;
                        }
                        case 7:
                        {
                            IList<string> list = new List<string>();
                            char[] chArray = new char[lpcbData / 2];
                            num3 = Microsoft.PowerShell.Commands.Internal.Win32Native.RegQueryValueEx(this.hkey, name, null, ref lpType, chArray, ref lpcbData);
                            int startIndex = 0;
                            int length = chArray.Length;
                            while ((num3 == 0) && (startIndex < length))
                            {
                                int index = startIndex;
                                while ((index < length) && (chArray[index] != '\0'))
                                {
                                    index++;
                                }
                                if (index < length)
                                {
                                    if ((index - startIndex) > 0)
                                    {
                                        list.Add(new string(chArray, startIndex, index - startIndex));
                                    }
                                    else if (index != (length - 1))
                                    {
                                        list.Add(string.Empty);
                                    }
                                }
                                else
                                {
                                    list.Add(new string(chArray, startIndex, length - startIndex));
                                }
                                startIndex = index + 1;
                            }
                            obj2 = new string[list.Count];
                            list.CopyTo((string[]) obj2, 0);
                            return obj2;
                        }
                        case 11:
                            goto Label_0093;
                    }
                    break;
            }
            return obj2;
        Label_006F:
            buffer = new byte[lpcbData];
            num3 = Microsoft.PowerShell.Commands.Internal.Win32Native.RegQueryValueEx(this.hkey, name, null, ref lpType, buffer, ref lpcbData);
            return buffer;
        Label_0093:
            if (lpcbData > 8)
            {
                goto Label_006F;
            }
            long lpData = 0L;
            num3 = Microsoft.PowerShell.Commands.Internal.Win32Native.RegQueryValueEx(this.hkey, name, null, ref lpType, ref lpData, ref lpcbData);
            return lpData;
        }

        internal TransactedRegistryKey InternalOpenSubKey(string name, bool writable)
        {
            ValidateKeyName(name);
            this.EnsureNotDisposed();
            int registryKeyAccess = GetRegistryKeyAccess(writable);
            SafeRegistryHandle hkResult = null;
            SafeTransactionHandle transactionHandle = this.GetTransactionHandle();
            if ((this.RegOpenKeyTransactedWrapper(this.hkey, name, 0, registryKeyAccess, out hkResult, transactionHandle, IntPtr.Zero) == 0) && !hkResult.IsInvalid)
            {
                return new TransactedRegistryKey(hkResult, writable, false, Transaction.Current, transactionHandle) { keyName = this.keyName + @"\" + name };
            }
            return null;
        }

        private TransactedRegistryKey InternalOpenSubKey(string name, RegistryKeyPermissionCheck permissionCheck, int rights)
        {
            ValidateKeyName(name);
            ValidateKeyMode(permissionCheck);
            ValidateKeyRights(rights);
            this.EnsureNotDisposed();
            name = FixupName(name);
            this.CheckOpenSubKeyPermission(name, permissionCheck);
            SafeRegistryHandle hkResult = null;
            int num = 0;
            SafeTransactionHandle transactionHandle = this.GetTransactionHandle();
            num = this.RegOpenKeyTransactedWrapper(this.hkey, name, 0, rights, out hkResult, transactionHandle, IntPtr.Zero);
            if ((num == 0) && !hkResult.IsInvalid)
            {
                return new TransactedRegistryKey(hkResult, permissionCheck == RegistryKeyPermissionCheck.ReadWriteSubTree, false, Transaction.Current, transactionHandle) { keyName = this.keyName + @"\" + name, checkMode = permissionCheck };
            }
            if ((num == 5) || (num == 0x542))
            {
                throw new SecurityException(RegistryProviderStrings.Security_RegistryPermission);
            }
            return null;
        }

        internal int InternalSubKeyCount()
        {
            this.EnsureNotDisposed();
            int lpcSubKeys = 0;
            int lpcValues = 0;
            int errorCode = Microsoft.PowerShell.Commands.Internal.Win32Native.RegQueryInfoKey(this.hkey, null, null, Microsoft.PowerShell.Commands.Internal.Win32Native.NULL, ref lpcSubKeys, null, null, ref lpcValues, null, null, null, null);
            if (errorCode != 0)
            {
                this.Win32Error(errorCode, null);
            }
            return lpcSubKeys;
        }

        internal int InternalValueCount()
        {
            this.EnsureNotDisposed();
            int lpcValues = 0;
            int lpcSubKeys = 0;
            int errorCode = Microsoft.PowerShell.Commands.Internal.Win32Native.RegQueryInfoKey(this.hkey, null, null, Microsoft.PowerShell.Commands.Internal.Win32Native.NULL, ref lpcSubKeys, null, null, ref lpcValues, null, null, null, null);
            if (errorCode != 0)
            {
                this.Win32Error(errorCode, null);
            }
            return lpcValues;
        }

        private bool IsDirty()
        {
            return ((this.state & 1) != 0);
        }

        private bool IsSystemKey()
        {
            return ((this.state & 2) != 0);
        }

        private bool IsWritable()
        {
            return ((this.state & 4) != 0);
        }

        public TransactedRegistryKey OpenSubKey(string name)
        {
            return this.OpenSubKey(name, false);
        }

        [ComVisible(false)]
        public TransactedRegistryKey OpenSubKey(string name, RegistryKeyPermissionCheck permissionCheck)
        {
            ValidateKeyMode(permissionCheck);
            return this.InternalOpenSubKey(name, permissionCheck, GetRegistryKeyAccess(permissionCheck));
        }

        public TransactedRegistryKey OpenSubKey(string name, bool writable)
        {
            ValidateKeyName(name);
            this.EnsureNotDisposed();
            name = FixupName(name);
            this.CheckOpenSubKeyPermission(name, writable);
            SafeRegistryHandle hkResult = null;
            int num = 0;
            SafeTransactionHandle transactionHandle = this.GetTransactionHandle();
            num = this.RegOpenKeyTransactedWrapper(this.hkey, name, 0, GetRegistryKeyAccess(writable), out hkResult, transactionHandle, IntPtr.Zero);
            if ((num == 0) && !hkResult.IsInvalid)
            {
                return new TransactedRegistryKey(hkResult, writable, false, Transaction.Current, transactionHandle) { checkMode = this.GetSubKeyPermissonCheck(writable), keyName = this.keyName + @"\" + name };
            }
            if ((num == 5) || (num == 0x542))
            {
                throw new SecurityException(RegistryProviderStrings.Security_RegistryPermission);
            }
            return null;
        }

        [ComVisible(false)]
        public TransactedRegistryKey OpenSubKey(string name, RegistryKeyPermissionCheck permissionCheck, RegistryRights rights)
        {
            return this.InternalOpenSubKey(name, permissionCheck, (int) rights);
        }

        private int RegOpenKeyTransactedWrapper(SafeRegistryHandle hKey, string lpSubKey, int ulOptions, int samDesired, out SafeRegistryHandle hkResult, SafeTransactionHandle hTransaction, IntPtr pExtendedParameter)
        {
            int num = 0;
            SafeRegistryHandle handle = null;
            num = Microsoft.PowerShell.Commands.Internal.Win32Native.RegOpenKeyTransacted(this.hkey, lpSubKey, ulOptions, samDesired, out handle, hTransaction, pExtendedParameter);
            if ((num == 0) && !handle.IsInvalid)
            {
                int lpcSubKeys = 0;
                int lpcValues = 0;
                num = Microsoft.PowerShell.Commands.Internal.Win32Native.RegQueryInfoKey(handle, null, null, Microsoft.PowerShell.Commands.Internal.Win32Native.NULL, ref lpcSubKeys, null, null, ref lpcValues, null, null, null, null);
                if (0x1a2c == num)
                {
                    SafeRegistryHandle handle2 = null;
                    SafeRegistryHandle handle3 = null;
                    num = Microsoft.PowerShell.Commands.Internal.Win32Native.RegOpenKeyEx(this.hkey, lpSubKey, ulOptions, samDesired, out handle2);
                    if (num == 0)
                    {
                        num = Microsoft.PowerShell.Commands.Internal.Win32Native.RegOpenKeyTransacted(handle2, null, ulOptions, samDesired, out handle3, hTransaction, pExtendedParameter);
                        if (num == 0)
                        {
                            handle.Dispose();
                            handle = handle3;
                        }
                        handle2.Dispose();
                        handle2 = null;
                    }
                }
            }
            hkResult = handle;
            return num;
        }

        public void SetAccessControl(TransactedRegistrySecurity registrySecurity)
        {
            this.EnsureWriteable();
            if (registrySecurity == null)
            {
                throw new ArgumentNullException("registrySecurity");
            }
            this.VerifyTransaction();
            registrySecurity.Persist(this.hkey, this.keyName);
        }

        private void SetDirty()
        {
            this.state |= 1;
        }

        public void SetValue(string name, object value)
        {
            this.SetValue(name, value, RegistryValueKind.Unknown);
        }

        [ComVisible(false)]
        public void SetValue(string name, object value, RegistryValueKind valueKind)
        {
            if (value == null)
            {
                throw new ArgumentNullException(RegistryProviderStrings.Arg_Value);
            }
            if ((name != null) && (name.Length > 0x3fff))
            {
                throw new ArgumentException(RegistryProviderStrings.Arg_RegValueNameStrLenBug);
            }
            if (!Enum.IsDefined(typeof(RegistryValueKind), valueKind))
            {
                throw new ArgumentException(RegistryProviderStrings.Arg_RegBadKeyKind);
            }
            this.EnsureWriteable();
            this.VerifyTransaction();
            if (this.ContainsRegistryValue(name))
            {
                this.CheckValueWritePermission(name);
            }
            else
            {
                this.CheckValueCreatePermission(name);
            }
            if (valueKind == RegistryValueKind.Unknown)
            {
                valueKind = this.CalculateValueKind(value);
            }
            int errorCode = 0;
            try
            {
                string str;
                string[] strArray = new string[0];
                int num2 = 0;
                int num3 = 0;
                byte[] buffer2;
                switch (valueKind)
                {
                    case RegistryValueKind.String:
                    case RegistryValueKind.ExpandString:
                        str = value.ToString();
                        if (0x80000 < str.Length)
                        {
                            throw new ArgumentException(RegistryProviderStrings.Arg_ValueDataLenBug);
                        }
                        break;

                    case RegistryValueKind.Binary:
                        buffer2 = (byte[]) value;
                        if (0x100000 < buffer2.Length)
                        {
                            throw new ArgumentException(RegistryProviderStrings.Arg_ValueDataLenBug);
                        }
                        goto Label_01ED;

                    case RegistryValueKind.DWord:
                    {
                        int lpData = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                        errorCode = Microsoft.PowerShell.Commands.Internal.Win32Native.RegSetValueEx(this.hkey, name, 0, RegistryValueKind.DWord, ref lpData, 4);
                        goto Label_0277;
                    }
                    case RegistryValueKind.MultiString:
                        strArray = (string[]) ((string[]) value).Clone();
                        num2 = 0;
                        num3 = 0;
                        goto Label_0138;

                    case RegistryValueKind.QWord:
                    {
                        long num8 = Convert.ToInt64(value, CultureInfo.InvariantCulture);
                        errorCode = Microsoft.PowerShell.Commands.Internal.Win32Native.RegSetValueEx(this.hkey, name, 0, RegistryValueKind.QWord, ref num8, 8);
                        goto Label_0277;
                    }
                    default:
                        goto Label_0277;
                }
                errorCode = Microsoft.PowerShell.Commands.Internal.Win32Native.RegSetValueEx(this.hkey, name, 0, valueKind, str, (str.Length * 2) + 2);
                goto Label_0277;
            Label_0111:
                if (strArray[num3] == null)
                {
                    throw new ArgumentException(RegistryProviderStrings.Arg_RegSetStrArrNull);
                }
                num2 += (strArray[num3].Length + 1) * 2;
                num3++;
            Label_0138:
                if (num3 < strArray.Length)
                {
                    goto Label_0111;
                }
                num2 += 2;
                if (0x100000 < num2)
                {
                    throw new ArgumentException(RegistryProviderStrings.Arg_ValueDataLenBug);
                }
                byte[] bytes = new byte[num2];
                byte[] buffer3 = bytes;
                if (buffer3 != null)
                {
                    int length = buffer3.Length;
                }
                int byteIndex = 0;
                int num5 = 0;
                for (int i = 0; i < strArray.Length; i++)
                {
                    num5 = Encoding.Unicode.GetBytes(strArray[i], 0, strArray[i].Length, bytes, byteIndex);
                    byteIndex += num5;
                    bytes[byteIndex] = 0;
                    bytes[byteIndex + 1] = 0;
                    byteIndex += 2;
                }
                errorCode = Microsoft.PowerShell.Commands.Internal.Win32Native.RegSetValueEx(this.hkey, name, 0, RegistryValueKind.MultiString, bytes, num2);
                goto Label_0277;
            Label_01ED:
                errorCode = Microsoft.PowerShell.Commands.Internal.Win32Native.RegSetValueEx(this.hkey, name, 0, RegistryValueKind.Binary, buffer2, buffer2.Length);
            }
            catch (OverflowException)
            {
                throw new ArgumentException(RegistryProviderStrings.Arg_RegSetMismatchedKind);
            }
            catch (InvalidOperationException)
            {
                throw new ArgumentException(RegistryProviderStrings.Arg_RegSetMismatchedKind);
            }
            catch (FormatException)
            {
                throw new ArgumentException(RegistryProviderStrings.Arg_RegSetMismatchedKind);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(RegistryProviderStrings.Arg_RegSetMismatchedKind);
            }
        Label_0277:
            if (errorCode == 0)
            {
                this.SetDirty();
            }
            else
            {
                this.Win32Error(errorCode, null);
            }
        }

        public override string ToString()
        {
            this.EnsureNotDisposed();
            return this.keyName;
        }

        private static void ValidateKeyMode(RegistryKeyPermissionCheck mode)
        {
            if ((mode < RegistryKeyPermissionCheck.Default) || (mode > RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                throw new ArgumentException(RegistryProviderStrings.Argument_InvalidRegistryKeyPermissionCheck);
            }
        }

        private static void ValidateKeyName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(RegistryProviderStrings.Arg_Name);
            }
            int index = name.IndexOf(@"\", StringComparison.OrdinalIgnoreCase);
            int startIndex = 0;
            while (index != -1)
            {
                if ((index - startIndex) > 0xff)
                {
                    throw new ArgumentException(RegistryProviderStrings.Arg_RegKeyStrLenBug);
                }
                startIndex = index + 1;
                index = name.IndexOf(@"\", startIndex, StringComparison.OrdinalIgnoreCase);
            }
            if ((name.Length - startIndex) > 0xff)
            {
                throw new ArgumentException(RegistryProviderStrings.Arg_RegKeyStrLenBug);
            }
        }

        private static void ValidateKeyRights(int rights)
        {
            if ((rights & -983104) != 0)
            {
                throw new SecurityException(RegistryProviderStrings.Security_RegistryPermission);
            }
        }

        private void VerifyTransaction()
        {
            if (null == this.myTransaction)
            {
                throw new InvalidOperationException(RegistryProviderStrings.InvalidOperation_NotAssociatedWithTransaction);
            }
            if (!this.myTransaction.Equals(Transaction.Current))
            {
                throw new InvalidOperationException(RegistryProviderStrings.InvalidOperation_MustUseSameTransaction);
            }
        }

        internal void Win32Error(int errorCode, string str)
        {
            switch (errorCode)
            {
                case 2:
                {
                    string format = RegistryProviderStrings.Arg_RegKeyNotFound;
                    throw new IOException(string.Format(CultureInfo.CurrentCulture, format, new object[] { errorCode.ToString(CultureInfo.InvariantCulture) }));
                }
                case 5:
                    if (str != null)
                    {
                        string str2 = RegistryProviderStrings.UnauthorizedAccess_RegistryKeyGeneric_Key;
                        throw new UnauthorizedAccessException(string.Format(CultureInfo.CurrentCulture, str2, new object[] { str }));
                    }
                    throw new UnauthorizedAccessException();

                case 6:
                    this.hkey.SetHandleAsInvalid();
                    this.hkey = null;
                    break;
            }
            throw new IOException(Microsoft.PowerShell.Commands.Internal.Win32Native.GetMessage(errorCode), errorCode);
        }

        internal static void Win32ErrorStatic(int errorCode, string str)
        {
            if (errorCode != 5)
            {
                throw new IOException(Microsoft.PowerShell.Commands.Internal.Win32Native.GetMessage(errorCode), errorCode);
            }
            if (str != null)
            {
                string format = RegistryProviderStrings.UnauthorizedAccess_RegistryKeyGeneric_Key;
                throw new UnauthorizedAccessException(string.Format(CultureInfo.CurrentCulture, format, new object[] { str }));
            }
            throw new UnauthorizedAccessException();
        }

        public string Name
        {
            get
            {
                this.EnsureNotDisposed();
                return this.keyName;
            }
        }

        public int SubKeyCount
        {
            get
            {
                this.CheckKeyReadPermission();
                return this.InternalSubKeyCount();
            }
        }

        public int ValueCount
        {
            get
            {
                this.CheckKeyReadPermission();
                return this.InternalValueCount();
            }
        }
    }
}

