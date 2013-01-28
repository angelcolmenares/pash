namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Text;

    public class WebResponseObject
    {
        private MemoryStream _rawContentStream;

        public WebResponseObject(WebResponse response) : this(response, null)
        {
        }

        public WebResponseObject(WebResponse response, MemoryStream contentStream)
        {
            this.SetResponse(response, contentStream);
            this.InitializeContent();
            this.InitializeRawContent(response);
        }

        private void InitializeContent()
        {
            this.Content = StreamHelper.ReadStream(this.RawContentStream, this.RawContentLength, null).ToArray();
        }

        private void InitializeRawContent(WebResponse baseResponse)
        {
            StringBuilder rawContentHeader = ContentHelper.GetRawContentHeader(baseResponse);
            if (this.Content.Length > 0)
            {
                rawContentHeader.Append(this.ToString());
            }
            this.RawContent = rawContentHeader.ToString();
        }

        private bool IsPrintable(char c)
        {
            if ((!char.IsLetterOrDigit(c) && !char.IsPunctuation(c)) && (!char.IsSeparator(c) && !char.IsSymbol(c)))
            {
                return char.IsWhiteSpace(c);
            }
            return true;
        }

        private void SetResponse(WebResponse response, MemoryStream contentStream)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            this.BaseResponse = response;
            if (contentStream != null)
            {
                this._rawContentStream = contentStream;
            }
            else
            {
                using (Stream stream = StreamHelper.GetResponseStream(response))
                {
                    this._rawContentStream = StreamHelper.ReadStream(stream, response.ContentLength, null);
                }
            }
            this._rawContentStream.Position = 0L;
        }

        public sealed override string ToString()
        {
            char[] chars = Encoding.ASCII.GetChars(this.Content);
            for (int i = 0; i < chars.Length; i++)
            {
                if (!this.IsPrintable(chars[i]))
                {
                    chars[i] = '.';
                }
            }
            return new string(chars);
        }

        public WebResponse BaseResponse { get; set; }

        public byte[] Content { get; protected set; }

        public Dictionary<string, string> Headers
        {
            get
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (string str in this.BaseResponse.Headers.Keys)
                {
                    dictionary[str] = this.BaseResponse.Headers[str];
                }
                return dictionary;
            }
        }

        public string RawContent { get; protected set; }

        public long RawContentLength
        {
            get
            {
                if (this.RawContentStream != null)
                {
                    return this.RawContentStream.Length;
                }
                return -1L;
            }
        }

        public MemoryStream RawContentStream
        {
            get
            {
                return this._rawContentStream;
            }
        }

        public int StatusCode
        {
            get
            {
                return WebResponseHelper.GetStatusCode(this.BaseResponse);
            }
        }

        public string StatusDescription
        {
            get
            {
                return WebResponseHelper.GetStatusDescription(this.BaseResponse);
            }
        }
    }
}

