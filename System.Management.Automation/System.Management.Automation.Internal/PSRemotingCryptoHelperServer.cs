namespace System.Management.Automation.Internal
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Runtime.InteropServices;
    using System.Security;

    internal class PSRemotingCryptoHelperServer : PSRemotingCryptoHelper
    {
        private RemoteSession _session;

        internal PSRemotingCryptoHelperServer()
        {
            base._rsaCryptoProvider = PSRSACryptoServiceProvider.GetRSACryptoServiceProviderForServer();
        }

        internal override SecureString DecryptSecureString(string encryptedString)
        {
            base.RunKeyExchangeIfRequired();
            return base.DecryptSecureStringCore(encryptedString);
        }

        internal override string EncryptSecureString(SecureString secureString)
        {
            ServerRemoteSession session = this.Session as ServerRemoteSession;
            if ((session != null) && (session.Context.ClientCapability.ProtocolVersion >= RemotingConstants.ProtocolVersionWin8RTM))
            {
                base._rsaCryptoProvider.GenerateSessionKey();
            }
            else
            {
                base.RunKeyExchangeIfRequired();
            }
            return base.EncryptSecureStringCore(secureString);
        }

        internal bool ExportEncryptedSessionKey(out string encryptedSessionKey)
        {
            try
            {
                encryptedSessionKey = base._rsaCryptoProvider.SafeExportSessionKey();
            }
            catch (PSCryptoException)
            {
                encryptedSessionKey = string.Empty;
                return false;
            }
            return true;
        }

        internal static PSRemotingCryptoHelperServer GetTestRemotingCryptHelperServer()
        {
            return new PSRemotingCryptoHelperServer { Session = new TestHelperSession() };
        }

        internal bool ImportRemotePublicKey(string publicKeyAsString)
        {
            try
            {
                base._rsaCryptoProvider.ImportPublicKeyFromBase64EncodedString(publicKeyAsString);
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

