namespace System.Management.Automation.Remoting
{
    using System;
    using System.Management.Automation.Runspaces;

    internal sealed class DefaultRemotePowerShellConfiguration : PSSessionConfiguration
    {
        public override InitialSessionState GetInitialSessionState(PSSenderInfo senderInfo)
        {
            InitialSessionState state = InitialSessionState.CreateDefault();
            if ((senderInfo.ConnectionString != null) && senderInfo.ConnectionString.Contains("MSP=7a83d074-bb86-4e52-aa3e-6cc73cc066c8"))
            {
                PSSessionConfigurationData.IsServerManager = true;
            }
            return state;
        }

        public override InitialSessionState GetInitialSessionState(PSSessionConfigurationData sessionConfigurationData, PSSenderInfo senderInfo, string configProviderId)
        {
            if (sessionConfigurationData == null)
            {
                throw new ArgumentNullException("sessionConfigurationData");
            }
            if (senderInfo == null)
            {
                throw new ArgumentNullException("senderInfo");
            }
            if (configProviderId == null)
            {
                throw new ArgumentNullException("configProviderId");
            }
            InitialSessionState state = InitialSessionState.CreateDefault();
            if ((sessionConfigurationData != null) && (sessionConfigurationData.ModulesToImport != null))
            {
                foreach (string str in sessionConfigurationData.ModulesToImport)
                {
                    state.ImportPSModulesFromPath(str);
                }
            }
            if ((senderInfo.ConnectionString != null) && senderInfo.ConnectionString.Contains("MSP=7a83d074-bb86-4e52-aa3e-6cc73cc066c8"))
            {
                PSSessionConfigurationData.IsServerManager = true;
            }
            return state;
        }
    }
}

