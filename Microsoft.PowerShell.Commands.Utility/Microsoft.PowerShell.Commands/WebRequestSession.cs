namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography.X509Certificates;

    public class WebRequestSession
    {
        public WebRequestSession()
        {
            this.Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this.Cookies = new CookieContainer();
            this.UseDefaultCredentials = false;
            this.Credentials = null;
            this.Certificates = null;
            this.UserAgent = PSUserAgent.UserAgent;
            this.Proxy = null;
            this.MaximumRedirection = -1;
        }

        internal void AddCertificate(X509Certificate certificate)
        {
            if (this.Certificates == null)
            {
                this.Certificates = new X509CertificateCollection();
            }
            this.Certificates.Add(certificate);
        }

        public X509CertificateCollection Certificates { get; set; }

        public CookieContainer Cookies { get; set; }

        public ICredentials Credentials { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public int MaximumRedirection { get; set; }

        public IWebProxy Proxy { get; set; }

        public bool UseDefaultCredentials { get; set; }

        public string UserAgent { get; set; }
    }
}

