namespace System.Management.Automation
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Management.Automation.Internal;
    using System.Security.Cryptography.X509Certificates;

    public sealed class Signature
    {
        private string path;
        private X509Certificate2 signerCert;
        private SignatureStatus status;
        private string statusMessage;
        private X509Certificate2 timeStamperCert;
        private uint win32Error;

        internal Signature(string filePath, X509Certificate2 signer)
        {
            this.status = SignatureStatus.UnknownError;
            this.statusMessage = string.Empty;
            Utils.CheckArgForNullOrEmpty(filePath, "filePath");
            Utils.CheckArgForNull(signer, "signer");
            this.Init(filePath, signer, 0, null);
        }

        internal Signature(string filePath, uint error)
        {
            this.status = SignatureStatus.UnknownError;
            this.statusMessage = string.Empty;
            Utils.CheckArgForNullOrEmpty(filePath, "filePath");
            this.Init(filePath, null, error, null);
        }

        internal Signature(string filePath, uint error, X509Certificate2 signer)
        {
            this.status = SignatureStatus.UnknownError;
            this.statusMessage = string.Empty;
            Utils.CheckArgForNullOrEmpty(filePath, "filePath");
            Utils.CheckArgForNull(signer, "signer");
            this.Init(filePath, signer, error, null);
        }

        internal Signature(string filePath, uint error, X509Certificate2 signer, X509Certificate2 timestamper)
        {
            this.status = SignatureStatus.UnknownError;
            this.statusMessage = string.Empty;
            Utils.CheckArgForNullOrEmpty(filePath, "filePath");
            Utils.CheckArgForNull(signer, "signer");
            Utils.CheckArgForNull(timestamper, "timestamper");
            this.Init(filePath, signer, error, timestamper);
        }

        private static SignatureStatus GetSignatureStatusFromWin32Error(uint error)
        {
            SignatureStatus unknownError = SignatureStatus.UnknownError;
            uint num = error;
            if (num <= 0x8009200d)
            {
                switch (num)
                {
                    case 0:
                        return SignatureStatus.Valid;

                    case 0x80090008:
                        return SignatureStatus.Incompatible;

                    case 0x8009200d:
                        goto Label_005A;
                }
                return unknownError;
            }
            if (num <= 0x800b0001)
            {
                switch (num)
                {
                    case 0x80096010:
                        goto Label_005A;

                    case 0x800b0001:
                        return SignatureStatus.NotSupportedFileFormat;
                }
                return unknownError;
            }
            switch (num)
            {
                case 0x800b0100:
                    return SignatureStatus.NotSigned;

                case 0x800b0111:
                    return SignatureStatus.NotTrusted;

                default:
                    return unknownError;
            }
        Label_005A:
            return SignatureStatus.HashMismatch;
        }

        private static string GetSignatureStatusMessage(SignatureStatus status, uint error, string filePath)
        {
            string message = null;
            string formatSpec = null;
            string extension = null;
            switch (status)
            {
                case SignatureStatus.Valid:
                    formatSpec = MshSignature.MshSignature_Valid;
                    goto Label_00A4;

                case SignatureStatus.UnknownError:
                {
                    Win32Exception exception = new Win32Exception(SecuritySupport.GetIntFromDWORD(error));
                    message = exception.Message;
                    goto Label_00A4;
                }
                case SignatureStatus.NotSigned:
                    formatSpec = MshSignature.MshSignature_NotSigned;
                    extension = filePath;
                    goto Label_00A4;

                case SignatureStatus.HashMismatch:
                    formatSpec = MshSignature.MshSignature_HashMismatch;
                    extension = filePath;
                    goto Label_00A4;

                case SignatureStatus.NotTrusted:
                    formatSpec = MshSignature.MshSignature_NotTrusted;
                    extension = filePath;
                    goto Label_00A4;

                case SignatureStatus.NotSupportedFileFormat:
                    formatSpec = MshSignature.MshSignature_NotSupportedFileFormat;
                    extension = System.IO.Path.GetExtension(filePath);
                    if (string.IsNullOrEmpty(extension))
                    {
                        formatSpec = MshSignature.MshSignature_NotSupportedFileFormat_NoExtension;
                        extension = null;
                    }
                    goto Label_00A4;

                case SignatureStatus.Incompatible:
                    if (error != 0x80090008)
                    {
                        formatSpec = MshSignature.MshSignature_Incompatible;
                        break;
                    }
                    formatSpec = MshSignature.MshSignature_Incompatible_HashAlgorithm;
                    break;

                default:
                    goto Label_00A4;
            }
            extension = filePath;
        Label_00A4:
            if (message != null)
            {
                return message;
            }
            if (extension == null)
            {
                return formatSpec;
            }
            return StringUtil.Format(formatSpec, extension);
        }

        private void Init(string filePath, X509Certificate2 signer, uint error, X509Certificate2 timestamper)
        {
            this.path = filePath;
            this.win32Error = error;
            this.signerCert = signer;
            this.timeStamperCert = timestamper;
            SignatureStatus status = GetSignatureStatusFromWin32Error(error);
            this.status = status;
            this.statusMessage = GetSignatureStatusMessage(status, error, filePath);
        }

        public string Path
        {
            get
            {
                return this.path;
            }
        }

        public X509Certificate2 SignerCertificate
        {
            get
            {
                return this.signerCert;
            }
        }

        public SignatureStatus Status
        {
            get
            {
                return this.status;
            }
        }

        public string StatusMessage
        {
            get
            {
                return this.statusMessage;
            }
        }

        public X509Certificate2 TimeStamperCertificate
        {
            get
            {
                return this.timeStamperCert;
            }
        }
    }
}

