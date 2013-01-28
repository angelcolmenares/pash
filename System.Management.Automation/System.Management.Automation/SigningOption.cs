namespace System.Management.Automation
{
    using System;

    public enum SigningOption
    {
        AddFullCertificateChain = 1,
        AddFullCertificateChainExceptRoot = 2,
        AddOnlyCertificate = 0,
        Default = 2
    }
}

