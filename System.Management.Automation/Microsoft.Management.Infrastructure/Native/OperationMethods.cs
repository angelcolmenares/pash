using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.Management.Infrastructure.Native
{
	internal class OperationMethods
	{
		private OperationMethods()
		{
		}

		[SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId="System.Runtime.InteropServices.SafeHandle.DangerousGetHandle", Justification="C# layer internally manages the lifetime of OperationHandle + have to do this to call inline methods")]
		internal static MiResult Cancel(OperationHandle operationHandle, MiCancellationReason cancellationReason)
		{
			return MiResult.OK; //TODO: (MiResult)<Module>.?A0xf5a90918.MI_Operation_Cancel((void*)operationHandle.DangerousGetHandle(), (_MI_CancellationReason)cancellationReason);
		}

		[SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId="System.Runtime.InteropServices.SafeHandle.DangerousGetHandle", Justification="C# layer internally manages the lifetime of OperationHandle + have to do this to call inline methods")]
		internal static unsafe MiResult GetClass(OperationHandle operationHandle, out ClassHandle classHandle, out bool moreResults, out MiResult result, out string errorMessage, out InstanceHandle completionDetails)
		{
			result = MiResult.OK;
			classHandle = null;
			completionDetails = null;
			errorMessage = "";
			moreResults = false;
			return MiResult.OK;
			/*
			int num;
			classHandle = null;
			moreResults = false;
			result = MiResult.OK;
			errorMessage = null;
			completionDetails = null;
			_MI_Class modopt(System.Runtime.CompilerServices.IsConst)* _MIClass modopt(System.Runtime.CompilerServices.IsConst)Pointer = (_MI_Class modopt(System.Runtime.CompilerServices.IsConst)*)((long)0);
			byte num1 = 0;
			_MI_Result _MIResult = 0;
			UInt16 modopt(System.Runtime.CompilerServices.IsConst)* uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer = (UInt16 modopt(System.Runtime.CompilerServices.IsConst)*)((long)0);
			_MI_Instance modopt(System.Runtime.CompilerServices.IsConst)* _MIInstance modopt(System.Runtime.CompilerServices.IsConst)Pointer = (_MI_Instance modopt(System.Runtime.CompilerServices.IsConst)*)((long)0);
			_MI_Result modopt(System.Runtime.CompilerServices.CallConvCdecl) _MIResult modopt(System.Runtime.CompilerServices.CallConvCdecl) = (_MI_Result)<Module>.?A0xf5a90918.MI_Operation_GetClass((void*)operationHandle.DangerousGetHandle(), ref (_MI_Class modopt(System.Runtime.CompilerServices.IsConst)*)((long)0), ref num1, (_MI_Result*)(&_MIResult), ref (UInt16 modopt(System.Runtime.CompilerServices.IsConst)*)((long)0), ref (_MI_Instance modopt(System.Runtime.CompilerServices.IsConst)*)((long)0));
			if ((_MI_Class modopt(System.Runtime.CompilerServices.IsConst)*)((long)0) != null)
			{
				IntPtr intPtr = (IntPtr)_MIClass modopt(System.Runtime.CompilerServices.IsConst)Pointer;
				classHandle = new ClassHandle(intPtr, false);
			}
			if (0 != num1)
			{
				num = 1;
			}
			else
			{
				num = 0;
			}
			int num2 = num;
			moreResults = num2;
			result = (MiResult)_MIResult;
			if (_MIInstance modopt(System.Runtime.CompilerServices.IsConst)Pointer != null)
			{
				IntPtr intPtr1 = (IntPtr)_MIInstance modopt(System.Runtime.CompilerServices.IsConst)Pointer;
				completionDetails = new InstanceHandle(intPtr1, false);
			}
			if (uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer != null)
			{
				IntPtr intPtr2 = (IntPtr)uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer;
				errorMessage = Marshal.PtrToStringUni(intPtr2);
			}
			return (MiResult)_MIResult modopt(System.Runtime.CompilerServices.CallConvCdecl);
			*/
		}

		[SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId="System.Runtime.InteropServices.SafeHandle.DangerousGetHandle", Justification="C# layer internally manages the lifetime of OperationHandle + have to do this to call inline methods")]
		internal static unsafe MiResult GetIndication(OperationHandle operationHandle, out InstanceHandle instanceHandle, out string bookmark, out string machineID, out bool moreResults, out MiResult result, out string errorMessage, out InstanceHandle completionDetails)
		{
			moreResults = false;
			machineID = System.Net.Dns.GetHostName ();
			errorMessage = "";
			bookmark = null;
			instanceHandle = null;
			result = MiResult.OK;
			instanceHandle = null;
			completionDetails = null;
			return MiResult.OK;
			/*
			int num;
			instanceHandle = null;
			moreResults = false;
			result = MiResult.OK;
			errorMessage = null;
			bookmark = null;
			machineID = null;
			completionDetails = null;
			_MI_Instance modopt(System.Runtime.CompilerServices.IsConst)* _MIInstance modopt(System.Runtime.CompilerServices.IsConst)Pointer = (_MI_Instance modopt(System.Runtime.CompilerServices.IsConst)*)((long)0);
			UInt16 modopt(System.Runtime.CompilerServices.IsConst)* uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer = (UInt16 modopt(System.Runtime.CompilerServices.IsConst)*)((long)0);
			UInt16 modopt(System.Runtime.CompilerServices.IsConst)* uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer1 = (UInt16 modopt(System.Runtime.CompilerServices.IsConst)*)((long)0);
			byte num1 = 0;
			_MI_Result _MIResult = 0;
			UInt16 modopt(System.Runtime.CompilerServices.IsConst)* uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer2 = (UInt16 modopt(System.Runtime.CompilerServices.IsConst)*)((long)0);
			_MI_Instance modopt(System.Runtime.CompilerServices.IsConst)* _MIInstance modopt(System.Runtime.CompilerServices.IsConst)Pointer1 = (_MI_Instance modopt(System.Runtime.CompilerServices.IsConst)*)((long)0);
			_MI_Result modopt(System.Runtime.CompilerServices.CallConvCdecl) _MIResult modopt(System.Runtime.CompilerServices.CallConvCdecl) = (_MI_Result)<Module>.?A0xf5a90918.MI_Operation_GetIndication((void*)operationHandle.DangerousGetHandle(), ref (_MI_Instance modopt(System.Runtime.CompilerServices.IsConst)*)((long)0), ref (UInt16 modopt(System.Runtime.CompilerServices.IsConst)*)((long)0), ref (UInt16 modopt(System.Runtime.CompilerServices.IsConst)*)((long)0), ref num1, (_MI_Result*)(&_MIResult), ref (UInt16 modopt(System.Runtime.CompilerServices.IsConst)*)((long)0), ref (_MI_Instance modopt(System.Runtime.CompilerServices.IsConst)*)((long)0));
			if ((_MI_Instance modopt(System.Runtime.CompilerServices.IsConst)*)((long)0) != null)
			{
				IntPtr intPtr = (IntPtr)_MIInstance modopt(System.Runtime.CompilerServices.IsConst)Pointer;
				instanceHandle = new InstanceHandle(intPtr, false);
			}
			if (0 != num1)
			{
				num = 1;
			}
			else
			{
				num = 0;
			}
			int num2 = num;
			moreResults = num2;
			result = (MiResult)_MIResult;
			if (_MIInstance modopt(System.Runtime.CompilerServices.IsConst)Pointer1 != null)
			{
				IntPtr intPtr1 = (IntPtr)_MIInstance modopt(System.Runtime.CompilerServices.IsConst)Pointer1;
				completionDetails = new InstanceHandle(intPtr1, false);
			}
			if (uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer2 != null)
			{
				IntPtr intPtr2 = (IntPtr)uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer2;
				errorMessage = Marshal.PtrToStringUni(intPtr2);
			}
			if (uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer != null)
			{
				IntPtr intPtr3 = (IntPtr)uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer;
				bookmark = Marshal.PtrToStringUni(intPtr3);
			}
			if (uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer1 != null)
			{
				IntPtr intPtr4 = (IntPtr)uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer1;
				machineID = Marshal.PtrToStringUni(intPtr4);
			}
			return (MiResult)_MIResult modopt(System.Runtime.CompilerServices.CallConvCdecl);
			*/
		}

		[SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId="System.Runtime.InteropServices.SafeHandle.DangerousGetHandle", Justification="C# layer internally manages the lifetime of OperationHandle + have to do this to call inline methods")]
		internal static unsafe MiResult GetInstance(OperationHandle operationHandle, out InstanceHandle instanceHandle, out bool moreResults, out MiResult result, out string errorMessage, out InstanceHandle completionDetails)
		{
			moreResults = false;
			result = MiResult.OK;
			errorMessage = "";
			instanceHandle = null;
			completionDetails = null;
			return MiResult.OK;
			/*
			int num;
			instanceHandle = null;
			moreResults = false;
			result = MiResult.OK;
			errorMessage = null;
			completionDetails = null;
			_MI_Instance modopt(System.Runtime.CompilerServices.IsConst)* _MIInstance modopt(System.Runtime.CompilerServices.IsConst)Pointer = (_MI_Instance modopt(System.Runtime.CompilerServices.IsConst)*)((long)0);
			byte num1 = 0;
			_MI_Result _MIResult = 0;
			UInt16 modopt(System.Runtime.CompilerServices.IsConst)* uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer = (UInt16 modopt(System.Runtime.CompilerServices.IsConst)*)((long)0);
			_MI_Instance modopt(System.Runtime.CompilerServices.IsConst)* _MIInstance modopt(System.Runtime.CompilerServices.IsConst)Pointer1 = (_MI_Instance modopt(System.Runtime.CompilerServices.IsConst)*)((long)0);
			_MI_Result modopt(System.Runtime.CompilerServices.CallConvCdecl) _MIResult modopt(System.Runtime.CompilerServices.CallConvCdecl) = (_MI_Result)<Module>.?A0xf5a90918.MI_Operation_GetInstance((void*)operationHandle.DangerousGetHandle(), ref (_MI_Instance modopt(System.Runtime.CompilerServices.IsConst)*)((long)0), ref num1, (_MI_Result*)(&_MIResult), ref (UInt16 modopt(System.Runtime.CompilerServices.IsConst)*)((long)0), ref (_MI_Instance modopt(System.Runtime.CompilerServices.IsConst)*)((long)0));
			if ((_MI_Instance modopt(System.Runtime.CompilerServices.IsConst)*)((long)0) != null)
			{
				IntPtr intPtr = (IntPtr)_MIInstance modopt(System.Runtime.CompilerServices.IsConst)Pointer;
				instanceHandle = new InstanceHandle(intPtr, false);
			}
			if (0 != num1)
			{
				num = 1;
			}
			else
			{
				num = 0;
			}
			int num2 = num;
			moreResults = num2;
			result = (MiResult)_MIResult;
			if (_MIInstance modopt(System.Runtime.CompilerServices.IsConst)Pointer1 != null)
			{
				IntPtr intPtr1 = (IntPtr)_MIInstance modopt(System.Runtime.CompilerServices.IsConst)Pointer1;
				completionDetails = new InstanceHandle(intPtr1, false);
			}
			if (uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer != null)
			{
				IntPtr intPtr2 = (IntPtr)uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer;
				errorMessage = Marshal.PtrToStringUni(intPtr2);
			}
			return (MiResult)_MIResult modopt(System.Runtime.CompilerServices.CallConvCdecl);
			*/
		}
	}
}