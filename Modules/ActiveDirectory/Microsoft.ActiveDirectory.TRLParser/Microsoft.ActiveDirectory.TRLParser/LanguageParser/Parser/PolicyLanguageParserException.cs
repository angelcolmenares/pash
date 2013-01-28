using Microsoft.ActiveDirectory.TRLParser;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser.Parser
{
	[Serializable]
	internal class PolicyLanguageParserException : Exception
	{
		private const string TextLineNumber = "LineNumber";

		private const string TextColumnNumber = "ColumnNumber";

		private const string TextSymbolText = "SymbolText";

		private const string TextErrorMessage = "ErrorMessage";

		private int _lineNum;

		private int _colNum;

		private string _text;

		private string _errMessage;

		public int ColumnNumber
		{
			get
			{
				return this._colNum;
			}
		}

		public string ErrMessage
		{
			get
			{
				return this._errMessage;
			}
		}

		public int LineNumber
		{
			get
			{
				return this._lineNum;
			}
		}

		public string Text
		{
			get
			{
				return this._text;
			}
		}

		public PolicyLanguageParserException()
		{
		}

		protected PolicyLanguageParserException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public PolicyLanguageParserException(int lineNumber, int columnNumber, string text, string errorMessage)
			: base(SR.GetString("POLICY0031", new object[] { lineNumber, columnNumber, text, errorMessage }))
		{
			this._lineNum = lineNumber;
			this._colNum = columnNumber;
			this._text = text;
			this._errMessage = errorMessage;
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("LineNumber", this._lineNum);
			info.AddValue("ColumnNumber", this._colNum);
			info.AddValue("SymbolText", this._text);
			info.AddValue("ErrorMessage", this._errMessage);
		}
	}
}