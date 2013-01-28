namespace System.Management.Automation.Remoting
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal.Host;
    using System.Reflection;
    using System.Security;

    internal class RemoteHostCall
    {
        private long _callId;
        private string _computerName;
        private RemoteHostMethodId _methodId;
        private RemoteHostMethodInfo _methodInfo;
        private object[] _parameters;

        internal RemoteHostCall(long callId, RemoteHostMethodId methodId, object[] parameters)
        {
            this._callId = callId;
            this._methodId = methodId;
            this._parameters = parameters;
            this._methodInfo = RemoteHostMethodInfo.LookUp(methodId);
        }

        private RemoteHostCall ConstructWarningMessageForGetBufferContents(string computerName)
        {
            string str = PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.RemoteHostGetBufferContents, new object[] { computerName.ToUpper(CultureInfo.CurrentCulture) });
            return new RemoteHostCall(-100L, RemoteHostMethodId.WriteWarningLine, new object[] { str });
        }

        private RemoteHostCall ConstructWarningMessageForSecureString(string computerName, string resourceString)
        {
            string str = PSRemotingErrorInvariants.FormatResourceString(resourceString, new object[] { computerName.ToUpper(CultureInfo.CurrentCulture) });
            return new RemoteHostCall(-100L, RemoteHostMethodId.WriteWarningLine, new object[] { str });
        }

        internal static RemoteHostCall Decode(PSObject data)
        {
            long propertyValue = RemotingDecoder.GetPropertyValue<long>(data, "ci");
            PSObject parametersPSObject = RemotingDecoder.GetPropertyValue<PSObject>(data, "mp");
            RemoteHostMethodId methodId = RemotingDecoder.GetPropertyValue<RemoteHostMethodId>(data, "mi");
            RemoteHostMethodInfo info = RemoteHostMethodInfo.LookUp(methodId);
			object[] objects = DecodeParameters(parametersPSObject, info.ParameterTypes);
            return new RemoteHostCall(propertyValue, methodId, objects);
        }

        private static object[] DecodeParameters(PSObject parametersPSObject, Type[] parameterTypes)
        {
            ArrayList baseObject = (ArrayList) parametersPSObject.BaseObject;
            List<object> list2 = new List<object>();
            for (int i = 0; i < baseObject.Count; i++)
            {
                object item = (baseObject[i] == null) ? null : RemoteHostEncoder.DecodeObject(baseObject[i], parameterTypes[i]);
                list2.Add(item);
            }
            return list2.ToArray();
        }

        internal PSObject Encode()
        {
            PSObject obj2 = RemotingEncoder.CreateEmptyPSObject();
            PSObject obj3 = EncodeParameters(this._parameters);
            obj2.Properties.Add(new PSNoteProperty("ci", this._callId));
            obj2.Properties.Add(new PSNoteProperty("mi", this._methodId));
            obj2.Properties.Add(new PSNoteProperty("mp", obj3));
            return obj2;
        }

        private static PSObject EncodeParameters(object[] parameters)
        {
            ArrayList list = new ArrayList();
            for (int i = 0; i < parameters.Length; i++)
            {
                object obj2 = (parameters[i] == null) ? null : RemoteHostEncoder.EncodeObject(parameters[i]);
                list.Add(obj2);
            }
            return new PSObject(list);
        }

        internal RemoteHostResponse ExecuteNonVoidMethod(PSHost clientHost)
        {
            if (clientHost == null)
            {
                throw RemoteHostExceptions.NewNullClientHostException();
            }
            object instance = this.SelectTargetObject(clientHost);
            return this.ExecuteNonVoidMethodOnObject(instance);
        }

        private RemoteHostResponse ExecuteNonVoidMethodOnObject(object instance)
        {
            Exception innerException = null;
            object returnValue = null;
            try
            {
                if (this._methodId == RemoteHostMethodId.GetBufferContents)
                {
                    throw new PSRemotingDataStructureException(RemotingErrorIdStrings.RemoteHostGetBufferContents, new object[] { this._computerName.ToUpper(CultureInfo.CurrentCulture) });
                }
                returnValue = this.MyMethodBase.Invoke(instance, this._parameters);
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
                innerException = exception2.InnerException;
            }
            return new RemoteHostResponse(this._callId, this._methodId, returnValue, innerException);
        }

        internal void ExecuteVoidMethod(PSHost clientHost)
        {
            if (clientHost != null)
            {
                RemoteRunspace remoteRunspaceToClose = null;
                if (this.IsSetShouldExitOrPopRunspace)
                {
                    remoteRunspaceToClose = this.GetRemoteRunspaceToClose(clientHost);
                }
                try
                {
                    object obj2 = this.SelectTargetObject(clientHost);
                    this.MyMethodBase.Invoke(obj2, this._parameters);
                }
                finally
                {
                    if (remoteRunspaceToClose != null)
                    {
                        remoteRunspaceToClose.Close();
                    }
                }
            }
        }

        private RemoteRunspace GetRemoteRunspaceToClose(PSHost clientHost)
        {
            IHostSupportsInteractiveSession session = clientHost as IHostSupportsInteractiveSession;
            if ((session != null) && session.IsRunspacePushed)
            {
                RemoteRunspace runspace = session.Runspace as RemoteRunspace;
                if ((runspace != null) && runspace.ShouldCloseOnPop)
                {
                    return runspace;
                }
            }
            return null;
        }

        private string ModifyCaption(string caption)
        {
            string str = CredUI.PromptForCredential_DefaultCaption;
            if (!caption.Equals(str, StringComparison.OrdinalIgnoreCase))
            {
                return PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.RemoteHostPromptForCredentialModifiedCaption, new object[] { caption });
            }
            return caption;
        }

        private string ModifyMessage(string message, string computerName)
        {
            return PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.RemoteHostPromptForCredentialModifiedMessage, new object[] { computerName.ToUpper(CultureInfo.CurrentCulture), message });
        }

        internal Collection<RemoteHostCall> PerformSecurityChecksOnHostMessage(string computerName)
        {
            this._computerName = computerName;
            Collection<RemoteHostCall> collection = new Collection<RemoteHostCall>();
            if ((this._methodId == RemoteHostMethodId.PromptForCredential1) || (this._methodId == RemoteHostMethodId.PromptForCredential2))
            {
                string str = this.ModifyCaption((string) this._parameters[0]);
                string str2 = this.ModifyMessage((string) this._parameters[1], computerName);
                this._parameters[0] = str;
                this._parameters[1] = str2;
                return collection;
            }
            if (this._methodId == RemoteHostMethodId.Prompt)
            {
                if (this._parameters.Length == 3)
                {
                    Collection<FieldDescription> collection2 = (Collection<FieldDescription>) this._parameters[2];
                    bool flag = false;
                    foreach (FieldDescription description in collection2)
                    {
                        description.IsFromRemoteHost = true;
                        Type fieldType = InternalHostUserInterface.GetFieldType(description);
                        if (fieldType != null)
                        {
                            if (fieldType == typeof(PSCredential))
                            {
                                flag = true;
                                description.ModifiedByRemotingProtocol = true;
                            }
                            else if (fieldType == typeof(SecureString))
                            {
                                collection.Add(this.ConstructWarningMessageForSecureString(computerName, RemotingErrorIdStrings.RemoteHostPromptSecureStringPrompt));
                            }
                        }
                    }
                    if (flag)
                    {
                        string str3 = this.ModifyCaption((string) this._parameters[0]);
                        string str4 = this.ModifyMessage((string) this._parameters[1], computerName);
                        this._parameters[0] = str3;
                        this._parameters[1] = str4;
                    }
                }
                return collection;
            }
            if (this._methodId == RemoteHostMethodId.ReadLineAsSecureString)
            {
                collection.Add(this.ConstructWarningMessageForSecureString(computerName, RemotingErrorIdStrings.RemoteHostReadLineAsSecureStringPrompt));
                return collection;
            }
            if (this._methodId == RemoteHostMethodId.GetBufferContents)
            {
                collection.Add(this.ConstructWarningMessageForGetBufferContents(computerName));
            }
            return collection;
        }

        private object SelectTargetObject(PSHost host)
        {
            if ((host == null) || (host.UI == null))
            {
                return null;
            }
            if (this._methodInfo.InterfaceType == typeof(PSHost))
            {
                return host;
            }
            if (this._methodInfo.InterfaceType == typeof(IHostSupportsInteractiveSession))
            {
                return host;
            }
            if (this._methodInfo.InterfaceType == typeof(PSHostUserInterface))
            {
                return host.UI;
            }
            if (this._methodInfo.InterfaceType == typeof(IHostUISupportsMultipleChoiceSelection))
            {
                return host.UI;
            }
            if (this._methodInfo.InterfaceType != typeof(PSHostRawUserInterface))
            {
                throw RemoteHostExceptions.NewUnknownTargetClassException(this._methodInfo.InterfaceType.ToString());
            }
            return host.UI.RawUI;
        }

        internal long CallId
        {
            get
            {
                return this._callId;
            }
        }

        internal bool IsSetShouldExit
        {
            get
            {
                return (this._methodId == RemoteHostMethodId.SetShouldExit);
            }
        }

        internal bool IsSetShouldExitOrPopRunspace
        {
            get
            {
                if (this._methodId != RemoteHostMethodId.SetShouldExit)
                {
                    return (this._methodId == RemoteHostMethodId.PopRunspace);
                }
                return true;
            }
        }

        internal bool IsVoidMethod
        {
            get
            {
                return (this._methodInfo.ReturnType == typeof(void));
            }
        }

        internal RemoteHostMethodId MethodId
        {
            get
            {
                return this._methodId;
            }
        }

        internal string MethodName
        {
            get
            {
                return this._methodInfo.Name;
            }
        }

        private MethodBase MyMethodBase
        {
            get
            {
                return this._methodInfo.InterfaceType.GetMethod(this._methodInfo.Name, this._methodInfo.ParameterTypes);
            }
        }

        internal object[] Parameters
        {
            get
            {
                return this._parameters;
            }
        }
    }
}

