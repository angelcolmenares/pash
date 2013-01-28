namespace System.Management.Automation.Internal
{
    using System;
    using System.ComponentModel;
    using System.Text;

    internal class PSRSACryptoServiceProvider : IDisposable
    {
        private static PSSafeCryptProvHandle _hStaticProv;
        private static PSSafeCryptKey _hStaticRSAKey;
        private bool canEncrypt;
        private PSSafeCryptProvHandle hProv;
        private PSSafeCryptKey hRSAKey;
        private PSSafeCryptKey hSessionKey;
        private static bool keyPairGenerated = false;
        private bool sessionKeyGenerated;
        private static object syncObject = new object();

        private PSRSACryptoServiceProvider(bool serverMode)
        {
            if (serverMode)
            {
                this.hProv = new PSSafeCryptProvHandle();
				bool flag = PSCryptoNativeUtils.CryptAcquireContext(ref this.hProv, null, null, 0x18, Int32.MaxValue); // (0xf0000000).ToInt32());
                this.CheckStatus(flag);
                this.hRSAKey = new PSSafeCryptKey();
            }
            this.hSessionKey = new PSSafeCryptKey();
        }

        private void CheckStatus(bool value)
        {
            if (!value)
            {
                int lastError = PSCryptoNativeUtils.GetLastError();
                StringBuilder message = new StringBuilder(new Win32Exception((int) lastError).Message);
                throw new PSCryptoException(lastError, message);
            }
        }

        internal byte[] DecryptWithSessionKey(byte[] data)
        {
            byte[] destinationArray = new byte[data.Length];
            Array.Copy(data, 0, destinationArray, 0, data.Length);
            int length = destinationArray.Length;
            if (!PSCryptoNativeUtils.CryptDecrypt(this.hSessionKey, IntPtr.Zero, true, 0, destinationArray, ref length))
            {
                destinationArray = new byte[length];
                Array.Copy(data, 0, destinationArray, 0, data.Length);
                bool flag = PSCryptoNativeUtils.CryptDecrypt(this.hSessionKey, IntPtr.Zero, true, 0, destinationArray, ref length);
                this.CheckStatus(flag);
            }
            byte[] buffer2 = new byte[length];
            Array.Copy(destinationArray, 0, buffer2, 0, length);
            for (int i = 0; i < destinationArray.Length; i++)
            {
                destinationArray[i] = 0;
            }
            return buffer2;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.hSessionKey != null)
                {
                    if (!this.hSessionKey.IsInvalid)
                    {
                        this.hSessionKey.Dispose();
                    }
                    this.hSessionKey = null;
                }
                if ((_hStaticRSAKey == null) && (this.hRSAKey != null))
                {
                    if (!this.hRSAKey.IsInvalid)
                    {
                        this.hRSAKey.Dispose();
                    }
                    this.hRSAKey = null;
                }
                if ((_hStaticProv == null) && (this.hProv != null))
                {
                    if (!this.hProv.IsInvalid)
                    {
                        this.hProv.Dispose();
                    }
                    this.hProv = null;
                }
            }
        }

        internal byte[] EncryptWithSessionKey(byte[] data)
        {
            byte[] destinationArray = new byte[data.Length];
            Array.Copy(data, 0, destinationArray, 0, data.Length);
            int length = destinationArray.Length;
            if (!PSCryptoNativeUtils.CryptEncrypt(this.hSessionKey, IntPtr.Zero, true, 0, destinationArray, ref length, data.Length))
            {
                for (int i = 0; i < destinationArray.Length; i++)
                {
                    destinationArray[i] = 0;
                }
                destinationArray = new byte[length];
                Array.Copy(data, 0, destinationArray, 0, data.Length);
                length = data.Length;
                bool flag = PSCryptoNativeUtils.CryptEncrypt(this.hSessionKey, IntPtr.Zero, true, 0, destinationArray, ref length, destinationArray.Length);
                this.CheckStatus(flag);
            }
            byte[] buffer2 = new byte[length];
            Array.Copy(destinationArray, 0, buffer2, 0, length);
            return buffer2;
        }

        ~PSRSACryptoServiceProvider()
        {
            this.Dispose(true);
        }

        internal void GenerateKeyPair()
        {
            if (!keyPairGenerated)
            {
                lock (syncObject)
                {
                    if (!keyPairGenerated)
                    {
                        _hStaticProv = new PSSafeCryptProvHandle();
                        bool flag = PSCryptoNativeUtils.CryptAcquireContext(ref _hStaticProv, null, null, 0x18, (0xf0000000).ToInt32());
                        this.CheckStatus(flag);
                        _hStaticRSAKey = new PSSafeCryptKey();
                        flag = PSCryptoNativeUtils.CryptGenKey(_hStaticProv, 1, 0x8000001, ref _hStaticRSAKey);
                        this.CheckStatus(flag);
                        keyPairGenerated = true;
                    }
                }
            }
            this.hProv = _hStaticProv;
            this.hRSAKey = _hStaticRSAKey;
        }

        internal void GenerateSessionKey()
        {
            if (!this.sessionKeyGenerated)
            {
                lock (syncObject)
                {
                    if (!this.sessionKeyGenerated)
                    {
                        bool flag = PSCryptoNativeUtils.CryptGenKey(this.hProv, 0x6610, 0x1000005, ref this.hSessionKey);
                        this.CheckStatus(flag);
                        this.sessionKeyGenerated = true;
                        this.canEncrypt = true;
                    }
                }
            }
        }

        internal string GetPublicKeyAsBase64EncodedString()
        {
            int pdwDataLen = 0;
            bool flag = PSCryptoNativeUtils.CryptExportKey(this.hRSAKey, PSSafeCryptKey.Zero, 6, 0, null, ref pdwDataLen);
            this.CheckStatus(flag);
            byte[] pbData = new byte[pdwDataLen];
            flag = PSCryptoNativeUtils.CryptExportKey(this.hRSAKey, PSSafeCryptKey.Zero, 6, 0, pbData, ref pdwDataLen);
            this.CheckStatus(flag);
            return Convert.ToBase64String(pbData, Base64FormattingOptions.None);
        }

        internal static PSRSACryptoServiceProvider GetRSACryptoServiceProviderForClient()
        {
            return new PSRSACryptoServiceProvider(false) { hProv = _hStaticProv, hRSAKey = _hStaticRSAKey };
        }

        internal static PSRSACryptoServiceProvider GetRSACryptoServiceProviderForServer()
        {
            return new PSRSACryptoServiceProvider(true);
        }

        internal void ImportPublicKeyFromBase64EncodedString(string publicKey)
        {
            byte[] pbData = Convert.FromBase64String(publicKey);
            bool flag = PSCryptoNativeUtils.CryptImportKey(this.hProv, pbData, pbData.Length, PSSafeCryptKey.Zero, 0, ref this.hRSAKey);
            this.CheckStatus(flag);
        }

        internal void ImportSessionKeyFromBase64EncodedString(string sessionKey)
        {
            byte[] pbData = Convert.FromBase64String(sessionKey);
            bool flag = PSCryptoNativeUtils.CryptImportKey(this.hProv, pbData, pbData.Length, this.hRSAKey, 0, ref this.hSessionKey);
            this.CheckStatus(flag);
            this.canEncrypt = true;
        }

        internal string SafeExportSessionKey()
        {
            this.GenerateSessionKey();
            int pdwDataLen = 0;
            bool flag = PSCryptoNativeUtils.CryptExportKey(this.hSessionKey, this.hRSAKey, 1, 0, null, ref pdwDataLen);
            this.CheckStatus(flag);
            byte[] pbData = new byte[pdwDataLen];
            flag = PSCryptoNativeUtils.CryptExportKey(this.hSessionKey, this.hRSAKey, 1, 0, pbData, ref pdwDataLen);
            this.CheckStatus(flag);
            this.canEncrypt = true;
            return Convert.ToBase64String(pbData, Base64FormattingOptions.None);
        }

        internal bool CanEncrypt
        {
            get
            {
                return this.canEncrypt;
            }
            set
            {
                this.canEncrypt = value;
            }
        }
    }
}

