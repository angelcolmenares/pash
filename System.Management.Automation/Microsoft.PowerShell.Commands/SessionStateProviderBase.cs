namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Provider;
    using System.Security;

    public abstract class SessionStateProviderBase : ContainerCmdletProvider, IContentCmdletProvider
    {
        [TraceSource("SessionStateProvider", "Providers that produce a view of session state data.")]
        private static readonly PSTraceSource tracer = PSTraceSource.GetTracer("SessionStateProvider", "Providers that produce a view of session state data.");

        protected SessionStateProviderBase()
        {
        }

        internal virtual bool CanRenameItem(object item)
        {
            bool flag = true;
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        public void ClearContent(string path)
        {
            throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "IContent_Clear_NotSupported", new object[0]);
        }

        public object ClearContentDynamicParameters(string path)
        {
            return null;
        }

        protected override void ClearItem(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                base.WriteError(new ErrorRecord(PSTraceSource.NewArgumentNullException("path"), "ClearItemNullPath", ErrorCategory.InvalidArgument, path));
            }
            else
            {
                try
                {
                    string clearItemAction = SessionStateProviderBaseStrings.ClearItemAction;
                    string clearItemResourceTemplate = SessionStateProviderBaseStrings.ClearItemResourceTemplate;
                    string target = string.Format(base.Host.CurrentCulture, clearItemResourceTemplate, new object[] { path });
                    if (base.ShouldProcess(target, clearItemAction))
                    {
                        this.SetSessionStateItem(path, null, false);
                    }
                }
                catch (SessionStateException exception)
                {
                    base.WriteError(new ErrorRecord(exception.ErrorRecord, exception));
                }
                catch (PSArgumentException exception2)
                {
                    base.WriteError(new ErrorRecord(exception2.ErrorRecord, exception2));
                }
            }
        }

        protected override void CopyItem(string path, string copyPath, bool recurse)
        {
            if (string.IsNullOrEmpty(path))
            {
                Exception exception = PSTraceSource.NewArgumentException("path");
                base.WriteError(new ErrorRecord(exception, "CopyItemNullPath", ErrorCategory.InvalidArgument, path));
            }
            else if (string.IsNullOrEmpty(copyPath))
            {
                this.GetItem(path);
            }
            else
            {
                object item = null;
                try
                {
                    item = this.GetSessionStateItem(path);
                }
                catch (SecurityException exception2)
                {
                    base.WriteError(new ErrorRecord(exception2, "CopyItemSecurityException", ErrorCategory.ReadError, path));
                    return;
                }
                if (item != null)
                {
                    string copyItemAction = SessionStateProviderBaseStrings.CopyItemAction;
                    string copyItemResourceTemplate = SessionStateProviderBaseStrings.CopyItemResourceTemplate;
                    string target = string.Format(base.Host.CurrentCulture, copyItemResourceTemplate, new object[] { path, copyPath });
                    if (!base.ShouldProcess(target, copyItemAction))
                    {
                        return;
                    }
                    try
                    {
                        this.SetSessionStateItem(copyPath, this.GetValueOfItem(item), true);
                        return;
                    }
                    catch (SessionStateException exception3)
                    {
                        base.WriteError(new ErrorRecord(exception3.ErrorRecord, exception3));
                        return;
                    }
                    catch (PSArgumentException exception4)
                    {
                        base.WriteError(new ErrorRecord(exception4.ErrorRecord, exception4));
                        return;
                    }
                }
                PSArgumentException replaceParentContainsErrorRecordException = PSTraceSource.NewArgumentException("path", "SessionStateStrings", "CopyItemDoesntExist", new object[] { path });
                base.WriteError(new ErrorRecord(replaceParentContainsErrorRecordException.ErrorRecord, replaceParentContainsErrorRecordException));
            }
        }

        protected override void GetChildItems(string path, bool recurse)
        {
            CommandOrigin origin = base.Context.Origin;
            if (string.IsNullOrEmpty(path))
            {
                IDictionary sessionStateTable = null;
                try
                {
                    sessionStateTable = this.GetSessionStateTable();
                }
                catch (SecurityException exception)
                {
                    base.WriteError(new ErrorRecord(exception, "GetTableSecurityException", ErrorCategory.ReadError, path));
                    return;
                }
                List<DictionaryEntry> list = new List<DictionaryEntry>(sessionStateTable.Count + 1);
                foreach (DictionaryEntry entry in sessionStateTable)
                {
                    list.Add(entry);
                }
                list.Sort(delegate (DictionaryEntry left, DictionaryEntry right) {
                    string x = (string) left.Key;
                    string key = (string) right.Key;
                    return StringComparer.CurrentCultureIgnoreCase.Compare(x, key);
                });
                foreach (DictionaryEntry entry2 in list)
                {
                    try
                    {
                        if (SessionState.IsVisible(origin, entry2.Value))
                        {
                            base.WriteItemObject(entry2.Value, (string) entry2.Key, false);
                        }
                    }
                    catch (PSArgumentException exception2)
                    {
                        base.WriteError(new ErrorRecord(exception2.ErrorRecord, exception2));
                        break;
                    }
                    catch (SecurityException exception3)
                    {
                        base.WriteError(new ErrorRecord(exception3, "GetItemSecurityException", ErrorCategory.PermissionDenied, (string) entry2.Key));
                        break;
                    }
                }
            }
            else
            {
                object valueToCheck = null;
                try
                {
                    valueToCheck = this.GetSessionStateItem(path);
                }
                catch (PSArgumentException exception4)
                {
                    base.WriteError(new ErrorRecord(exception4.ErrorRecord, exception4));
                    return;
                }
                catch (SecurityException exception5)
                {
                    base.WriteError(new ErrorRecord(exception5, "GetItemSecurityException", ErrorCategory.PermissionDenied, path));
                    return;
                }
                if ((valueToCheck != null) && SessionState.IsVisible(origin, valueToCheck))
                {
                    base.WriteItemObject(valueToCheck, path, false);
                }
            }
        }

        protected override void GetChildNames(string path, ReturnContainers returnContainers)
        {
            CommandOrigin origin = base.Context.Origin;
            if (string.IsNullOrEmpty(path))
            {
                IDictionary sessionStateTable = null;
                try
                {
                    sessionStateTable = this.GetSessionStateTable();
                }
                catch (SecurityException exception)
                {
                    base.WriteError(new ErrorRecord(exception, "GetChildNamesSecurityException", ErrorCategory.ReadError, path));
                    return;
                }
                foreach (DictionaryEntry entry in sessionStateTable)
                {
                    try
                    {
                        if (SessionState.IsVisible(origin, entry.Value))
                        {
                            base.WriteItemObject(entry.Key, (string) entry.Key, false);
                        }
                    }
                    catch (PSArgumentException exception2)
                    {
                        base.WriteError(new ErrorRecord(exception2.ErrorRecord, exception2));
                        break;
                    }
                    catch (SecurityException exception3)
                    {
                        base.WriteError(new ErrorRecord(exception3, "GetItemSecurityException", ErrorCategory.PermissionDenied, (string) entry.Key));
                        break;
                    }
                }
            }
            else
            {
                object valueToCheck = null;
                try
                {
                    valueToCheck = this.GetSessionStateItem(path);
                }
                catch (SecurityException exception4)
                {
                    base.WriteError(new ErrorRecord(exception4, "GetChildNamesSecurityException", ErrorCategory.ReadError, path));
                    return;
                }
                if ((valueToCheck != null) && SessionState.IsVisible(origin, valueToCheck))
                {
                    base.WriteItemObject(path, path, false);
                }
            }
        }

        public IContentReader GetContentReader(string path)
        {
            return new SessionStateProviderBaseContentReaderWriter(path, this);
        }

        public object GetContentReaderDynamicParameters(string path)
        {
            return null;
        }

        public IContentWriter GetContentWriter(string path)
        {
            return new SessionStateProviderBaseContentReaderWriter(path, this);
        }

        public object GetContentWriterDynamicParameters(string path)
        {
            return null;
        }

        protected override void GetItem(string name)
        {
            bool isContainer = false;
            object valueToCheck = null;
            IDictionary sessionStateTable = this.GetSessionStateTable();
            if (sessionStateTable != null)
            {
                if (string.IsNullOrEmpty(name))
                {
                    isContainer = true;
                    valueToCheck = sessionStateTable.Values;
                }
                else
                {
                    valueToCheck = sessionStateTable[name];
                }
            }
            if ((valueToCheck != null) && SessionState.IsVisible(base.Context.Origin, valueToCheck))
            {
                base.WriteItemObject(valueToCheck, name, isContainer);
            }
        }

        internal abstract object GetSessionStateItem(string name);
        internal abstract IDictionary GetSessionStateTable();
        internal virtual object GetValueOfItem(object item)
        {
            object obj2 = item;
            if (item is DictionaryEntry)
            {
                DictionaryEntry entry = (DictionaryEntry) item;
                obj2 = entry.Value;
            }
            return obj2;
        }

        protected override bool HasChildItems(string path)
        {
            bool flag = false;
            if (string.IsNullOrEmpty(path))
            {
                try
                {
                    if (this.GetSessionStateTable().Count > 0)
                    {
                        flag = true;
                    }
                }
                catch (SecurityException exception)
                {
                    base.WriteError(new ErrorRecord(exception, "HasChildItemsSecurityException", ErrorCategory.ReadError, path));
                }
            }
            else
            {
                flag = false;
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        protected override bool IsValidPath(string path)
        {
            bool flag = true;
            if (string.IsNullOrEmpty(path))
            {
                flag = false;
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        protected override bool ItemExists(string path)
        {
            bool flag = false;
            if (string.IsNullOrEmpty(path))
            {
                flag = true;
            }
            else
            {
                object sessionStateItem = null;
                try
                {
                    sessionStateItem = this.GetSessionStateItem(path);
                }
                catch (SecurityException exception)
                {
                    base.WriteError(new ErrorRecord(exception, "ItemExistsSecurityException", ErrorCategory.ReadError, path));
                }
                if (sessionStateItem != null)
                {
                    flag = true;
                }
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        protected override void NewItem(string path, string type, object newItem)
        {
            if (string.IsNullOrEmpty(path))
            {
                Exception exception = PSTraceSource.NewArgumentException("path");
                base.WriteError(new ErrorRecord(exception, "NewItemNullPath", ErrorCategory.InvalidArgument, path));
            }
            else if (newItem == null)
            {
                ArgumentNullException exception2 = PSTraceSource.NewArgumentNullException("value");
                base.WriteError(new ErrorRecord(exception2, "NewItemValueNotSpecified", ErrorCategory.InvalidArgument, path));
            }
            else if (this.ItemExists(path) && (base.Force == 0))
            {
                PSArgumentException replaceParentContainsErrorRecordException = PSTraceSource.NewArgumentException("path", "SessionStateStrings", "NewItemAlreadyExists", new object[] { path });
                base.WriteError(new ErrorRecord(replaceParentContainsErrorRecordException.ErrorRecord, replaceParentContainsErrorRecordException));
            }
            else
            {
                string newItemAction = SessionStateProviderBaseStrings.NewItemAction;
                string newItemResourceTemplate = SessionStateProviderBaseStrings.NewItemResourceTemplate;
                string target = string.Format(base.Host.CurrentCulture, newItemResourceTemplate, new object[] { path, type, newItem });
                if (base.ShouldProcess(target, newItemAction))
                {
                    this.SetItem(path, newItem);
                }
            }
        }

        protected override void RemoveItem(string path, bool recurse)
        {
            if (string.IsNullOrEmpty(path))
            {
                Exception exception = PSTraceSource.NewArgumentException("path");
                base.WriteError(new ErrorRecord(exception, "RemoveItemNullPath", ErrorCategory.InvalidArgument, path));
            }
            else
            {
                string removeItemAction = SessionStateProviderBaseStrings.RemoveItemAction;
                string removeItemResourceTemplate = SessionStateProviderBaseStrings.RemoveItemResourceTemplate;
                string target = string.Format(base.Host.CurrentCulture, removeItemResourceTemplate, new object[] { path });
                if (base.ShouldProcess(target, removeItemAction))
                {
                    try
                    {
                        this.RemoveSessionStateItem(path);
                    }
                    catch (SessionStateException exception2)
                    {
                        base.WriteError(new ErrorRecord(exception2.ErrorRecord, exception2));
                    }
                    catch (SecurityException exception3)
                    {
                        base.WriteError(new ErrorRecord(exception3, "RemoveItemSecurityException", ErrorCategory.PermissionDenied, path));
                    }
                    catch (PSArgumentException exception4)
                    {
                        base.WriteError(new ErrorRecord(exception4.ErrorRecord, exception4));
                    }
                }
            }
        }

        internal abstract void RemoveSessionStateItem(string name);
        protected override void RenameItem(string name, string newName)
        {
            if (string.IsNullOrEmpty(name))
            {
                Exception exception = PSTraceSource.NewArgumentException("name");
                base.WriteError(new ErrorRecord(exception, "RenameItemNullPath", ErrorCategory.InvalidArgument, name));
            }
            else
            {
                object item = null;
                try
                {
                    item = this.GetSessionStateItem(name);
                }
                catch (SecurityException exception2)
                {
                    base.WriteError(new ErrorRecord(exception2, "RenameItemSecurityException", ErrorCategory.ReadError, name));
                    return;
                }
                if (item != null)
                {
                    if (this.ItemExists(newName) && (base.Force == 0))
                    {
                        PSArgumentException replaceParentContainsErrorRecordException = PSTraceSource.NewArgumentException("newName", "SessionStateStrings", "NewItemAlreadyExists", new object[] { newName });
                        base.WriteError(new ErrorRecord(replaceParentContainsErrorRecordException.ErrorRecord, replaceParentContainsErrorRecordException));
                    }
                    else
                    {
                        try
                        {
                            if (this.CanRenameItem(item))
                            {
                                string renameItemAction = SessionStateProviderBaseStrings.RenameItemAction;
                                string renameItemResourceTemplate = SessionStateProviderBaseStrings.RenameItemResourceTemplate;
                                string target = string.Format(base.Host.CurrentCulture, renameItemResourceTemplate, new object[] { name, newName });
                                if (base.ShouldProcess(target, renameItemAction))
                                {
                                    if (string.Equals(name, newName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        this.GetItem(newName);
                                    }
                                    else
                                    {
                                        try
                                        {
                                            this.SetSessionStateItem(newName, item, true);
                                            this.RemoveSessionStateItem(name);
                                        }
                                        catch (SessionStateException exception4)
                                        {
                                            base.WriteError(new ErrorRecord(exception4.ErrorRecord, exception4));
                                            return;
                                        }
                                        catch (PSArgumentException exception5)
                                        {
                                            base.WriteError(new ErrorRecord(exception5.ErrorRecord, exception5));
                                            return;
                                        }
                                        catch (SecurityException exception6)
                                        {
                                            base.WriteError(new ErrorRecord(exception6, "RenameItemSecurityException", ErrorCategory.PermissionDenied, name));
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        catch (SessionStateException exception7)
                        {
                            base.WriteError(new ErrorRecord(exception7.ErrorRecord, exception7));
                        }
                    }
                }
                else
                {
                    PSArgumentException exception8 = PSTraceSource.NewArgumentException("name", "SessionStateStrings", "RenameItemDoesntExist", new object[] { name });
                    base.WriteError(new ErrorRecord(exception8.ErrorRecord, exception8));
                }
            }
        }

        protected override void SetItem(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
            {
                base.WriteError(new ErrorRecord(PSTraceSource.NewArgumentNullException("name"), "SetItemNullName", ErrorCategory.InvalidArgument, name));
            }
            else
            {
                try
                {
                    string setItemAction = SessionStateProviderBaseStrings.SetItemAction;
                    string setItemResourceTemplate = SessionStateProviderBaseStrings.SetItemResourceTemplate;
                    string target = string.Format(base.Host.CurrentCulture, setItemResourceTemplate, new object[] { name, value });
                    if (base.ShouldProcess(target, setItemAction))
                    {
                        this.SetSessionStateItem(name, value, true);
                    }
                }
                catch (SessionStateException exception)
                {
                    base.WriteError(new ErrorRecord(exception.ErrorRecord, exception));
                }
                catch (PSArgumentException exception2)
                {
                    base.WriteError(new ErrorRecord(exception2.ErrorRecord, exception2));
                }
            }
        }

        internal abstract void SetSessionStateItem(string name, object value, bool writeItem);
    }
}

