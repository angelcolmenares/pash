namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Management.Automation;
    using System.Text;

    [Cmdlet("Export", "PSSession", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135213"), OutputType(new Type[] { typeof(FileInfo) })]
    public sealed class ExportPSSessionCommand : ImplicitRemotingCommandBase
    {
        private const string copyItemScript = "\r\n                param($sourcePath, $destinationPath)\r\n                Copy-Item -Recurse $sourcePath\\\\* -Destination $destinationPath\\\\\r\n                Remove-item $sourcePath -Recurse -Force \r\n            ";
        private System.Text.Encoding encoding = System.Text.Encoding.UTF8;
        private bool force;
        private const string getChildItemScript = "\r\n                param($path)\r\n                Get-ChildItem -LiteralPath $path\r\n            ";
        private string moduleName;

        protected override void BeginProcessing()
        {
            Dictionary<string, string> dictionary;
            DirectoryInfo info = PathUtils.CreateModuleDirectory(this, this.OutputModule, this.Force.IsPresent);
            DirectoryInfo moduleRootDirectory = PathUtils.CreateTemporaryDirectory();
            List<CommandMetadata> remoteCommandMetadata = base.GetRemoteCommandMetadata(out dictionary);
            List<ExtendedTypeDefinition> remoteFormatData = base.GetRemoteFormatData();
            base.GenerateProxyModule(moduleRootDirectory, Path.GetFileName(info.FullName), this.encoding, this.force, remoteCommandMetadata, dictionary, remoteFormatData);
            base.Context.Engine.ParseScriptBlock("\r\n                param($sourcePath, $destinationPath)\r\n                Copy-Item -Recurse $sourcePath\\\\* -Destination $destinationPath\\\\\r\n                Remove-item $sourcePath -Recurse -Force \r\n            ", false).Invoke(new object[] { moduleRootDirectory, info });
            this.DisplayDirectory(new List<string> { info.FullName });
        }

        private void DisplayDirectory(List<string> generatedFiles)
        {
            foreach (PSObject obj2 in base.Context.Engine.ParseScriptBlock("\r\n                param($path)\r\n                Get-ChildItem -LiteralPath $path\r\n            ", false).Invoke(new object[] { generatedFiles.ToArray() }))
            {
                base.WriteObject(obj2);
            }
        }

        [Parameter, ValidateSet(new string[] { "Unicode", "UTF7", "UTF8", "ASCII", "UTF32", "BigEndianUnicode", "Default", "OEM" })]
        public string Encoding
        {
            get
            {
                return this.encoding.GetType().Name;
            }
            set
            {
                this.encoding = EncodingConversion.Convert(this, value);
            }
        }

        [Parameter]
        public SwitchParameter Force
        {
            get
            {
                return new SwitchParameter(this.force);
            }
            set
            {
                this.force = value.IsPresent;
            }
        }

        [Alias(new string[] { "PSPath", "ModuleName" }), ValidateNotNullOrEmpty, Parameter(Mandatory=true, Position=1)]
        public string OutputModule
        {
            get
            {
                return this.moduleName;
            }
            set
            {
                this.moduleName = value;
            }
        }

        public static Version VersionOfScriptGenerator
        {
            get
            {
                return ImplicitRemotingCodeGenerator.VersionOfScriptWriter;
            }
        }
    }
}

