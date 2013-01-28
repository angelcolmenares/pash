namespace System.Management.Automation.Remoting
{
    using System;

    public sealed class PSCertificateDetails
    {
        private string issuerName;
        private string issuerThumbprint;
        private string subject;

        public PSCertificateDetails(string subject, string issuerName, string issuerThumbprint)
        {
            this.subject = subject;
            this.issuerName = issuerName;
            this.issuerThumbprint = issuerThumbprint;
        }

        public string IssuerName
        {
            get
            {
                return this.issuerName;
            }
        }

        public string IssuerThumbprint
        {
            get
            {
                return this.issuerThumbprint;
            }
        }

        public string Subject
        {
            get
            {
                return this.subject;
            }
        }
    }
}

