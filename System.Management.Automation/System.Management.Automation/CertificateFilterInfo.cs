namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    internal sealed class CertificateFilterInfo
    {
        internal const string CodeSigningOid = "1.3.6.1.5.5.7.3.3";
        private string dnsName;
        private string[] eku;
        private int expiringInDays = -1;
        private CertificatePurpose purpose;
        private bool sslServerAuthentication;
        internal const string szOID_PKIX_KP_SERVER_AUTH = "1.3.6.1.5.5.7.3.1";

        internal CertificateFilterInfo()
        {
        }

        private string AppendFilter(string filterString, string name, string value)
        {
            string str = value;
            if (str.Length != 0)
            {
                if (str.Contains("=") || str.Contains("&"))
                {
                    throw Marshal.GetExceptionForHR(-2147024883);
                }
                str = name + "=" + str;
                if (filterString.Length != 0)
                {
                    str = "&" + str;
                }
            }
            return (filterString + str);
        }

        internal string DnsName
        {
            set
            {
                this.dnsName = value;
            }
        }

        internal string[] Eku
        {
            set
            {
                this.eku = value;
            }
        }

        internal int ExpiringInDays
        {
            set
            {
                this.expiringInDays = value;
            }
        }

        internal string FilterString
        {
            get
            {
                string filterString = "";
                if (this.dnsName != null)
                {
                    filterString = this.AppendFilter(filterString, "dns", this.dnsName);
                }
                string str2 = "";
                if (this.eku != null)
                {
                    for (int i = 0; i < this.eku.Length; i++)
                    {
                        if (str2.Length != 0)
                        {
                            str2 = str2 + ",";
                        }
                        str2 = str2 + this.eku[i];
                    }
                }
                if (this.purpose == CertificatePurpose.CodeSigning)
                {
                    if (str2.Length != 0)
                    {
                        str2 = str2 + ",";
                    }
                    str2 = str2 + "1.3.6.1.5.5.7.3.3";
                }
                if (this.sslServerAuthentication)
                {
                    if (str2.Length != 0)
                    {
                        str2 = str2 + ",";
                    }
                    str2 = str2 + "1.3.6.1.5.5.7.3.1";
                }
                if (str2.Length != 0)
                {
                    filterString = this.AppendFilter(filterString, "eku", str2);
                    if ((this.purpose == CertificatePurpose.CodeSigning) || this.sslServerAuthentication)
                    {
                        filterString = this.AppendFilter(filterString, "key", "*");
                    }
                }
                if (this.expiringInDays >= 0)
                {
                    filterString = this.AppendFilter(filterString, "ExpiringInDays", this.expiringInDays.ToString(CultureInfo.InvariantCulture));
                }
                if (filterString.Length == 0)
                {
                    filterString = null;
                }
                return filterString;
            }
        }

        internal CertificatePurpose Purpose
        {
            get
            {
                return this.purpose;
            }
            set
            {
                this.purpose = value;
            }
        }

        internal bool SSLServerAuthentication
        {
            get
            {
                return this.sslServerAuthentication;
            }
            set
            {
                this.sslServerAuthentication = value;
            }
        }
    }
}

