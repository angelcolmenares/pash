using System;
using Microsoft.Management.Infrastructure.Native;
using Microsoft.WSMan.Enumeration;
using System.ServiceModel;
using Microsoft.WSMan.Management;

namespace Microsoft.WSMan.Cim
{
	internal class WSManNativeLocalCimHandler : INativeCimHandler
	{
		private IEnumerationRequestHandler _handler = new CimEnumerationRequestHandler() { IsLocal = true };

		public WSManNativeLocalCimHandler ()
		{

		}

		#region INativeCimHandler implementation

		public NativeCimInstance InvokeMethod (string namespaceName, string className, string methodName, NativeCimInstance instance, NativeCimInstance inSignature)
		{
			var classObj = new System.Management.ManagementClass(string.Format ("//./{0}/{1}",  namespaceName, className));
			var inObj = classObj.GetMethodParameters (className);
			inObj.Properties["CommandLine"].Value = "notepad.exe";
			var result = classObj.InvokeMethod(methodName, inObj, new System.Management.InvokeMethodOptions());
			var endpoint = CimEnumerator.ToEndointAddress (result, true);
			return CimEnumerationHelper.CreateInstance (endpoint);
		}

		public System.Collections.Generic.IEnumerable<NativeCimInstance> QueryInstances (NativeDestinationOptions options, string namespaceName, string queryDialect, string queryExpression, bool keysOnly)
		{
			var context = new EnumerationContext(CimNamespaces.CimNamespace, new Filter(queryDialect, new CimEnumerationFilter { Namespace = namespaceName, Filter = queryExpression }), new Selector[0]);
			foreach (EndpointAddress address in _handler.Enumerate (context)) {
				yield return CimEnumerationHelper.CreateInstance (address);
			}
		}

		public System.Collections.Generic.IEnumerable<NativeCimClass> QueryClasses (NativeDestinationOptions options, string namespaceName, string queryDialect, string queryExpression)
		{
			var context = new EnumerationContext(CimNamespaces.CimNamespace, new Filter(queryDialect, new CimEnumerationFilter { Namespace = namespaceName, Filter = queryExpression }), new Selector[0]);
			foreach (EndpointAddress address in _handler.Enumerate (context)) {
				yield return CimEnumerationHelper.CreateClass (address);
			}
		}

		#endregion
	}
}

