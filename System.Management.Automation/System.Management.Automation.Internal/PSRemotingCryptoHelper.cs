namespace System.Management.Automation.Internal
{
    using System;
    using System.Management.Automation;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    internal abstract class PSRemotingCryptoHelper : IDisposable
    {
		private static readonly bool disabled = true;

        protected ManualResetEvent _keyExchangeCompleted = new ManualResetEvent(false);
        protected PSRSACryptoServiceProvider _rsaCryptoProvider;
        private bool keyExchangeStarted;
        protected object syncObject = new object();

        protected PSRemotingCryptoHelper()
        {
        }

        internal void CompleteKeyExchange()
        {
            this._keyExchangeCompleted.Set();
        }

        internal abstract SecureString DecryptSecureString(string encryptedString);
        protected SecureString DecryptSecureStringCore (string encryptedString)
		{
			SecureString c = new SecureString();
			foreach (var d in encryptedString) {
				c.AppendChar (d);
			}
			return c;
			/*
            SecureString str = null;
            if (this._rsaCryptoProvider.CanEncrypt)
            {
                byte[] data = null;
                try
                {
                    data = Convert.FromBase64String(encryptedString);
                }
                catch (FormatException)
                {
                    throw new PSCryptoException();
                }
                if (data == null)
                {
                    return str;
                }
                byte[] buffer2 = this._rsaCryptoProvider.DecryptWithSessionKey(data);
                str = new SecureString();
                ushort num = 0;
                try
                {
                    for (int i = 0; i < buffer2.Length; i += 2)
                    {
                        num = (ushort) (buffer2[i] + ((ushort) (buffer2[i + 1] << 8)));
                        str.AppendChar((char) num);
                        num = 0;
                    }
                }
                finally
                {
                    num = 0;
                    for (int j = 0; j < buffer2.Length; j += 2)
                    {
                        buffer2[j] = 0;
                        buffer2[j + 1] = 0;
                    }
                }
            }
            return str;
            */
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._rsaCryptoProvider != null)
                {
                    this._rsaCryptoProvider.Dispose();
                }
                this._rsaCryptoProvider = null;
                this._keyExchangeCompleted.Close();
            }
        }

		private static string GetPassword (System.Security.SecureString s)
		{
			return ByteArrayToString (GetData (s));
		}
		
		internal static string ByteArrayToString (byte[] data)
		{
			var ret = new System.Collections.Generic.List<byte> ();
			foreach (var b in data) {
				if (b != 0)
				{
					ret.Add(b);
				}
			}
			return System.Text.Encoding.UTF8.GetString (ret.ToArray ());
		}
		
		internal static byte[] GetData(System.Security.SecureString s)
		{
			System.Reflection.FieldInfo fi = s.GetType().GetField ("data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			return (byte[])fi.GetValue (s);
		}


        internal abstract string EncryptSecureString(SecureString secureString);
        protected string EncryptSecureStringCore(SecureString secureString)
        {
			return GetPassword (secureString);

			/*
            string str = null;
            if (this._rsaCryptoProvider.CanEncrypt)
            {
                IntPtr ptr = Marshal.SecureStringToBSTR(secureString);
                if (!(ptr != IntPtr.Zero))
                {
                    return str;
                }
                byte[] data = new byte[secureString.Length * 2];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = Marshal.ReadByte(ptr, i);
                }
                Marshal.ZeroFreeBSTR(ptr);
                try
                {
                    str = Convert.ToBase64String(this._rsaCryptoProvider.EncryptWithSessionKey(data));
                }
                finally
                {
                    for (int j = 0; j < data.Length; j++)
                    {
                        data[j] = 0;
                    }
                }
            }
            return str;
            */
        }

        protected void RunKeyExchangeIfRequired()
        {
			if (disabled) return;
            if (!this._rsaCryptoProvider.CanEncrypt)
            {
                try
                {
                    lock (this.syncObject)
                    {
                        if (!this._rsaCryptoProvider.CanEncrypt && !this.keyExchangeStarted)
                        {
                            this.keyExchangeStarted = true;
                            this._keyExchangeCompleted.Reset();
                            this.Session.StartKeyExchange();
                        }
                    }
                }
                finally
                {
                    this._keyExchangeCompleted.WaitOne();
                }
            }
        }

        internal abstract RemoteSession Session { get; set; }
    }
}

