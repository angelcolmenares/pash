namespace Microsoft.PowerShell.Commands
{
    using mshtml;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Management.Automation;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;

    public class HtmlWebResponseObject : WebResponseObject
    {
        private WebCmdletElementCollection _allElements;
        private static Regex _attribNameValueRegex;
        private static Regex _attribsRegex;
        private readonly System.Management.Automation.ExecutionContext _executionContext;
        private FormObjectCollection _forms;
        private bool _htmlParsed;
        private WebCmdletElementCollection _images;
        private WebCmdletElementCollection _inputFields;
        private WebCmdletElementCollection _links;
        private ManualResetEventSlim _loadDocumentResetEvent;
        private HTMLDocumentEvents2_onreadystatechangeEventHandler _onreadystatechangeEventHandler;
        private IHTMLDocument2 _parsedHtml;
        private Exception _parsingException;
        private WebCmdletElementCollection _scripts;
        private ManualResetEventSlim _stateChangeResetEvent;
        private bool _stopWorkerThread;
        private static Regex _tagRegex;

        internal HtmlWebResponseObject(WebResponse response, System.Management.Automation.ExecutionContext executionContext) : this(response, null, executionContext)
        {
        }

        internal HtmlWebResponseObject(WebResponse response, MemoryStream contentStream, System.Management.Automation.ExecutionContext executionContext) : base(response, contentStream)
        {
            if (executionContext == null)
            {
                throw PSTraceSource.NewArgumentNullException("executionContext");
            }
            this._executionContext = executionContext;
            this.InitializeContent();
            this.InitializeRawContent(response);
        }

        private FormObjectCollection BuildFormsCollection()
        {
            FormObjectCollection objects = new FormObjectCollection();
            this.EnsureHtmlParser();
            foreach (IHTMLFormElement element in this._parsedHtml.forms)
            {
                string elementId = this.GetElementId(element as IHTMLElement);
                if (elementId == null)
                {
                    elementId = element.name;
                }
                FormObject item = new FormObject(elementId, element.method, element.action);
                foreach (IHTMLElement element2 in element)
                {
                    IHTMLInputElement element3 = element2 as IHTMLInputElement;
                    if (element3 != null)
                    {
                        elementId = this.GetElementId(element3 as IHTMLElement);
                        if (elementId == null)
                        {
                            elementId = element3.name;
                        }
                        item.AddField(elementId, element3.value);
                    }
                }
                objects.Add(item);
            }
            return objects;
        }

        private PSObject CreateHtmlObject(IHTMLElement element, bool addTagName)
        {
            PSObject elementObject = new PSObject();
            elementObject.Properties.Add(new PSNoteProperty("innerHTML", element.innerHTML));
            elementObject.Properties.Add(new PSNoteProperty("innerText", element.innerText));
            elementObject.Properties.Add(new PSNoteProperty("outerHTML", element.outerHTML));
            elementObject.Properties.Add(new PSNoteProperty("outerText", element.outerText));
            if (addTagName)
            {
                elementObject.Properties.Add(new PSNoteProperty("tagName", element.tagName));
            }
            this.ParseAttributes(element.outerHTML, elementObject);
            return elementObject;
        }

        private void EnsureHtmlParser()
        {
            if (!this._htmlParsed)
            {
                this._stopWorkerThread = false;
                this._parsingException = null;
                this._stateChangeResetEvent = new ManualResetEventSlim();
                this._loadDocumentResetEvent = new ManualResetEventSlim();
                this._onreadystatechangeEventHandler = new HTMLDocumentEvents2_onreadystatechangeEventHandler(this.ReadyStateChanged);
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.LoadDocumentInMtaThread));
                for (bool flag = true; flag; flag = !this._loadDocumentResetEvent.Wait(500))
                {
                    if (this._executionContext.CurrentPipelineStopping)
                    {
                        this._stopWorkerThread = true;
                        this._loadDocumentResetEvent.Wait();
                        break;
                    }
                }
                if (this._executionContext.CurrentPipelineStopping)
                {
                    throw new PipelineStoppedException();
                }
                if (this._parsingException != null)
                {
                    throw this._parsingException;
                }
                this._htmlParsed = true;
            }
            if (_tagRegex == null)
            {
                _tagRegex = new Regex("<\\w+((\\s+[^\"'>/=\\s\\p{Cc}]+(\\s*=\\s*(?:\".*?\"|'.*?'|[^'\">\\s]+))?)+\\s*|\\s*)/?>", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            if (_attribsRegex == null)
            {
                _attribsRegex = new Regex("(?<=\\s+)([^\"'>/=\\s\\p{Cc}]+(\\s*=\\s*(?:\".*?\"|'.*?'|[^'\">\\s]+))?)", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            if (_attribNameValueRegex == null)
            {
                _attribNameValueRegex = new Regex("([^\"'>/=\\s\\p{Cc}]+)(?:\\s*=\\s*(?:\"(.*?)\"|'(.*?)'|([^'\">\\s]+)))?", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }

        private string GetElementId(IHTMLElement element)
        {
            if (element != null)
            {
                return element.id;
            }
            return null;
        }

        private void InitializeContent()
        {
            if (ContentHelper.IsText(ContentHelper.GetContentType(base.BaseResponse)))
            {
                string characterSet = WebResponseHelper.GetCharacterSet(base.BaseResponse);
                this.Content = StreamHelper.DecodeStream(base.RawContentStream, characterSet);
            }
            else
            {
                this.Content = string.Empty;
            }
        }

        private void InitializeRawContent(WebResponse baseResponse)
        {
            StringBuilder rawContentHeader = ContentHelper.GetRawContentHeader(baseResponse);
            rawContentHeader.Append(this.Content);
            base.RawContent = rawContentHeader.ToString();
        }

        private void LoadDocumentInMtaThread(object state)
        {
            try
            {
                this._parsedHtml = (IHTMLDocument2) ((HTMLDocument) Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("25336920-03F9-11CF-8FD0-00AA00686F13"))));
                HTMLDocumentEvents2_Event target = (HTMLDocumentEvents2_Event) this._parsedHtml;
                //TODO: REVIEW: new ComAwareEventInfo(typeof(HTMLDocumentEvents2_Event), "onreadystatechange").AddEventHandler(target, this._onreadystatechangeEventHandler);
                this._parsedHtml.write(new object[] { this.Content });
                this._parsedHtml.close();
                for (bool flag = true; flag && !this._stopWorkerThread; flag = !this._stateChangeResetEvent.Wait(100))
                {
                    if (string.Equals("complete", this._parsedHtml.readyState, StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                }
				//TODO: REVIEW: new ComAwareEventInfo(typeof(HTMLDocumentEvents2_Event), "onreadystatechange").RemoveEventHandler(target, this._onreadystatechangeEventHandler);
            }
            catch (Exception exception)
            {
                this._parsingException = exception;
            }
            finally
            {
                this._loadDocumentResetEvent.Set();
            }
        }

        private void ParseAttributes(string outerHtml, PSObject elementObject)
        {
            if (!string.IsNullOrEmpty(outerHtml))
            {
                Match match = _tagRegex.Match(outerHtml);
                foreach (Match match2 in _attribsRegex.Matches(match.Value))
                {
                    Match match3 = _attribNameValueRegex.Match(match2.Value);
                    string name = match3.Groups[1].Value;
                    string str2 = null;
                    if (match3.Groups[2].Success)
                    {
                        str2 = match3.Groups[2].Value;
                    }
                    else if (match3.Groups[3].Success)
                    {
                        str2 = match3.Groups[3].Value;
                    }
                    else if (match3.Groups[4].Success)
                    {
                        str2 = match3.Groups[4].Value;
                    }
                    elementObject.Properties.Add(new PSNoteProperty(name, str2));
                }
            }
        }

        private void ReadyStateChanged(IHTMLEventObj obj)
        {
            if (string.Equals("complete", this._parsedHtml.readyState, StringComparison.OrdinalIgnoreCase))
            {
                this._stateChangeResetEvent.Set();
            }
        }

        public WebCmdletElementCollection AllElements
        {
            get
            {
                if (this._allElements == null)
                {
                    this.EnsureHtmlParser();
                    List<PSObject> list = new List<PSObject>();
                    foreach (IHTMLElement element in this._parsedHtml.all)
                    {
                        list.Add(this.CreateHtmlObject(element, true));
                    }
                    this._allElements = new WebCmdletElementCollection(list);
                }
                return this._allElements;
            }
        }

        public string Content { get; private set; }

        public FormObjectCollection Forms
        {
            get
            {
                if (this._forms == null)
                {
                    this._forms = this.BuildFormsCollection();
                }
                return this._forms;
            }
        }

        public WebCmdletElementCollection Images
        {
            get
            {
                if (this._images == null)
                {
                    this.EnsureHtmlParser();
                    List<PSObject> list = new List<PSObject>();
                    foreach (IHTMLElement element in this._parsedHtml.images)
                    {
                        list.Add(this.CreateHtmlObject(element, true));
                    }
                    this._images = new WebCmdletElementCollection(list);
                }
                return this._images;
            }
        }

        public WebCmdletElementCollection InputFields
        {
            get
            {
                if (this._inputFields == null)
                {
                    this.EnsureHtmlParser();
                    List<PSObject> list = new List<PSObject>();
                    foreach (IHTMLElement element in this._parsedHtml.all)
                    {
                        if (element.tagName.Equals("INPUT", StringComparison.OrdinalIgnoreCase))
                        {
                            list.Add(this.CreateHtmlObject(element, true));
                        }
                    }
                    this._inputFields = new WebCmdletElementCollection(list);
                }
                return this._inputFields;
            }
        }

        public WebCmdletElementCollection Links
        {
            get
            {
                if (this._links == null)
                {
                    this.EnsureHtmlParser();
                    List<PSObject> list = new List<PSObject>();
                    foreach (IHTMLElement element in this._parsedHtml.links)
                    {
                        list.Add(this.CreateHtmlObject(element, true));
                    }
                    this._links = new WebCmdletElementCollection(list);
                }
                return this._links;
            }
        }

        public IHTMLDocument2 ParsedHtml
        {
            get
            {
                this.EnsureHtmlParser();
                return this._parsedHtml;
            }
        }

        public WebCmdletElementCollection Scripts
        {
            get
            {
                if (this._scripts == null)
                {
                    this.EnsureHtmlParser();
                    List<PSObject> list = new List<PSObject>();
                    foreach (IHTMLElement element in this._parsedHtml.scripts)
                    {
                        list.Add(this.CreateHtmlObject(element, true));
                    }
                    this._scripts = new WebCmdletElementCollection(list);
                }
                return this._scripts;
            }
        }
    }
}

