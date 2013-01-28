namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation;

    internal class PathResolver
    {
        private PathInfo ResolvePath(string pathToResolve, bool isLiteralPath, bool allowNonexistingPaths, PSCmdlet cmdlet)
        {
            CmdletProviderContext context = new CmdletProviderContext(cmdlet) {
                SuppressWildcardExpansion = isLiteralPath
            };
            Collection<PathInfo> targetObject = new Collection<PathInfo>();
            try
            {
                foreach (PathInfo info in cmdlet.SessionState.Path.GetResolvedPSPathFromPSPath(pathToResolve, context))
                {
                    targetObject.Add(info);
                }
            }
            catch (PSNotSupportedException exception)
            {
                cmdlet.ThrowTerminatingError(new ErrorRecord(exception.ErrorRecord, exception));
            }
            catch (DriveNotFoundException exception2)
            {
                cmdlet.ThrowTerminatingError(new ErrorRecord(exception2.ErrorRecord, exception2));
            }
            catch (ProviderNotFoundException exception3)
            {
                cmdlet.ThrowTerminatingError(new ErrorRecord(exception3.ErrorRecord, exception3));
            }
            catch (ItemNotFoundException exception4)
            {
                if (allowNonexistingPaths)
                {
                    ProviderInfo provider = null;
                    PSDriveInfo drive = null;
                    string path = cmdlet.SessionState.Path.GetUnresolvedProviderPathFromPSPath(pathToResolve, context, out provider, out drive);
                    PathInfo item = new PathInfo(drive, provider, path, cmdlet.SessionState);
                    targetObject.Add(item);
                }
                else
                {
                    cmdlet.ThrowTerminatingError(new ErrorRecord(exception4.ErrorRecord, exception4));
                }
            }
            if (targetObject.Count == 1)
            {
                return targetObject[0];
            }
            Exception exception5 = PSTraceSource.NewNotSupportedException();
            cmdlet.ThrowTerminatingError(new ErrorRecord(exception5, "NotSupported", ErrorCategory.NotImplemented, targetObject));
            return null;
        }

        internal string ResolveProviderAndPath(string path, bool isLiteralPath, PSCmdlet cmdlet, bool allowNonexistingPaths, string resourceBaseName, string multipeProviderErrorId)
        {
            PathInfo info = this.ResolvePath(path, isLiteralPath, allowNonexistingPaths, cmdlet);
            if (info.Provider.ImplementingType != typeof(FileSystemProvider))
            {
                throw PSTraceSource.NewInvalidOperationException(resourceBaseName, multipeProviderErrorId, new object[] { info.Provider.Name });
            }
            return info.ProviderPath;
        }
    }
}

