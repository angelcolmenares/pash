namespace System.Management.Automation.Runspaces.Internal
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;

    internal static class RemoteRunspacePoolEnumeration
    {
        private static bool CheckForSSL(WSManConnectionInfo wsmanConnectionInfo)
        {
            return (!string.IsNullOrEmpty(wsmanConnectionInfo.Scheme) && (wsmanConnectionInfo.Scheme.IndexOf("https", StringComparison.OrdinalIgnoreCase) != -1));
        }

        private static int ConvertPSAuthToWSManAuth(AuthenticationMechanism psAuth)
        {
            switch (psAuth)
            {
                case AuthenticationMechanism.Default:
                    return 1;

                case AuthenticationMechanism.Basic:
                    return 8;

                case AuthenticationMechanism.Negotiate:
                    return 4;

                case AuthenticationMechanism.Credssp:
                    return 0x80;

                case AuthenticationMechanism.Digest:
                    return 2;

                case AuthenticationMechanism.Kerberos:
                    return 0x10;
            }
            return 1;
        }

        internal static Collection<PSObject> GetRemoteCommands(Guid shellId, WSManConnectionInfo wsmanConnectionInfo)
        {
            using (PowerShell shell = PowerShell.Create())
            {
                shell.AddCommand("Get-WSManInstance");
                string str = string.Format(CultureInfo.InvariantCulture, "ShellId='{0}'", new object[] { shellId.ToString().ToUpper(CultureInfo.InvariantCulture) });
                shell.AddParameter("ResourceURI", "Shell/Command");
                shell.AddParameter("Enumerate", true);
                shell.AddParameter("Dialect", "Selector");
                shell.AddParameter("Filter", str);
                shell.AddParameter("ComputerName", wsmanConnectionInfo.ComputerName);
                shell.AddParameter("Authentication", ConvertPSAuthToWSManAuth(wsmanConnectionInfo.AuthenticationMechanism));
                if (wsmanConnectionInfo.Credential != null)
                {
                    shell.AddParameter("Credential", wsmanConnectionInfo.Credential);
                }
                if (wsmanConnectionInfo.CertificateThumbprint != null)
                {
                    shell.AddParameter("CertificateThumbprint", wsmanConnectionInfo.CertificateThumbprint);
                }
                if (wsmanConnectionInfo.PortSetting != -1)
                {
                    shell.AddParameter("Port", wsmanConnectionInfo.Port);
                }
                if (CheckForSSL(wsmanConnectionInfo))
                {
                    shell.AddParameter("UseSSL", true);
                }
                if (!string.IsNullOrEmpty(wsmanConnectionInfo.AppName))
                {
                    string str2 = wsmanConnectionInfo.AppName.TrimStart(new char[] { '/' });
                    shell.AddParameter("ApplicationName", str2);
                }
                shell.AddParameter("SessionOption", GetSessionOptions(wsmanConnectionInfo));
                return shell.Invoke();
            }
        }

        internal static Collection<PSObject> GetRemotePools(WSManConnectionInfo wsmanConnectionInfo)
        {
            using (PowerShell shell = PowerShell.Create())
            {
                shell.AddCommand("Get-WSManInstance");
                shell.AddParameter("ResourceURI", "Shell");
                shell.AddParameter("Enumerate", true);
                shell.AddParameter("ComputerName", wsmanConnectionInfo.ComputerName);
                shell.AddParameter("Authentication", ConvertPSAuthToWSManAuth(wsmanConnectionInfo.AuthenticationMechanism));
                if (wsmanConnectionInfo.Credential != null)
                {
                    shell.AddParameter("Credential", wsmanConnectionInfo.Credential);
                }
                if (wsmanConnectionInfo.CertificateThumbprint != null)
                {
                    shell.AddParameter("CertificateThumbprint", wsmanConnectionInfo.CertificateThumbprint);
                }
                if (wsmanConnectionInfo.PortSetting != -1)
                {
                    shell.AddParameter("Port", wsmanConnectionInfo.Port);
                }
                if (CheckForSSL(wsmanConnectionInfo))
                {
                    shell.AddParameter("UseSSL", true);
                }
                if (!string.IsNullOrEmpty(wsmanConnectionInfo.AppName))
                {
                    string str = wsmanConnectionInfo.AppName.TrimStart(new char[] { '/' });
                    shell.AddParameter("ApplicationName", str);
                }
                shell.AddParameter("SessionOption", GetSessionOptions(wsmanConnectionInfo));
                return shell.Invoke();
            }
        }

        private static object GetSessionOptions(WSManConnectionInfo wsmanConnectionInfo)
        {
            Collection<PSObject> collection;
            using (PowerShell shell = PowerShell.Create())
            {
                shell.AddCommand("New-WSManSessionOption");
                if (wsmanConnectionInfo.ProxyAccessType != ProxyAccessType.None)
                {
                    shell.AddParameter("ProxyAccessType", "Proxy" + wsmanConnectionInfo.ProxyAccessType.ToString());
                    shell.AddParameter("ProxyAuthentication", wsmanConnectionInfo.ProxyAuthentication.ToString());
                    if (wsmanConnectionInfo.ProxyCredential != null)
                    {
                        shell.AddParameter("ProxyCredential", wsmanConnectionInfo.ProxyCredential);
                    }
                }
                if (wsmanConnectionInfo.IncludePortInSPN)
                {
                    shell.AddParameter("SPNPort", wsmanConnectionInfo.Port);
                }
                shell.AddParameter("SkipCACheck", wsmanConnectionInfo.SkipCACheck);
                shell.AddParameter("SkipCNCheck", wsmanConnectionInfo.SkipCNCheck);
                shell.AddParameter("SkipRevocationCheck", wsmanConnectionInfo.SkipRevocationCheck);
                shell.AddParameter("OperationTimeout", wsmanConnectionInfo.OperationTimeout);
                shell.AddParameter("NoEncryption", wsmanConnectionInfo.NoEncryption);
                shell.AddParameter("UseUTF16", wsmanConnectionInfo.UseUTF16);
                collection = shell.Invoke();
            }
            return collection[0].BaseObject;
        }
    }
}

