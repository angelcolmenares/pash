using Microsoft.Management.Odata.Common;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.IO;
using System.Xml;

namespace Microsoft.Management.Odata.Core
{

	internal sealed class FormatSupportInspector : IDispatchMessageInspector
	{
		public FormatSupportInspector()
		{
		}

		public object AfterReceiveRequest (ref Message request, IClientChannel channel, InstanceContext instanceContext)
		{
			TraceHelper.Current.DebugMessage("Entering FormatSupportInspector: AfterReceiveRequest");
			if (request.Properties.ContainsKey("UriTemplateMatchResults"))
			{
				HttpRequestMessageProperty item = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];
				UriTemplateMatch uriTemplateMatch = (UriTemplateMatch)request.Properties["UriTemplateMatchResults"];
				string acceptHeaderValueForFormatQuery = this.GetAcceptHeaderValueForFormatQuery(uriTemplateMatch.QueryParameters["$format"]);
				if (acceptHeaderValueForFormatQuery != null)
				{
					uriTemplateMatch.QueryParameters.Remove("$format");
					item.Headers["Accept"] = acceptHeaderValueForFormatQuery;
				}
			}
			return null;
		}

		public void BeforeSendReply(ref Message reply, object correlationState)
		{
			TraceHelper.Current.DebugMessage("Entering FormatSupportInspector: BeforeSendReply");
			TraceHelper.Current.RequestEnd();
			if (DataServiceController.IsCurrentInstancePresent())
			{
				DataServiceController.Current.PerfCounters.ActiveRequests.Decrement();
			}
		}

		private string GetAcceptHeaderValueForFormatQuery(string format)
		{
			if (format != null)
			{
				string lowerInvariant = format.ToLowerInvariant();
				string str = lowerInvariant;
				if (lowerInvariant != null)
				{
					if (str == "json")
					{
						return "application/json";
					}
					else
					{
						if (str == "jsonverbose")
						{
							return "application/json;odata=verbose";
						}
						else
						{
							if (str == "atom")
							{
								return "application/atom+xml";
							}
						}
					}
				}
				return format;
			}
			else
			{
				return null;
			}
		}
	}
}