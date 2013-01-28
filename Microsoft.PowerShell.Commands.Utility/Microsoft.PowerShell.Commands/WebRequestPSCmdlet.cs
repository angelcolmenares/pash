namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using Microsoft.Win32;
    using mshtml;
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Xml;

    public abstract class WebRequestPSCmdlet : PSCmdlet
    {
        private string _originalFilePath;
        private WebRequest _webRequest;
        private const string DefaultProtocol = "http://";
        private int maximumRedirection = -1;
        private WebRequestMethod method;

        protected WebRequestPSCmdlet()
        {
        }

        private System.Uri CheckProtocol(System.Uri uri)
        {
            if (null == uri)
            {
                throw new ArgumentNullException("uri");
            }
            if (!uri.IsAbsoluteUri)
            {
                uri = new System.Uri("http://" + uri.OriginalString);
            }
            return uri;
        }

        internal virtual void FillRequestStream(WebRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (this.ContentType != null)
            {
                request.ContentType = this.ContentType;
            }
            else if (this.Method == WebRequestMethod.Post)
            {
                request.ContentType = "application/x-www-form-urlencoded";
            }
            if (this.Body != null)
            {
                object body = this.Body;
                PSObject obj3 = this.Body as PSObject;
                if (obj3 != null)
                {
                    body = obj3.BaseObject;
                }
                if (!(body is HtmlWebResponseObject))
                {
                    if (body is FormObject)
                    {
                        FormObject obj5 = body as FormObject;
                        this.SetRequestContent(request, obj5.Fields);
                    }
                    else if ((body is IDictionary) && (request.Method != "GET"))
                    {
                        IDictionary content = body as IDictionary;
                        this.SetRequestContent(request, content);
                    }
                    else if (body is XmlNode)
                    {
                        XmlNode xmlNode = body as XmlNode;
                        this.SetRequestContent(request, xmlNode);
                    }
                    else if (body is Stream)
                    {
                        Stream contentStream = body as Stream;
                        this.SetRequestContent(request, contentStream);
                    }
                    else if (body is byte[])
                    {
                        byte[] buffer = body as byte[];
                        this.SetRequestContent(request, buffer);
                    }
                    else
                    {
                        this.SetRequestContent(request, (string) LanguagePrimitives.ConvertTo(body, typeof(string), CultureInfo.InvariantCulture));
                    }
                }
                else
                {
                    HtmlWebResponseObject obj4 = body as HtmlWebResponseObject;
                    if (obj4.Forms.Count == 1)
                    {
                        this.SetRequestContent(request, obj4.Forms[0].Fields);
                    }
                }
            }
            else
            {
                if (this.InFile != null)
                {
                    try
                    {
                        using (FileStream stream2 = new FileStream(this.InFile, FileMode.Open))
                        {
                            this.SetRequestContent(request, stream2);
                        }
                        return;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        throw new UnauthorizedAccessException(string.Format(CultureInfo.InvariantCulture, WebCmdletStrings.AccessDenied, new object[] { this._originalFilePath }));
                    }
                }
                request.ContentLength = 0L;
            }
        }

        private string FormatDictionary(IDictionary content)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }
            StringBuilder builder = new StringBuilder();
            foreach (string str in content.Keys)
            {
                if (0 < builder.Length)
                {
                    builder.Append("&");
                }
                object obj2 = content[str];
                string str2 = HttpUtility.UrlEncode(str);
                string str3 = string.Empty;
                if (obj2 != null)
                {
                    str3 = HttpUtility.UrlEncode(obj2.ToString());
                }
                builder.AppendFormat("{0}={1}", str2, str3);
            }
            return builder.ToString();
        }

        internal virtual WebRequest GetRequest(System.Uri uri)
        {
            IDictionary dictionary;
            uri = this.CheckProtocol(uri);
            LanguagePrimitives.TryConvertTo<IDictionary>(this.Body, out dictionary);
            if ((dictionary != null) && ((this.Method == WebRequestMethod.Default) || (this.Method == WebRequestMethod.Get)))
            {
                UriBuilder builder = new UriBuilder(uri);
                if ((builder.Query != null) && (builder.Query.Length > 1))
                {
                    builder.Query = builder.Query.Substring(1) + "&" + this.FormatDictionary(dictionary);
                }
                else
                {
                    builder.Query = this.FormatDictionary(dictionary);
                }
                uri = builder.Uri;
                this.Body = null;
            }
            WebRequest request = WebRequest.Create(uri);
            if (0 < this.WebSession.Headers.Count)
            {
                try
                {
                    request.Headers.Clear();
                    foreach (string str in this.WebSession.Headers.Keys)
                    {
                        request.Headers[str] = this.WebSession.Headers[str];
                    }
                }
                catch (NotImplementedException)
                {
                }
            }
            if (this.WebSession.UseDefaultCredentials)
            {
                request.UseDefaultCredentials = true;
            }
            else if (this.WebSession.Credentials != null)
            {
                request.Credentials = this.WebSession.Credentials;
            }
            if (this.WebSession.Proxy != null)
            {
                request.Proxy = this.WebSession.Proxy;
            }
            if (this.Method != WebRequestMethod.Default)
            {
                request.Method = this.Method.ToString().ToUpperInvariant();
            }
            HttpWebRequest request2 = request as HttpWebRequest;
            if (request2 != null)
            {
                request2.CookieContainer = this.WebSession.Cookies;
                request2.UserAgent = this.WebSession.UserAgent;
                if (this.WebSession.Certificates != null)
                {
                    request2.ClientCertificates = this.WebSession.Certificates;
                }
                if (-1 < this.WebSession.MaximumRedirection)
                {
                    if (this.WebSession.MaximumRedirection == 0)
                    {
                        request2.AllowAutoRedirect = false;
                    }
                    else
                    {
                        request2.MaximumAutomaticRedirections = this.WebSession.MaximumRedirection;
                    }
                }
                if (0 < this.TimeoutSec)
                {
                    if (this.TimeoutSec > 0x20c49b)
                    {
                        request2.Timeout = 0x7fffffff;
                    }
                    else
                    {
                        request2.Timeout = this.TimeoutSec * 0x3e8;
                    }
                }
                if (this.DisableKeepAlive != 0)
                {
                    request2.KeepAlive = false;
                }
                if (this.TransferEncoding != null)
                {
                    request2.SendChunked = true;
                    request2.TransferEncoding = this.TransferEncoding;
                }
            }
            return request;
        }

        internal virtual WebResponse GetResponse(WebRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            HttpWebRequest request2 = request as HttpWebRequest;
            TimeoutState state = null;
            if ((request2 != null) && (request2.Timeout > 0))
            {
                state = new TimeoutState(request2);
            }
            this._webRequest = request;
            WebRequestState state2 = new WebRequestState(request);
            IAsyncResult result = request.BeginGetResponse(new AsyncCallback(WebRequestPSCmdlet.ResponseCallback), state2);
            if (state != null)
            {
                ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(WebRequestPSCmdlet.TimeoutCallback), state, state.httpRequest.Timeout, true);
            }
            state2.waithandle.WaitOne(-1, false);
            this._webRequest = null;
            if (state2.webException == null)
            {
                return state2.response;
            }
            if (((state != null) && state.abort) && state2.webException.Status.Equals(WebExceptionStatus.RequestCanceled))
            {
                throw new WebException(WebCmdletStrings.RequestTimeout, WebExceptionStatus.Timeout);
            }
            throw state2.webException;
        }

        private ErrorRecord GetValidationError(string resourceId, string errorId)
        {
            return new ErrorRecord(new ValidationMetadataException(this.GetResourceString("WebCmdletStrings", resourceId)), errorId, ErrorCategory.InvalidArgument, this);
        }

        private ErrorRecord GetValidationError(string resourceId, string errorId, params object[] args)
        {
            string resourceString = this.GetResourceString("WebCmdletStrings", resourceId);
            return new ErrorRecord(new ValidationMetadataException(string.Format(CultureInfo.InvariantCulture, resourceString, args)), errorId, ErrorCategory.InvalidArgument, this);
        }

        internal virtual void PrepareSession()
        {
            if (this.WebSession == null)
            {
                this.WebSession = new WebRequestSession();
            }
            if (this.SessionVariable != null)
            {
                base.SessionState.PSVariable.Set(this.SessionVariable, this.WebSession);
            }
            if (this.Credential != null)
            {
                NetworkCredential networkCredential = this.Credential.GetNetworkCredential();
                this.WebSession.Credentials = networkCredential;
                this.WebSession.UseDefaultCredentials = false;
            }
            else if (this.UseDefaultCredentials != 0)
            {
                this.WebSession.UseDefaultCredentials = true;
            }
            if (this.CertificateThumbprint != null)
            {
                X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.OpenExistingOnly);
                X509Certificate2Collection certificates2 = store.Certificates.Find(X509FindType.FindByThumbprint, this.CertificateThumbprint, false);
                if (certificates2.Count == 0)
                {
                    CryptographicException exception = new CryptographicException(WebCmdletStrings.ThumbprintNotFound);
                    throw exception;
                }
                X509Certificate2Enumerator enumerator = certificates2.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    X509Certificate current = enumerator.Current;
                    this.WebSession.AddCertificate(current);
                }
            }
            if (this.Certificate != null)
            {
                this.WebSession.AddCertificate(this.Certificate);
            }
            if (this.UserAgent != null)
            {
                this.WebSession.UserAgent = this.UserAgent;
            }
            if (null != this.Proxy)
            {
                WebProxy proxy = new WebProxy(this.Proxy) {
                    BypassProxyOnLocal = false
                };
                if (this.ProxyCredential != null)
                {
                    proxy.Credentials = this.ProxyCredential.GetNetworkCredential();
                    proxy.UseDefaultCredentials = false;
                }
                else if (this.ProxyUseDefaultCredentials != 0)
                {
                    proxy.UseDefaultCredentials = true;
                }
                this.WebSession.Proxy = proxy;
            }
            if (-1 < this.MaximumRedirection)
            {
                this.WebSession.MaximumRedirection = this.MaximumRedirection;
            }
            if (this.Headers != null)
            {
                foreach (string str in this.Headers.Keys)
                {
                    this.WebSession.Headers[str] = (string) this.Headers[str];
                }
            }
        }

        protected override void ProcessRecord()
        {
            try
            {
                this.ValidateParameters();
                this.PrepareSession();
                WebRequest request = this.GetRequest(this.Uri);
                this.FillRequestStream(request);
                ServicePointManager.Expect100Continue = false;
                try
                {
                    string text = string.Format(CultureInfo.CurrentCulture, "{0} {1} with {2}-byte payload", new object[] { request.Method, request.RequestUri, request.ContentLength });
                    base.WriteVerbose(text);
                    WebResponse response = this.GetResponse(request);
                    string contentType = ContentHelper.GetContentType(response);
                    string str3 = string.Format(CultureInfo.CurrentCulture, "received {0}-byte response of content type {1}", new object[] { response.ContentLength, contentType });
                    base.WriteVerbose(str3);
                    this.ProcessResponse(response);
                    this.UpdateSession(response);
                    HttpWebRequest targetObject = request as HttpWebRequest;
                    if ((targetObject != null) && !targetObject.AllowAutoRedirect)
                    {
                        HttpWebResponse response2 = response as HttpWebResponse;
                        if (((response2.StatusCode == HttpStatusCode.Found) || (response2.StatusCode == HttpStatusCode.MovedPermanently)) || (response2.StatusCode == HttpStatusCode.MovedPermanently))
                        {
                            ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(), "MaximumRedirectExceeded", ErrorCategory.InvalidOperation, targetObject) {
                                ErrorDetails = new ErrorDetails(WebCmdletStrings.MaximumRedirectionCountExceeded)
                            };
                            base.WriteError(errorRecord);
                        }
                    }
                }
                catch (WebException exception)
                {
                    WebException exception2 = exception;
                    string outerText = string.Empty;
                    try
                    {
                        if (exception.Response.ContentLength > 0L)
                        {
                            outerText = new StreamReader(StreamHelper.GetResponseStream(exception.Response)).ReadToEnd();
                            this.VerifyInternetExplorerAvailable(false);
                            try
                            {
                                IHTMLDocument2 document = (IHTMLDocument2) ((HTMLDocument) Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("25336920-03F9-11CF-8FD0-00AA00686F13"))));
                                document.write(new object[] { outerText });
                                outerText = document.body.outerText;
                            }
                            catch (COMException)
                            {
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                    ErrorRecord record2 = new ErrorRecord(exception2, "WebCmdletWebResponseException", ErrorCategory.InvalidOperation, request);
                    if (!string.IsNullOrEmpty(outerText))
                    {
                        record2.ErrorDetails = new ErrorDetails(outerText);
                    }
                    base.ThrowTerminatingError(record2);
                }
            }
            catch (CryptographicException exception3)
            {
                ErrorRecord record3 = new ErrorRecord(exception3, "WebCmdletCertificateException", ErrorCategory.SecurityError, null);
                base.ThrowTerminatingError(record3);
            }
            catch (NotSupportedException exception4)
            {
                ErrorRecord record4 = new ErrorRecord(exception4, "WebCmdletIEDomNotSupportedException", ErrorCategory.NotImplemented, null);
                base.ThrowTerminatingError(record4);
            }
        }

        internal abstract void ProcessResponse(WebResponse response);
        private string QualifyFilePath(string path)
        {
            return PathUtils.ResolveFilePath(path, this, false);
        }

        private static void ResponseCallback(IAsyncResult asyncResult)
        {
            WebRequestState asyncState = (WebRequestState) asyncResult.AsyncState;
            try
            {
                asyncState.response = asyncState.request.EndGetResponse(asyncResult);
            }
            catch (WebException exception)
            {
                asyncState.response = null;
                asyncState.webException = exception;
            }
            finally
            {
                asyncState.waithandle.Set();
            }
        }

        internal long SetRequestContent(WebRequest request, IDictionary content)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }
            string str = this.FormatDictionary(content);
            return this.SetRequestContent(request, str);
        }

        internal long SetRequestContent(WebRequest request, Stream contentStream)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (contentStream == null)
            {
                throw new ArgumentNullException("contentStream");
            }
            request.ContentLength = contentStream.Length;
            StreamHelper.WriteToStream(contentStream, request.GetRequestStream(), this);
            return request.ContentLength;
        }

        internal long SetRequestContent(WebRequest request, string content)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (content != null)
            {
                byte[] input = StreamHelper.EncodeToBytes(content);
                request.ContentLength = input.Length;
                StreamHelper.WriteToStream(input, request.GetRequestStream());
            }
            else
            {
                request.ContentLength = 0L;
            }
            return request.ContentLength;
        }

        internal long SetRequestContent(WebRequest request, XmlNode xmlNode)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (xmlNode != null)
            {
                byte[] input = null;
                XmlDocument document = xmlNode as XmlDocument;
                if ((document != null) && (document.FirstChild is XmlDeclaration))
                {
                    XmlDeclaration firstChild = document.FirstChild as XmlDeclaration;
                    Encoding encoding = Encoding.GetEncoding(firstChild.Encoding);
                    input = StreamHelper.EncodeToBytes(document.OuterXml, encoding);
                }
                else
                {
                    input = StreamHelper.EncodeToBytes(xmlNode.OuterXml);
                }
                request.ContentLength = input.Length;
                StreamHelper.WriteToStream(input, request.GetRequestStream());
            }
            else
            {
                request.ContentLength = 0L;
            }
            return request.ContentLength;
        }

        internal long SetRequestContent(WebRequest request, byte[] content)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (content != null)
            {
                request.ContentLength = content.Length;
                StreamHelper.WriteToStream(content, request.GetRequestStream());
            }
            else
            {
                request.ContentLength = 0L;
            }
            return request.ContentLength;
        }

        protected override void StopProcessing()
        {
            if (this._webRequest != null)
            {
                this._webRequest.Abort();
            }
        }

        private static void TimeoutCallback(object state, bool timeout)
        {
            if (timeout)
            {
                TimeoutState state2 = state as TimeoutState;
                if (state2 != null)
                {
                    state2.abort = true;
                    state2.httpRequest.Abort();
                }
            }
        }

        internal virtual void UpdateSession(WebResponse response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            HttpWebResponse response2 = response as HttpWebResponse;
            if ((this.WebSession != null) && (response2 != null))
            {
                this.WebSession.Cookies.Add(response2.Cookies);
            }
        }

        internal virtual void ValidateParameters()
        {
            if ((this.WebSession != null) && (this.SessionVariable != null))
            {
                ErrorRecord validationError = this.GetValidationError("SessionConflict", "WebCmdletSessionConflictException");
                base.ThrowTerminatingError(validationError);
            }
            if ((this.UseDefaultCredentials != 0) && (this.Credential != null))
            {
                ErrorRecord errorRecord = this.GetValidationError("CredentialConflict", "WebCmdletCredentialConflictException");
                base.ThrowTerminatingError(errorRecord);
            }
            if ((this.ProxyUseDefaultCredentials != 0) && (this.ProxyCredential != null))
            {
                ErrorRecord record3 = this.GetValidationError("ProxyCredentialConflict", "WebCmdletProxyCredentialConflictException");
                base.ThrowTerminatingError(record3);
            }
            else if ((null == this.Proxy) && ((this.ProxyCredential != null) || (this.ProxyUseDefaultCredentials != 0)))
            {
                ErrorRecord record4 = this.GetValidationError("ProxyUriNotSupplied", "WebCmdletProxyUriNotSuppliedException");
                base.ThrowTerminatingError(record4);
            }
            if ((this.Body != null) && (this.InFile != null))
            {
                ErrorRecord record5 = this.GetValidationError("BodyConflict", "WebCmdletBodyConflictException");
                base.ThrowTerminatingError(record5);
            }
            if (this.InFile != null)
            {
                ProviderInfo provider = null;
                ErrorRecord record6 = null;
                try
                {
                    Collection<string> resolvedProviderPathFromPSPath = base.GetResolvedProviderPathFromPSPath(this.InFile, out provider);
                    if (!provider.Name.Equals("FileSystem", StringComparison.OrdinalIgnoreCase))
                    {
                        record6 = this.GetValidationError("NotFilesystemPath", "WebCmdletInFileNotFilesystemPathException", new object[] { this.InFile });
                    }
                    else if (resolvedProviderPathFromPSPath.Count > 1)
                    {
                        record6 = this.GetValidationError("MultiplePathsResolved", "WebCmdletInFileMultiplePathsResolvedException", new object[] { this.InFile });
                    }
                    else if (resolvedProviderPathFromPSPath.Count == 0)
                    {
                        record6 = this.GetValidationError("NoPathResolved", "WebCmdletInFileNoPathResolvedException", new object[] { this.InFile });
                    }
                    else
                    {
                        if (Directory.Exists(resolvedProviderPathFromPSPath[0]))
                        {
                            record6 = this.GetValidationError("DirecotryPathSpecified", "WebCmdletInFileNotFilePathException", new object[] { this.InFile });
                        }
                        this._originalFilePath = this.InFile;
                        this.InFile = resolvedProviderPathFromPSPath[0];
                    }
                }
                catch (ItemNotFoundException exception)
                {
                    record6 = new ErrorRecord(exception.ErrorRecord, exception);
                }
                catch (ProviderNotFoundException exception2)
                {
                    record6 = new ErrorRecord(exception2.ErrorRecord, exception2);
                }
                catch (System.Management.Automation.DriveNotFoundException exception3)
                {
                    record6 = new ErrorRecord(exception3.ErrorRecord, exception3);
                }
                if (record6 != null)
                {
                    base.ThrowTerminatingError(record6);
                }
            }
            if ((this.PassThru != 0) && (this.OutFile == null))
            {
                ErrorRecord record7 = this.GetValidationError("OutFileMissing", "WebCmdletOutFileMissingException");
                base.ThrowTerminatingError(record7);
            }
        }

        protected void VerifyInternetExplorerAvailable(bool checkComObject)
        {
            bool flag = false;
            string[] strArray = new string[] { @"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Internet Explorer\Main", @"HKEY_CURRENT_USER\Software\Policies\Microsoft\Internet Explorer\Main", @"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main", @"HKEY_LOCAL_MACHINE\Software\Microsoft\Internet Explorer\Main" };
            foreach (string str in strArray)
            {
                object obj2 = Registry.GetValue(str, "DisableFirstRunCustomize", string.Empty);
                if (((obj2 != null) && !string.Empty.Equals(obj2)) && (Convert.ToInt32(obj2, CultureInfo.InvariantCulture) > 0))
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Internet Explorer\Main"))
                {
                    if (key == null)
                    {
                        throw new NotSupportedException(WebCmdletStrings.IEDomNotSupported);
                    }
                    foreach (string str2 in key.GetValueNames())
                    {
                        if (str2.IndexOf("RunOnce", StringComparison.OrdinalIgnoreCase) > -1)
                        {
                            flag = true;
                            goto Label_00E8;
                        }
                    }
                }
            }
        Label_00E8:
            if (!flag)
            {
                throw new NotSupportedException(WebCmdletStrings.IEDomNotSupported);
            }
            if (checkComObject)
            {
                try
                {
                    IHTMLDocument2 document1 = (IHTMLDocument2) ((HTMLDocument) Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("25336920-03F9-11CF-8FD0-00AA00686F13"))));
                }
                catch (COMException)
                {
                    throw new NotSupportedException(WebCmdletStrings.IEDomNotSupported);
                }
            }
        }

        [Parameter(ValueFromPipeline=true)]
        public virtual object Body { get; set; }

        [Parameter, ValidateNotNull]
        public virtual X509Certificate Certificate { get; set; }

        [ValidateNotNullOrEmpty, Parameter]
        public virtual string CertificateThumbprint { get; set; }

        [Parameter]
        public virtual string ContentType { get; set; }

        [Credential, Parameter]
        public virtual PSCredential Credential { get; set; }

        [Parameter]
        public virtual SwitchParameter DisableKeepAlive { get; set; }

        [Parameter]
        public virtual IDictionary Headers { get; set; }

        [Parameter]
        public virtual string InFile { get; set; }

        [ValidateRange(0, 0x7fffffff), Parameter]
        public virtual int MaximumRedirection
        {
            get
            {
                return this.maximumRedirection;
            }
            set
            {
                this.maximumRedirection = value;
            }
        }

        [Parameter]
        public virtual WebRequestMethod Method
        {
            get
            {
                return this.method;
            }
            set
            {
                this.method = value;
            }
        }

        [Parameter]
        public virtual string OutFile { get; set; }

        [Parameter]
        public virtual SwitchParameter PassThru { get; set; }

        [Parameter]
        public virtual System.Uri Proxy { get; set; }

        [Credential, Parameter]
        public virtual PSCredential ProxyCredential { get; set; }

        [Parameter]
        public virtual SwitchParameter ProxyUseDefaultCredentials { get; set; }

        internal string QualifiedOutFile
        {
            get
            {
                return this.QualifyFilePath(this.OutFile);
            }
        }

        [Parameter, Alias(new string[] { "SV" })]
        public virtual string SessionVariable { get; set; }

        internal bool ShouldSaveToOutFile
        {
            get
            {
                return !string.IsNullOrEmpty(this.OutFile);
            }
        }

        internal bool ShouldWriteToPipeline
        {
            get
            {
                if (this.ShouldSaveToOutFile)
                {
                    return (bool) this.PassThru;
                }
                return true;
            }
        }

        [Parameter]
        public virtual int TimeoutSec { get; set; }

        [ValidateSet(new string[] { "chunked", "compress", "deflate", "gzip", "identity" }, IgnoreCase=true), Parameter]
        public virtual string TransferEncoding { get; set; }

        [ValidateNotNullOrEmpty, Parameter(Position=0, Mandatory=true)]
        public virtual System.Uri Uri { get; set; }

        [Parameter]
        public virtual SwitchParameter UseDefaultCredentials { get; set; }

        [Parameter]
        public virtual string UserAgent { get; set; }

        [Parameter]
        public virtual WebRequestSession WebSession { get; set; }

        private class TimeoutState
        {
            public bool abort;
            public HttpWebRequest httpRequest;

            public TimeoutState(HttpWebRequest request)
            {
                this.httpRequest = request;
                this.abort = false;
            }
        }

        private class WebRequestState
        {
            public WebRequest request;
            public WebResponse response;
            public ManualResetEvent waithandle;
            public WebException webException;

            public WebRequestState(WebRequest webRequest)
            {
                this.request = webRequest;
                this.response = null;
                this.webException = null;
                this.waithandle = new ManualResetEvent(false);
            }
        }
    }
}

