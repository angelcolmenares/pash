using Microsoft.Management.Odata.Core;
using System;
using System.Data.Services;
using System.Data.Services.Common;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.Management.Odata
{
	[FormatSupportBehavior]
	internal class DataService : DataServiceProvider
	{
		public DataService()
		{

		}

		public static void InitializeService(DataServiceConfiguration config)
		{
			config.SetEntitySetAccessRule("*", EntitySetRights.All);
			config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V3;
			config.DataServiceBehavior.AcceptProjectionRequests = true;
			config.UseVerboseErrors = true;
			config.SetEntitySetPageSize("*", DataServiceController.Current.Configuration.GetResultSetLimit("*"));
			foreach (DSConfiguration.WcfConfigElement entitySet in DataServiceController.Current.Configuration.DataServicesConfig.EntitySets)
			{
				config.SetEntitySetPageSize(entitySet.Name, entitySet.MaxResults);
			}
			config.MaxExpandCount = DataServiceController.Current.Configuration.DataServicesConfig.MaxExpandCount;
			config.MaxExpandDepth = DataServiceController.Current.Configuration.DataServicesConfig.MaxExpandDepth;
			config.DataServiceBehavior.AcceptAnyAllRequests = true;
			config.DataServiceBehavior.AcceptSpatialLiteralsInQuery = false;
		}

		/*
		public static void CreateHost ()
		{
			var handlerType = Type.GetType ("System.ServiceModel.Channels.SvcHttpHandler, System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
			object handler = Activator.CreateInstance (handlerType, new object[] { typeof(DataService), typeof(DataServiceHostFactory), "/Microsoft.Management.Odata.svc" }, null);
			FieldInfo hostField = handlerType.GetField ("host", BindingFlags.NonPublic | BindingFlags.Instance);
			var host = new DataServiceHost(typeof(DataService), new Uri[] { new Uri("http://127.0.0.1:7000/Microsoft.Management.Odata.svc", UriKind.Absolute) });
			host.Description.Behaviors.Add (new FormatSupportBehaviorAttribute());
			host.Open ();
			var binding = host.Description.Endpoints[0].Binding as WebHttpBinding;
			var endpointBehaviors = host.Description.Endpoints[0].Behaviors;
			var contractBehaviors = host.Description.Endpoints[0].Contract.Behaviors;
			var opBehaviors = host.Description.Endpoints[0].Contract.Operations[0].Behaviors;
			hostField.SetValue (handler, host);
			var factoryType = Type.GetType ("System.ServiceModel.Channels.SvcHttpHandlerFactory, System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
			FieldInfo handlersField = factoryType.GetField ("handlers", BindingFlags.NonPublic | BindingFlags.Static);
			object handlers = handlersField.GetValue(null);
			MethodInfo addMethod = handlers.GetType ().GetMethod ("Add");
			addMethod.Invoke (handlers, new object[] { "/Microsoft.Management.Odata.svc", handler });
		}
        */
	}
}