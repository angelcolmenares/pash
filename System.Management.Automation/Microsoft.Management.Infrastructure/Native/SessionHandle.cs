using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Security.Principal;
using Microsoft.Management.Infrastructure.Internal;

namespace Microsoft.Management.Infrastructure.Native
{
	internal class SessionHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		private IntPtr handleToSecurityTokenUsedForCreation;

		internal SessionHandle (IntPtr handle) : base(true)
		{
			try {
				this.handle = handle;
				IntPtr currentSecurityToken = Helpers.GetCurrentSecurityToken ();
				this.handleToSecurityTokenUsedForCreation = currentSecurityToken;
			} catch (Exception ex) {

			}
		}

		[Conditional("DEBUG")]
		internal void AssertValidInternalState()
		{
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		protected override bool ReleaseHandle()
		{
			return (this.ReleaseHandleAsynchronously(null) == MiResult.OK);
		}

		internal unsafe MiResult ReleaseHandleAsynchronously(SessionHandle.OnSessionHandleReleasedDelegate completionCallback)
		{
			/*
			_MI_Result _MIResult;
			base.SetHandleAsInvalid();
			_MI_Session* _MISessionPointer = (_MI_Session*)((void*)this.handle);
			this.handle = IntPtr.Zero;
			SessionHandle_ReleaseHandle_CallbackWrapper* sessionHandleReleaseHandleCallbackWrapperPointer = (SessionHandle_ReleaseHandle_CallbackWrapper*)<Module>.Microsoft.Management.Infrastructure.Native.MI_CLI_malloc_core((long)24);
			<Module>.Microsoft.Management.Infrastructure.Native.SessionHandle_ReleaseHandle_CallbackWrapper_Initialize(sessionHandleReleaseHandleCallbackWrapperPointer, _MISessionPointer, completionCallback);
			WindowsImpersonationContext windowsImpersonationContext = WindowsIdentity.Impersonate(this.handleToSecurityTokenUsedForCreation);
			_MI_Result _MIResult1 = (_MI_Result)<Module>.?A0xf16864c4.MI_Session_Close(_MISessionPointer, sessionHandleReleaseHandleCallbackWrapperPointer, <Module>.__unep@?SessionHandle_OnReleaseHandleCompleted@Native@Infrastructure@Management@Microsoft@@$$FYAXPEAX@Z);
			if (<Module>.CloseHandle((void*)this.handleToSecurityTokenUsedForCreation) == null)
			{
				if (_MIResult1 == 0)
				{
					_MIResult = 1;
				}
				else
				{
					_MIResult = _MIResult1;
				}
				_MIResult1 = _MIResult;
			}
			else
			{
				this.handleToSecurityTokenUsedForCreation = IntPtr.Zero;
			}
			windowsImpersonationContext.Undo();
			return (MiResult)_MIResult1;
			*/
			return MiResult.OK;
		}

		internal unsafe MiResult ReleaseHandleSynchronously()
		{
			/*
			_MI_Result _MIResult;
			base.SetHandleAsInvalid();
			_MI_Session* _MISessionPointer = (_MI_Session*)((void*)this.handle);
			this.handle = IntPtr.Zero;
			WindowsImpersonationContext windowsImpersonationContext = WindowsIdentity.Impersonate(this.handleToSecurityTokenUsedForCreation);
			_MI_Result _MIResult1 = (_MI_Result)<Module>.?A0xf16864c4.MI_Session_Close(_MISessionPointer, (long)0, (long)0);
			_MISessionPointer;
			0;
			<Module>.free(_MISessionPointer);
			if (<Module>.CloseHandle((void*)this.handleToSecurityTokenUsedForCreation) == null)
			{
				if (_MIResult1 == 0)
				{
					_MIResult = 1;
				}
				else
				{
					_MIResult = _MIResult1;
				}
				_MIResult1 = _MIResult;
			}
			else
			{
				this.handleToSecurityTokenUsedForCreation = IntPtr.Zero;
			}
			windowsImpersonationContext.Undo();
			return (MiResult)_MIResult1;
			*/
			return MiResult.OK;
		}

		internal delegate void OnSessionHandleReleasedDelegate();


	}
}