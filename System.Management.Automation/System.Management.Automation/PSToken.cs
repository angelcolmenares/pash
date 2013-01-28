namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Language;

    public sealed class PSToken
    {
        private string _content;
        private readonly IScriptExtent _extent;
        private static readonly PSTokenType[] _tokenKindMapping;
        private PSTokenType _type;

        static PSToken()
        {
            PSTokenType[] typeArray = new PSTokenType[0x9c];
            typeArray[1] = PSTokenType.Variable;
            typeArray[2] = PSTokenType.Variable;
            typeArray[3] = PSTokenType.CommandParameter;
            typeArray[4] = PSTokenType.Number;
            typeArray[5] = PSTokenType.LoopLabel;
            typeArray[6] = PSTokenType.CommandArgument;
            typeArray[7] = PSTokenType.CommandArgument;
            typeArray[8] = PSTokenType.NewLine;
            typeArray[9] = PSTokenType.LineContinuation;
            typeArray[10] = PSTokenType.Comment;
            typeArray[12] = PSTokenType.String;
            typeArray[13] = PSTokenType.String;
            typeArray[14] = PSTokenType.String;
            typeArray[15] = PSTokenType.String;
            typeArray[0x10] = PSTokenType.GroupStart;
            typeArray[0x11] = PSTokenType.GroupEnd;
            typeArray[0x12] = PSTokenType.GroupStart;
            typeArray[0x13] = PSTokenType.GroupEnd;
            typeArray[20] = PSTokenType.Operator;
            typeArray[0x15] = PSTokenType.Operator;
            typeArray[0x16] = PSTokenType.GroupStart;
            typeArray[0x17] = PSTokenType.GroupStart;
            typeArray[0x18] = PSTokenType.GroupStart;
            typeArray[0x19] = PSTokenType.StatementSeparator;
            typeArray[0x1a] = PSTokenType.Operator;
            typeArray[0x1b] = PSTokenType.Operator;
            typeArray[0x1c] = PSTokenType.Operator;
            typeArray[0x1d] = PSTokenType.Operator;
            typeArray[30] = PSTokenType.Operator;
            typeArray[0x1f] = PSTokenType.Operator;
            typeArray[0x20] = PSTokenType.Operator;
            typeArray[0x21] = PSTokenType.Operator;
            typeArray[0x22] = PSTokenType.Operator;
            typeArray[0x23] = PSTokenType.Operator;
            typeArray[0x24] = PSTokenType.Operator;
            typeArray[0x25] = PSTokenType.Operator;
            typeArray[0x26] = PSTokenType.Operator;
            typeArray[0x27] = PSTokenType.Operator;
            typeArray[40] = PSTokenType.Operator;
            typeArray[0x29] = PSTokenType.Operator;
            typeArray[0x2a] = PSTokenType.Operator;
            typeArray[0x2b] = PSTokenType.Operator;
            typeArray[0x2c] = PSTokenType.Operator;
            typeArray[0x2d] = PSTokenType.Operator;
            typeArray[0x2e] = PSTokenType.Operator;
            typeArray[0x2f] = PSTokenType.Operator;
            typeArray[0x30] = PSTokenType.Operator;
            typeArray[0x31] = PSTokenType.Operator;
            typeArray[50] = PSTokenType.Operator;
            typeArray[0x33] = PSTokenType.Operator;
            typeArray[0x34] = PSTokenType.Operator;
            typeArray[0x35] = PSTokenType.Operator;
            typeArray[0x36] = PSTokenType.Operator;
            typeArray[0x37] = PSTokenType.Operator;
            typeArray[0x38] = PSTokenType.Operator;
            typeArray[0x39] = PSTokenType.Operator;
            typeArray[0x3a] = PSTokenType.Operator;
            typeArray[0x3b] = PSTokenType.Operator;
            typeArray[60] = PSTokenType.Operator;
            typeArray[0x3d] = PSTokenType.Operator;
            typeArray[0x3e] = PSTokenType.Operator;
            typeArray[0x3f] = PSTokenType.Operator;
            typeArray[0x40] = PSTokenType.Operator;
            typeArray[0x41] = PSTokenType.Operator;
            typeArray[0x42] = PSTokenType.Operator;
            typeArray[0x43] = PSTokenType.Operator;
            typeArray[0x44] = PSTokenType.Operator;
            typeArray[0x45] = PSTokenType.Operator;
            typeArray[70] = PSTokenType.Operator;
            typeArray[0x47] = PSTokenType.Operator;
            typeArray[0x48] = PSTokenType.Operator;
            typeArray[0x49] = PSTokenType.Operator;
            typeArray[0x4a] = PSTokenType.Operator;
            typeArray[0x4b] = PSTokenType.Operator;
            typeArray[0x4c] = PSTokenType.Operator;
            typeArray[0x4d] = PSTokenType.Operator;
            typeArray[0x4e] = PSTokenType.Operator;
            typeArray[0x4f] = PSTokenType.Operator;
            typeArray[80] = PSTokenType.Operator;
            typeArray[0x51] = PSTokenType.Operator;
            typeArray[0x52] = PSTokenType.Operator;
            typeArray[0x53] = PSTokenType.Operator;
            typeArray[0x54] = PSTokenType.Operator;
            typeArray[0x55] = PSTokenType.Operator;
            typeArray[0x56] = PSTokenType.Operator;
            typeArray[0x57] = PSTokenType.Operator;
            typeArray[0x58] = PSTokenType.Operator;
            typeArray[0x59] = PSTokenType.Operator;
            typeArray[90] = PSTokenType.Operator;
            typeArray[0x5b] = PSTokenType.Operator;
            typeArray[0x5c] = PSTokenType.Operator;
            typeArray[0x5d] = PSTokenType.Operator;
            typeArray[0x5e] = PSTokenType.Operator;
            typeArray[0x5f] = PSTokenType.Operator;
            typeArray[0x60] = PSTokenType.Operator;
            typeArray[0x61] = PSTokenType.Operator;
            typeArray[0x62] = PSTokenType.Operator;
            typeArray[0x77] = PSTokenType.Keyword;
            typeArray[120] = PSTokenType.Keyword;
            typeArray[0x79] = PSTokenType.Keyword;
            typeArray[0x7a] = PSTokenType.Keyword;
            typeArray[0x7b] = PSTokenType.Keyword;
            typeArray[0x7c] = PSTokenType.Keyword;
            typeArray[0x7d] = PSTokenType.Keyword;
            typeArray[0x7e] = PSTokenType.Keyword;
            typeArray[0x7f] = PSTokenType.Keyword;
            typeArray[0x80] = PSTokenType.Keyword;
            typeArray[0x81] = PSTokenType.Keyword;
            typeArray[130] = PSTokenType.Keyword;
            typeArray[0x83] = PSTokenType.Keyword;
            typeArray[0x84] = PSTokenType.Keyword;
            typeArray[0x85] = PSTokenType.Keyword;
            typeArray[0x86] = PSTokenType.Keyword;
            typeArray[0x87] = PSTokenType.Keyword;
            typeArray[0x88] = PSTokenType.Keyword;
            typeArray[0x89] = PSTokenType.Keyword;
            typeArray[0x8a] = PSTokenType.Keyword;
            typeArray[0x8b] = PSTokenType.Keyword;
            typeArray[140] = PSTokenType.Keyword;
            typeArray[0x8d] = PSTokenType.Keyword;
            typeArray[0x8e] = PSTokenType.Keyword;
            typeArray[0x8f] = PSTokenType.Keyword;
            typeArray[0x90] = PSTokenType.Keyword;
            typeArray[0x91] = PSTokenType.Keyword;
            typeArray[0x92] = PSTokenType.Keyword;
            typeArray[0x93] = PSTokenType.Keyword;
            typeArray[0x94] = PSTokenType.Keyword;
            typeArray[0x95] = PSTokenType.Keyword;
            typeArray[150] = PSTokenType.Keyword;
            typeArray[0x97] = PSTokenType.Keyword;
            typeArray[0x98] = PSTokenType.Keyword;
            typeArray[0x99] = PSTokenType.Keyword;
            typeArray[0x9a] = PSTokenType.Keyword;
            _tokenKindMapping = typeArray;
        }

        internal PSToken(IScriptExtent extent)
        {
            this._type = PSTokenType.Position;
            this._extent = extent;
        }

        internal PSToken(Token token)
        {
            this._type = GetPSTokenType(token);
            this._extent = token.Extent;
            if (token is StringToken)
            {
                this._content = ((StringToken) token).Value;
            }
            else if (token is VariableToken)
            {
                this._content = ((VariableToken) token).VariablePath.ToString();
            }
        }

        public static PSTokenType GetPSTokenType(Token token)
        {
            if ((token.TokenFlags & TokenFlags.CommandName) != TokenFlags.None)
            {
                return PSTokenType.Command;
            }
            if ((token.TokenFlags & TokenFlags.MemberName) != TokenFlags.None)
            {
                return PSTokenType.Member;
            }
            if ((token.TokenFlags & TokenFlags.AttributeName) != TokenFlags.None)
            {
                return PSTokenType.Attribute;
            }
            if ((token.TokenFlags & TokenFlags.TypeName) != TokenFlags.None)
            {
                return PSTokenType.Type;
            }
            return _tokenKindMapping[(int) token.Kind];
        }

        public string Content
        {
            get
            {
                return (this._content ?? this._extent.Text);
            }
        }

        public int EndColumn
        {
            get
            {
                return this._extent.EndColumnNumber;
            }
        }

        public int EndLine
        {
            get
            {
                return this._extent.EndLineNumber;
            }
        }

        public int Length
        {
            get
            {
                return (this._extent.EndOffset - this._extent.StartOffset);
            }
        }

        public int Start
        {
            get
            {
                return this._extent.StartOffset;
            }
        }

        public int StartColumn
        {
            get
            {
                return this._extent.StartColumnNumber;
            }
        }

        public int StartLine
        {
            get
            {
                return this._extent.StartLineNumber;
            }
        }

        public PSTokenType Type
        {
            get
            {
                return this._type;
            }
        }
    }
}

