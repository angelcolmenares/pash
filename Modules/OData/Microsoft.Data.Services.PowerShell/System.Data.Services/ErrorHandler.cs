namespace System.Data.Services
{
    using Microsoft.Data.OData;
    using System;
    using System.Data.Services.Serializers;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;

    internal class ErrorHandler
    {
        private readonly Encoding encoding;
        private readonly HandleExceptionArgs exceptionArgs;
        private const int MaxInnerErrorDepth = 100;

        private ErrorHandler(HandleExceptionArgs args, Encoding encoding)
        {
            this.exceptionArgs = args;
            this.encoding = encoding;
        }

        private static Action<Stream> CreateErrorSerializer(ODataFormat responseFormat, HandleExceptionArgs args, Encoding encoding)
        {
            ErrorHandler handler = new ErrorHandler(args, encoding);
            if (responseFormat == ODataFormat.VerboseJson)
            {
                return new Action<Stream>(handler.SerializeJsonErrorToStream);
            }
            return new Action<Stream>(handler.SerializeXmlErrorToStream);
        }

        private static ODataError CreateODataErrorFromExceptionArgs(HandleExceptionArgs args)
        {
            string str;
            string str2;
            string str3;
            DataServiceException exception = ExtractErrorValues(args.Exception, out str, out str2, out str3);
            ODataError error = new ODataError {
                ErrorCode = str,
                Message = str2,
                MessageLanguage = str3
            };
            if (args.UseVerboseErrors)
            {
                Exception exception2 = (exception == null) ? args.Exception : exception.InnerException;
                error.InnerError = (exception2 == null) ? null : new ODataInnerError(exception2);
            }
            return error;
        }

        private static DataServiceException ExtractErrorValues(Exception exception, out string errorCode, out string message, out string messageLang)
        {
            DataServiceException exception2 = exception as DataServiceException;
            if (exception2 != null)
            {
                errorCode = exception2.ErrorCode ?? string.Empty;
                message = exception2.Message ?? string.Empty;
                messageLang = exception2.MessageLanguage ?? CultureInfo.CurrentCulture.Name;
                return exception2;
            }
            errorCode = string.Empty;
            message = System.Data.Services.Strings.DataServiceException_GeneralError;
            messageLang = CultureInfo.CurrentCulture.Name;
            return null;
        }

        internal static void HandleBatchInStreamError(IDataService service, Exception exception, ODataBatchWriter batchWriter, Stream responseStream)
        {
            string str;
            Encoding encoding;
            string str2;
            Version version;
            DataServiceHostWrapper host = (service.OperationContext == null) ? null : service.OperationContext.Host;
            TryGetResponseFormatForError(service, host, RequestDescription.DataServiceDefaultResponseVersion, out str, out encoding, out str2, out version);
            HandleExceptionArgs args = new HandleExceptionArgs(exception, false, str2, service.Configuration.UseVerboseErrors);
            service.InternalHandleException(args);
            batchWriter.Flush();
            using (XmlWriter writer = XmlUtil.CreateXmlWriterAndWriteProcessingInstruction(responseStream, encoding))
            {
                ODataError error = CreateODataErrorFromExceptionArgs(args);
                ErrorUtils.WriteXmlError(writer, error, args.UseVerboseErrors, 100);
            }
        }

        internal static void HandleBatchOperationError(IDataService service, DataServiceHostWrapper host, Exception exception, ODataBatchWriter batchWriter, Stream responseStream, Version defaultResponseVersion)
        {
            string str;
            Encoding encoding;
            string str2;
            Version version;
            TryGetResponseFormatForError(service, host, defaultResponseVersion, out str, out encoding, out str2, out version);
            HandleExceptionArgs args = new HandleExceptionArgs(exception, false, str2, service.Configuration.UseVerboseErrors);
            service.InternalHandleException(args);
            Action<Stream> action = null;
            if (host != null)
            {
                host.ResponseVersion = version.ToString(2) + ";";
                host.ProcessException(args);
                action = ProcessBenignException(exception, service);
            }
            if (action == null)
            {
                ODataBatchOperationResponseMessage operationResponseMessage;
                if (host != null)
                {
                    operationResponseMessage = host.BatchServiceHost.GetOperationResponseMessage();
                    WebUtil.SetResponseHeadersForBatchRequests(operationResponseMessage, host.BatchServiceHost);
                }
                else
                {
                    operationResponseMessage = batchWriter.CreateOperationResponseMessage();
                    operationResponseMessage.StatusCode = args.ResponseStatusCode;
                }
                using (ODataMessageWriter writer = ResponseBodyWriter.CreateMessageWriter(null, service, version, operationResponseMessage, str, null))
                {
                    SerializeODataError(args, writer, responseStream, encoding);
                }
            }
        }

        internal static Action<Stream> HandleBeforeWritingException(Exception exception, IDataService service)
        {
            string str;
            Encoding encoding;
            string str2;
            Version responseVersion = null;
            DataServiceHostWrapper host = service.OperationContext.Host;
            TryGetResponseFormatForError(service, host, RequestDescription.DataServiceDefaultResponseVersion, out str, out encoding, out str2, out responseVersion);
            bool verboseResponse = (service.Configuration != null) ? service.Configuration.UseVerboseErrors : false;
            HandleExceptionArgs args = new HandleExceptionArgs(exception, false, str2, verboseResponse);
            service.InternalHandleException(args);
            host.ResponseVersion = responseVersion.ToString(2) + ";";
            host.ProcessException(args);
            Action<Stream> action = ProcessBenignException(exception, service);
            ODataFormat atom = ODataFormat.Atom;
            if (WebUtil.GetContentFormat(str) == ContentFormat.VerboseJson)
            {
                atom = ODataFormat.VerboseJson;
            }
            return (action ?? CreateErrorSerializer(atom, args, encoding));
        }

        internal static void HandleDuringWritingException(Exception exception, IDataService service, string contentType, IExceptionWriter exceptionWriter)
        {
            HandleExceptionArgs args = new HandleExceptionArgs(exception, true, contentType, service.Configuration.UseVerboseErrors);
            service.InternalHandleException(args);
            service.OperationContext.Host.ProcessException(args);
            exceptionWriter.WriteException(args);
        }

        internal static void HandleDuringWritingException(Exception exception, IDataService service, string contentType, ODataMessageWriter messageWriter, Stream responseStream, Encoding encoding)
        {
            HandleExceptionArgs args = new HandleExceptionArgs(exception, true, contentType, service.Configuration.UseVerboseErrors);
            service.InternalHandleException(args);
            service.OperationContext.Host.ProcessException(args);
            SerializeODataError(args, messageWriter, responseStream, encoding);
        }

        internal static void HandleTargetInvocationException(TargetInvocationException exception)
        {
            DataServiceException innerException = exception.InnerException as DataServiceException;
            if (innerException != null)
            {
                throw new DataServiceException(innerException.StatusCode, innerException.ErrorCode, innerException.Message, innerException.MessageLanguage, exception);
            }
        }

        private static Action<Stream> ProcessBenignException(Exception exception, IDataService service)
        {
            DataServiceException exception2 = exception as DataServiceException;
            if ((exception2 != null) && (exception2.StatusCode == 0x130))
            {
                service.OperationContext.Host.ResponseStatusCode = 0x130;
                return WebUtil.GetEmptyStreamWriter();
            }
            return null;
        }

        private void SerializeJsonError(JsonWriter writer)
        {
            string str;
            string str2;
            string str3;
            writer.StartObjectScope();
            writer.WriteName("error");
            DataServiceException exception = ExtractErrorValues(this.exceptionArgs.Exception, out str, out str2, out str3);
            writer.StartObjectScope();
            writer.WriteName("code");
            writer.WriteValue(str);
            writer.WriteName("message");
            writer.StartObjectScope();
            writer.WriteName("lang");
            writer.WriteValue(str3);
            writer.WriteName("value");
            writer.WriteValue(str2);
            writer.EndScope();
            if (this.exceptionArgs.UseVerboseErrors)
            {
                Exception exception2 = (exception == null) ? this.exceptionArgs.Exception : exception.InnerException;
                SerializeJsonException(writer, exception2);
            }
            writer.EndScope();
            writer.EndScope();
            writer.Flush();
        }

        private void SerializeJsonErrorToStream(Stream stream)
        {
            JsonWriter writer = new JsonWriter(new StreamWriter(stream, this.encoding));
            try
            {
                this.SerializeJsonError(writer);
            }
            finally
            {
                writer.Flush();
            }
        }

        private static void SerializeJsonException(JsonWriter writer, Exception exception)
        {
            string name = "innererror";
            int num = 0;
            while (exception != null)
            {
                writer.WriteName(name);
                writer.StartObjectScope();
                num++;
                string s = exception.Message ?? string.Empty;
                writer.WriteName("message");
                writer.WriteValue(s);
                string fullName = exception.GetType().FullName;
                writer.WriteName("type");
                writer.WriteValue(fullName);
                string str4 = exception.StackTrace ?? string.Empty;
                writer.WriteName("stacktrace");
                writer.WriteValue(str4);
                exception = exception.InnerException;
                name = "internalexception";
            }
            while (num > 0)
            {
                writer.EndScope();
                num--;
            }
        }

        internal static void SerializeODataError(HandleExceptionArgs args, ODataMessageWriter writer, Stream outputStream, Encoding encoding)
        {
            ODataError error = CreateODataErrorFromExceptionArgs(args);
            try
            {
                writer.WriteError(error, args.UseVerboseErrors);
            }
            catch (InvalidOperationException)
            {
                if (!WebUtil.CompareMimeType(args.ResponseContentType, "application/json;odata=verbose"))
                {
                    WebUtil.Dispose(writer);
                    using (XmlWriter writer2 = XmlWriter.Create(outputStream, XmlUtil.CreateXmlWriterSettings(encoding)))
                    {
                        ErrorUtils.WriteXmlError(writer2, error, args.UseVerboseErrors, 100);
                    }
                }
            }
        }

        private void SerializeXmlError(XmlWriter writer)
        {
            string str;
            string str2;
            string str3;
            writer.WriteStartElement("error", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            DataServiceException exception = ExtractErrorValues(this.exceptionArgs.Exception, out str, out str2, out str3);
            writer.WriteStartElement("code", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            writer.WriteString(str);
            writer.WriteEndElement();
            writer.WriteStartElement("message", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            writer.WriteAttributeString("xml", "lang", null, str3);
            writer.WriteString(str2);
            writer.WriteEndElement();
            if (this.exceptionArgs.UseVerboseErrors)
            {
                Exception exception2 = (exception == null) ? this.exceptionArgs.Exception : exception.InnerException;
                SerializeXmlException(writer, exception2);
            }
            writer.WriteEndElement();
            writer.Flush();
        }

        internal static void SerializeXmlError(HandleExceptionArgs args, XmlWriter writer)
        {
            new ErrorHandler(args, null).SerializeXmlError(writer);
        }

        private void SerializeXmlErrorToStream(Stream stream)
        {
            using (XmlWriter writer = XmlUtil.CreateXmlWriterAndWriteProcessingInstruction(stream, this.encoding))
            {
                this.SerializeXmlError(writer);
            }
        }

        private static void SerializeXmlException(XmlWriter writer, Exception exception)
        {
            string localName = "innererror";
            int num = 0;
            while (exception != null)
            {
                writer.WriteStartElement(localName, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
                num++;
                string text = exception.Message ?? string.Empty;
                writer.WriteStartElement("message", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
                writer.WriteString(text);
                writer.WriteEndElement();
                string fullName = exception.GetType().FullName;
                writer.WriteStartElement("type", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
                writer.WriteString(fullName);
                writer.WriteEndElement();
                string str4 = exception.StackTrace ?? string.Empty;
                writer.WriteStartElement("stacktrace", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
                writer.WriteString(str4);
                writer.WriteEndElement();
                exception = exception.InnerException;
                localName = "internalexception";
            }
            while (num > 0)
            {
                writer.WriteEndElement();
                num--;
            }
        }

        internal static bool TryGetMinResponseVersionForError(DataServiceHostWrapper host, Version maxProtocolVersion, out Version responseVersion)
        {
            responseVersion = null;
            try
            {
                Version version;
                if (((maxProtocolVersion > RequestDescription.Version2Dot0) && (host.RequestMaxVersion > RequestDescription.Version2Dot0)) && WebUtil.ResponseMediaTypeWouldBeJsonLight(host.RequestAccept, false))
                {
                    responseVersion = RequestDescription.Version3Dot0;
                }
                if (!host.TryGetMinDataServiceVersionFromWrappedHost(out version))
                {
                    return (responseVersion != null);
                }
                if ((responseVersion == null) || (version > responseVersion))
                {
                    responseVersion = version;
                }
                if (maxProtocolVersion < RequestDescription.Version3Dot0)
                {
                    responseVersion = RequestDescription.DataServiceDefaultResponseVersion;
                    return true;
                }
                if (!RequestDescription.IsKnownRequestVersion(responseVersion) || (responseVersion > maxProtocolVersion))
                {
                    return false;
                }
                return true;
            }
            catch (Exception exception)
            {
                if (!CommonUtil.IsCatchableExceptionType(exception))
                {
                    throw;
                }
            }
            return false;
        }

        private static void TryGetResponseFormatForError(string accept, string acceptCharset, Version responseVersion, out string contentType, out Encoding encoding, out string contentTypeWithCharsetAppended)
        {
            contentType = null;
            encoding = null;
            contentTypeWithCharsetAppended = null;
            if (accept != null)
            {
                try
                {
                    contentType = WebUtil.SelectResponseMediaType(accept, false, responseVersion < RequestDescription.Version3Dot0);
                }
                catch (DataServiceException)
                {
                }
            }
            if (acceptCharset != null)
            {
                try
                {
                    encoding = HttpProcessUtility.EncodingFromAcceptCharset(acceptCharset);
                }
                catch (DataServiceException)
                {
                }
            }
            contentType = contentType ?? "application/xml";
            encoding = encoding ?? HttpProcessUtility.FallbackEncoding;
            contentTypeWithCharsetAppended = contentType + ";" + "charset" + "=" + encoding.WebName;
        }

        private static void TryGetResponseFormatForError(IDataService service, DataServiceHostWrapper host, Version defaultResponseVersion, out string contentType, out Encoding encoding, out string contentTypeWithCharsetAppended, out Version responseVersion)
        {
            if (host == null)
            {
                responseVersion = defaultResponseVersion;
                TryGetResponseFormatForError(null, null, null, out contentType, out encoding, out contentTypeWithCharsetAppended);
            }
            else
            {
                Version maxProtocolVersion = service.Configuration.DataServiceBehavior.MaxProtocolVersion.ToVersion();
                if (!TryGetMinResponseVersionForError(host, maxProtocolVersion, out responseVersion))
                {
                    responseVersion = defaultResponseVersion;
                }
                TryGetResponseFormatForError(host.RequestAccept, host.RequestAcceptCharSet, responseVersion, out contentType, out encoding, out contentTypeWithCharsetAppended);
            }
        }
    }
}

