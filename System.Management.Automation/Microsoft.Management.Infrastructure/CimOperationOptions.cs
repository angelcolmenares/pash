using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Management.Infrastructure
{
    public delegate CimResponseType PromptUserCallback(string message, CimPromptType promptType);
    public delegate CimResponseType WriteErrorCallback(CimInstance cimError);
    public delegate void WriteMessageCallback(uint channel, string message);
    public delegate void WriteProgressCallback(string activity, string currentOperation, string statusDescription, int percentageCompleted, int secondsRemaining);


    public class CimOperationOptions : IDisposable, ICloneable
    {
        private bool _disposed;
        private readonly OperationCallbacks _operationCallback;
        private PromptUserCallback _promptUserCallback;
        private WriteErrorCallback _writeErrorCallback;
        private WriteMessageCallback _writeMessageCallback;
        private WriteProgressCallback _writeProgressCallback;

        public CimOperationOptions()
            : this(false)
        {
        }

        public CimOperationOptions(CimOperationOptions optionsToClone)
        {
            if (optionsToClone == null)
            {
                throw new ArgumentNullException("optionsToClone");
            }
            this._operationCallback = optionsToClone.GetOperationCallbacks();
            this._writeMessageCallback = optionsToClone._writeMessageCallback;
            this._writeProgressCallback = optionsToClone._writeProgressCallback;
            this._writeErrorCallback = optionsToClone._writeErrorCallback;
            this._promptUserCallback = optionsToClone._promptUserCallback;
        }

        private OperationCallbacks GetOperationCallbacks()
        {
            return _operationCallback;
        }

        public CimOperationOptions(bool mustUnderstand)
        {
            this._operationCallback = new OperationCallbacks();
            this._writeMessageCallback = null;
            this._writeProgressCallback = null;
            this._writeErrorCallback = null;
            this._promptUserCallback = null;
        }


        public CancellationToken? CancellationToken
        {
            get;
            set;
        }

        public bool ClassNamesOnly
        {
            get;
            set;
        }

        public bool EnableMethodResultStreaming
        {
            get;
            set;
        }

        public CimOperationFlags Flags
        {
            get;
            set;
        }

        public CimCallbackMode PromptUserMode
        {
            get;
            set;
        }

        public bool IsDisposed
        {
            get
            {
                return this._disposed;
            }
        }

        public bool KeysOnly
        {
            get;
            set;
        }

        public bool ReportOperationStarted
		{
			get
			{
				return (this.Flags & CimOperationFlags.ReportOperationStarted) == CimOperationFlags.ReportOperationStarted;
			}
		}

        public Uri ResourceUri
        {
            get;
            set;
        }

        public Uri ResourceUriPrefix
        {
            get;
            set;
        }


        public bool ShortenLifetimeOfResults { get; set; }

        public TimeSpan Timeout
        {
            get;
            set;
        }

        public bool UseMachineId
        {
            get;
            set;
        }

        public void SetPromptUserRegularMode(CimCallbackMode callbackMode, bool automaticConfirmation)
        {
            this.AssertNotDisposed();
            //CimException.ThrowIfMiResultFailure(OperationOptionsMethods.SetPromptUserRegularMode(this.OperationOptionsHandleOnDemand, (MiCallbackMode)callbackMode, automaticConfirmation));
        }

		internal OperationOptionsHandle OperationOptionsHandle
		{
			get;set;
		}

        object ICloneable.Clone()
        {
            return new CimOperationOptions(this);
        }

        internal void WriteErrorCallbackInternal(OperationCallbackProcessingContext callbackProcessingContext, OperationHandle operationHandle, InstanceHandle instanceHandle, out MIResponseType response)
        {
            response = MIResponseType.MIResponseTypeYes;
            if (this._writeErrorCallback != null)
            {
                CimInstance cimInstance = null;
                using (cimInstance)
                {
                    if (instanceHandle != null)
                    {
						cimInstance = new CimInstance(instanceHandle, null);
                        //CimAsyncCallbacksReceiverBase managedOperationContext = (CimAsyncCallbacksReceiverBase)callbackProcessingContext.ManagedOperationContext;
                        CimResponseType userResponse = CimResponseType.None;
                        //managedOperationContext.CallIntoUserCallback(callbackProcessingContext, () => userResponse = this._writeErrorCallback(cimInstance), false, false);
                        response = (MIResponseType)userResponse;
                    }
                }
            }
        }

        internal void WriteMessageCallbackInternal(OperationCallbackProcessingContext callbackProcessingContext, OperationHandle operationHandle, uint channel, string message)
        {
            Action userCallback = null;
            if (this._writeMessageCallback != null)
            {
                //CimAsyncCallbacksReceiverBase managedOperationContext = (CimAsyncCallbacksReceiverBase)callbackProcessingContext.ManagedOperationContext;
                if (userCallback == null)
                {
                    userCallback = () => this._writeMessageCallback(channel, message);
                }
                //managedOperationContext.CallIntoUserCallback(callbackProcessingContext, userCallback, false, false);
            }
        }

        private void WriteProgressCallbackInternal(OperationCallbackProcessingContext callbackProcessingContext, OperationHandle operationHandle, string activity, string currentOperation, string statusDescription, int percentageCompleted, int secondsRemaining)
        {
            Action userCallback = null;
            if (this._writeProgressCallback != null)
            {
                //CimAsyncCallbacksReceiverBase managedOperationContext = (CimAsyncCallbacksReceiverBase)callbackProcessingContext.ManagedOperationContext;
                if (userCallback == null)
                {
                    userCallback = () => this._writeProgressCallback(activity, currentOperation, statusDescription, percentageCompleted, secondsRemaining);
                }
                //managedOperationContext.CallIntoUserCallback(callbackProcessingContext, userCallback, false, false);
            }
        }

        internal void PromptUserCallbackInternal(OperationCallbackProcessingContext callbackProcessingContext, OperationHandle operationHandle, string message, MiPromptType promptType, out MIResponseType response)
        {
            response = MIResponseType.MIResponseTypeYes;
            if (this._promptUserCallback != null)
            {
                //CimAsyncCallbacksReceiverBase managedOperationContext = (CimAsyncCallbacksReceiverBase)callbackProcessingContext.ManagedOperationContext;
                CimResponseType userResponse = CimResponseType.None;
                //managedOperationContext.CallIntoUserCallback(callbackProcessingContext, () => userResponse = this._promptUserCallback(message, (CimPromptType)promptType), false, false);
                response = (MIResponseType)userResponse;
            }
        }

        internal OperationCallbacks OperationCallback
        {
            get
            {
                this.AssertNotDisposed();
                return this._operationCallback;
            }
        }

        public WriteErrorCallback WriteError
        {
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.AssertNotDisposed();
                this._writeErrorCallback = value;
                this.OperationCallback.WriteErrorCallback = new OperationCallbacks.WriteErrorCallbackDelegate(this.WriteErrorCallbackInternal);
            }
        }

        public CimCallbackMode WriteErrorMode
        {
            get;
            set;
        }

        public WriteMessageCallback WriteMessage
        {
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.AssertNotDisposed();
                this._writeMessageCallback = value;
                this.OperationCallback.WriteMessageCallback = new OperationCallbacks.WriteMessageCallbackDelegate(this.WriteMessageCallbackInternal);
            }
        }


        public WriteProgressCallback WriteProgress
        {
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.AssertNotDisposed();
                this._writeProgressCallback = value;
                this.OperationCallback.WriteProgressCallback = new OperationCallbacks.WriteProgressCallbackDelegate(this.WriteProgressCallbackInternal);
            }
        }

        public PromptUserCallback PromptUser
        {
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.AssertNotDisposed();
                this._promptUserCallback = value;
                this.OperationCallback.PromptUserCallback = new OperationCallbacks.PromptUserCallbackDelegate(this.PromptUserCallbackInternal);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _disposed = true;
        }


        internal void AssertNotDisposed()
        {
            if (!this._disposed)
            {
                return;
            }
            else
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        internal void SetCustomOption(string p1, string str, bool p2)
        {
            
        }

        internal void EnableChannel(int p)
        {
            throw new NotImplementedException();
        }

        internal void DisableChannel(int p)
        {
            throw new NotImplementedException();
        }

        internal void SetOption(string p1, uint p2)
        {
            throw new NotImplementedException();
        }

        internal void SetCustomOption(string optionName, object cim, CimType cimType, bool p)
        {
            throw new NotImplementedException();
        }
    }
}
