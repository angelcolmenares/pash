using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Tracing;
using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Providers;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace Microsoft.Management.Odata.Common
{
	internal static class TraceHelper
	{
		public static Tracer Current
		{
			get; private set;
		}

		internal static bool IsTestHookEnabled
		{
			get;set;
		}

		internal static Guid TestHookClientRequestGuid
		{
			get;set;
		}

		static TraceHelper()
		{
			TraceHelper.Current = new Tracer();
		}

		public static void CorrelateWithClientRequestId(DataServiceOperationContext context)
		{
			Guid guid;
			string str = context.RequestHeaders.Get("client-request-id");
			if (!string.IsNullOrWhiteSpace(str) && Guid.TryParse(str, out guid))
			{
				TraceHelper.Current.CorrelateWithActivity(guid);
				if (TraceHelper.IsTestHookEnabled)
				{
					TraceHelper.TestHookClientRequestGuid = guid;
				}
			}
		}

		public static void GetDetails(this IIdentity identity, out string userName, out string authenticationType, out bool isAuthenticated, out bool isWindowsIdentity)
		{
			string name;
			string empty;
			bool flag;
			bool flag1;
			if (identity != null)
			{
				name = identity.Name;
			}
			else
			{
				name = string.Empty;
			}
			(userName) = name;
			if (identity != null)
			{
				empty = identity.AuthenticationType;
			}
			else
			{
				empty = string.Empty;
			}
			(authenticationType) = empty;
			if (identity != null)
			{
				flag = identity.IsAuthenticated;
			}
			else
			{
				flag = false;
			}
			(isAuthenticated) = flag;
			if (identity == null || identity as WindowsIdentity == null)
			{
				flag1 = false;
			}
			else
			{
				flag1 = true;
			}
			(isWindowsIdentity) = flag1;
		}

		public static string GetHttpMethod(this MessageProperties properties)
		{
			string method = string.Empty;
			IEnumerator<string> enumerator = properties.Keys.GetEnumerator();
			using (enumerator)
			{
				while (enumerator.MoveNext())
				{
					string current = enumerator.Current;
					HttpRequestMessageProperty item = properties[current] as HttpRequestMessageProperty;
					if (item == null)
					{
						continue;
					}
					method = item.Method;
					return method;
				}
			}
			return method;
		}

		public static bool IsEnabled(byte level = 5)
		{
			return TraceHelper.Current.IsProviderEnabled(level, (long)-1);
		}

		public static string ToTraceMessage(this Exception ex, string message = "Exception")
		{
			return ex.ToTraceMessage(message, new StringBuilder()).ToString();
		}

		public static StringBuilder ToTraceMessage(this Exception ex, StringBuilder builder)
		{
			return ex.ToTraceMessage("Exception", builder);
		}

		public static StringBuilder ToTraceMessage(this Exception ex, string message, StringBuilder builder)
		{
			if (ex != null)
			{
				builder.AppendLine(message);
				builder.AppendLine(string.Concat("Message = ", ex.Message));
				builder.AppendLine(string.Concat("Type = ", ex.GetType().AssemblyQualifiedName));
				if (ex.HelpLink != null)
				{
					builder.AppendLine(string.Concat("HelpLink = ", ex.HelpLink));
				}
				builder.AppendLine(string.Concat("Source = ", ex.Source));
				builder.AppendLine(string.Concat("Stack Trace = ", ex.StackTrace));
				if (ex.InnerException != null)
				{
					builder = ex.InnerException.ToTraceMessage(string.Concat("\nInner Exception of ", message), builder);
				}
			}
			return builder;
		}

		public static StringBuilder ToTraceMessage(this Dictionary<string, string> dictionary, StringBuilder stringBuilder)
		{
			stringBuilder.AppendLine(string.Concat("Total entries = ", dictionary.Count));
			dictionary.Keys.ToList<string>().ForEach((string item) => stringBuilder.AppendLine(string.Concat(item, " = ", dictionary[item])));
			return stringBuilder;
		}

		public static StringBuilder ToTraceMessage(this ResourceSet resourceSet, StringBuilder builder)
		{
			builder.AppendLine(string.Concat("Resource Set = ", resourceSet.Name));
			builder.AppendLine(string.Concat("\tType = ", resourceSet.ResourceType.FullName));
			builder.AppendLine(string.Concat("\tIs readonly = ", resourceSet.ResourceType.IsReadOnly));
			return builder;
		}

		public static StringBuilder ToTraceMessage(this ResourceType resourceType, StringBuilder builder)
		{
			string fullName;
			string str;
			Action<ResourceProperty> action = null;
			Action<ResourceProperty> action1 = null;
			Action<ResourceProperty> action2 = null;
			builder.AppendLine(string.Concat("Resource Type = ", resourceType.Name));
			builder.AppendLine(string.Concat("\tResource Type full name = ", resourceType.FullName));
			builder.AppendLine(string.Concat("\tNamespace = ", resourceType.Namespace));
			builder.AppendLine(string.Concat("\tResourceTypeKind = ", resourceType.ResourceTypeKind.ToString()));
			StringBuilder stringBuilder = builder;
			string str1 = "\tBase Type = ";
			if (resourceType.BaseType != null)
			{
				fullName = resourceType.BaseType.FullName;
			}
			else
			{
				fullName = "null";
			}
			stringBuilder.AppendLine(string.Concat(str1, fullName));
			StringBuilder stringBuilder1 = builder;
			string str2 = "\tInstance Type = ";
			if (resourceType.InstanceType != null)
			{
				str = resourceType.InstanceType.FullName;
			}
			else
			{
				str = "null";
			}
			stringBuilder1.AppendLine(string.Concat(str2, str));
			bool isAbstract = resourceType.IsAbstract;
			builder.AppendLine(string.Concat("\tIsAbstract = ", isAbstract.ToString()));
			bool isReadOnly = resourceType.IsReadOnly;
			builder.AppendLine(string.Concat("\tIsReadOnly = ", isReadOnly.ToString()));
			bool flag = resourceType.IsReadOnly;
			builder.AppendLine(string.Concat("\tIsReadOnly = ", flag.ToString()));
			if (resourceType.KeyProperties != null)
			{
				builder.AppendLine("\tKeyProperties");
				List<ResourceProperty> list = resourceType.KeyProperties.ToList<ResourceProperty>();
				if (action == null)
				{
					action = (ResourceProperty item) => builder.AppendLine(string.Concat("\t\t", item.Name));
				}
				list.ForEach(action);
			}
			if (resourceType.ETagProperties != null)
			{
				builder.AppendLine("\tETagProperties");
				List<ResourceProperty> resourceProperties = resourceType.ETagProperties.ToList<ResourceProperty>();
				if (action1 == null)
				{
					action1 = (ResourceProperty item) => builder.AppendLine(string.Concat("\t\t", item.Name));
				}
				resourceProperties.ForEach(action1);
			}
			if (resourceType.Properties != null)
			{
				builder.AppendLine("\tProperties");
				List<ResourceProperty> list1 = resourceType.Properties.ToList<ResourceProperty>();
				if (action2 == null)
				{
					action2 = (ResourceProperty item) => {
						string[] name = new string[6];
						name[0] = "\t\tName = ";
						name[1] = item.Name;
						name[2] = " Kind =  ";
						name[3] = item.Kind.ToString();
						name[4] = " Type = ";
						name[5] = item.ResourceType.FullName;
						builder.AppendLine(string.Concat(name));
					}
					;
				}
				list1.ForEach(action2);
			}
			return builder;
		}

		public static StringBuilder ToTraceMessage(this WebHeaderCollection webHeader, string webHeaderName, StringBuilder stringBuilder)
		{
			Action<string> action = null;
			stringBuilder.AppendLine(string.Concat(webHeaderName, " Header"));
			if (webHeaderName != null)
			{
				List<string> list = webHeader.AllKeys.ToList<string>();
				if (action == null)
				{
					action = (string key) => stringBuilder.AppendLine(string.Concat(key, " = ", webHeader[key]));
				}
				list.ForEach(action);
			}
			else
			{
				stringBuilder.Append(" = <null>");
			}
			return stringBuilder;
		}

		public static string ToTraceMessage(this PSCommand commands)
		{
			Action<Command> action = null;
			StringBuilder stringBuilder = new StringBuilder();
			if (commands != null)
			{
				stringBuilder.AppendLine(string.Concat("Count = ", commands.Commands.Count<Command>()));
				List<Command> list = commands.Commands.ToList<Command>();
				if (action == null)
				{
					action = (Command item) => stringBuilder = item.ToTraceMessage("\nCommand", stringBuilder);
				}
				list.ForEach(action);
			}
			else
			{
				stringBuilder.AppendLine("<null>");
			}
			return stringBuilder.ToString();
		}

		public static StringBuilder ToTraceMessage(this Command command, string message, StringBuilder builder)
		{
			Action<CommandParameter> action = null;
			builder.AppendLine(message);
			builder.AppendLine(string.Concat("CommandText = ", command.CommandText));
			bool isScript = command.IsScript;
			builder.AppendLine(string.Concat("IsScript = ", isScript.ToString()));
			builder.AppendLine(string.Concat("MergeUnclaimedPreviousCommandResults = ", command.MergeUnclaimedPreviousCommandResults.ToString()));
			if (command.Parameters != null)
			{
				builder.AppendLine(string.Concat("Parameters = ", command.Parameters.Count<CommandParameter>()));
				List<CommandParameter> list = command.Parameters.ToList<CommandParameter>();
				if (action == null)
				{
					action = (CommandParameter item) => {
						string str;
						StringBuilder stringBuilder = builder;
						string name = item.Name;
						string str1 = " = ";
						if (item.Value != null)
						{
							str = item.Value.ToString();
						}
						else
						{
							str = "<null>";
						}
						stringBuilder.AppendLine(string.Concat(name, str1, str));
					}
					;
				}
				list.ForEach(action);
			}
			return builder;
		}

		public static string ToTraceMessage(this InitialSessionState iss)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Initial Session State");
			stringBuilder.AppendLine(string.Concat("Language Mode = ", iss.LanguageMode.ToString()));
			stringBuilder.AppendLine(string.Concat("Apartment State = ", iss.ApartmentState.ToString()));
			stringBuilder.AppendLine(string.Concat("Thread Options = ", iss.ThreadOptions.ToString()));
			bool useFullLanguageModeInDebugger = iss.UseFullLanguageModeInDebugger;
			stringBuilder.AppendLine(string.Concat("Full language mode in debugger = ", useFullLanguageModeInDebugger.ToString()));
			stringBuilder.AppendLine("Visible Commands ");
			InitialSessionStateEntryCollection<SessionStateCommandEntry> commands = iss.Commands;
			commands.Where<SessionStateCommandEntry>((SessionStateCommandEntry item) => item.Visibility == SessionStateEntryVisibility.Public).ToList<SessionStateCommandEntry>().ForEach((SessionStateCommandEntry item) => stringBuilder.AppendLine(string.Concat("\t", item.Name)));
			stringBuilder.AppendLine("Invisible Commands ");
			InitialSessionStateEntryCollection<SessionStateCommandEntry> sessionStateCommandEntries = iss.Commands;
			sessionStateCommandEntries.Where<SessionStateCommandEntry>((SessionStateCommandEntry item) => item.Visibility == SessionStateEntryVisibility.Private).ToList<SessionStateCommandEntry>().ForEach((SessionStateCommandEntry item) => stringBuilder.AppendLine(string.Concat("\t", item.Name)));
			return stringBuilder.ToString();
		}

		public static string ToTraceMessage(this IIdentity identity)
		{
			string str = null;
			string str1 = null;
			bool flag = false;
			bool flag1 = false;
			identity.GetDetails(out str, out str1, out flag, out flag1);
			object[] objArray = new object[4];
			objArray[0] = str;
			objArray[1] = str1;
			objArray[2] = flag;
			objArray[3] = flag1;
			return string.Format(CultureInfo.CurrentCulture, Resources.IdentityDescription, objArray);
		}

		public static void Trace(this Exception ex, string message = null)
		{
			TraceHelper.Current.DebugMessage(ex.ToTraceMessage(message));
		}

		public static void Trace(this UriBuilder uriBuilder, string message)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(string.Concat("URI builder ", message));
			stringBuilder.AppendLine(string.Concat("URI: ", uriBuilder.Uri));
			stringBuilder.AppendLine(string.Concat("Scheme: ", uriBuilder.Scheme));
			stringBuilder.AppendLine(string.Concat("Host: ", uriBuilder.Host));
			stringBuilder.AppendLine(string.Concat("Fragment: ", uriBuilder.Fragment));
			stringBuilder.AppendLine(string.Concat("Port: ", uriBuilder.Port));
			stringBuilder.AppendLine(string.Concat("Path: ", uriBuilder.Path));
			TraceHelper.Current.DebugMessage(stringBuilder.ToString());
		}

		public static void Trace(this DataServiceOperationContext ctx)
		{
			if (TraceHelper.IsEnabled(5))
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Operation Context");
				if (ctx != null)
				{
					stringBuilder.AppendLine(string.Concat("Absolute request uri = ", ctx.AbsoluteRequestUri));
					stringBuilder.AppendLine(string.Concat("Absolute response uri =  ", ctx.AbsoluteServiceUri));
					bool isBatchRequest = ctx.IsBatchRequest;
					stringBuilder.AppendLine(string.Concat("IsBatchRequest =  ", isBatchRequest.ToString()));
					stringBuilder.AppendLine(string.Concat("\nRequest method = ", ctx.RequestMethod));
					stringBuilder = ctx.RequestHeaders.ToTraceMessage("Request", stringBuilder);
					stringBuilder = ctx.ResponseHeaders.ToTraceMessage("Response", stringBuilder);
					int responseStatusCode = ctx.ResponseStatusCode;
					stringBuilder.Append(string.Concat("\nResponse status code = ", responseStatusCode.ToString()));
				}
				else
				{
					stringBuilder.Append("= <null>");
				}
				TraceHelper.Current.DebugMessage(stringBuilder.ToString());
			}
		}

		public static void Trace(this HandleExceptionArgs handleExceptionArgs)
		{
			string trueStringValue;
			string falseStringValue;
			string message = handleExceptionArgs.Exception.Message;
			Tracer current = TraceHelper.Current;
			string str = message;
			int responseStatusCode = handleExceptionArgs.ResponseStatusCode;
			string responseContentType = handleExceptionArgs.ResponseContentType;
			if (handleExceptionArgs.ResponseWritten)
			{
				trueStringValue = Resources.TrueStringValue;
			}
			else
			{
				trueStringValue = Resources.FalseStringValue;
			}
			if (handleExceptionArgs.UseVerboseErrors)
			{
				falseStringValue = Resources.TrueStringValue;
			}
			else
			{
				falseStringValue = Resources.FalseStringValue;
			}
			current.DataServiceProviderHandleException(str, responseStatusCode, responseContentType, trueStringValue, falseStringValue);
		}

		public static void Trace(this System.Management.Automation.PowerShell powerShell)
		{
			string message;
			if (TraceHelper.IsEnabled(4))
			{
				string str = powerShell.InvocationStateInfo.State.ToString();
				if (powerShell.InvocationStateInfo.Reason != null)
				{
					message = powerShell.InvocationStateInfo.Reason.Message;
				}
				else
				{
					message = string.Empty;
				}
				string str1 = message;
				var command = string.Empty;
				powerShell.Commands.Commands.ToList<Command>().ForEach((Command item) => {
					command = string.Concat(command, item.CommandText, ".");
				}
				);
				TraceHelper.Current.PowerShellInstance(powerShell.InstanceId, command, str, str1);
				powerShell.Streams.Trace();
			}
		}

		public static void Trace(this PSDataStreams streams)
		{
			string message;
			string empty;
			string str;
			if (streams.Error != null && streams.Error.Count > 0)
			{
				foreach (ErrorRecord error in streams.Error)
				{
					if (error.Exception != null)
					{
						message = error.Exception.Message;
					}
					else
					{
						message = string.Empty;
					}
					string str1 = message;
					if (error.ErrorDetails != null)
					{
						empty = error.ErrorDetails.Message;
					}
					else
					{
						empty = string.Empty;
					}
					string str2 = empty;
					if (error.CategoryInfo != null)
					{
						str = error.CategoryInfo.GetMessage();
					}
					else
					{
						str = string.Empty;
					}
					string str3 = str;
					TraceHelper.Current.ErrorRecord(str1, str2, str3);
				}
			}
			if (streams.Verbose != null && streams.Verbose.Count > 0)
			{
				foreach (VerboseRecord verbose in streams.Verbose)
				{
					TraceHelper.Current.InformationRecord(verbose.Message);
				}
			}
			if (streams.Warning != null && streams.Warning.Count > 0)
			{
				foreach (WarningRecord warning in streams.Warning)
				{
					TraceHelper.Current.InformationRecord(warning.Message);
				}
			}
		}

		public static void Trace(this InitialSessionState iss)
		{
			if (TraceHelper.IsEnabled(5))
			{
				TraceHelper.Current.DebugMessage(iss.ToTraceMessage());
			}
		}

		public static void Trace(this IIdentity identity)
		{
			string str = null;
			string str1 = null;
			bool flag = false;
			bool flag1 = false;
			string trueStringValue;
			string falseStringValue;
			identity.GetDetails(out str, out str1, out flag, out flag1);
			Tracer current = TraceHelper.Current;
			string str2 = str;
			string str3 = str1;
			if (flag)
			{
				trueStringValue = Resources.TrueStringValue;
			}
			else
			{
				trueStringValue = Resources.FalseStringValue;
			}
			if (flag1)
			{
				falseStringValue = Resources.TrueStringValue;
			}
			else
			{
				falseStringValue = Resources.FalseStringValue;
			}
			current.IdentityDescription(str2, str3, trueStringValue, falseStringValue);
		}

		public static void TraceIncomingMessage(this OperationContext context)
		{
			if (TraceHelper.IsEnabled(4))
			{
				string empty = string.Empty;
				string httpMethod = string.Empty;
				if (context != null)
				{
					empty = context.IncomingMessageHeaders.To.ToString();
					httpMethod = context.IncomingMessageProperties.GetHttpMethod();
				}
				TraceHelper.Current.IncomingMessage(empty, httpMethod);
			}
		}

		public static void TraceOutgoingMessage(this OperationContext context)
		{
			if (TraceHelper.IsEnabled(5))
			{
				string empty = string.Empty;
				int statusCode = 0;
				string str = string.Empty;
				string empty1 = string.Empty;
				string str1 = string.Empty;
				if (context != null)
				{
					empty = context.IncomingMessageHeaders.To.ToString();
					foreach (object value in context.OutgoingMessageProperties.Values)
					{
						HttpResponseMessageProperty httpResponseMessageProperty = value as HttpResponseMessageProperty;
						if (httpResponseMessageProperty == null)
						{
							continue;
						}
						statusCode = (int)httpResponseMessageProperty.StatusCode;
						str = httpResponseMessageProperty.Headers.Get("request-id");
						empty1 = httpResponseMessageProperty.Headers.Get("DataServiceVersion");
						str1 = httpResponseMessageProperty.Headers.Get("Content-Type");
						break;
					}
				}
				TraceHelper.Current.OutgoingMessage(empty, statusCode, str, empty1, str1);
			}
		}
	}
}