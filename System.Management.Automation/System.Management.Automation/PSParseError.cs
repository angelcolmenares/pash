namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Language;

    public sealed class PSParseError
    {
        private string _message;
        private PSToken _psToken;

        internal PSParseError(ParseError error)
        {
            this._message = error.Message;
            this._psToken = new PSToken(error.Extent);
        }

        internal PSParseError(RuntimeException rte)
        {
            this._message = rte.Message;
            this._psToken = new PSToken(rte.ErrorToken);
        }

        public string Message
        {
            get
            {
                return this._message;
            }
        }

        public PSToken Token
        {
            get
            {
                return this._psToken;
            }
        }
    }
}

