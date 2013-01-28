using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using System;
using System.Collections.Specialized;
using System.Data.Services;
using System.Net;
using System.Web;

namespace Microsoft.Management.Odata.Core
{
	internal static class UriParametersHelper
	{
		private const string ODataReservedPrefix = "$";

		public static void AddParametersToCommand(ICommand command, Uri uri)
		{
			try
			{
				NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(uri.Query);
				foreach (string key in nameValueCollection.Keys)
				{
					if (key == null || string.IsNullOrWhiteSpace(key))
					{
						object[] str = new object[1];
						str[0] = uri.ToString();
						throw new DataServiceException(0x190, ExceptionHelpers.GetDataServiceExceptionMessage(HttpStatusCode.BadRequest, Resources.InvalidQueryParameterMessage, str));
					}
					else
					{
						string[] values = nameValueCollection.GetValues(key);
						if ((int)values.Length == 1)
						{
							string str1 = values[0];
							if (!string.IsNullOrWhiteSpace(str1))
							{
								string str2 = key.Trim();
								if (str2.StartsWith("$", StringComparison.OrdinalIgnoreCase))
								{
									continue;
								}
								try
								{
									command.AddParameter(str2, str1.Trim(), true);
								}
								catch (ArgumentException argumentException1)
								{
									ArgumentException argumentException = argumentException1;
									object[] objArray = new object[1];
									objArray[0] = uri.ToString();
									throw new DataServiceException(0x190, string.Empty, ExceptionHelpers.GetDataServiceExceptionMessage(HttpStatusCode.BadRequest, Resources.InvalidQueryParameterMessage, objArray), string.Empty, argumentException);
								}
							}
							else
							{
								object[] objArray1 = new object[1];
								objArray1[0] = uri.ToString();
								throw new DataServiceException(0x190, ExceptionHelpers.GetExceptionMessage(Resources.InvalidQueryParameterMessage, objArray1));
							}
						}
						else
						{
							object[] objArray2 = new object[1];
							objArray2[0] = uri.ToString();
							throw new DataServiceException(0x190, ExceptionHelpers.GetDataServiceExceptionMessage(HttpStatusCode.BadRequest, Resources.InvalidQueryParameterMessage, objArray2));
						}
					}
				}
			}
			catch (Exception exception)
			{
				TraceHelper.Current.UriParsingFailed(uri.ToString());
				throw;
			}
		}
	}
}