namespace System.Management.Automation.Help
{
    using System;
    using System.Globalization;

    internal class UpdatableHelpUri
    {
        private CultureInfo _culture;
        private Guid _moduleGuid;
        private string _moduleName;
        private string _resolvedUri;

        internal UpdatableHelpUri(string moduleName, Guid moduleGuid, CultureInfo culture, string resolvedUri)
        {
            this._moduleName = moduleName;
            this._moduleGuid = moduleGuid;
            this._culture = culture;
            this._resolvedUri = resolvedUri;
        }

        internal CultureInfo Culture
        {
            get
            {
                return this._culture;
            }
        }

        internal Guid ModuleGuid
        {
            get
            {
                return this._moduleGuid;
            }
        }

        internal string ModuleName
        {
            get
            {
                return this._moduleName;
            }
        }

        internal string ResolvedUri
        {
            get
            {
                return this._resolvedUri;
            }
        }
    }
}

