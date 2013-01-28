namespace Microsoft.PowerShell
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Text;

    internal static class SecureStringHelper
    {
        internal static string SecureStringExportHeader = "76492d1116743f0423413b16050a5345";

        internal static byte[] ByteArrayFromString(string s)
        {
            int num = s.Length / 2;
            byte[] buffer = new byte[num];
            if (s.Length > 0)
            {
                for (int i = 0; i < num; i++)
                {
                    buffer[i] = byte.Parse(s.Substring(2 * i, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                }
            }
            return buffer;
        }

        internal static string ByteArrayToString(byte[] data)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                builder.Append(data[i].ToString("x2", CultureInfo.InvariantCulture));
            }
            return builder.ToString();
        }

        internal static SecureString Decrypt(string input, byte[] key, byte[] IV)
        {
            System.Management.Automation.Utils.CheckArgForNullOrEmpty(input, "input");
            System.Management.Automation.Utils.CheckKeyArg(key, "key");
            byte[] data = null;
            byte[] buffer = null;
            SecureString str = null;
            SymmetricAlgorithm algorithm = SymmetricAlgorithm.Create();
            buffer = ByteArrayFromString(input);
            ICryptoTransform transform = null;
            if (IV != null)
            {
                transform = algorithm.CreateDecryptor(key, IV);
            }
            else
            {
                algorithm.Key = key;
                transform = algorithm.CreateDecryptor();
            }
            MemoryStream stream = new MemoryStream(buffer);
            using (CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Read))
            {
                byte[] buffer3 = new byte[buffer.Length];
                int num = 0;
                num = stream2.Read(buffer3, 0, buffer3.Length);
                data = new byte[num];
                for (int i = 0; i < num; i++)
                {
                    data[i] = buffer3[i];
                }
                str = New(data);
                Array.Clear(data, 0, data.Length);
                Array.Clear(buffer3, 0, buffer3.Length);
                return str;
            }
        }

        internal static SecureString Decrypt(string input, SecureString key, byte[] IV)
        {
            SecureString str = null;
            byte[] data = GetData(key);
            str = Decrypt(input, data, IV);
            Array.Clear(data, 0, data.Length);
            return str;
        }

        internal static EncryptionResult Encrypt(SecureString input, byte[] key)
        {
            return Encrypt(input, key, null);
        }

        internal static EncryptionResult Encrypt(SecureString input, SecureString key)
        {
            EncryptionResult result = null;
            byte[] data = GetData(key);
            result = Encrypt(input, data);
            Array.Clear(data, 0, data.Length);
            return result;
        }

        internal static EncryptionResult Encrypt(SecureString input, byte[] key, byte[] iv)
        {
            System.Management.Automation.Utils.CheckSecureStringArg(input, "input");
            System.Management.Automation.Utils.CheckKeyArg(key, "key");
            MemoryStream stream = null;
            ICryptoTransform transform = null;
            CryptoStream stream2 = null;
            SymmetricAlgorithm algorithm = SymmetricAlgorithm.Create();
            if (iv == null)
            {
                iv = algorithm.IV;
            }
            transform = algorithm.CreateEncryptor(key, iv);
            stream = new MemoryStream();
            using (stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Write))
            {
                byte[] data = GetData(input);
                stream2.Write(data, 0, data.Length);
                stream2.FlushFinalBlock();
                Array.Clear(data, 0, data.Length);
                return new EncryptionResult(ByteArrayToString(stream.ToArray()), Convert.ToBase64String(iv));
            }
        }

        [ArchitectureSensitive]
        internal static byte[] GetData(SecureString s)
        {
            byte[] destination = new byte[s.Length * 2];
            if (s.Length > 0)
            {
                IntPtr source = Marshal.SecureStringToGlobalAllocUnicode(s);
                try
                {
                    Marshal.Copy(source, destination, 0, destination.Length);
                }
                finally
                {
                    Marshal.FreeHGlobal(source);
                }
            }
            return destination;
        }

        private static SecureString New(byte[] data)
        {
            if ((data.Length % 2) != 0)
            {
                throw new PSArgumentException(Serialization.InvalidKey);
            }
            SecureString str2 = new SecureString();
            int num = data.Length / 2;
            for (int i = 0; i < num; i++)
            {
                char c = (char) ((data[(2 * i) + 1] * 0x100) + data[2 * i]);
                str2.AppendChar(c);
                data[2 * i] = 0;
                data[(2 * i) + 1] = 0;
            }
            return str2;
        }

        internal static string Protect(SecureString input)
        {
            System.Management.Automation.Utils.CheckSecureStringArg(input, "input");
            byte[] userData = null;
            byte[] data = null;
            userData = GetData(input);
            data = ProtectedData.Protect(userData, null, DataProtectionScope.CurrentUser);
            for (int i = 0; i < userData.Length; i++)
            {
                userData[i] = 0;
            }
            return ByteArrayToString(data);
		}

        internal static SecureString Unprotect(string input)
        {
            System.Management.Automation.Utils.CheckArgForNullOrEmpty(input, "input");
            if ((input.Length % 2) != 0)
            {
                throw PSTraceSource.NewArgumentException("input", "Serialization", "InvalidEncryptedString", new object[] { input });
            }
            return New(ProtectedData.Unprotect(ByteArrayFromString(input), null, DataProtectionScope.CurrentUser));
        }

		public static SecureString EasyUnprotect (string pwd)
		{
			var d = new SecureString ();
			foreach (var c in pwd) {
				d.AppendChar (c);
			}
			return d;
		}
    }
}

