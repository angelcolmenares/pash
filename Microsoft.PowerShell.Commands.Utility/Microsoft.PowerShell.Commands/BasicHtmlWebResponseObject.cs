namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Management.Automation;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;

    public class BasicHtmlWebResponseObject : WebResponseObject
    {
        private static Regex _attribNameValueRegex;
        private static Regex _attribsRegex;
        private static Regex _imageRegex;
        private WebCmdletElementCollection _images;
        private static Regex _inputFieldRegex;
        private WebCmdletElementCollection _inputFields;
        private static Regex _linkRegex;
        private WebCmdletElementCollection _links;
        private static Regex _tagRegex;

        public BasicHtmlWebResponseObject(WebResponse response) : this(response, null)
        {
        }

        public BasicHtmlWebResponseObject(WebResponse response, MemoryStream contentStream) : base(response, contentStream)
        {
            this.EnsureHtmlParser();
            this.InitializeContent();
            this.InitializeRawContent(response);
        }

        private PSObject CreateHtmlObject(string html, string tagName)
        {
            PSObject elementObject = new PSObject();
            elementObject.Properties.Add(new PSNoteProperty("outerHTML", html));
            elementObject.Properties.Add(new PSNoteProperty("tagName", tagName));
            this.ParseAttributes(html, elementObject);
            return elementObject;
        }

        private void EnsureHtmlParser()
        {
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
            if (_inputFieldRegex == null)
            {
                _inputFieldRegex = new Regex(@"<input\s+[^>]*(/>|>.*?</input>)", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            if (_linkRegex == null)
            {
                _linkRegex = new Regex(@"<a\s+([^>]*\s+)*href\s*=\s*[^>\s]+[^>]*(/>|>.*?</a>)", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            if (_imageRegex == null)
            {
                _imageRegex = new Regex(@"<img\s+[^>]*>", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
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

        public string Content { get; private set; }

        public WebCmdletElementCollection Images
        {
            get
            {
                if (this._images == null)
                {
                    this.EnsureHtmlParser();
                    List<PSObject> list = new List<PSObject>();
                    foreach (Match match in _imageRegex.Matches(this.Content))
                    {
                        list.Add(this.CreateHtmlObject(match.Value, "IMG"));
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
                    foreach (Match match in _inputFieldRegex.Matches(this.Content))
                    {
                        list.Add(this.CreateHtmlObject(match.Value, "INPUT"));
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
                    foreach (Match match in _linkRegex.Matches(this.Content))
                    {
                        list.Add(this.CreateHtmlObject(match.Value, "A"));
                    }
                    this._links = new WebCmdletElementCollection(list);
                }
                return this._links;
            }
        }
    }
}

