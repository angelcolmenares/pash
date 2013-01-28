using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Schema;
using System;
using System.Data.Services;
using System.Data.Services.Providers;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Microsoft.Management.Odata.Core
{
	internal class DataServiceProvider : DataService<DataContext>, IServiceProvider
	{
		private DataContext dataContext;

		public DataServiceProvider()
		{
			base.ProcessingPipeline.ProcessingRequest += new EventHandler<DataServiceProcessingPipelineEventArgs>(QuotaSystem.ProcessingRequestHandler);
			base.ProcessingPipeline.ProcessedRequest += new EventHandler<DataServiceProcessingPipelineEventArgs>(QuotaSystem.ProcessedRequestHandler);
		}

		public DataContext CreateDataSource(IIdentity identity, X509Certificate2 cert, Uri requestUri)
		{
			UserContext userContext = new UserContext(identity, cert);
			string membershipId = DataServiceController.Current.GetMembershipId(userContext, requestUri);
			Envelope<Microsoft.Management.Odata.Schema.Schema, UserContext> schema = DataServiceController.Current.GetSchema(userContext, membershipId);
			using (schema)
			{
				this.dataContext = new DataContext(schema.Item, userContext, membershipId);
			}
			DataServiceController.Current.SetCurrentContext(this.dataContext);
			return this.dataContext;
		}

		protected override DataContext CreateDataSource()
		{
			if (this.dataContext == null)
			{
				this.CreateDataSource(CurrentRequestHelper.Identity, CurrentRequestHelper.Certificate, CurrentRequestHelper.Uri);
				this.OverridePublicRoot();
			}
			return this.dataContext;
		}

		public object GetService(Type serviceType)
		{
			TraceHelper.Current.MethodCall1("DataServiceProvider", "GetService", serviceType.ToString());
			if (serviceType != typeof(IDataServiceMetadataProvider))
			{
				if (serviceType != typeof(IDataServiceQueryProvider))
				{
					if (serviceType != typeof(IDataServiceUpdateProvider))
					{
						return null;
					}
					else
					{
						return new DataServiceUpdateProvider(this.CreateDataSource());
					}
				}
				else
				{
					return new DataServiceQueryProvider();
				}
			}
			else
			{
				return new DataServiceMetadataProvider(this.CreateDataSource());
			}
		}

		protected override void HandleException(HandleExceptionArgs args)
		{
			args.Trace();
			QuotaSystem.ProcessedRequestHandler(null, null);
			if (OperationContext.Current != null)
			{
				OperationContext.Current.TraceOutgoingMessage();
			}
			base.HandleException(args);
		}

		private void OverridePublicRoot()
		{
			if (DataServiceController.Current == null || !DataServiceController.Current.Configuration.DataServicesConfig.EnablePublicServerOverride)
			{
				return;
			}
			else
			{
				if (WebOperationContext.Current != null || WebOperationContext.Current.IncomingRequest.UriTemplateMatch != null)
				{
					string str = WebOperationContext.Current.IncomingRequest.Headers.Get("public-server-uri");
					if (str != null)
					{
						Uri uri = null;
						try
						{
							uri = new Uri(str);
						}
						catch (UriFormatException uriFormatException1)
						{
							UriFormatException uriFormatException = uriFormatException1;
							TraceHelper.Current.DebugMessage(string.Concat("DataServiceProvider: CreateDataSource. Converting the PublicServerRootHeader to Uri failed. \nUri: ", str, "\nIgnoring exception: ", uriFormatException.ToTraceMessage("Exception")));
							TraceHelper.Current.InvalidUriForPublicRootHeader(str);
							return;
						}
						UriBuilder uriBuilder = new UriBuilder(WebOperationContext.Current.IncomingRequest.UriTemplateMatch.BaseUri);
						UriBuilder host = new UriBuilder(WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri);
						uriBuilder.Host = uri.Host;
						host.Host = uri.Host;
						uriBuilder.Scheme = uri.Scheme;
						host.Scheme = uri.Scheme;
						uriBuilder.Port = uri.Port;
						host.Port = uri.Port;
						uriBuilder.Trace("Final root URI");
						host.Trace("Final request URI");
						if (OperationContext.Current != null)
						{
							TraceHelper.Current.DebugMessage("DataServiceProvider: CreateDataSource Updating root URI and request URI in operation context");
							OperationContext.Current.IncomingMessageProperties["MicrosoftDataServicesRootUri"] = uriBuilder.Uri;
							OperationContext.Current.IncomingMessageProperties["MicrosoftDataServicesRequestUri"] = host.Uri;
						}
						return;
					}
					else
					{
						TraceHelper.Current.DebugMessage("DataServiceProvider: CreateDataSource. Allow public override is enabled. But cannot get PublicServerRootHeader header in list of headers");
						return;
					}
				}
				else
				{
					return;
				}
			}
		}
	}
}