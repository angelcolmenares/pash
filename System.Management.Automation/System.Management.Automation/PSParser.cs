namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Language;
    using System.Runtime.InteropServices;
    using System.Text;

    public sealed class PSParser
    {
        private ParseError[] errors;
        private readonly List<Token> tokenList = new List<Token>();

        private PSParser()
        {
        }

        private void Parse(string script)
        {
            try
            {
                Parser parser2 = new Parser {
                    ProduceV2Tokens = true
                };
                parser2.Parse(null, script, this.tokenList, out this.errors);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
        }

        public static Collection<PSToken> Tokenize(object[] script, out Collection<PSParseError> errors)
        {
            if (script == null)
            {
                throw PSTraceSource.NewArgumentNullException("script");
            }
            StringBuilder builder = new StringBuilder();
            foreach (object obj2 in script)
            {
                if (obj2 != null)
                {
                    builder.AppendLine(obj2.ToString());
                }
            }
            return Tokenize(builder.ToString(), out errors);
        }

        public static Collection<PSToken> Tokenize(string script, out Collection<PSParseError> errors)
        {
            if (script == null)
            {
                throw PSTraceSource.NewArgumentNullException("script");
            }
            PSParser parser = new PSParser();
            parser.Parse(script);
            errors = parser.Errors;
            return parser.Tokens;
        }

        private Collection<PSParseError> Errors
        {
            get
            {
                Collection<PSParseError> collection = new Collection<PSParseError>();
                foreach (ParseError error in this.errors)
                {
                    collection.Add(new PSParseError(error));
                }
                return collection;
            }
        }

        private Collection<PSToken> Tokens
        {
            get
            {
                Collection<PSToken> collection = new Collection<PSToken>();
                for (int i = 0; i < (this.tokenList.Count - 1); i++)
                {
                    Token token = this.tokenList[i];
                    collection.Add(new PSToken(token));
                }
                return collection;
            }
        }
    }
}

