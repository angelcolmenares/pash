namespace System.Management.Automation.Runspaces
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Host;

    public sealed class FormatTable
    {
        private TypeInfoDataBaseManager formatDBMgr;

        internal FormatTable()
        {
            this.formatDBMgr = new TypeInfoDataBaseManager();
        }

        public FormatTable(IEnumerable<string> formatFiles) : this(formatFiles, null, null)
        {
        }

        internal FormatTable(IEnumerable<string> formatFiles, AuthorizationManager authorizationManager, PSHost host)
        {
            if (formatFiles == null)
            {
                throw PSTraceSource.NewArgumentNullException("formatFiles");
            }
            this.formatDBMgr = new TypeInfoDataBaseManager(formatFiles, true, authorizationManager, host);
        }

        internal void Add(string formatFile, bool shouldPrepend)
        {
            this.formatDBMgr.Add(formatFile, shouldPrepend);
        }

        public void AppendFormatData(IEnumerable<ExtendedTypeDefinition> formatData)
        {
            if (formatData == null)
            {
                throw PSTraceSource.NewArgumentNullException("formatData");
            }
            this.formatDBMgr.AddFormatData(formatData, false);
        }

        public static FormatTable LoadDefaultFormatFiles()
        {
            Func<string, string> selector = null;
            string defaultPowerShellShellID = Utils.DefaultPowerShellShellID;
            string psHome = Utils.GetApplicationBase(defaultPowerShellShellID);
            List<string> formatFiles = new List<string>();
            List<string> source = new List<string> { "Certificate.Format.ps1xml", "Event.Format.ps1xml", "Diagnostics.Format.ps1xml", "DotNetTypes.Format.ps1xml", "FileSystem.Format.ps1xml", "Help.Format.ps1xml", "HelpV3.Format.ps1xml", "PowerShellCore.format.ps1xml", "PowerShellTrace.format.ps1xml", "Registry.format.ps1xml", "WSMan.Format.ps1xml" };
            if (!string.IsNullOrEmpty(psHome))
            {
                if (selector == null)
                {
                    selector = file => Path.Combine(psHome, file);
                }
                formatFiles.AddRange(source.Select<string, string>(selector));
            }
            return new FormatTable(formatFiles);
        }

        public void PrependFormatData(IEnumerable<ExtendedTypeDefinition> formatData)
        {
            if (formatData == null)
            {
                throw PSTraceSource.NewArgumentNullException("formatData");
            }
            this.formatDBMgr.AddFormatData(formatData, true);
        }

        internal void Remove(string formatFile)
        {
            this.formatDBMgr.Remove(formatFile);
        }

        internal TypeInfoDataBaseManager FormatDBManager
        {
            get
            {
                return this.formatDBMgr;
            }
        }
    }
}

