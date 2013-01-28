namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Text;

    [Cmdlet("Import", "PSSession", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135221"), OutputType(new Type[] { typeof(PSModuleInfo) })]
    public sealed class ImportPSSessionCommand : ImplicitRemotingCommandBase
    {
        private bool disableNameChecking;
        private const string importModuleScript = "\r\n                param($name, $session, $prefix, $disableNameChecking)\r\n                Import-Module -Name $name -Alias * -Function * -Prefix $prefix -DisableNameChecking:$disableNameChecking -PassThru -ArgumentList @($session)\r\n            ";
        private const string runspaceStateChangedScript = "& {\r\n            if ('Closed' -eq $eventArgs.RunspaceStateInfo.State)\r\n            {\r\n                $sourceIdentifier = [system.management.automation.wildcardpattern]::Escape($eventSubscriber.SourceIdentifier)\r\n                Unregister-Event -SourceIdentifier $sourceIdentifier -Force -ErrorAction SilentlyContinue\r\n\r\n                $moduleInfo = $event.MessageData\r\n                Remove-Module -ModuleInfo $moduleInfo -Force -ErrorAction SilentlyContinue\r\n\r\n                Remove-Item -LiteralPath $moduleInfo.ModuleBase -Recurse -Force -ErrorAction SilentlyContinue\r\n                $moduleInfo = $null\r\n            }\r\n}\r\n            ";
        private const string unregisterEventCleanUpScript = "\r\n            $sourceIdentifier = [system.management.automation.wildcardpattern]::Escape($eventSubscriber.SourceIdentifier)\r\n            Unregister-Event -SourceIdentifier $sourceIdentifier -Force -ErrorAction SilentlyContinue\r\n\r\n            if ($previousScript -ne $null)\r\n            {\r\n                & $previousScript $args\r\n            }\r\n            ";

        protected override void BeginProcessing()
        {
            Dictionary<string, string> dictionary;
            DirectoryInfo moduleRootDirectory = PathUtils.CreateTemporaryDirectory();
            List<CommandMetadata> remoteCommandMetadata = base.GetRemoteCommandMetadata(out dictionary);
            List<ExtendedTypeDefinition> remoteFormatData = base.GetRemoteFormatData();
            List<string> list3 = base.GenerateProxyModule(moduleRootDirectory, Path.GetFileName(moduleRootDirectory.FullName), Encoding.Unicode, false, remoteCommandMetadata, dictionary, remoteFormatData);
            string manifestFile = null;
            foreach (string str2 in list3)
            {
                if (Path.GetExtension(str2).Equals(".psd1", StringComparison.OrdinalIgnoreCase))
                {
                    manifestFile = str2;
                }
            }
            PSModuleInfo moduleInfo = this.CreateModule(manifestFile);
            this.RegisterModuleCleanUp(moduleInfo);
            base.WriteObject(moduleInfo);
        }

        private PSModuleInfo CreateModule(string manifestFile)
        {
            return (PSModuleInfo) base.Context.Engine.ParseScriptBlock("\r\n                param($name, $session, $prefix, $disableNameChecking)\r\n                Import-Module -Name $name -Alias * -Function * -Prefix $prefix -DisableNameChecking:$disableNameChecking -PassThru -ArgumentList @($session)\r\n            ", false).Invoke(new object[] { manifestFile, base.Session, this.Prefix, this.disableNameChecking })[0].BaseObject;
        }

        private void RegisterModuleCleanUp(PSModuleInfo moduleInfo)
        {
            if (moduleInfo == null)
            {
                throw PSTraceSource.NewArgumentNullException("moduleInfo");
            }
            string sourceIdentifier = StringUtil.Format(ImplicitRemotingStrings.EventSourceIdentifier, base.Session.InstanceId, base.ModuleGuid);
            PSEventSubscriber subscriber = base.Context.Events.SubscribeEvent(base.Session.Runspace, "StateChanged", sourceIdentifier, PSObject.AsPSObject(moduleInfo), base.Context.Engine.ParseScriptBlock("& {\r\n            if ('Closed' -eq $eventArgs.RunspaceStateInfo.State)\r\n            {\r\n                $sourceIdentifier = [system.management.automation.wildcardpattern]::Escape($eventSubscriber.SourceIdentifier)\r\n                Unregister-Event -SourceIdentifier $sourceIdentifier -Force -ErrorAction SilentlyContinue\r\n\r\n                $moduleInfo = $event.MessageData\r\n                Remove-Module -ModuleInfo $moduleInfo -Force -ErrorAction SilentlyContinue\r\n\r\n                Remove-Item -LiteralPath $moduleInfo.ModuleBase -Recurse -Force -ErrorAction SilentlyContinue\r\n                $moduleInfo = $null\r\n            }\r\n}\r\n            ", false), true, false);
            ScriptBlock newClosure = base.Context.Engine.ParseScriptBlock("\r\n            $sourceIdentifier = [system.management.automation.wildcardpattern]::Escape($eventSubscriber.SourceIdentifier)\r\n            Unregister-Event -SourceIdentifier $sourceIdentifier -Force -ErrorAction SilentlyContinue\r\n\r\n            if ($previousScript -ne $null)\r\n            {\r\n                & $previousScript $args\r\n            }\r\n            ", false).GetNewClosure();
            newClosure.Module.SessionState.PSVariable.Set("eventSubscriber", subscriber);
            newClosure.Module.SessionState.PSVariable.Set("previousScript", moduleInfo.OnRemove);
            moduleInfo.OnRemove = newClosure;
        }

        [Parameter]
        public SwitchParameter DisableNameChecking
        {
            get
            {
                return this.disableNameChecking;
            }
            set
            {
                this.disableNameChecking = (bool) value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter]
        public string Prefix
        {
            get
            {
                return base.Prefix;
            }
            set
            {
                base.Prefix = value;
            }
        }
    }
}

