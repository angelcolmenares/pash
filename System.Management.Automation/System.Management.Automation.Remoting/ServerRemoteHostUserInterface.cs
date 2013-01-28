namespace System.Management.Automation.Remoting
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal.Host;
    using System.Security;

    internal class ServerRemoteHostUserInterface : PSHostUserInterface, IHostUISupportsMultipleChoiceSelection
    {
        private PSHostRawUserInterface _rawUI;
        private System.Management.Automation.Remoting.ServerRemoteHost _remoteHost;
        private ServerMethodExecutor _serverMethodExecutor;

        internal ServerRemoteHostUserInterface(System.Management.Automation.Remoting.ServerRemoteHost remoteHost)
        {
            this._remoteHost = remoteHost;
            this._serverMethodExecutor = remoteHost.ServerMethodExecutor;
            this._rawUI = remoteHost.HostInfo.IsHostRawUINull ? null : new ServerRemoteHostRawUserInterface(this);
			AddHostDefaultData();
        }

        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            Dictionary<string, PSObject> dictionary = this._serverMethodExecutor.ExecuteMethod<Dictionary<string, PSObject>>(RemoteHostMethodId.Prompt, new object[] { caption, message, descriptions });
            foreach (FieldDescription description in descriptions)
            {
                PSObject obj2;
                object obj3;
                Type fieldType = InternalHostUserInterface.GetFieldType(description);
                if (((fieldType != null) && dictionary.TryGetValue(description.Name, out obj2)) && LanguagePrimitives.TryConvertTo(obj2, fieldType, CultureInfo.InvariantCulture, out obj3))
                {
                    if (obj3 != null)
                    {
                        dictionary[description.Name] = PSObject.AsPSObject(obj3);
                    }
                    else
                    {
                        dictionary[description.Name] = null;
                    }
                }
            }
            return dictionary;
        }

		void AddHostDefaultData ()
		{
			var hostData = this._remoteHost.HostInfo.HostDefaultData;
			if (!hostData.HasValue (HostDefaultDataId.WindowPosition)) {
				hostData.SetValue (HostDefaultDataId.WindowPosition, new Coordinates (0, 0));
			}
			if (!hostData.HasValue (HostDefaultDataId.MaxWindowSize)) {
				hostData.SetValue (HostDefaultDataId.MaxWindowSize, new Size (1080, 640));
			}
			if (!hostData.HasValue (HostDefaultDataId.WindowSize)) {
				hostData.SetValue (HostDefaultDataId.WindowSize, new Size (1080, 640));
			}
			if (!hostData.HasValue (HostDefaultDataId.CursorPosition)) {
				hostData.SetValue (HostDefaultDataId.WindowSize, new Coordinates (0, 0));
			}
		}

        public Collection<int> PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, IEnumerable<int> defaultChoices)
        {
            return this._serverMethodExecutor.ExecuteMethod<Collection<int>>(RemoteHostMethodId.PromptForChoiceMultipleSelection, new object[] { caption, message, choices, defaultChoices });
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            return this._serverMethodExecutor.ExecuteMethod<int>(RemoteHostMethodId.PromptForChoice, new object[] { caption, message, choices, defaultChoice });
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            return this._serverMethodExecutor.ExecuteMethod<PSCredential>(RemoteHostMethodId.PromptForCredential1, new object[] { caption, message, userName, targetName });
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            return this._serverMethodExecutor.ExecuteMethod<PSCredential>(RemoteHostMethodId.PromptForCredential2, new object[] { caption, message, userName, targetName, allowedCredentialTypes, options });
        }

        public override string ReadLine()
        {
            return this._serverMethodExecutor.ExecuteMethod<string>(RemoteHostMethodId.ReadLine);
        }

        public override SecureString ReadLineAsSecureString()
        {
            return this._serverMethodExecutor.ExecuteMethod<SecureString>(RemoteHostMethodId.ReadLineAsSecureString);
        }

        public override void Write(string message)
        {
            this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.Write1, new object[] { message });
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message)
        {
            this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.Write2, new object[] { foregroundColor, backgroundColor, message });
        }

        public override void WriteDebugLine(string message)
        {
            this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteDebugLine, new object[] { message });
        }

        public override void WriteErrorLine(string message)
        {
            this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteErrorLine, new object[] { message });
        }

        public override void WriteLine()
        {
            this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteLine1);
        }

        public override void WriteLine(string message)
        {
            this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteLine2, new object[] { message });
        }

        public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message)
        {
            this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteLine3, new object[] { foregroundColor, backgroundColor, message });
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteProgress, new object[] { sourceId, record });
        }

        public override void WriteVerboseLine(string message)
        {
            this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteVerboseLine, new object[] { message });
        }

        public override void WriteWarningLine(string message)
        {
            this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteWarningLine, new object[] { message });
        }

        public override PSHostRawUserInterface RawUI
        {
            get
            {
                return this._rawUI;
            }
        }

        internal System.Management.Automation.Remoting.ServerRemoteHost ServerRemoteHost
        {
            get
            {
                return this._remoteHost;
            }
        }
    }
}

