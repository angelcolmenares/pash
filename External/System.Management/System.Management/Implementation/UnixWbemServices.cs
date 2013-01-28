using System;
using System.Collections.Generic;
using System.Linq;
namespace System.Management
{
	internal class UnixWbemServices : IWbemServices
	{
		private string _currentNamespace;

		internal UnixWbemServices ()
			: this(CimNamespaces.CimV2)
		{

		}

		internal UnixWbemServices (string nameSpace)
		{
			_currentNamespace = nameSpace;
		}

		internal string CurrentNamespace {
			get { return _currentNamespace; }
		}

		#region IWbemServices implementation

		public int CancelAsyncCall_ (IWbemObjectSink pSink)
		{
			return 0;
		}

		public int CreateClassEnum_ (string strSuperclass, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum)
		{
			ppEnum = null;
			return 0;
		}

		public int CreateClassEnumAsync_ (string strSuperclass, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			return 0;
		}

		public int CreateInstanceEnum_ (string strFilter, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum)
		{
			var items = WMIDatabaseFactory.Get (_currentNamespace, strFilter);
			ppEnum = new UnixEnumWbemClassObject(items);
			return 0;
		}

		public int CreateInstanceEnumAsync_ (string strFilter, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			return 0;
		}

		public int DeleteClass_ (string strClass, int lFlags, IWbemContext pCtx, IntPtr ppCallResult)
		{
			return 0;
		}

		public int DeleteClassAsync_ (string strClass, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			return 0;
		}

		public int DeleteInstance_ (string strObjectPath, int lFlags, IWbemContext pCtx, IntPtr ppCallResult)
		{
			string strQuery = QueryParser.GetQueryFromPath (strObjectPath);
			return 0;
		}

		public int DeleteInstanceAsync_ (string strObjectPath, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			string strQuery = QueryParser.GetQueryFromPath (strObjectPath);
			return 0;
		}

		public int ExecMethod_ (string strObjectPath, string strMethodName, int lFlags, IWbemContext pCtx, IntPtr pInParams, out IWbemClassObjectFreeThreaded ppOutParams, IntPtr ppCallResult)
		{
			string strQuery = QueryParser.GetQueryFromPath (strObjectPath);
			var items = WMIDatabaseFactory.Get (_currentNamespace, strQuery);
			ppOutParams = null;
			foreach (var obj in items) {
				var intPtr = MarshalWbemObject.GetInstance(null).MarshalManagedToNative (obj);
				ppOutParams = new IWbemClassObjectFreeThreaded(intPtr);
			}
			return 0;
		}

		public int ExecMethodAsync_ (string strObjectPath, string strMethodName, int lFlags, IWbemContext pCtx, IntPtr pInParams, IWbemObjectSink pResponseHandler)
		{
			string strQuery = QueryParser.GetQueryFromPath (strObjectPath);
			return 0;
		}

		public int ExecNotificationQuery_ (string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum)
		{
			ppEnum = new UnixEnumWbemClassObject(WMIDatabaseFactory.Get (_currentNamespace, strQuery));
			return 0;
		}

		public int ExecNotificationQueryAsync_ (string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			return 0;
		}

		public int ExecQuery_ (string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum)
		{
			IEnumerable<IWbemClassObject_DoNotMarshal> list = WMIDatabaseFactory.Get(_currentNamespace, strQuery);

			ppEnum = new UnixEnumWbemClassObject(list);
			return 0;
		}

		public int ExecQueryAsync_ (string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			IEnumerable<IWbemClassObject_DoNotMarshal> list = WMIDatabaseFactory.Get(_currentNamespace, strQuery);
			pResponseHandler.Indicate_ (list.Count(), new IntPtr[0]);
			return 0;
		}

		public int GetObject_ (string strObjectPath, int lFlags, IWbemContext pCtx, out IWbemClassObjectFreeThreaded ppObject, IntPtr ppCallResult)
		{
			if (ppCallResult == IntPtr.Zero) {
				//Get Class from Path


				//Get IntPtr for Class
				ppCallResult = System.Runtime.InteropServices.Marshal.GetIUnknownForObject (this);
			}
			ppObject = new IWbemClassObjectFreeThreaded(ppCallResult);
			return 0;
		}

		public int GetObjectAsync_ (string strObjectPath, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			return 0;
		}

		public int OpenNamespace_ (string strNamespace, int lFlags, IWbemContext pCtx, out IWbemServices ppWorkingNamespace, IntPtr ppCallResult)
		{
			ppWorkingNamespace = new UnixWbemServices(strNamespace);
			return 0;
		}

		public int PutClass_ (IntPtr pObject, int lFlags, IWbemContext pCtx, IntPtr ppCallResult)
		{
			return 0;
		}

		public int PutClassAsync_ (IntPtr pObject, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			return 0;
		}

		public int PutInstance_ (IntPtr pInst, int lFlags, IWbemContext pCtx, IntPtr ppCallResult)
		{
			return 0;
		}

		public int PutInstanceAsync_ (IntPtr pInst, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			return 0;
		}

		public int QueryObjectSink_ (int lFlags, out IWbemObjectSink ppResponseHandler)
		{
			ppResponseHandler = null;
			return 0;
		}

		#endregion
	}
}

