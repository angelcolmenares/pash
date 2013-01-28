using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	internal class WmiGetEventSink : WmiEventSink
	{
		private ManagementObject managementObject;

		private static ManagementOperationObserver watcherParameter;

		private static object contextParameter;

		private static ManagementScope scopeParameter;

		private static ManagementObject managementObjectParameter;

		private static WmiGetEventSink wmiGetEventSinkNew;

		private WmiGetEventSink(ManagementOperationObserver watcher, object context, ManagementScope scope, ManagementObject managementObject) : base(watcher, context, scope, null, null)
		{
			this.managementObject = managementObject;
		}

		internal static WmiGetEventSink GetWmiGetEventSink(ManagementOperationObserver watcher, object context, ManagementScope scope, ManagementObject managementObject)
		{
			if (!MTAHelper.IsNoContextMTA())
			{
				WmiGetEventSink.watcherParameter = watcher;
				WmiGetEventSink.contextParameter = context;
				WmiGetEventSink.scopeParameter = scope;
				WmiGetEventSink.managementObjectParameter = managementObject;
				ThreadDispatch threadDispatch = new ThreadDispatch(new ThreadDispatch.ThreadWorkerMethod(WmiGetEventSink.HackToCreateWmiGetEventSink));
				threadDispatch.Start();
				return WmiGetEventSink.wmiGetEventSinkNew;
			}
			else
			{
				return new WmiGetEventSink(watcher, context, scope, managementObject);
			}
		}

		private static void HackToCreateWmiGetEventSink()
		{
			WmiGetEventSink.wmiGetEventSinkNew = new WmiGetEventSink(WmiGetEventSink.watcherParameter, WmiGetEventSink.contextParameter, WmiGetEventSink.scopeParameter, WmiGetEventSink.managementObjectParameter);
		}

		public override void Indicate(IntPtr pIWbemClassObject)
		{
			Marshal.AddRef(pIWbemClassObject);
			IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded = new IWbemClassObjectFreeThreaded(pIWbemClassObject);
			if (this.managementObject != null)
			{
				try
				{
					this.managementObject.wbemObject = wbemClassObjectFreeThreaded;
				}
				catch
				{
				}
			}
		}
	}
}