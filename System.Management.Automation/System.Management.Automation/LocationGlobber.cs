namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Management.Automation.Provider;
    using System.Runtime.InteropServices;
    using System.Text;

    internal sealed class LocationGlobber
    {
        [TraceSource("PathResolution", "Traces the path resolution algorithm.")]
        private static PSTraceSource pathResolutionTracer = PSTraceSource.GetTracer("PathResolution", "Traces the path resolution algorithm.", false);
        private SessionState sessionState;
        [TraceSource("LocationGlobber", "The location globber converts PowerShell paths with glob characters to zero or more paths.")]
        private static PSTraceSource tracer = PSTraceSource.GetTracer("LocationGlobber", "The location globber converts PowerShell paths with glob characters to zero or more paths.");

        internal LocationGlobber(SessionState sessionState)
        {
            if (sessionState == null)
            {
                throw PSTraceSource.NewArgumentNullException("sessionState");
            }
            this.sessionState = sessionState;
        }

        private static string ConvertMshEscapeToRegexEscape(string path)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            char[] chArray = path.ToCharArray();
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < chArray.GetLength(0); i++)
            {
                if (chArray[i] == '`')
                {
                    if ((i + 1) < chArray.GetLength(0))
                    {
                        if (chArray[i + 1] == '`')
                        {
                            builder.Append('`');
                            i++;
                        }
                        else
                        {
                            builder.Append('\\');
                        }
                    }
                    else
                    {
                        builder.Append('\\');
                    }
                }
                else if (chArray[i] == '\\')
                {
                    builder.Append(@"\\");
                }
                else
                {
                    builder.Append(chArray[i]);
                }
            }
            tracer.WriteLine("Original path: {0} Converted to: {1}", new object[] { path, builder.ToString() });
            return builder.ToString();
        }

        internal Collection<string> ExpandGlobPath(string path, bool allowNonexistingPaths, ContainerCmdletProvider provider, CmdletProviderContext context)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (provider == null)
            {
                throw PSTraceSource.NewArgumentNullException("provider");
            }
            string updatedPath = null;
            string updatedFilter = null;
            string filter = context.Filter;
            bool flag = provider.ConvertPath(path, context.Filter, ref updatedPath, ref updatedFilter, context);
            if (flag)
            {
                tracer.WriteLine("Provider converted path and filter.", new object[0]);
                tracer.WriteLine("Original path: " + path, new object[0]);
                tracer.WriteLine("Converted path: " + updatedPath, new object[0]);
                tracer.WriteLine("Original filter: " + context.Filter, new object[0]);
                tracer.WriteLine("Converted filter: " + updatedFilter, new object[0]);
                path = updatedPath;
                filter = context.Filter;
            }
            NavigationCmdletProvider provider2 = provider as NavigationCmdletProvider;
            tracer.WriteLine("path = {0}", new object[] { path });
            Collection<string> collection = new Collection<string>();
            using (pathResolutionTracer.TraceScope("EXPANDING WILDCARDS", new object[0]))
            {
                if (ShouldPerformGlobbing(path, context))
                {
                    StringCollection currentDirs = new StringCollection();
                    Stack<string> stack = new Stack<string>();
                    using (pathResolutionTracer.TraceScope("Tokenizing path", new object[0]))
                    {
                        while (StringContainsGlobCharacters(path))
                        {
                            if (context.Stopping)
                            {
                                throw new PipelineStoppedException();
                            }
                            string childName = path;
                            if (provider2 != null)
                            {
                                childName = provider2.GetChildName(path, context);
                            }
                            if (string.IsNullOrEmpty(childName))
                            {
                                break;
                            }
                            tracer.WriteLine("Pushing leaf element: {0}", new object[] { childName });
                            pathResolutionTracer.WriteLine("Leaf element: {0}", new object[] { childName });
                            stack.Push(childName);
                            if (provider2 != null)
                            {
                                string root = string.Empty;
                                if (context != null)
                                {
                                    PSDriveInfo drive = context.Drive;
                                    if (drive != null)
                                    {
                                        root = drive.Root;
                                    }
                                }
                                string a = provider2.GetParentPath(path, root, context);
                                if (string.Equals(a, path, StringComparison.OrdinalIgnoreCase))
                                {
                                    throw PSTraceSource.NewInvalidOperationException("SessionStateStrings", "ProviderImplementationInconsistent", new object[] { provider.ProviderInfo.Name, path });
                                }
                                path = a;
                            }
                            else
                            {
                                path = string.Empty;
                            }
                            tracer.WriteLine("New path: {0}", new object[] { path });
                            pathResolutionTracer.WriteLine("Parent path: {0}", new object[] { path });
                        }
                        tracer.WriteLine("Base container path: {0}", new object[] { path });
                        if (stack.Count == 0)
                        {
                            string str7 = path;
                            if (provider2 != null)
                            {
                                str7 = provider2.GetChildName(path, context);
                                if (!string.IsNullOrEmpty(str7))
                                {
                                    path = provider2.GetParentPath(path, null, context);
                                }
                            }
                            else
                            {
                                path = string.Empty;
                            }
                            stack.Push(str7);
                            pathResolutionTracer.WriteLine("Leaf element: {0}", new object[] { str7 });
                        }
                        pathResolutionTracer.WriteLine("Root path of resolution: {0}", new object[] { path });
                    }
                    currentDirs.Add(path);
                    while (stack.Count > 0)
                    {
                        if (context.Stopping)
                        {
                            throw new PipelineStoppedException();
                        }
                        string leafElement = stack.Pop();
                        currentDirs = this.GenerateNewPathsWithGlobLeaf(currentDirs, leafElement, stack.Count == 0, provider, context);
                        if (stack.Count > 0)
                        {
                            using (pathResolutionTracer.TraceScope("Checking matches to ensure they are containers", new object[0]))
                            {
                                int index = 0;
                                while (index < currentDirs.Count)
                                {
                                    if (context.Stopping)
                                    {
                                        throw new PipelineStoppedException();
                                    }
                                    if ((provider2 != null) && !provider2.IsItemContainer(currentDirs[index], context))
                                    {
                                        tracer.WriteLine("Removing {0} because it is not a container", new object[] { currentDirs[index] });
                                        pathResolutionTracer.WriteLine("{0} is not a container", new object[] { currentDirs[index] });
                                        currentDirs.RemoveAt(index);
                                    }
                                    else if (provider2 != null)
                                    {
                                        pathResolutionTracer.WriteLine("{0} is a container", new object[] { currentDirs[index] });
                                        index++;
                                    }
                                }
                                continue;
                            }
                        }
                    }
                    foreach (string str9 in currentDirs)
                    {
                        pathResolutionTracer.WriteLine("RESOLVED PATH: {0}", new object[] { str9 });
                        collection.Add(str9);
                    }
                }
                else
                {
                    string str10 = context.SuppressWildcardExpansion ? path : RemoveGlobEscaping(path);
                    if (allowNonexistingPaths || provider.ItemExists(str10, context))
                    {
                        pathResolutionTracer.WriteLine("RESOLVED PATH: {0}", new object[] { str10 });
                        collection.Add(str10);
                    }
                    else
                    {
                        ItemNotFoundException exception2 = new ItemNotFoundException(path, "PathNotFound", SessionStateStrings.PathNotFound);
                        pathResolutionTracer.TraceError("Item does not exist: {0}", new object[] { path });
                        throw exception2;
                    }
                }
            }
            if (flag)
            {
                context.Filter = filter;
            }
            return collection;
        }

        private Collection<string> ExpandMshGlobPath(string path, bool allowNonexistingPaths, PSDriveInfo drive, ContainerCmdletProvider provider, CmdletProviderContext context)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (provider == null)
            {
                throw PSTraceSource.NewArgumentNullException("provider");
            }
            if (drive == null)
            {
                throw PSTraceSource.NewArgumentNullException("drive");
            }
            tracer.WriteLine("path = {0}", new object[] { path });
            NavigationCmdletProvider provider2 = provider as NavigationCmdletProvider;
            Collection<string> collection = new Collection<string>();
            using (pathResolutionTracer.TraceScope("EXPANDING WILDCARDS", new object[0]))
            {
                if (ShouldPerformGlobbing(path, context))
                {
                    StringCollection currentDirs = new StringCollection();
                    Stack<string> stack = new Stack<string>();
                    using (pathResolutionTracer.TraceScope("Tokenizing path", new object[0]))
                    {
                        while (StringContainsGlobCharacters(path))
                        {
                            if (context.Stopping)
                            {
                                throw new PipelineStoppedException();
                            }
                            string childName = path;
                            if (provider2 != null)
                            {
                                childName = provider2.GetChildName(path, context);
                            }
                            if (string.IsNullOrEmpty(childName))
                            {
                                break;
                            }
                            tracer.WriteLine("Pushing leaf element: {0}", new object[] { childName });
                            pathResolutionTracer.WriteLine("Leaf element: {0}", new object[] { childName });
                            stack.Push(childName);
                            if (provider2 != null)
                            {
                                string a = provider2.GetParentPath(path, drive.Root, context);
                                if (string.Equals(a, path, StringComparison.OrdinalIgnoreCase))
                                {
                                    throw PSTraceSource.NewInvalidOperationException("SessionStateStrings", "ProviderImplementationInconsistent", new object[] { provider.ProviderInfo.Name, path });
                                }
                                path = a;
                            }
                            else
                            {
                                path = string.Empty;
                            }
                            tracer.WriteLine("New path: {0}", new object[] { path });
                            pathResolutionTracer.WriteLine("Parent path: {0}", new object[] { path });
                        }
                        tracer.WriteLine("Base container path: {0}", new object[] { path });
                        if (stack.Count == 0)
                        {
                            string str3 = path;
                            if (provider2 != null)
                            {
                                str3 = provider2.GetChildName(path, context);
                                if (!string.IsNullOrEmpty(str3))
                                {
                                    path = provider2.GetParentPath(path, null, context);
                                }
                            }
                            else
                            {
                                path = string.Empty;
                            }
                            stack.Push(str3);
                            pathResolutionTracer.WriteLine("Leaf element: {0}", new object[] { str3 });
                        }
                        pathResolutionTracer.WriteLine("Root path of resolution: {0}", new object[] { path });
                    }
                    currentDirs.Add(path);
                    while (stack.Count > 0)
                    {
                        if (context.Stopping)
                        {
                            throw new PipelineStoppedException();
                        }
                        string leafElement = stack.Pop();
                        currentDirs = this.GenerateNewPSPathsWithGlobLeaf(currentDirs, drive, leafElement, stack.Count == 0, provider, context);
                        if (stack.Count > 0)
                        {
                            using (pathResolutionTracer.TraceScope("Checking matches to ensure they are containers", new object[0]))
                            {
                                int index = 0;
                                while (index < currentDirs.Count)
                                {
                                    if (context.Stopping)
                                    {
                                        throw new PipelineStoppedException();
                                    }
                                    string mshQualifiedPath = GetMshQualifiedPath(currentDirs[index], drive);
                                    if ((provider2 != null) && !this.sessionState.Internal.IsItemContainer(mshQualifiedPath, context))
                                    {
                                        tracer.WriteLine("Removing {0} because it is not a container", new object[] { currentDirs[index] });
                                        pathResolutionTracer.WriteLine("{0} is not a container", new object[] { currentDirs[index] });
                                        currentDirs.RemoveAt(index);
                                    }
                                    else if (provider2 != null)
                                    {
                                        pathResolutionTracer.WriteLine("{0} is a container", new object[] { currentDirs[index] });
                                        index++;
                                    }
                                }
                                continue;
                            }
                        }
                    }
                    foreach (string str6 in currentDirs)
                    {
                        pathResolutionTracer.WriteLine("RESOLVED PATH: {0}", new object[] { str6 });
                        collection.Add(str6);
                    }
                    return collection;
                }
                string str7 = context.SuppressWildcardExpansion ? path : RemoveGlobEscaping(path);
				string format = OSHelper.IsUnix && provider.GetType () == typeof(Microsoft.PowerShell.Commands.FileSystemProvider) ? (str7.StartsWith ("/") ? "{1}" : "{0}/{1}") : "{0}:" + '\\' + "{1}";
                if (drive.Hidden)
                {
                    if (IsProviderDirectPath(str7))
                    {
                        format = "{1}";
                    }
                    else
                    {
                        format = "{0}::{1}";
                    }
                }
                else
                {
					char ch = OSHelper.IsUnix && provider.GetType () == typeof(Microsoft.PowerShell.Commands.FileSystemProvider) ? '/' : '\\';
                    if (path.StartsWith(ch.ToString(), StringComparison.Ordinal))
                    {
						format = OSHelper.IsUnix && provider.GetType () == typeof(Microsoft.PowerShell.Commands.FileSystemProvider) ? "{1}" : "{0}:{1}";
                    }
                }
                string str9 = string.Format(CultureInfo.InvariantCulture, format, new object[] { drive.Name, str7 });
                if (allowNonexistingPaths || provider.ItemExists(this.GetProviderPath(str9, context), context))
                {
                    pathResolutionTracer.WriteLine("RESOLVED PATH: {0}", new object[] { str9 });
                    collection.Add(str9);
                    return collection;
                }
                ItemNotFoundException exception2 = new ItemNotFoundException(str9, "PathNotFound", SessionStateStrings.PathNotFound);
                pathResolutionTracer.TraceError("Item does not exist: {0}", new object[] { path });
                throw exception2;
            }
        }

        internal StringCollection GenerateNewPathsWithGlobLeaf(StringCollection currentDirs, string leafElement, bool isLastLeaf, ContainerCmdletProvider provider, CmdletProviderContext context)
        {
            if (currentDirs == null)
            {
                throw PSTraceSource.NewArgumentNullException("currentDirs");
            }
            if (provider == null)
            {
                throw PSTraceSource.NewArgumentNullException("provider");
            }
            NavigationCmdletProvider provider2 = provider as NavigationCmdletProvider;
            StringCollection strings = new StringCollection();
            if (((leafElement != null) && (leafElement.Length > 0)) && (StringContainsGlobCharacters(leafElement) || isLastLeaf))
            {
                WildcardPattern stringMatcher = new WildcardPattern(ConvertMshEscapeToRegexEscape(leafElement), WildcardOptions.IgnoreCase);
                Collection<WildcardPattern> includeMatcher = SessionStateUtilities.CreateWildcardsFromStrings(context.Include, WildcardOptions.IgnoreCase);
                Collection<WildcardPattern> excludeMatcher = SessionStateUtilities.CreateWildcardsFromStrings(context.Exclude, WildcardOptions.IgnoreCase);
                foreach (string str2 in currentDirs)
                {
                    using (pathResolutionTracer.TraceScope("Expanding wildcards for items under '{0}'", new object[] { str2 }))
                    {
                        if (context.Stopping)
                        {
                            throw new PipelineStoppedException();
                        }
                        string modifiedDirPath = null;
                        Collection<PSObject> collection3 = this.GetChildNamesInDir(str2, leafElement, !isLastLeaf, context, true, null, provider, out modifiedDirPath);
                        if (collection3 == null)
                        {
                            tracer.TraceError("GetChildNames returned a null array", new object[0]);
                            pathResolutionTracer.WriteLine("No child names returned for '{0}'", new object[] { str2 });
                        }
                        else
                        {
                            foreach (PSObject obj2 in collection3)
                            {
                                if (context.Stopping)
                                {
                                    throw new PipelineStoppedException();
                                }
                                string childName = string.Empty;
                                if (IsChildNameAMatch(obj2, stringMatcher, includeMatcher, excludeMatcher, out childName))
                                {
                                    string str5 = childName;
                                    if (provider2 != null)
                                    {
                                        str5 = provider2.MakePath(modifiedDirPath, childName, context);
                                    }
                                    tracer.WriteLine("Adding child path to dirs {0}", new object[] { str5 });
                                    strings.Add(str5);
                                }
                            }
                        }
                    }
                }
                return strings;
            }
            tracer.WriteLine("LeafElement does not contain any glob characters so do a MakePath", new object[0]);
            foreach (string str6 in currentDirs)
            {
                using (pathResolutionTracer.TraceScope("Expanding intermediate containers under '{0}'", new object[] { str6 }))
                {
                    if (context.Stopping)
                    {
                        throw new PipelineStoppedException();
                    }
                    string child = ConvertMshEscapeToRegexEscape(leafElement);
                    string parent = context.SuppressWildcardExpansion ? str6 : RemoveGlobEscaping(str6);
                    string path = child;
                    if (provider2 != null)
                    {
                        path = provider2.MakePath(parent, child, context);
                    }
                    if (provider.ItemExists(path, context))
                    {
                        tracer.WriteLine("Adding child path to dirs {0}", new object[] { path });
                        strings.Add(path);
                        pathResolutionTracer.WriteLine("Valid intermediate container: {0}", new object[] { path });
                    }
                }
            }
            return strings;
        }

        private StringCollection GenerateNewPSPathsWithGlobLeaf(StringCollection currentDirs, PSDriveInfo drive, string leafElement, bool isLastLeaf, ContainerCmdletProvider provider, CmdletProviderContext context)
        {
            if (currentDirs == null)
            {
                throw PSTraceSource.NewArgumentNullException("currentDirs");
            }
            if (provider == null)
            {
                throw PSTraceSource.NewArgumentNullException("provider");
            }
            NavigationCmdletProvider provider2 = provider as NavigationCmdletProvider;
            StringCollection strings = new StringCollection();
            if ((((leafElement != null) && (leafElement.Length > 0)) && StringContainsGlobCharacters(leafElement)) || isLastLeaf)
            {
                WildcardPattern stringMatcher = new WildcardPattern(ConvertMshEscapeToRegexEscape(leafElement), WildcardOptions.IgnoreCase);
                Collection<WildcardPattern> includeMatcher = SessionStateUtilities.CreateWildcardsFromStrings(context.Include, WildcardOptions.IgnoreCase);
                Collection<WildcardPattern> excludeMatcher = SessionStateUtilities.CreateWildcardsFromStrings(context.Exclude, WildcardOptions.IgnoreCase);
                foreach (string str2 in currentDirs)
                {
                    using (pathResolutionTracer.TraceScope("Expanding wildcards for items under '{0}'", new object[] { str2 }))
                    {
                        if (context.Stopping)
                        {
                            throw new PipelineStoppedException();
                        }
                        string modifiedDirPath = string.Empty;
                        Collection<PSObject> collection3 = this.GetChildNamesInDir(str2, leafElement, !isLastLeaf, context, false, drive, provider, out modifiedDirPath);
                        if (collection3 == null)
                        {
                            tracer.TraceError("GetChildNames returned a null array", new object[0]);
                            pathResolutionTracer.WriteLine("No child names returned for '{0}'", new object[] { str2 });
                        }
                        else
                        {
                            foreach (PSObject obj2 in collection3)
                            {
                                if (context.Stopping)
                                {
                                    throw new PipelineStoppedException();
                                }
                                string childName = string.Empty;
                                if (IsChildNameAMatch(obj2, stringMatcher, includeMatcher, excludeMatcher, out childName))
                                {
                                    string pattern = childName;
                                    if (provider2 != null)
                                    {
                                        string parent = RemoveMshQualifier(modifiedDirPath, drive);
                                        pattern = GetMshQualifiedPath(this.sessionState.Internal.MakePath(parent, childName, context), drive);
                                    }
                                    tracer.WriteLine("Adding child path to dirs {0}", new object[] { pattern });
                                    pattern = isLastLeaf ? pattern : WildcardPattern.Escape(pattern);
                                    strings.Add(pattern);
                                }
                            }
                        }
                    }
                }
                return strings;
            }
            tracer.WriteLine("LeafElement does not contain any glob characters so do a MakePath", new object[0]);
            foreach (string str7 in currentDirs)
            {
                using (pathResolutionTracer.TraceScope("Expanding intermediate containers under '{0}'", new object[] { str7 }))
                {
                    if (context.Stopping)
                    {
                        throw new PipelineStoppedException();
                    }
                    string child = ConvertMshEscapeToRegexEscape(leafElement);
                    string path = context.SuppressWildcardExpansion ? str7 : RemoveGlobEscaping(str7);
                    string mshQualifiedPath = GetMshQualifiedPath(path, drive);
                    string str11 = child;
                    if (provider2 != null)
                    {
                        string str12 = RemoveMshQualifier(mshQualifiedPath, drive);
                        str11 = GetMshQualifiedPath(this.sessionState.Internal.MakePath(str12, child, context), drive);
                    }
                    if (this.sessionState.Internal.ItemExists(str11, context))
                    {
                        tracer.WriteLine("Adding child path to dirs {0}", new object[] { str11 });
                        pathResolutionTracer.WriteLine("Valid intermediate container: {0}", new object[] { str11 });
                        strings.Add(str11);
                    }
                }
            }
            return strings;
        }

        internal string GenerateRelativePath(PSDriveInfo drive, string path, bool escapeCurrentLocation, CmdletProvider providerInstance, CmdletProviderContext context)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (drive == null)
            {
                throw PSTraceSource.NewArgumentNullException("drive");
            }
            string currentLocation = drive.CurrentLocation;
			bool flag1 = OSHelper.IsUnix && drive.Root.StartsWith ("/");
			if (!flag1 && !string.IsNullOrEmpty(currentLocation) && currentLocation.StartsWith(drive.Root, StringComparison.Ordinal))
            {
                currentLocation = currentLocation.Substring(drive.Root.Length);
            }
            if (escapeCurrentLocation)
            {
                currentLocation = WildcardPattern.Escape(currentLocation);
            }
            if (!string.IsNullOrEmpty(path))
            {
				var flag6 = OSHelper.IsUnix;
				var flag7 = flag6;
				if (!flag6) flag6 = (path[0] != '/');
                if ((path[0] != '\\') && flag6)
                {
                Label_024B:
                    if ((path.Length > 0) && this.HasRelativePathTokens(path))
                    {
                        if (context.Stopping)
                        {
                            throw new PipelineStoppedException();
                        }
                        bool flag = false;
                        bool flag2 = path.StartsWith("..", StringComparison.Ordinal);
                        bool flag3 = path.Length == 2;
                        bool flag4 = (path.Length > 2) && ((path[2] == '\\') || (path[2] == '/'));
						if (flag7) flag4 = flag7;
                        if (flag2 && (flag3 || flag4))
                        {
                            if (!string.IsNullOrEmpty(currentLocation))
                            {
                                currentLocation = this.sessionState.Internal.GetParentPath(providerInstance, currentLocation, drive.Root, context);
                            }
                            tracer.WriteLine("Parent path = {0}", new object[] { currentLocation });
                            path = path.Substring(2);
                            tracer.WriteLine("path = {0}", new object[] { path });
                            flag = true;
                            if (path.Length != 0)
                            {
								if (!flag7) {
	                                if ((path[0] == '\\') || (path[0] == '/'))
	                                {
	                                    path = path.Substring(1);
	                                }
								}
                                tracer.WriteLine("path = {0}", new object[] { path });
                                if (path.Length != 0)
                                {
                                    goto Label_024B;
                                }
                            }
                            goto Label_0260;
                        }
                        if (path.Equals(".", StringComparison.OrdinalIgnoreCase))
                        {
                            flag = true;
                            path = string.Empty;
                            goto Label_0260;
                        }
                        if (path.StartsWith(@".\", StringComparison.Ordinal) || path.StartsWith("./", StringComparison.Ordinal))
                        {
                            path = path.Substring(@".\".Length);
                            flag = true;
                            tracer.WriteLine("path = {0}", new object[] { path });
                            if (path.Length == 0)
                            {
                                goto Label_0260;
                            }
                        }
                        if ((path.Length == 0) || !flag)
                        {
                            goto Label_0260;
                        }
                        goto Label_024B;
                    }
                }
                else
                {
                    currentLocation = string.Empty;
                    path = path.Substring(1);
                    tracer.WriteLine("path = {0}", new object[] { path });
                }
            }
        Label_0260:
            if (!string.IsNullOrEmpty(path))
            {
                currentLocation = this.sessionState.Internal.MakePath(providerInstance, currentLocation, path, context);
            }
            NavigationCmdletProvider provider = providerInstance as NavigationCmdletProvider;
            if (provider != null)
            {
                string str2 = this.sessionState.Internal.MakePath(context.Drive.Root, currentLocation, context);
                string str3 = provider.ContractRelativePath(str2, context.Drive.Root, false, context);
                if (!string.IsNullOrEmpty(str3))
                {
					flag1 = OSHelper.IsUnix && context.Drive.Root.StartsWith ("/");
					if (!flag1 && str3.StartsWith(context.Drive.Root, StringComparison.Ordinal))
                    {
                        currentLocation = str3.Substring(context.Drive.Root.Length);
                    }
                    else
                    {
                        currentLocation = str3;
                    }
                }
                else
                {
                    currentLocation = "";
                }
            }
            tracer.WriteLine("result = {0}", new object[] { currentLocation });
            return currentLocation;
        }

        private Collection<PSObject> GetChildNamesInDir(string dir, string leafElement, bool getAllContainers, CmdletProviderContext context, bool dirIsProviderPath, PSDriveInfo drive, ContainerCmdletProvider provider, out string modifiedDirPath)
        {
            string updatedPath = null;
            string updatedFilter = null;
            Collection<PSObject> collection4;
            string filter = context.Filter;
            bool flag = provider.ConvertPath(leafElement, context.Filter, ref updatedPath, ref updatedFilter, context);
            if (flag)
            {
                tracer.WriteLine("Provider converted path and filter.", new object[0]);
                tracer.WriteLine("Original path: " + leafElement, new object[0]);
                tracer.WriteLine("Converted path: " + updatedPath, new object[0]);
                tracer.WriteLine("Original filter: " + context.Filter, new object[0]);
                tracer.WriteLine("Converted filter: " + updatedFilter, new object[0]);
                leafElement = updatedPath;
                context.Filter = updatedFilter;
            }
            ReturnContainers returnAllContainers = ReturnContainers.ReturnAllContainers;
            if (!getAllContainers)
            {
                returnAllContainers = ReturnContainers.ReturnMatchingContainers;
            }
            CmdletProviderContext context2 = new CmdletProviderContext(context);
            context2.SetFilters(new Collection<string>(), new Collection<string>(), context.Filter);
            try
            {
                string path = null;
                modifiedDirPath = null;
                if (dirIsProviderPath)
                {
                    modifiedDirPath = path = context.SuppressWildcardExpansion ? dir : RemoveGlobEscaping(dir);
                }
                else
                {
                    modifiedDirPath = GetMshQualifiedPath(dir, drive);
                    ProviderInfo info = null;
                    CmdletProvider providerInstance = null;
                    Collection<string> collection = this.GetGlobbedProviderPathsFromMonadPath(modifiedDirPath, false, context2, out info, out providerInstance);
                    modifiedDirPath = context.SuppressWildcardExpansion ? modifiedDirPath : RemoveGlobEscaping(modifiedDirPath);
                    if (collection.Count > 0)
                    {
                        path = collection[0];
                    }
                    else
                    {
                        if (flag)
                        {
                            context.Filter = filter;
                        }
                        return new Collection<PSObject>();
                    }
                }
                if (provider.HasChildItems(path, context2))
                {
                    provider.GetChildNames(path, returnAllContainers, context2);
                }
                if (context2.HasErrors())
                {
                    Collection<ErrorRecord> accumulatedErrorObjects = context2.GetAccumulatedErrorObjects();
                    if ((accumulatedErrorObjects != null) && (accumulatedErrorObjects.Count > 0))
                    {
                        foreach (ErrorRecord record in accumulatedErrorObjects)
                        {
                            context.WriteError(record);
                        }
                    }
                }
                Collection<PSObject> accumulatedObjects = context2.GetAccumulatedObjects();
                if (flag)
                {
                    context.Filter = filter;
                }
                collection4 = accumulatedObjects;
            }
            finally
            {
                context2.RemoveStopReferral();
            }
            return collection4;
        }

        internal static string GetDriveQualifiedPath(string path, PSDriveInfo drive)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (drive == null)
            {
                throw PSTraceSource.NewArgumentNullException("drive");
            }
            string str = path;
            bool flag = true;
            int index = path.IndexOf(':');
            if (index != -1)
            {
                if (drive.Hidden)
                {
                    flag = false;
                }
                else if (string.Equals(path.Substring(0, index), drive.Name, StringComparison.OrdinalIgnoreCase))
                {
                    flag = false;
                }
            }
            if (flag)
            {
				if (OSHelper.IsUnix && drive.Name.StartsWith ("/"))
				{
					str = path;
				}
				else {
	                string format = "{0}:" + '\\' + "{1}";
	                char ch = '\\';
	                if (path.StartsWith(ch.ToString(), StringComparison.Ordinal))
	                {
	                    format = "{0}:{1}";
	                }
	                str = string.Format(CultureInfo.InvariantCulture, format, new object[] { drive.Name, path });
				}
            }
            tracer.WriteLine("result = {0}", new object[] { str });
            return str;
        }

        private string GetDriveRootRelativePathFromProviderPath(string providerPath, PSDriveInfo drive, CmdletProviderContext context)
        {
            string childName = "";
            CmdletProvider containerProviderInstance = this.sessionState.Internal.GetContainerProviderInstance(drive.Provider);
            NavigationCmdletProvider provider2 = containerProviderInstance as NavigationCmdletProvider;
			if (!OSHelper.IsUnix) { providerPath = providerPath.Replace('/', '\\'); }
            providerPath = providerPath.TrimEnd(new char[] { '\\' });
			string str2 = OSHelper.IsUnix ? drive.Root : drive.Root.Replace('/', '\\').TrimEnd(new char[] { '\\' });
            while (!string.IsNullOrEmpty(providerPath) && !providerPath.Equals(str2, StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(childName))
                {
                    childName = this.sessionState.Internal.MakePath(containerProviderInstance, provider2.GetChildName(providerPath, context), childName, context);
                }
                else
                {
                    childName = provider2.GetChildName(providerPath, context);
                }
                providerPath = this.sessionState.Internal.GetParentPath(containerProviderInstance, providerPath, drive.Root, context);
            }
            return childName;
        }

        internal string GetDriveRootRelativePathFromPSPath (string path, CmdletProviderContext context, bool escapeCurrentLocation, out PSDriveInfo workingDriveForPath, out CmdletProvider providerInstance)
		{
			if (path == null) {
				throw PSTraceSource.NewArgumentNullException ("path");
			}
			if (OSHelper.IsUnix) {
				int index = path.IndexOf ("::");
				if (index != -1)
				{
					path = path.Substring (index + 2);
				}
			}
            workingDriveForPath = null;
            string driveName = null;
            if (this.sessionState.Drive.Current != null)
            {
                driveName = this.sessionState.Drive.Current.Name;
            }
            bool flag = false;
            if (this.IsAbsolutePath(path, out driveName))
            {
                tracer.WriteLine("Drive Name: {0}", new object[] { driveName });
                try
                {
                    workingDriveForPath = this.sessionState.Drive.Get(driveName);
                }
                catch (DriveNotFoundException)
                {
                    if (this.sessionState.Drive.Current == null)
                    {
                        throw;
                    }
					if (Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix)
					{
						flag = path.StartsWith ("/", StringComparison.OrdinalIgnoreCase);
						workingDriveForPath = this.sessionState.Drive.Current;
					}
					else
					{
						string str2 = OSHelper.IsUnix ? this.sessionState.Drive.Current.Root : this.sessionState.Drive.Current.Root.Replace('/', '\\');
						string tempPath = OSHelper.IsUnix ? path : path.Replace('/', '\\');
	                    if ((str2.IndexOf(":", StringComparison.CurrentCulture) >= 0) && tempPath.StartsWith(str2, StringComparison.OrdinalIgnoreCase))
	                    {
	                        flag = true;
							if (!OSHelper.IsUnix) {
		                        path = path.Substring(str2.Length);
		                        path = path.TrimStart(new char[] { '\\' });
		                        path = '\\' + path;
							}
	                        workingDriveForPath = this.sessionState.Drive.Current;
	                    }
	                    if (!flag)
	                    {
	                        throw;
	                    }
					}
                }
				if (!flag) /* && !OSHelper.IsUnix */
                {
                    path = path.Substring(driveName.Length + 1);
                }
            }
            else
            {
                workingDriveForPath = this.sessionState.Drive.Current;
            }
            if (workingDriveForPath == null)
            {
                ItemNotFoundException exception = new ItemNotFoundException(path, "PathNotFound", SessionStateStrings.PathNotFound);
                pathResolutionTracer.TraceError("Item does not exist: {0}", new object[] { path });
                throw exception;
            }
            try
            {
                providerInstance = this.sessionState.Internal.GetContainerProviderInstance(workingDriveForPath.Provider);
                context.Drive = workingDriveForPath;
                return this.GenerateRelativePath(workingDriveForPath, path, escapeCurrentLocation, providerInstance, context);
            }
            catch (PSNotSupportedException)
            {
                providerInstance = null;
                return "";
            }
        }

        internal Collection<PathInfo> GetGlobbedMonadPathsFromMonadPath(string path, bool allowNonexistingPaths, out CmdletProvider providerInstance)
        {
            CmdletProviderContext context = new CmdletProviderContext(this.sessionState.Internal.ExecutionContext);
            return this.GetGlobbedMonadPathsFromMonadPath(path, allowNonexistingPaths, context, out providerInstance);
        }

        internal Collection<PathInfo> GetGlobbedMonadPathsFromMonadPath (string path, bool allowNonexistingPaths, CmdletProviderContext context, out CmdletProvider providerInstance)
		{
			providerInstance = null;
			if (path == null) {
				throw PSTraceSource.NewArgumentNullException ("path");
			}
			if (context == null) {
				throw PSTraceSource.NewArgumentNullException ("context");
			}
            Collection<PathInfo> collection = new Collection<PathInfo>();
            using (pathResolutionTracer.TraceScope("Resolving MSH path \"{0}\" to MSH path", new object[] { path }))
            {
                TraceFilters(context);
                if (IsHomePath(path))
                {
                    using (pathResolutionTracer.TraceScope("Resolving HOME relative path.", new object[0]))
                    {
                        path = this.GetHomeRelativePath(path);
                    }
                }
                bool isProviderDirectPath = IsProviderDirectPath(path);
                bool isProviderQualifiedPath = IsProviderQualifiedPath(path);
                if (isProviderDirectPath || isProviderQualifiedPath)
                {
                    collection = this.ResolvePSPathFromProviderPath(path, context, allowNonexistingPaths, isProviderDirectPath, isProviderQualifiedPath, out providerInstance);
                }
                else
                {
                    collection = this.ResolveDriveQualifiedPath(path, context, allowNonexistingPaths, out providerInstance);
                }
                if (((allowNonexistingPaths || (collection.Count >= 1)) || WildcardPattern.ContainsWildcardCharacters(path)) || ((context.Include != null) && (context.Include.Count != 0)))
                {
                    return collection;
                }
                if ((context.Exclude != null) && (context.Exclude.Count != 0))
                {
                    return collection;
                }
                ItemNotFoundException exception = new ItemNotFoundException(path, "PathNotFound", SessionStateStrings.PathNotFound);
                pathResolutionTracer.TraceError("Item does not exist: {0}", new object[] { path });
                throw exception;
            }
        }

        internal Collection<string> GetGlobbedProviderPathsFromMonadPath(string path, bool allowNonexistingPaths, out ProviderInfo provider, out CmdletProvider providerInstance)
        {
            providerInstance = null;
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.sessionState.Internal.ExecutionContext);
            return this.GetGlobbedProviderPathsFromMonadPath(path, allowNonexistingPaths, context, out provider, out providerInstance);
        }

        internal Collection<string> GetGlobbedProviderPathsFromMonadPath(string path, bool allowNonexistingPaths, CmdletProviderContext context, out ProviderInfo provider, out CmdletProvider providerInstance)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (context == null)
            {
                throw PSTraceSource.NewArgumentNullException("context");
            }
            using (pathResolutionTracer.TraceScope("Resolving MSH path \"{0}\" to PROVIDER-INTERNAL path", new object[] { path }))
            {
                TraceFilters(context);
                if (IsProviderQualifiedPath(path))
                {
                    context.Drive = null;
                }
                PSDriveInfo drive = null;
                if (this.GetProviderPath(path, context, out provider, out drive) == null)
                {
                    providerInstance = null;
                    tracer.WriteLine("provider returned a null path so return an empty array", new object[0]);
                    pathResolutionTracer.WriteLine("Provider '{0}' returned null", new object[] { provider });
                    return new Collection<string>();
                }
                if (drive != null)
                {
                    context.Drive = drive;
                }
                Collection<string> collection = new Collection<string>();
                foreach (PathInfo info2 in this.GetGlobbedMonadPathsFromMonadPath(path, allowNonexistingPaths, context, out providerInstance))
                {
                    collection.Add(info2.ProviderPath);
                }
                return collection;
            }
        }

        internal Collection<string> GetGlobbedProviderPathsFromProviderPath(string path, bool allowNonexistingPaths, ContainerCmdletProvider containerProvider, CmdletProviderContext context)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (containerProvider == null)
            {
                throw PSTraceSource.NewArgumentNullException("containerProvider");
            }
            if (context == null)
            {
                throw PSTraceSource.NewArgumentNullException("context");
            }
            return this.ExpandGlobPath(path, allowNonexistingPaths, containerProvider, context);
        }

        internal Collection<string> GetGlobbedProviderPathsFromProviderPath(string path, bool allowNonexistingPaths, string providerId, out CmdletProvider providerInstance)
        {
            providerInstance = null;
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.sessionState.Internal.ExecutionContext);
            Collection<string> collection = this.GetGlobbedProviderPathsFromProviderPath(path, allowNonexistingPaths, providerId, context, out providerInstance);
            if (context.HasErrors())
            {
                ErrorRecord record = context.GetAccumulatedErrorObjects()[0];
                if (record != null)
                {
                    throw record.Exception;
                }
            }
            return collection;
        }

        internal Collection<string> GetGlobbedProviderPathsFromProviderPath(string path, bool allowNonexistingPaths, string providerId, CmdletProviderContext context, out CmdletProvider providerInstance)
        {
            providerInstance = null;
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (providerId == null)
            {
                throw PSTraceSource.NewArgumentNullException("providerId");
            }
            if (context == null)
            {
                throw PSTraceSource.NewArgumentNullException("context");
            }
            using (pathResolutionTracer.TraceScope("Resolving PROVIDER-INTERNAL path \"{0}\" to PROVIDER-INTERNAL path", new object[] { path }))
            {
                TraceFilters(context);
                return this.ResolveProviderPathFromProviderPath(path, providerId, allowNonexistingPaths, context, out providerInstance);
            }
        }

        internal string GetHomeRelativePath(string path)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            string str = path;
            if (IsHomePath(path) && (this.sessionState.Drive.Current != null))
            {
                ProviderInfo provider = this.sessionState.Drive.Current.Provider;
                if (IsProviderQualifiedPath(path))
                {
                    int index = path.IndexOf("::", StringComparison.Ordinal);
                    if (index != -1)
                    {
                        string name = path.Substring(0, index);
                        provider = this.sessionState.Internal.GetSingleProvider(name);
                        path = path.Substring(index + "::".Length);
                    }
                }
                if (path.IndexOf("~", StringComparison.Ordinal) == 0)
                {
                    if ((path.Length > 1) && ((path[1] == '\\') || (path[1] == '/')))
                    {
                        path = path.Substring(2);
                    }
                    else
                    {
                        path = path.Substring(1);
                    }
                    if ((provider.Home == null) || (provider.Home.Length <= 0))
                    {
                        InvalidOperationException exception = PSTraceSource.NewInvalidOperationException("SessionStateStrings", "HomePathNotSet", new object[] { provider.Name });
                        pathResolutionTracer.TraceError("HOME path not set for provider: {0}", new object[] { provider.Name });
                        throw exception;
                    }
                    CmdletProviderContext context = new CmdletProviderContext(this.sessionState.Internal.ExecutionContext);
                    pathResolutionTracer.WriteLine("Getting home path for provider: {0}", new object[] { provider.Name });
                    pathResolutionTracer.WriteLine("Provider HOME path: {0}", new object[] { provider.Home });
                    if (string.IsNullOrEmpty(path))
                    {
                        path = provider.Home;
                    }
                    else
                    {
                        path = this.sessionState.Internal.MakePath(provider, provider.Home, path, context);
                    }
                    pathResolutionTracer.WriteLine("HOME relative path: {0}", new object[] { path });
                }
                str = path;
            }
            tracer.WriteLine("result = {0}", new object[] { str });
            return str;
        }

        internal static string GetMshQualifiedPath(string path, PSDriveInfo drive)
        {
            if (drive.Hidden)
            {
                if (IsProviderDirectPath(path))
                {
                    return path;
                }
                return GetProviderQualifiedPath(path, drive.Provider);
            }
            return GetDriveQualifiedPath(path, drive);
        }

        internal string GetProviderPath(string path)
        {
            ProviderInfo provider = null;
            return this.GetProviderPath(path, out provider);
        }

        internal string GetProviderPath(string path, CmdletProviderContext context)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            PSDriveInfo drive = null;
            ProviderInfo provider = null;
            return this.GetProviderPath(path, context, out provider, out drive);
        }

        internal string GetProviderPath(string path, out ProviderInfo provider)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            CmdletProviderContext context = new CmdletProviderContext(this.sessionState.Internal.ExecutionContext);
            PSDriveInfo drive = null;
            provider = null;
            string str = this.GetProviderPath(path, context, out provider, out drive);
            if (context.HasErrors())
            {
                Collection<ErrorRecord> accumulatedErrorObjects = context.GetAccumulatedErrorObjects();
                if ((accumulatedErrorObjects != null) && (accumulatedErrorObjects.Count > 0))
                {
                    throw accumulatedErrorObjects[0].Exception;
                }
            }
            return str;
        }

        internal string GetProviderPath(string path, CmdletProviderContext context, out ProviderInfo provider, out PSDriveInfo drive)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (context == null)
            {
                throw PSTraceSource.NewArgumentNullException("context");
            }
            string str = null;
            provider = null;
            drive = null;
            if (IsHomePath(path))
            {
                using (pathResolutionTracer.TraceScope("Resolving HOME relative path.", new object[0]))
                {
                    path = this.GetHomeRelativePath(path);
                }
            }
            if (IsProviderDirectPath(path))
            {
                pathResolutionTracer.WriteLine("Path is PROVIDER-DIRECT", new object[0]);
                str = path;
                drive = null;
                provider = this.sessionState.Path.CurrentLocation.Provider;
                pathResolutionTracer.WriteLine("PROVIDER-INTERNAL path: {0}", new object[] { str });
                pathResolutionTracer.WriteLine("Provider: {0}", new object[] { provider });
            }
            else if (IsProviderQualifiedPath(path))
            {
                pathResolutionTracer.WriteLine("Path is PROVIDER-QUALIFIED", new object[0]);
                string providerId = null;
                str = ParseProviderPath(path, out providerId);
                drive = null;
                provider = this.sessionState.Internal.GetSingleProvider(providerId);
                pathResolutionTracer.WriteLine("PROVIDER-INTERNAL path: {0}", new object[] { str });
                pathResolutionTracer.WriteLine("Provider: {0}", new object[] { provider });
            }
            else
            {
                pathResolutionTracer.WriteLine("Path is DRIVE-QUALIFIED", new object[0]);
                CmdletProvider providerInstance = null;
                string workingPath = this.GetDriveRootRelativePathFromPSPath(path, context, false, out drive, out providerInstance);
                pathResolutionTracer.WriteLine("DRIVE-RELATIVE path: {0}", new object[] { workingPath });
                pathResolutionTracer.WriteLine("Drive: {0}", new object[] { drive.Name });
                pathResolutionTracer.WriteLine("Provider: {0}", new object[] { drive.Provider });
                context.Drive = drive;
                if (drive.Hidden)
                {
                    str = workingPath;
                }
                else
                {
                    str = this.GetProviderSpecificPath(drive, workingPath, context);
                }
                provider = drive.Provider;
            }
            tracer.WriteLine("result = {0}", new object[] { str });
            pathResolutionTracer.WriteLine("RESOLVED PATH: {0}", new object[] { str });
            return str;
        }

        internal static string GetProviderQualifiedPath(string path, ProviderInfo provider)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (provider == null)
            {
                throw PSTraceSource.NewArgumentNullException("provider");
            }
            string str = path;
            bool flag = false;
            int index = path.IndexOf("::", StringComparison.Ordinal);
            if (index != -1)
            {
                string providerName = path.Substring(0, index);
                if (provider.NameEquals(providerName))
                {
                    flag = true;
                }
            }
            if (!flag)
            {
                str = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", new object[] { provider.FullName, "::", path });
            }
            tracer.WriteLine("result = {0}", new object[] { str });
            return str;
        }

        private string GetProviderSpecificPath(PSDriveInfo drive, string workingPath, CmdletProviderContext context)
        {
            if (drive == null)
            {
                throw PSTraceSource.NewArgumentNullException("drive");
            }
            if (workingPath == null)
            {
                throw PSTraceSource.NewArgumentNullException("workingPath");
            }
            drive.Trace();
            tracer.WriteLine("workingPath = {0}", new object[] { workingPath });
            string root = drive.Root;
            try
            {
                root = this.sessionState.Internal.MakePath(drive.Provider, root, workingPath, context);
            }
            catch (NotSupportedException)
            {
            }
            return root;
        }

        private bool HasRelativePathTokens(string path)
        {
            string str = path.Replace('/', '\\');
            if (((!str.Equals(".", StringComparison.OrdinalIgnoreCase) && !str.Equals("..", StringComparison.OrdinalIgnoreCase)) && (!str.Contains(@"\.\") && !str.Contains(@"\..\"))) && ((!str.EndsWith(@"\..", StringComparison.OrdinalIgnoreCase) && !str.EndsWith(@"\.", StringComparison.OrdinalIgnoreCase)) && (!str.StartsWith(@"..\", StringComparison.OrdinalIgnoreCase) && !str.StartsWith(@".\", StringComparison.OrdinalIgnoreCase))))
            {
                return str.StartsWith("~", StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }

        internal static bool IsAbsolutePath(string path)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            bool flag = false;
            if (path.Length == 0)
            {
                flag = false;
            }
            else if (path.StartsWith(@".\", StringComparison.Ordinal))
            {
                flag = false;
            }
            else
            {
				if (Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix)
				{
					flag = path.StartsWith ("/", StringComparison.OrdinalIgnoreCase);
				}
				else
				{
	                int index = path.IndexOf(":", StringComparison.Ordinal);
	                if (index == -1)
	                {
	                    flag = false;
	                }
	                else if (index > 0)
	                {
	                    flag = true;
	                }
				}
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        internal bool IsAbsolutePath(string path, out string driveName)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            bool flag = false;
            if (this.sessionState.Drive.Current != null)
            {
                driveName = this.sessionState.Drive.Current.Name;
            }
            else
            {
                driveName = null;
            }
            if (path.Length == 0)
            {
                flag = false;
            }
            else if (path.StartsWith(@".\", StringComparison.Ordinal) || path.StartsWith("./", StringComparison.Ordinal))
            {
                flag = false;
            }
            else
            {
				if (OSHelper.IsUnix)
				{
					flag = path.StartsWith ("/", StringComparison.OrdinalIgnoreCase);
					if (!flag) {
						int index = path.IndexOf(":", StringComparison.CurrentCulture);
						if (index == -1)
						{
							flag = false;
						}
						else if (index > 0)
						{
							driveName = path.Substring(0, index);
							flag = true;
						}
					}
				}
				else
				{
	                int index = path.IndexOf(":", StringComparison.CurrentCulture);
	                if (index == -1)
	                {
	                    flag = false;
	                }
	                else if (index > 0)
	                {
	                    driveName = path.Substring(0, index);
	                    flag = true;
	                }
				}
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        private static bool IsChildNameAMatch(PSObject childObject, WildcardPattern stringMatcher, Collection<WildcardPattern> includeMatcher, Collection<WildcardPattern> excludeMatcher, out string childName)
        {
            bool flag = false;
            childName = null;
            object baseObject = childObject.BaseObject;
            if (baseObject is PSCustomObject)
            {
                tracer.TraceError("GetChildNames returned a null object", new object[0]);
            }
            else
            {
                childName = baseObject as string;
                if (childName == null)
                {
                    tracer.TraceError("GetChildNames returned an object that wasn't a string", new object[0]);
                }
                else
                {
                    pathResolutionTracer.WriteLine("Name returned from provider: {0}", new object[] { childName });
                    bool flag2 = WildcardPattern.ContainsWildcardCharacters(stringMatcher.Pattern);
                    bool flag3 = stringMatcher.IsMatch(childName);
                    tracer.WriteLine("isChildMatch = {0}", new object[] { flag3 });
                    bool flag4 = includeMatcher.Count > 0;
                    bool flag5 = excludeMatcher.Count > 0;
                    bool flag6 = SessionStateUtilities.MatchesAnyWildcardPattern(childName, includeMatcher, true);
                    tracer.WriteLine("isIncludeMatch = {0}", new object[] { flag6 });
                    if (flag3 || ((flag2 && flag4) && flag6))
                    {
                        pathResolutionTracer.WriteLine("Path wildcard match: {0}", new object[] { childName });
                        flag = true;
                        if (flag4 && !flag6)
                        {
                            pathResolutionTracer.WriteLine("Not included match: {0}", new object[] { childName });
                            flag = false;
                        }
                        if (flag5 && SessionStateUtilities.MatchesAnyWildcardPattern(childName, excludeMatcher, false))
                        {
                            pathResolutionTracer.WriteLine("Excluded match: {0}", new object[] { childName });
                            flag = false;
                        }
                    }
                    else
                    {
                        pathResolutionTracer.WriteLine("NOT path wildcard match: {0}", new object[] { childName });
                    }
                }
            }
            tracer.WriteLine("result = {0}; childName = {1}", new object[] { flag, childName });
            return flag;
        }

        internal static bool IsHomePath(string path)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            bool flag = false;
            if (IsProviderQualifiedPath(path))
            {
                int index = path.IndexOf("::", StringComparison.Ordinal);
                if (index != -1)
                {
                    path = path.Substring(index + "::".Length);
                }
            }
            if (path.IndexOf("~", StringComparison.Ordinal) == 0)
            {
                if (path.Length == 1)
                {
                    flag = true;
                }
                else if ((path.Length > 1) && ((path[1] == '\\') || (path[1] == '/')))
                {
                    flag = true;
                }
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        internal static bool IsProviderDirectPath(string path)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            bool flag = false;
			if (OSHelper.IsUnix)
			{
				flag = path.StartsWith ("/", StringComparison.OrdinalIgnoreCase);
			}
			else if (path.StartsWith(@"\\", StringComparison.Ordinal) || path.StartsWith("//", StringComparison.Ordinal))
            {
                flag = true;
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        internal static bool IsProviderQualifiedPath(string path)
        {
            string providerId = null;
            return IsProviderQualifiedPath(path, out providerId);
        }

        internal static bool IsProviderQualifiedPath(string path, out string providerId)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            providerId = null;
            bool flag = false;
            if (path.Length == 0)
            {
                flag = false;
            }
            else if (path.StartsWith(@".\", StringComparison.Ordinal) || path.StartsWith("./", StringComparison.Ordinal))
            {
                flag = false;
            }
            else
            {
				if (OSHelper.IsUnix)
				{
					flag = path.StartsWith ("/", StringComparison.OrdinalIgnoreCase);
					if (flag) providerId = "/";
					else {
						int index = path.IndexOf(':');
						if (((index == -1) || ((index + 1) >= path.Length) || (path[index + 1] != ':')))
						{
							flag = false;
						}
						else if (index > 0)
						{
							flag = true;
							providerId = path.Substring(0, index);
							tracer.WriteLine("providerId = {0}", new object[] { providerId });
						}
					}
				}
				else
				{

	                int index = path.IndexOf(':');
	                if (((index == -1) || ((index + 1) >= path.Length)) || (path[index + 1] != ':'))
	                {
	                    flag = false;
	                }
	                else if (index > 0)
	                {
	                    flag = true;
	                    providerId = path.Substring(0, index);
	                    tracer.WriteLine("providerId = {0}", new object[] { providerId });
	                }
				}
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        internal bool IsShellVirtualDrive(string driveName, out SessionStateScope scope)
        {
            if (driveName == null)
            {
                throw PSTraceSource.NewArgumentNullException("driveName");
            }
            bool flag = false;
            if (string.Compare(driveName, "GLOBAL", StringComparison.OrdinalIgnoreCase) == 0)
            {
                tracer.WriteLine("match found: {0}", new object[] { "GLOBAL" });
                flag = true;
                scope = this.sessionState.Internal.GlobalScope;
            }
            else if (string.Compare(driveName, "LOCAL", StringComparison.OrdinalIgnoreCase) == 0)
            {
                tracer.WriteLine("match found: {0}", new object[] { driveName });
                flag = true;
                scope = this.sessionState.Internal.CurrentScope;
            }
            else
            {
                scope = null;
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        private static string ParseProviderPath(string path, out string providerId)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
			string separator = "::";
            int index = path.IndexOf(separator, StringComparison.Ordinal);
		
            if (index <= 0)
            {
				separator = ":";
				index = path.IndexOf(separator, StringComparison.Ordinal);
				if (index <= 1)
				{
                	throw PSTraceSource.NewArgumentException("path", "SessionStateStrings", "NotProviderQualifiedPath", new object[0]);
				}
            }
            providerId = path.Substring(0, index);
			string str = path.Substring(index + separator.Length);
            tracer.WriteLine("result = {0}", new object[] { str });
            return str;
        }

        private static string RemoveDriveQualifier(string path)
        {
            string str = path;
			if (Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return str;
			}
            int index = path.IndexOf(":", StringComparison.Ordinal);
            if (index != -1)
            {
                if ((path[index + 1] == '\\') || (path[index + 1] == '/'))
                {
                    index++;
                }
                str = path.Substring(index + 1);
            }
            tracer.WriteLine("result = {0}", new object[] { str });
            return str;
        }

        private static string RemoveGlobEscaping(string path)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            return WildcardPattern.Unescape(path);
        }

        internal static string RemoveMshQualifier(string path, PSDriveInfo drive)
        {
            if (drive.Hidden)
            {
                return RemoveProviderQualifier(path);
            }
            return RemoveDriveQualifier(path);
        }

        internal static string RemoveProviderQualifier(string path)
        {
            string str = path;
            int index = path.IndexOf("::", StringComparison.Ordinal);
            if (index != -1)
            {
                str = path.Substring(index + "::".Length);
            }
            tracer.WriteLine("result = {0}", new object[] { str });
            return str;
        }

        private Collection<PathInfo> ResolveDriveQualifiedPath(string path, CmdletProviderContext context, bool allowNonexistingPaths, out CmdletProvider providerInstance)
        {
            providerInstance = null;
            PSDriveInfo workingDriveForPath = null;
            Collection<PathInfo> collection = new Collection<PathInfo>();
            pathResolutionTracer.WriteLine("Path is DRIVE-QUALIFIED", new object[0]);
            string str = this.GetDriveRootRelativePathFromPSPath(path, context, true, out workingDriveForPath, out providerInstance);
            pathResolutionTracer.WriteLine("DRIVE-RELATIVE path: {0}", new object[] { str });
            pathResolutionTracer.WriteLine("Drive: {0}", new object[] { workingDriveForPath.Name });
            pathResolutionTracer.WriteLine("Provider: {0}", new object[] { workingDriveForPath.Provider });
            context.Drive = workingDriveForPath;
            providerInstance = this.sessionState.Internal.GetContainerProviderInstance(workingDriveForPath.Provider);
            ContainerCmdletProvider provider = providerInstance as ContainerCmdletProvider;
            ItemCmdletProvider provider2 = providerInstance as ItemCmdletProvider;
            ProviderInfo providerInfo = providerInstance.ProviderInfo;
            string item = null;
            string providerPath = null;
            if (workingDriveForPath.Hidden)
            {
                item = GetProviderQualifiedPath(str, providerInfo);
                providerPath = str;
            }
            else
            {
                item = GetDriveQualifiedPath(str, workingDriveForPath);
                providerPath = this.GetProviderPath(path, context);
            }
            pathResolutionTracer.WriteLine("PROVIDER path: {0}", new object[] { providerPath });
            Collection<string> collection2 = new Collection<string>();
            if (!context.SuppressWildcardExpansion)
            {
                if (CmdletProviderManagementIntrinsics.CheckProviderCapabilities(ProviderCapabilities.ExpandWildcards, providerInfo))
                {
                    pathResolutionTracer.WriteLine("Wildcard matching is being performed by the provider.", new object[0]);
                    if ((provider2 != null) && WildcardPattern.ContainsWildcardCharacters(str))
                    {
                        foreach (string str4 in provider2.ExpandPath(providerPath, context))
                        {
                            collection2.Add(this.GetDriveRootRelativePathFromProviderPath(str4, workingDriveForPath, context));
                        }
                    }
                    else
                    {
                        collection2.Add(this.GetDriveRootRelativePathFromProviderPath(providerPath, workingDriveForPath, context));
                    }
                }
                else
                {
                    pathResolutionTracer.WriteLine("Wildcard matching is being performed by the engine.", new object[0]);
                    collection2 = this.ExpandMshGlobPath(str, allowNonexistingPaths, workingDriveForPath, provider, context);
                }
            }
            else if (provider2 != null)
            {
                if (allowNonexistingPaths || provider2.ItemExists(providerPath, context))
                {
                    collection2.Add(item);
                }
            }
            else
            {
                collection2.Add(item);
            }
            if ((((!allowNonexistingPaths && (collection2.Count < 1)) && !WildcardPattern.ContainsWildcardCharacters(path)) && ((context.Include == null) || (context.Include.Count == 0))) && ((context.Exclude == null) || (context.Exclude.Count == 0)))
            {
                ItemNotFoundException exception = new ItemNotFoundException(path, "PathNotFound", SessionStateStrings.PathNotFound);
                pathResolutionTracer.TraceError("Item does not exist: {0}", new object[] { path });
                throw exception;
            }
            foreach (string str5 in collection2)
            {
                if (context.Stopping)
                {
                    throw new PipelineStoppedException();
                }
                item = null;
                if (workingDriveForPath.Hidden)
                {
                    if (IsProviderDirectPath(str5))
                    {
                        item = str5;
                    }
                    else
                    {
                        item = GetProviderQualifiedPath(str5, providerInfo);
                    }
                }
                else
                {
                    item = GetDriveQualifiedPath(str5, workingDriveForPath);
                }
                collection.Add(new PathInfo(workingDriveForPath, providerInfo, item, this.sessionState));
                pathResolutionTracer.WriteLine("RESOLVED PATH: {0}", new object[] { item });
            }
            return collection;
        }

        private Collection<string> ResolveProviderPathFromProviderPath(string providerPath, string providerId, bool allowNonexistingPaths, CmdletProviderContext context, out CmdletProvider providerInstance)
        {
            providerInstance = this.sessionState.Internal.GetProviderInstance(providerId);
            ContainerCmdletProvider containerProvider = providerInstance as ContainerCmdletProvider;
            ItemCmdletProvider provider2 = providerInstance as ItemCmdletProvider;
            Collection<string> collection = new Collection<string>();
            if (!context.SuppressWildcardExpansion)
            {
                if (CmdletProviderManagementIntrinsics.CheckProviderCapabilities(ProviderCapabilities.ExpandWildcards, providerInstance.ProviderInfo))
                {
                    pathResolutionTracer.WriteLine("Wildcard matching is being performed by the provider.", new object[0]);
                    if ((provider2 != null) && WildcardPattern.ContainsWildcardCharacters(providerPath))
                    {
                        collection = new Collection<string>(provider2.ExpandPath(providerPath, context));
                    }
                    else
                    {
                        collection.Add(providerPath);
                    }
                }
                else
                {
                    pathResolutionTracer.WriteLine("Wildcard matching is being performed by the engine.", new object[0]);
                    if (containerProvider != null)
                    {
                        collection = this.GetGlobbedProviderPathsFromProviderPath(providerPath, allowNonexistingPaths, containerProvider, context);
                    }
                    else
                    {
                        collection.Add(providerPath);
                    }
                }
            }
            else if (provider2 != null)
            {
                if (allowNonexistingPaths || provider2.ItemExists(providerPath, context))
                {
                    collection.Add(providerPath);
                }
            }
            else
            {
                collection.Add(providerPath);
            }
            if (((allowNonexistingPaths || (collection.Count >= 1)) || WildcardPattern.ContainsWildcardCharacters(providerPath)) || ((context.Include != null) && (context.Include.Count != 0)))
            {
                return collection;
            }
            if ((context.Exclude != null) && (context.Exclude.Count != 0))
            {
                return collection;
            }
            ItemNotFoundException exception = new ItemNotFoundException(providerPath, "PathNotFound", SessionStateStrings.PathNotFound);
            pathResolutionTracer.TraceError("Item does not exist: {0}", new object[] { providerPath });
            throw exception;
        }

        private Collection<PathInfo> ResolvePSPathFromProviderPath(string path, CmdletProviderContext context, bool allowNonexistingPaths, bool isProviderDirectPath, bool isProviderQualifiedPath, out CmdletProvider providerInstance)
        {
            Collection<PathInfo> collection = new Collection<PathInfo>();
            providerInstance = null;
            string providerId = null;
            PSDriveInfo drive = null;
            string providerPath = null;
            if (isProviderDirectPath)
            {
                pathResolutionTracer.WriteLine("Path is PROVIDER-DIRECT", new object[0]);
                providerPath = path;
                providerId = this.sessionState.Path.CurrentLocation.Provider.Name;
            }
            else if (isProviderQualifiedPath)
            {
                pathResolutionTracer.WriteLine("Path is PROVIDER-QUALIFIED", new object[0]);
                providerPath = ParseProviderPath(path, out providerId);
            }
            pathResolutionTracer.WriteLine("PROVIDER-INTERNAL path: {0}", new object[] { providerPath });
            pathResolutionTracer.WriteLine("Provider: {0}", new object[] { providerId });
            Collection<string> collection2 = this.ResolveProviderPathFromProviderPath(providerPath, providerId, allowNonexistingPaths, context, out providerInstance);
            drive = providerInstance.ProviderInfo.HiddenDrive;
            foreach (string str3 in collection2)
            {
                string str4 = str3;
                if (context.Stopping)
                {
                    throw new PipelineStoppedException();
                }
                string str5 = null;
                if (IsProviderDirectPath(str4))
                {
                    str5 = str4;
                }
                else
                {
                    str5 = string.Format(CultureInfo.InvariantCulture, "{0}::{1}", new object[] { providerId, str4 });
                }
                collection.Add(new PathInfo(drive, providerInstance.ProviderInfo, str5, this.sessionState));
                pathResolutionTracer.WriteLine("RESOLVED PATH: {0}", new object[] { str5 });
            }
            return collection;
        }

        internal static bool ShouldPerformGlobbing(string path, CmdletProviderContext context)
        {
            bool flag = false;
            if (path != null)
            {
                flag = StringContainsGlobCharacters(path);
            }
            bool flag2 = false;
            bool suppressWildcardExpansion = false;
            if (context != null)
            {
                bool flag4 = (context.Include != null) && (context.Include.Count > 0);
                pathResolutionTracer.WriteLine("INCLUDE filter present: {0}", new object[] { flag4 });
                bool flag5 = (context.Exclude != null) && (context.Exclude.Count > 0);
                pathResolutionTracer.WriteLine("EXCLUDE filter present: {0}", new object[] { flag5 });
                flag2 = flag4 || flag5;
                suppressWildcardExpansion = context.SuppressWildcardExpansion;
                pathResolutionTracer.WriteLine("NOGLOB parameter present: {0}", new object[] { suppressWildcardExpansion });
            }
            pathResolutionTracer.WriteLine("Path contains wildcard characters: {0}", new object[] { flag });
            bool flag6 = (flag || flag2) && !suppressWildcardExpansion;
            tracer.WriteLine("result = {0}", new object[] { flag6 });
            return flag6;
        }

        internal static bool StringContainsGlobCharacters(string path)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            return WildcardPattern.ContainsWildcardCharacters(path);
        }

        private static void TraceFilters(CmdletProviderContext context)
        {
            if ((pathResolutionTracer.Options & PSTraceSourceOptions.WriteLine) != PSTraceSourceOptions.None)
            {
                pathResolutionTracer.WriteLine("Filter: {0}", new object[] { (context.Filter == null) ? string.Empty : context.Filter });
                if (context.Include != null)
                {
                    StringBuilder builder = new StringBuilder();
                    foreach (string str in context.Include)
                    {
                        builder.AppendFormat("{0} ", str);
                    }
                    pathResolutionTracer.WriteLine("Include: {0}", new object[] { builder.ToString() });
                }
                if (context.Exclude != null)
                {
                    StringBuilder builder2 = new StringBuilder();
                    foreach (string str2 in context.Exclude)
                    {
                        builder2.AppendFormat("{0} ", str2);
                    }
                    pathResolutionTracer.WriteLine("Exclude: {0}", new object[] { builder2.ToString() });
                }
            }
        }
    }
}

