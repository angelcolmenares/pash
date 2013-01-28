using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("9556DC99-828C-11CF-A37E-00AA003240C7")]
	[InterfaceType(1)]
	[TypeLibType(0x200)]
	internal interface IWbemServices
	{
		int CancelAsyncCall_(IWbemObjectSink pSink);

		int CreateClassEnum_(string strSuperclass, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum);

		int CreateClassEnumAsync_(string strSuperclass, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler);

		int CreateInstanceEnum_(string strFilter, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum);

		int CreateInstanceEnumAsync_(string strFilter, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler);

		int DeleteClass_(string strClass, int lFlags, IWbemContext pCtx, IntPtr ppCallResult);

		int DeleteClassAsync_(string strClass, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler);

		int DeleteInstance_(string strObjectPath, int lFlags, IWbemContext pCtx, IntPtr ppCallResult);

		int DeleteInstanceAsync_(string strObjectPath, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler);

		int ExecMethod_(string strObjectPath, string strMethodName, int lFlags, IWbemContext pCtx, IntPtr pInParams, out IWbemClassObjectFreeThreaded ppOutParams, IntPtr ppCallResult);

		int ExecMethodAsync_(string strObjectPath, string strMethodName, int lFlags, IWbemContext pCtx, IntPtr pInParams, IWbemObjectSink pResponseHandler);

		int ExecNotificationQuery_(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum);

		int ExecNotificationQueryAsync_(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler);

		int ExecQuery_(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum);

		int ExecQueryAsync_(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler);

		int GetObject_(string strObjectPath, int lFlags, IWbemContext pCtx, out IWbemClassObjectFreeThreaded ppObject, IntPtr ppCallResult);

		int GetObjectAsync_(string strObjectPath, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler);

		int OpenNamespace_(string strNamespace, int lFlags, IWbemContext pCtx, out IWbemServices ppWorkingNamespace, IntPtr ppCallResult);

		int PutClass_(IntPtr pObject, int lFlags, IWbemContext pCtx, IntPtr ppCallResult);

		int PutClassAsync_(IntPtr pObject, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler);

		int PutInstance_(IntPtr pInst, int lFlags, IWbemContext pCtx, IntPtr ppCallResult);

		int PutInstanceAsync_(IntPtr pInst, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler);

		int QueryObjectSink_(int lFlags, out IWbemObjectSink ppResponseHandler);
	}
}