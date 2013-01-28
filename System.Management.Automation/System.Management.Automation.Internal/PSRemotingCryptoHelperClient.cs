namespace System.Management.Automation.Internal
{
    using System;
    using System.Management.Automation;
    using System.Runtime.InteropServices;
    using System.Security;

    internal class PSRemotingCryptoHelperClient : PSRemotingCryptoHelper
    {
        private RemoteSession _session;

        internal PSRemotingCryptoHelperClient()
        {
            base._rsaCryptoProvider = PSRSACryptoServiceProvider.GetRSACryptoServiceProviderForClient();
        }

        internal override SecureString DecryptSecureString(string encryptedString)
        {
            base.RunKeyExchangeIfRequired();
            return base.DecryptSecureStringCore(encryptedString);
        }

        internal override string EncryptSecureString(SecureString secureString)
        {
            base.RunKeyExchangeIfRequired();
            return base.EncryptSecureStringCore(secureString);
        }

        internal bool ExportLocalPublicKey(out string publicKeyAsString)
        {
            try
            {
                base._rsaCryptoProvider.GenerateKeyPair();
            }
            catch (PSCryptoException)
            {
                throw;
            }
            try
            {
                publicKeyAsString = base._rsaCryptoProvider.GetPublicKeyAsBase64EncodedString();
            }
            catch (PSCryptoException)
            {
                publicKeyAsString = string.Empty;
                return false;
            }
            return true;
        }

        internal static PSRemotingCryptoHelperClient GetTestRemotingCryptHelperClient()
        {
            return new PSRemotingCryptoHelperClient { Session = new TestHelperSession() };
        }

        internal bool ImportEncryptedSessionKey(string encryptedSessionKey)
        {
            try
            {
                base._rsaCryptoProvider.ImportSessionKeyFromBase64EncodedString(encryptedSessionKey);
            }
            catch (PSCryptoException)
            {
                return false;
            }
            return true;
        }

        internal override RemoteSession Session
        {
            get
            {
                return this._session;
            }
            set
            {
                this._session = value;
            }
        }
    }
}

