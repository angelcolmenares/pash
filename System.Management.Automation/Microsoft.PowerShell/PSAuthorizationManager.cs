namespace Microsoft.PowerShell
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Policy;
    using System.Text;
    using System.Threading;

    public sealed class PSAuthorizationManager : AuthorizationManager
    {
        private ExecutionPolicy executionPolicy;
        private string shellId;

        public PSAuthorizationManager(string shellId) : base(shellId)
        {
            if (string.IsNullOrEmpty(shellId))
            {
                throw PSTraceSource.NewArgumentNullException("shellId");
            }
            this.shellId = shellId;
        }

        private RunPromptDecision AuthenticodePrompt(string path, System.Management.Automation.Signature signature, PSHost host)
        {
            Collection<ChoiceDescription> authenticodePromptChoices;
            string authenticodePromptCaption;
            string str2;
            if ((host == null) || (host.UI == null))
            {
                return RunPromptDecision.DoNotRun;
            }
            RunPromptDecision doNotRun = RunPromptDecision.DoNotRun;
            if (signature == null)
            {
                return doNotRun;
            }
            switch (signature.Status)
            {
                case SignatureStatus.Valid:
                    authenticodePromptChoices = this.GetAuthenticodePromptChoices();
                    authenticodePromptCaption = Authenticode.AuthenticodePromptCaption;
                    if (signature.SignerCertificate != null)
                    {
                        str2 = StringUtil.Format(Authenticode.AuthenticodePromptText, path, signature.SignerCertificate.SubjectName.Name);
                        break;
                    }
                    str2 = StringUtil.Format(Authenticode.AuthenticodePromptText_UnknownPublisher, path);
                    break;

                case SignatureStatus.UnknownError:
                case SignatureStatus.NotSigned:
                case SignatureStatus.HashMismatch:
                case SignatureStatus.NotSupportedFileFormat:
                    return RunPromptDecision.DoNotRun;

                default:
                    return RunPromptDecision.DoNotRun;
            }
            return (RunPromptDecision) host.UI.PromptForChoice(authenticodePromptCaption, str2, authenticodePromptChoices, 1);
        }

        private bool CheckPolicy (ExternalScriptInfo script, PSHost host, out Exception reason)
		{
			string str2;
			bool flag = false;
			reason = null;
			string path = script.Path;
			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				if (path.IndexOf ('\\') < 0) {
					throw PSTraceSource.NewArgumentException ("path");
				}
				if (path.LastIndexOf ('\\') == (path.Length - 1)) {
					throw PSTraceSource.NewArgumentException ("path");
				}
			}
            FileInfo info = new FileInfo(path);
            if (!info.Exists)
            {
                reason = new FileNotFoundException(path);
                return false;
            }
            if (!IsSupportedExtension(info.Extension))
            {
                return true;
            }
            if (this.IsProductBinary(path))
            {
                return true;
            }
            this.executionPolicy = SecuritySupport.GetExecutionPolicy(this.shellId);
            if (this.executionPolicy == ExecutionPolicy.Bypass)
            {
                return true;
            }
            SaferPolicy disallowed = SaferPolicy.Disallowed;
            int num = 0;
            bool flag2 = false;
            while (!flag2 && (num < 5))
            {
                try
                {
                    disallowed = SecuritySupport.GetSaferPolicy(path);
                    flag2 = true;
                    continue;
                }
                catch (Win32Exception)
                {
                    if (num > 4)
                    {
                        throw;
                    }
                    num++;
                    Thread.Sleep(100);
                    continue;
                }
            }
            if (disallowed == SaferPolicy.Disallowed)
            {
                str2 = StringUtil.Format(Authenticode.Reason_DisallowedBySafer, path);
                reason = new UnauthorizedAccessException(str2);
                return false;
            }
            if (this.executionPolicy != ExecutionPolicy.Unrestricted)
            {
                if (this.IsLocalFile(info.FullName) && (this.executionPolicy == ExecutionPolicy.RemoteSigned))
                {
                    return true;
                }
                if ((this.executionPolicy == ExecutionPolicy.AllSigned) || (this.executionPolicy == ExecutionPolicy.RemoteSigned))
                {
                    if (string.IsNullOrEmpty(script.ScriptContents))
                    {
                        str2 = StringUtil.Format(Authenticode.Reason_FileContentUnavailable, path);
                        reason = new UnauthorizedAccessException(str2);
                        return false;
                    }
                    System.Management.Automation.Signature signature = this.GetSignatureWithEncodingRetry(path, script);
                    if (signature.Status == SignatureStatus.Valid)
                    {
                        return (this.IsTrustedPublisher(signature, path) || this.SetPolicyFromAuthenticodePrompt(path, host, ref reason, signature));
                    }
                    flag = false;
                    if (signature.Status == SignatureStatus.NotTrusted)
                    {
                        reason = new UnauthorizedAccessException(StringUtil.Format(Authenticode.Reason_NotTrusted, path, signature.SignerCertificate.SubjectName.Name));
                        return flag;
                    }
                    reason = new UnauthorizedAccessException(StringUtil.Format(Authenticode.Reason_Unknown, path, signature.StatusMessage));
                    return flag;
                }
                flag = false;
                bool flag3 = false;
                if (string.Equals(info.Extension, ".ps1xml", StringComparison.OrdinalIgnoreCase))
                {
                    string[] strArray = new string[] { Environment.GetFolderPath(Environment.SpecialFolder.System), Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) };
                    foreach (string str3 in strArray)
                    {
                        if (info.FullName.StartsWith(str3, StringComparison.OrdinalIgnoreCase))
                        {
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        System.Management.Automation.Signature signature3 = this.GetSignatureWithEncodingRetry(path, script);
                        if (signature3.Status == SignatureStatus.Valid)
                        {
                            if (this.IsTrustedPublisher(signature3, path))
                            {
                                flag = true;
                            }
                            else
                            {
                                flag = this.SetPolicyFromAuthenticodePrompt(path, host, ref reason, signature3);
                                flag3 = true;
                            }
                        }
                    }
                }
                if (!flag && !flag3)
                {
                    reason = new UnauthorizedAccessException(StringUtil.Format(Authenticode.Reason_RestrictedMode, path));
                }
                return flag;
            }
            if (this.IsLocalFile(info.FullName))
            {
                return true;
            }
            if (string.IsNullOrEmpty(script.ScriptContents))
            {
                str2 = StringUtil.Format(Authenticode.Reason_FileContentUnavailable, path);
                reason = new UnauthorizedAccessException(str2);
                return false;
            }
            System.Management.Automation.Signature signatureWithEncodingRetry = this.GetSignatureWithEncodingRetry(path, script);
            if ((signatureWithEncodingRetry.Status == SignatureStatus.Valid) && this.IsTrustedPublisher(signatureWithEncodingRetry, path))
            {
                flag = true;
            }
            if (flag)
            {
                return flag;
            }
            RunPromptDecision doNotRun = RunPromptDecision.DoNotRun;
        Label_0149:
            doNotRun = this.RemoteFilePrompt(path, host);
            if (doNotRun == RunPromptDecision.Suspend)
            {
                host.EnterNestedPrompt();
            }
            switch (doNotRun)
            {
                case RunPromptDecision.RunOnce:
                    return true;

                case RunPromptDecision.Suspend:
                    goto Label_0149;
            }
            flag = false;
            str2 = StringUtil.Format(Authenticode.Reason_DoNotRun, path);
            reason = new UnauthorizedAccessException(str2);
            return flag;
        }

        private Collection<ChoiceDescription> GetAuthenticodePromptChoices()
        {
            Collection<ChoiceDescription> collection = new Collection<ChoiceDescription>();
            string label = Authenticode.Choice_NeverRun;
            string helpMessage = Authenticode.Choice_NeverRun_Help;
            string str3 = Authenticode.Choice_DoNotRun;
            string str4 = Authenticode.Choice_DoNotRun_Help;
            string str5 = Authenticode.Choice_RunOnce;
            string str6 = Authenticode.Choice_RunOnce_Help;
            string str7 = Authenticode.Choice_AlwaysRun;
            string str8 = Authenticode.Choice_AlwaysRun_Help;
            collection.Add(new ChoiceDescription(label, helpMessage));
            collection.Add(new ChoiceDescription(str3, str4));
            collection.Add(new ChoiceDescription(str5, str6));
            collection.Add(new ChoiceDescription(str7, str8));
            return collection;
        }

        private Collection<ChoiceDescription> GetRemoteFilePromptChoices()
        {
            Collection<ChoiceDescription> collection = new Collection<ChoiceDescription>();
            string label = Authenticode.Choice_DoNotRun;
            string helpMessage = Authenticode.Choice_DoNotRun_Help;
            string str3 = Authenticode.Choice_RunOnce;
            string str4 = Authenticode.Choice_RunOnce_Help;
            string str5 = Authenticode.Choice_Suspend;
            string str6 = Authenticode.Choice_Suspend_Help;
            collection.Add(new ChoiceDescription(label, helpMessage));
            collection.Add(new ChoiceDescription(str3, str4));
            collection.Add(new ChoiceDescription(str5, str6));
            return collection;
        }

        private System.Management.Automation.Signature GetSignatureWithEncodingRetry(string path, ExternalScriptInfo script)
        {
            string fileContent = Encoding.Unicode.GetString(script.OriginalEncoding.GetPreamble()) + script.ScriptContents;
            System.Management.Automation.Signature signature = SignatureHelper.GetSignature(path, fileContent);
            if ((signature.Status != SignatureStatus.Valid) && (script.OriginalEncoding != Encoding.Unicode))
            {
                fileContent = Encoding.Unicode.GetString(Encoding.Unicode.GetPreamble()) + script.ScriptContents;
                System.Management.Automation.Signature signature2 = SignatureHelper.GetSignature(path, fileContent);
                if (signature2.Status == SignatureStatus.Valid)
                {
                    signature = signature2;
                }
            }
            return signature;
        }

        private bool IsLocalFile(string filename)
        {
            Zone zone = Zone.CreateFromUrl(filename);
            if (((zone.SecurityZone != SecurityZone.MyComputer) && (zone.SecurityZone != SecurityZone.Intranet)) && (zone.SecurityZone != SecurityZone.Trusted))
            {
                return false;
            }
            return true;
        }

        private bool IsProductBinary(string file)
        {
            if (!string.Equals(new FileInfo(file).Extension, ".ps1", StringComparison.OrdinalIgnoreCase))
            {
                List<string> list = new List<string>();
				list.Add(PowerShellConfiguration.PowerShellEngine.ApplicationBase);
                list.Add(Environment.GetFolderPath(Environment.SpecialFolder.System));
                list.Add(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86));
                FileInfo info = new FileInfo(file);
                string str3 = info.FullName.ToUpper(CultureInfo.CurrentCulture);
                foreach (string str4 in list)
                {
                    if (str3.IndexOf(str4.ToUpper(CultureInfo.CurrentCulture), StringComparison.CurrentCulture) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool IsSupportedExtension(string ext)
        {
            if (((!ext.Equals(".ps1", StringComparison.OrdinalIgnoreCase) && !ext.Equals(".ps1xml", StringComparison.OrdinalIgnoreCase)) && (!ext.Equals(".psm1", StringComparison.OrdinalIgnoreCase) && !ext.Equals(".psd1", StringComparison.OrdinalIgnoreCase))) && !ext.Equals(".xaml", StringComparison.OrdinalIgnoreCase))
            {
                return ext.Equals(".cdxml", StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }

        private List<string> _trustedPublishersThumbs = new List<string>( new string[] { "9E95C625D81B2BA9C72FD70275C3699613AF61E3", "93859EBF98AFDEB488CCFA263899640E81BC49F1", "D57FAC60F1A8D34877AEB350E83F46F6EFC9E5F1", "19F8F76F4655074509769C20349FFAECCECD217D" } );

        private bool IsTrustedPublisher(System.Management.Automation.Signature signature, string file)
        {
            string thumbprint = signature.SignerCertificate.Thumbprint;
            if (_trustedPublishersThumbs.Contains(thumbprint)) return true;
            X509Store store = new X509Store(StoreName.TrustedPublisher);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Enumerator enumerator = store.Certificates.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (string.Equals(enumerator.Current.Thumbprint, thumbprint, StringComparison.OrdinalIgnoreCase) && !this.IsUntrustedPublisher(signature, file))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsUntrustedPublisher(System.Management.Automation.Signature signature, string file)
        {
            string thumbprint = signature.SignerCertificate.Thumbprint;
            X509Store store = new X509Store(StoreName.Disallowed);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Enumerator enumerator = store.Certificates.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (string.Equals(enumerator.Current.Thumbprint, thumbprint, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private RunPromptDecision RemoteFilePrompt(string path, PSHost host)
        {
            if ((host != null) && (host.UI != null))
            {
                Collection<ChoiceDescription> remoteFilePromptChoices = this.GetRemoteFilePromptChoices();
                string remoteFilePromptCaption = Authenticode.RemoteFilePromptCaption;
                string message = StringUtil.Format(Authenticode.RemoteFilePromptText, path);
                switch (host.UI.PromptForChoice(remoteFilePromptCaption, message, remoteFilePromptChoices, 0))
                {
                    case 0:
                        return RunPromptDecision.DoNotRun;

                    case 1:
                        return RunPromptDecision.RunOnce;

                    case 2:
                        return RunPromptDecision.Suspend;
                }
            }
            return RunPromptDecision.DoNotRun;
        }

        private bool SetPolicyFromAuthenticodePrompt(string path, PSHost host, ref Exception reason, System.Management.Automation.Signature signature)
        {
            string str;
            bool flag = false;
            switch (this.AuthenticodePrompt(path, signature, host))
            {
                case RunPromptDecision.NeverRun:
                    this.UntrustPublisher(signature);
                    str = StringUtil.Format(Authenticode.Reason_NeverRun, path);
                    reason = new UnauthorizedAccessException(str);
                    return false;

                case RunPromptDecision.DoNotRun:
                    flag = false;
                    str = StringUtil.Format(Authenticode.Reason_DoNotRun, path);
                    reason = new UnauthorizedAccessException(str);
                    return flag;

                case RunPromptDecision.RunOnce:
                    return true;

                case RunPromptDecision.AlwaysRun:
                    this.TrustPublisher(signature);
                    return true;
            }
            return flag;
        }

        protected internal override bool ShouldRun(CommandInfo commandInfo, CommandOrigin origin, PSHost host, out Exception reason)
        {
            bool flag = false;
            reason = null;
            Utils.CheckArgForNull(commandInfo, "commandInfo");
            Utils.CheckArgForNullOrEmpty(commandInfo.Name, "commandInfo.Name");
            CommandTypes commandType = commandInfo.CommandType;
            if (commandType <= CommandTypes.ExternalScript)
            {
                switch (commandType)
                {
                    case CommandTypes.Alias:
                        return true;

                    case CommandTypes.Function:
                    case CommandTypes.Filter:
                        goto Label_006C;

                    case (CommandTypes.Function | CommandTypes.Alias):
                        return flag;

                    case CommandTypes.Cmdlet:
                        return true;

                    case CommandTypes.ExternalScript:
                    {
                        ExternalScriptInfo script = commandInfo as ExternalScriptInfo;
                        if (script == null)
                        {
                            reason = PSTraceSource.NewArgumentException("scriptInfo");
                            return flag;
                        }
                        return this.CheckPolicy(script, host, out reason);
                    }
                }
                return flag;
            }
            switch (commandType)
            {
                case CommandTypes.Application:
                    return true;

                case CommandTypes.Script:
                    return true;

                case CommandTypes.Workflow:
                    break;

                default:
                    return flag;
            }
        Label_006C:
            return true;
        }

        private void TrustPublisher(System.Management.Automation.Signature signature)
        {
            X509Certificate2 signerCertificate = signature.SignerCertificate;
            X509Store store = new X509Store(StoreName.TrustedPublisher);
            try
            {
                store.Open(OpenFlags.ReadWrite);
                store.Add(signerCertificate);
            }
            finally
            {
                store.Close();
            }
        }

        private void UntrustPublisher(System.Management.Automation.Signature signature)
        {
            X509Certificate2 signerCertificate = signature.SignerCertificate;
            X509Store store = new X509Store(StoreName.Disallowed);
            X509Store store2 = new X509Store(StoreName.TrustedPublisher);
            try
            {
                store2.Open(OpenFlags.ReadWrite);
                store2.Remove(signerCertificate);
            }
            finally
            {
                store2.Close();
            }
            try
            {
                store.Open(OpenFlags.ReadWrite);
                store.Add(signerCertificate);
            }
            finally
            {
                store.Close();
            }
        }

        internal enum RunPromptDecision
        {
            NeverRun,
            DoNotRun,
            RunOnce,
            AlwaysRun,
            Suspend
        }
    }
}

