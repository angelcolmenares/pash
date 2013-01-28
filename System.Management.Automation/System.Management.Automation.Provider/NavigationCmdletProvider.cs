namespace System.Management.Automation.Provider
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Text;

    public abstract class NavigationCmdletProvider : ContainerCmdletProvider
    {
        protected NavigationCmdletProvider()
        {
        }

        internal string ContractRelativePath(string path, string basePath, bool allowNonExistingPaths, CmdletProviderContext context)
        {
            base.Context = context;
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                return string.Empty;
            }
            if (basePath == null)
            {
                basePath = string.Empty;
            }
            CmdletProvider.providerBaseTracer.WriteLine("basePath = {0}", new object[] { basePath });
            string child = path;
            bool flag = false;
            string a = path;
            string b = basePath;
            if (OSHelper.IsWindows && !string.Equals(context.ProviderInstance.ProviderInfo.FullName, @"Microsoft.ActiveDirectory.Management\ActiveDirectory", StringComparison.OrdinalIgnoreCase))
            {
                a = path.Replace('/', '\\');
                b = basePath.Replace('/', '\\');
            }
            string str4 = path;
            Stack<string> tokenizedPathStack = null;
			char ch = (OSHelper.IsUnix ? '/' : '\\');
            if (path.EndsWith(ch.ToString(), StringComparison.OrdinalIgnoreCase))
            {
				path = path.TrimEnd(new char[] { (OSHelper.IsUnix ? '/' : '\\') });
                flag = true;
            }
			basePath = basePath.TrimEnd(new char[] { (OSHelper.IsUnix ? '/' : '\\')});
			if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase) && !str4.EndsWith((OSHelper.IsUnix ? "/" : "\\"), StringComparison.OrdinalIgnoreCase))
            {
                string childName = this.GetChildName(path);
                child = this.MakePath("..", childName);
            }
            else if (!a.StartsWith(b, StringComparison.OrdinalIgnoreCase) && (basePath.Length > 0))
            {
                child = string.Empty;
                string commonBase = this.GetCommonBase(a, b);
                int count = this.TokenizePathToStack(b, commonBase).Count;
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
					if (string.Equals(a, commonBase, StringComparison.OrdinalIgnoreCase) && !a.EndsWith((OSHelper.IsUnix ? "/" : "\\"), StringComparison.OrdinalIgnoreCase))
                    {
                        string str7 = this.GetChildName(path);
                        child = this.MakePath("..", child);
                        child = this.MakePath(child, str7);
                    }
                    else
                    {
                        string[] strArray = this.TokenizePathToStack(a, commonBase).ToArray();
                        for (int j = 0; j < strArray.Length; j++)
                        {
                            child = this.MakePath(child, strArray[j]);
                        }
                    }
                }
            }
            else
            {
                tokenizedPathStack = this.TokenizePathToStack(path, basePath);
                Stack<string> normalizedPathStack = new Stack<string>();
                try
                {
                    normalizedPathStack = NormalizeThePath(tokenizedPathStack, path, basePath, allowNonExistingPaths);
                }
                catch (ArgumentException exception)
                {
                    base.WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.InvalidArgument, null));
                    child = null;
                    goto Label_0243;
                }
                child = this.CreateNormalizedRelativePathFromStack(normalizedPathStack);
            }
        Label_0243:
            if (flag)
            {
				child = child + (OSHelper.IsUnix ? '/' : '\\');
            }
            CmdletProvider.providerBaseTracer.WriteLine("result = {0}", new object[] { child });
            return child;
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
            CmdletProvider.providerBaseTracer.WriteLine("result = {0}", new object[] { str });
            return str;
        }

        protected virtual string GetChildName(string path)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw PSTraceSource.NewArgumentException("path");
                }
				if (OSHelper.IsWindows) {
					path = path.Replace('/', '\\');
				}
				path = path.TrimEnd(new char[] { (OSHelper.IsUnix ? '/' : '\\') });
                string str = null;
				int startIndex = path.LastIndexOf((OSHelper.IsUnix ? '/' : '\\'));
                if (startIndex == -1)
                {
                    str = path;
                }
                else if (base.ItemExists(path, base.Context))
                {
                    string parentPath = this.GetParentPath(path, null);
                    if (string.IsNullOrEmpty(parentPath))
                    {
                        str = path;
                    }
					else if (parentPath.IndexOf((OSHelper.IsUnix ? '/' : '\\')) == (parentPath.Length - 1))
                    {
                        startIndex = path.IndexOf(parentPath, StringComparison.OrdinalIgnoreCase) + parentPath.Length;
                        str = path.Substring(startIndex);
                    }
                    else
                    {
                        startIndex = path.IndexOf(parentPath, StringComparison.OrdinalIgnoreCase) + parentPath.Length;
                        str = path.Substring(startIndex + 1);
                    }
                }
                else
                {
                    str = path.Substring(startIndex + 1);
                }
                CmdletProvider.providerBaseTracer.WriteLine("Result = {0}", new object[] { str });
                return str;
            }
        }

        internal string GetChildName(string path, CmdletProviderContext context)
        {
            base.Context = context;
            return this.GetChildName(path);
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

        protected virtual string GetParentPath(string path, string root)
        {
            string str3;
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                string str = null;
                if (string.IsNullOrEmpty(path))
                {
                    throw PSTraceSource.NewArgumentException("path");
                }
                if ((root == null) && (base.PSDriveInfo != null))
                {
                    root = base.PSDriveInfo.Root;
                }
				if (OSHelper.IsWindows) {
                	path = path.Replace('/', '\\');
				}
                path = path.TrimEnd(new char[] { (OSHelper.IsUnix ? '/' : '\\') });
                string strB = string.Empty;
                if (root != null)
                {
					if (OSHelper.IsUnix)
					{
                    	strB = root;
					}
					else {
						strB = root.Replace('/', '\\');
					}
                }
                if (string.Compare(path, strB, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    str = string.Empty;
                }
                else
                {
					int length = path.LastIndexOf((OSHelper.IsUnix ? '/' : '\\'));
                    switch (length)
                    {
                        case -1:
                            goto Label_00A3;

                        case 0:
                            length++;
                            break;
                    }
                    str = path.Substring(0, length);
                }
                goto Label_00A9;
            Label_00A3:
                str = string.Empty;
            Label_00A9:
                str3 = str;
            }
            return str3;
        }

        internal string GetParentPath(string path, string root, CmdletProviderContext context)
        {
            base.Context = context;
            return this.GetParentPath(path, root);
        }

        protected virtual bool IsItemContainer(string path)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported", new object[0]);
            }
        }

        internal bool IsItemContainer(string path, CmdletProviderContext context)
        {
            base.Context = context;
            return this.IsItemContainer(path);
        }

        protected virtual string MakePath(string parent, string child)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                string str = null;
                if ((parent == null) && (child == null))
                {
                    throw PSTraceSource.NewArgumentException("parent");
                }
                if (string.IsNullOrEmpty(parent) && string.IsNullOrEmpty(child))
                {
                    str = string.Empty;
                }
                else if (string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(child))
                {
					if (OSHelper.IsUnix)
					{
						str = child;
					}
                    else { 
						str = child.Replace('/', '\\');
					}
                }
                else if (!string.IsNullOrEmpty(parent) && string.IsNullOrEmpty(child))
                {
                    char ch = OSHelper.IsUnix ? '/' : '\\';
                    if (parent.EndsWith(ch.ToString(), StringComparison.Ordinal))
                    {
                        str = parent;
                    }
                    else
                    {
						str = parent + ch;
                    }
                }
                else
                {
					if (OSHelper.IsWindows) {
	                    parent = parent.Replace('/', '\\');
	                    child = child.Replace('/', '\\');
					}
                    StringBuilder builder = new StringBuilder();
                    char ch2 = OSHelper.IsUnix ? '/' : '\\';
                    if (parent.EndsWith(ch2.ToString(), StringComparison.Ordinal))
                    {
                        char ch3 = ch2;
                        if (child.StartsWith(ch3.ToString(), StringComparison.Ordinal))
                        {
                            builder.Append(parent);
                            builder.Append(child, 1, child.Length - 1);
                        }
                        else
                        {
                            builder.Append(parent);
                            builder.Append(child);
                        }
                    }
					else if (child.StartsWith ("/") && child.StartsWith (parent, StringComparison.OrdinalIgnoreCase))
					{
						builder.Append (child);
					}
                    else
                    {
						char ch4 = OSHelper.IsUnix ? '/' : '\\';
                        if (child.StartsWith(ch4.ToString(), StringComparison.CurrentCulture))
                        {
                            builder.Append(parent);
                            if (parent.Length == 0)
                            {
                                builder.Append(child, 1, child.Length - 1);
                            }
                            else
                            {
                                builder.Append(child);
                            }
                        }
                        else
                        {
                            builder.Append(parent);
                            if ((parent.Length > 0) && (child.Length > 0))
                            {
                                builder.Append(ch4);
                            }
                            builder.Append(child);
                        }
                    }
                    str = builder.ToString();
                }
                CmdletProvider.providerBaseTracer.WriteLine("result={0}", new object[] { str });
                return str;
            }
        }

        internal string MakePath(string parent, string child, CmdletProviderContext context)
        {
            base.Context = context;
            return this.MakePath(parent, child);
        }

        protected virtual void MoveItem(string path, string destination)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported", new object[0]);
            }
        }

        internal void MoveItem(string path, string destination, CmdletProviderContext context)
        {
            base.Context = context;
            this.MoveItem(path, destination);
        }

        protected virtual object MoveItemDynamicParameters(string path, string destination)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return null;
            }
        }

        internal object MoveItemDynamicParameters(string path, string destination, CmdletProviderContext context)
        {
            base.Context = context;
            return this.MoveItemDynamicParameters(path, destination);
        }

        protected virtual string NormalizeRelativePath(string path, string basePath)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return this.ContractRelativePath(path, basePath, false, base.Context);
            }
        }

        internal string NormalizeRelativePath(string path, string basePath, CmdletProviderContext context)
        {
            base.Context = context;
            return this.NormalizeRelativePath(path, basePath);
        }

        private static Stack<string> NormalizeThePath(Stack<string> tokenizedPathStack, string path, string basePath, bool allowNonExistingPaths)
        {
            Stack<string> stack = new Stack<string>();
            while (tokenizedPathStack.Count > 0)
            {
                string item = tokenizedPathStack.Pop();
                CmdletProvider.providerBaseTracer.WriteLine("childName = {0}", new object[] { item });
                if (!item.Equals(".", StringComparison.OrdinalIgnoreCase))
                {
                    if (item.Equals("..", StringComparison.OrdinalIgnoreCase))
                    {
                        if (stack.Count > 0)
                        {
                            string str2 = stack.Pop();
                            CmdletProvider.providerBaseTracer.WriteLine("normalizedPathStack.Pop() : {0}", new object[] { str2 });
                            continue;
                        }
                        if (!allowNonExistingPaths)
                        {
                            throw PSTraceSource.NewArgumentException("path", "SessionStateStrings", "NormalizeRelativePathOutsideBase", new object[] { path, basePath });
                        }
                    }
                    CmdletProvider.providerBaseTracer.WriteLine("normalizedPathStack.Push({0})", new object[] { item });
                    stack.Push(item);
                }
            }
            return stack;
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
                    stack.Push(parentPath);
                    return stack;
                }
                CmdletProvider.providerBaseTracer.WriteLine("tokenizedPathStack.Push({0})", new object[] { childName });
                stack.Push(childName);
                parentPath = this.GetParentPath(parentPath, basePath);
                if (parentPath.Length >= str2.Length)
                {
                    return stack;
                }
            }
            return stack;
        }
    }
}

