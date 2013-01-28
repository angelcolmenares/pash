namespace System.Management.Automation
{
    using Microsoft.PowerShell;
    using System;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Cryptography;

    [Serializable]
    public sealed class PSCredential : ISerializable
    {
        private static GetSymmetricEncryptionKey _delegate = null;
        private NetworkCredential _netCred;
        private SecureString _password;
        private string _userName;
        private static readonly PSCredential empty = new PSCredential();
        private const string resBaseName = "Credential";

        private PSCredential()
        {
        }

        private PSCredential(SerializationInfo info, StreamingContext context)
        {
            if (info != null)
            {
                this._userName = (string) info.GetValue("UserName", typeof(string));
                string input = (string) info.GetValue("Password", typeof(string));
                if (input == string.Empty)
                {
                    this._password = new SecureString();
                }
                else
                {
                    byte[] buffer;
                    byte[] buffer2;
                    if ((_delegate != null) && _delegate(context, out buffer, out buffer2))
                    {
                        this._password = Microsoft.PowerShell.SecureStringHelper.Decrypt(input, buffer, buffer2);
                    }
                    else
                    {
                        this._password = Microsoft.PowerShell.SecureStringHelper.Unprotect(input);
                    }
                }
            }
        }

        public PSCredential(string userName, SecureString password)
        {
            System.Management.Automation.Utils.CheckArgForNullOrEmpty(userName, "userName");
            System.Management.Automation.Utils.CheckArgForNull(password, "password");
            this._userName = userName;
            this._password = password;
        }

        public NetworkCredential GetNetworkCredential()
        {
            if (this._netCred == null)
            {
                string user = null;
                string domain = null;
                if (IsValidUserName(this._userName, out user, out domain))
                {
					this._netCred = new NetworkCredential(user, Microsoft.PowerShell.SecureStringHelper.Protect(this._password), domain);
                }
            }
            return this._netCred;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info != null)
            {
                string encryptedData = string.Empty;
                if ((this._password != null) && (this._password.Length > 0))
                {
                    byte[] buffer;
                    byte[] buffer2;
                    if ((_delegate != null) && _delegate(context, out buffer, out buffer2))
                    {
                        encryptedData = Microsoft.PowerShell.SecureStringHelper.Encrypt(this._password, buffer, buffer2).EncryptedData;

                    }
                    else
                    {
                        try
                        {
                            encryptedData = Microsoft.PowerShell.SecureStringHelper.Protect(this._password);
                        }
                        catch (CryptographicException exception)
                        {
                            throw PSTraceSource.NewInvalidOperationException(exception, "Credential", "CredentialDisallowed", new object[0]);
                        }
                    }
                }
                info.AddValue("UserName", this._userName);
                info.AddValue("Password", encryptedData);
            }
        }

        private static bool IsValidUserName(string input, out string user, out string domain)
        {
            SplitUserDomain(input, out user, out domain);
            if (((user == null) || (domain == null)) || (user.Length == 0))
            {
                throw PSTraceSource.NewArgumentException("UserName", "Credential", "InvalidUserNameFormat", new object[0]);
            }
            return true;
        }

        public static explicit operator NetworkCredential(PSCredential credential)
        {
            if (credential == null)
            {
                throw PSTraceSource.NewArgumentNullException("credential");
            }
            return credential.GetNetworkCredential();
        }

        private static void SplitUserDomain(string input, out string user, out string domain)
        {
            int length = 0;
            user = null;
            domain = null;
            length = input.IndexOf('\\');
            if (length >= 0)
            {
                user = input.Substring(length + 1);
                domain = input.Substring(0, length);
            }
            else
            {
                length = input.LastIndexOf('@');
                if ((length >= 0) && ((input.LastIndexOf('.') < length) || (input.IndexOf('@') != length)))
                {
                    domain = input.Substring(length + 1);
                    user = input.Substring(0, length);
                }
                else
                {
                    user = input;
                    domain = "";
                }
            }
        }

        public static PSCredential Empty
        {
            get
            {
                return empty;
            }
        }

        public static GetSymmetricEncryptionKey GetSymmetricEncryptionKeyDelegate
        {
            get
            {
                return _delegate;
            }
            set
            {
                _delegate = value;
            }
        }

        public SecureString Password
        {
            get
            {
                return this._password;
            }
        }

        public string UserName
        {
            get
            {
                return this._userName;
            }
        }
    }
}

