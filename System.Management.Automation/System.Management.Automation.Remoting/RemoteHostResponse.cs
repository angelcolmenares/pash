namespace System.Management.Automation.Remoting
{
    using System;
    using System.Management.Automation;

    internal class RemoteHostResponse
    {
        private long _callId;
        private Exception _exception;
        private RemoteHostMethodId _methodId;
        private object _returnValue;

        internal RemoteHostResponse(long callId, RemoteHostMethodId methodId, object returnValue, Exception exception)
        {
            this._callId = callId;
            this._methodId = methodId;
            this._returnValue = returnValue;
            this._exception = exception;
        }

        internal static RemoteHostResponse Decode(PSObject data)
        {
            long propertyValue = RemotingDecoder.GetPropertyValue<long>(data, "ci");
            RemoteHostMethodId methodId = RemotingDecoder.GetPropertyValue<RemoteHostMethodId>(data, "mi");
            RemoteHostMethodInfo info = RemoteHostMethodInfo.LookUp(methodId);
            object returnValue = DecodeReturnValue(data, info.ReturnType);
            return new RemoteHostResponse(propertyValue, methodId, returnValue, DecodeException(data));
        }

        private static Exception DecodeException(PSObject psObject)
        {
            object obj2 = RemoteHostEncoder.DecodePropertyValue(psObject, "me", typeof(Exception));
            if (obj2 == null)
            {
                return null;
            }
            if (!(obj2 is Exception))
            {
                throw RemoteHostExceptions.NewDecodingFailedException();
            }
            return (Exception) obj2;
        }

        private static object DecodeReturnValue(PSObject psObject, Type returnType)
        {
            return RemoteHostEncoder.DecodePropertyValue(psObject, "mr", returnType);
        }

        internal PSObject Encode()
        {
            PSObject psObject = RemotingEncoder.CreateEmptyPSObject();
            EncodeAndAddReturnValue(psObject, this._returnValue);
            EncodeAndAddException(psObject, this._exception);
            psObject.Properties.Add(new PSNoteProperty("ci", this._callId));
            psObject.Properties.Add(new PSNoteProperty("mi", this._methodId));
            return psObject;
        }

        private static void EncodeAndAddException(PSObject psObject, Exception exception)
        {
            RemoteHostEncoder.EncodeAndAddAsProperty(psObject, "me", exception);
        }

        private static void EncodeAndAddReturnValue(PSObject psObject, object returnValue)
        {
            if (returnValue != null)
            {
                RemoteHostEncoder.EncodeAndAddAsProperty(psObject, "mr", returnValue);
            }
        }

        internal object SimulateExecution()
        {
            if (this._exception != null)
            {
                throw this._exception;
            }
            return this._returnValue;
        }

        internal long CallId
        {
            get
            {
                return this._callId;
            }
        }
    }
}

