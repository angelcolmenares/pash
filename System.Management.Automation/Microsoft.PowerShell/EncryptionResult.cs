namespace Microsoft.PowerShell
{
    using System;

    internal class EncryptionResult
    {
        private string encryptedData;
        private string iv;

        internal EncryptionResult(string encrypted, string IV)
        {
            this.encryptedData = encrypted;
            this.iv = IV;
        }

        internal string EncryptedData
        {
            get
            {
                return this.encryptedData;
            }
        }

        internal string IV
        {
            get
            {
                return this.iv;
            }
        }
    }
}

