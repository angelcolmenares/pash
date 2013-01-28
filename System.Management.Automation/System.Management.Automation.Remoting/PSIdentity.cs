namespace System.Management.Automation.Remoting
{
    using System;
    using System.Security.Principal;

    public sealed class PSIdentity : IIdentity
    {
        private string authenticationType;
        private PSCertificateDetails certDetails;
        private bool isAuthenticated;
        private string userName;

        public PSIdentity(string authType, bool isAuthenticated, string userName, PSCertificateDetails cert)
        {
            this.authenticationType = authType;
            this.isAuthenticated = isAuthenticated;
            this.userName = userName;
            this.certDetails = cert;
        }

        public string AuthenticationType
        {
            get
            {
                return this.authenticationType;
            }
        }

        public PSCertificateDetails CertificateDetails
        {
            get
            {
                return this.certDetails;
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return this.isAuthenticated;
            }
        }

        public string Name
        {
            get
            {
                return this.userName;
            }
        }
    }
}

