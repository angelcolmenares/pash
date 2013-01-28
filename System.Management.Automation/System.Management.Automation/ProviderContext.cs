namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Provider;

    internal class ProviderContext
    {
        private readonly ExecutionContext _executionContext;
        private readonly PathIntrinsics _pathIntrinsics;
        private readonly string _requestedPath;

        internal ProviderContext(string requestedPath, ExecutionContext executionContext, PathIntrinsics pathIntrinsics)
        {
            this._requestedPath = requestedPath;
            this._executionContext = executionContext;
            this._pathIntrinsics = pathIntrinsics;
        }

        internal MamlCommandHelpInfo GetProviderSpecificHelpInfo(string helpItemName)
        {
            ProviderInfo info = null;
            PSDriveInfo drive = null;
            string str = null;
            CmdletProviderContext context = new CmdletProviderContext(this._executionContext);
            try
            {
                string str2 = this._requestedPath;
                if (string.IsNullOrEmpty(this._requestedPath))
                {
                    str2 = this._pathIntrinsics.CurrentLocation.Path;
                }
                str = this._executionContext.LocationGlobber.GetProviderPath(str2, context, out info, out drive);
            }
            catch (ArgumentNullException)
            {
            }
            catch (ProviderNotFoundException)
            {
            }
            catch (DriveNotFoundException)
            {
            }
            catch (ProviderInvocationException)
            {
            }
            catch (NotSupportedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            catch (ItemNotFoundException)
            {
            }
            if (info == null)
            {
                return null;
            }
            CmdletProvider provider = info.CreateInstance();
            ICmdletProviderSupportsHelp help = provider as ICmdletProviderSupportsHelp;
            if (help == null)
            {
                return null;
            }
            if (str == null)
            {
                throw new ItemNotFoundException(this._requestedPath, "PathNotFound", SessionStateStrings.PathNotFound);
            }
            provider.Start(info, context);
            string path = str;
            string helpMaml = help.GetHelpMaml(helpItemName, path);
            if (string.IsNullOrEmpty(helpMaml))
            {
                return null;
            }
            return MamlCommandHelpInfo.Load(InternalDeserializer.LoadUnsafeXmlDocument(helpMaml, false, null).DocumentElement, HelpCategory.Provider);
        }

        internal string RequestedPath
        {
            get
            {
                return this._requestedPath;
            }
        }
    }
}

