namespace System.Management.Automation.Runspaces
{
    using System;

    public enum AuthenticationMechanism
    {
        Default,
        Basic,
        Negotiate,
        NegotiateWithImplicitCredential,
        Credssp,
        Digest,
        Kerberos
    }
}

