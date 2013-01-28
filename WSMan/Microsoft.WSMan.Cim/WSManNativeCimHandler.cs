using System;
using Microsoft.Management.Infrastructure.Native;
using Microsoft.WSMan.Enumeration;
using System.ServiceModel;

namespace Microsoft.WSMan.Cim
{
	internal class WSManNativeCimHandler : INativeCimHandler
	{
		private const int BatchSize = 20;
		/*
		private static readonly WSManServiceHost _host = CreateHost();

		private static WSManServiceHost CreateHost ()
		{
			var host = new WSManServiceHost();
			host.Open ();
			return host;
		}
		*/

		public WSManNativeCimHandler ()
		{

		}

		private EnumerationClient CreateClient (NativeDestinationOptions options, string queryDialect)
		{
			string serverName = string.IsNullOrEmpty (options.ServerName) ? "localhost" : options.ServerName;
			serverName = "localhost";
			int port = options.DestinationPort <= 0 ? 5985 : options.DestinationPort;
			string prefix = string.IsNullOrEmpty (options.UrlPrefix) ? "http://" : options.UrlPrefix;
			var binding = new WSManBinding();
			ChannelFactory<IWSEnumerationContract> cf = new ChannelFactory<IWSEnumerationContract>(binding);

			cf.Credentials.UserName.UserName = options.UserName;
			cf.Credentials.UserName.Password = options.Password;

			EnumerationClient client = new EnumerationClient(true, new Uri(string.Format("{0}{1}:{2}/wsman", prefix, serverName, port)), cf);
			client.BindFilterDialect(queryDialect, typeof(CimEnumerationFilter));
			return client;
		}

		#region INativeCimHandler implementation


		
		public NativeCimInstance InvokeMethod (string namespaceName, string className, string methodName, NativeCimInstance instance, NativeCimInstance inSignature)
		{
			return new NativeCimInstance();
		}

		public System.Collections.Generic.IEnumerable<NativeCimInstance> QueryInstances (NativeDestinationOptions options, string namespaceName, string queryDialect, string queryExpression, bool keysOnly)
		{
			var client = CreateClient(options, queryDialect);
			foreach (EndpointAddress item in client.EnumerateEPR(new Uri(CimNamespaces.CimNamespace), new Filter(queryDialect, new CimEnumerationFilter { Namespace = namespaceName, Filter = queryExpression }), BatchSize))
			{
				yield return CimEnumerationHelper.CreateInstance(item);
			}
		}

		public System.Collections.Generic.IEnumerable<NativeCimClass> QueryClasses (NativeDestinationOptions options, string namespaceName, string queryDialect, string queryExpression)
		{
			var client = CreateClient(options, queryDialect);
			foreach (EndpointAddress item in client.EnumerateEPR(new Uri(CimNamespaces.CimNamespace), new Filter(queryDialect, new CimEnumerationFilter { Namespace = namespaceName, Filter = queryExpression }), BatchSize))
			{
				yield return CimEnumerationHelper.CreateClass (item);
			}
		}

		#endregion
	}
}