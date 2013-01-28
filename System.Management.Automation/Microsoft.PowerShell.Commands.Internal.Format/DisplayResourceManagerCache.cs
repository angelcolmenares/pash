namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections;
    using System.Management.Automation;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Security;

    internal sealed class DisplayResourceManagerCache
    {
        private AssemblyNameResolver _assemblyNameResolver = new AssemblyNameResolver();
        private Hashtable _resourceReferenceToAssemblyCache = new Hashtable();

        private string GetString(StringResourceReference resourceReference)
        {
            LoadingResult result;
            AssemblyBindingStatus status;
            return this.GetStringHelper(resourceReference, out result, out status);
        }

        private string GetStringHelper(StringResourceReference resourceReference, out LoadingResult result, out AssemblyBindingStatus bindingStatus)
        {
            result = LoadingResult.AssemblyNotFound;
            bindingStatus = AssemblyBindingStatus.NotFound;
            AssemblyLoadResult result2 = null;
            if (this._resourceReferenceToAssemblyCache.Contains(resourceReference))
            {
                result2 = this._resourceReferenceToAssemblyCache[resourceReference] as AssemblyLoadResult;
                bindingStatus = result2.status;
            }
            else
            {
                bool flag;
                result2 = new AssemblyLoadResult {
                    a = this.LoadAssemblyFromResourceReference(resourceReference, out flag)
                };
                if (result2.a == null)
                {
                    result2.status = AssemblyBindingStatus.NotFound;
                }
                else
                {
                    result2.status = flag ? AssemblyBindingStatus.FoundInGac : AssemblyBindingStatus.FoundInPath;
                }
                this._resourceReferenceToAssemblyCache.Add(resourceReference, result2);
            }
            bindingStatus = result2.status;
            if (result2.a == null)
            {
                result = LoadingResult.AssemblyNotFound;
                return null;
            }
            try
            {
                string str = ResourceManagerCache.GetResourceString(result2.a, resourceReference.baseName, resourceReference.resourceId);
                if (str == null)
                {
                    result = LoadingResult.StringNotFound;
                    return null;
                }
                result = LoadingResult.NoError;
                return str;
            }
            catch (InvalidOperationException)
            {
                result = LoadingResult.ResourceNotFound;
            }
            catch (MissingManifestResourceException)
            {
                result = LoadingResult.ResourceNotFound;
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }

        internal string GetTextTokenString(TextToken tt)
        {
            if (tt.resource != null)
            {
                string str = this.GetString(tt.resource);
                if (str != null)
                {
                    return str;
                }
            }
            return tt.text;
        }

        private Assembly LoadAssemblyFromResourceReference(StringResourceReference resourceReference, out bool foundInGac)
        {
            foundInGac = false;
            return this._assemblyNameResolver.ResolveAssemblyName(resourceReference.assemblyName);
        }

        internal void VerifyResource(StringResourceReference resourceReference, out LoadingResult result, out AssemblyBindingStatus bindingStatus)
        {
            this.GetStringHelper(resourceReference, out result, out bindingStatus);
        }

        internal enum AssemblyBindingStatus
        {
            NotFound,
            FoundInGac,
            FoundInPath
        }

        private sealed class AssemblyLoadResult
        {
            internal Assembly a;
            internal DisplayResourceManagerCache.AssemblyBindingStatus status;
        }

        private class AssemblyNameResolver
        {
            private Hashtable _assemblyReferences = new Hashtable(StringComparer.OrdinalIgnoreCase);

            internal Assembly ResolveAssemblyName(string assemblyName)
            {
                if (string.IsNullOrEmpty(assemblyName))
                {
                    return null;
                }
                if (this._assemblyReferences.Contains(assemblyName))
                {
                    return (Assembly) this._assemblyReferences[assemblyName];
                }
                Assembly assembly = ResolveAssemblyNameInLoadedAssemblies(assemblyName, true);
                if (assembly == null)
                {
                    assembly = ResolveAssemblyNameInLoadedAssemblies(assemblyName, false);
                }
                this._assemblyReferences.Add(assemblyName, assembly);
                return assembly;
            }

            private static Assembly ResolveAssemblyNameInLoadedAssemblies(string assemblyName, bool fullName)
            {
                foreach (Assembly assembly2 in AppDomain.CurrentDomain.GetAssemblies())
                {
                    AssemblyName name = null;
                    try
                    {
                        name = assembly2.GetName();
                    }
                    catch (SecurityException)
                    {
                        continue;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    if (fullName)
                    {
                        if (string.Equals(name.FullName, assemblyName, StringComparison.Ordinal))
                        {
                            return assembly2;
                        }
                    }
                    else if (string.Equals(name.Name, assemblyName, StringComparison.Ordinal))
                    {
                        return assembly2;
                    }
                }
                return null;
            }
        }

        internal enum LoadingResult
        {
            NoError,
            AssemblyNotFound,
            ResourceNotFound,
            StringNotFound
        }
    }
}

